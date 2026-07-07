using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;

namespace FbF
{
	public class WebSocketServer
	{
		private readonly object m_clientsLock = new object();
		private readonly object m_networkWebSocketsLock = new object();
		private readonly List<TcpClient> m_tcpClients;
		private readonly List<INetworkWebSocket> m_networkWebSockets;

		private TcpListener m_tcpListener;
		private Thread m_tListener;
		private Thread m_tReader;
		private volatile bool m_bShutDown;

		public bool IsRunning
		{
			get { return !m_bShutDown && m_tcpListener != null; }
		}

		public WebSocketServer(IPAddress ipAdress, int port)
		{
			m_networkWebSockets = new List<INetworkWebSocket>();
			m_tcpClients = new List<TcpClient>();
			m_bShutDown = false;

			m_tcpListener = new TcpListener(ipAdress, port);
			m_tcpListener.Start();

			m_tListener = new Thread(Listener);
			m_tListener.IsBackground = true;
			m_tListener.Name = "Frame by Frame WebSocket Listener";

			m_tReader = new Thread(Reader);
			m_tReader.IsBackground = true;
			m_tReader.Name = "Frame by Frame WebSocket Reader";

			m_tListener.Start();
			m_tReader.Start();

			FbFManager.print("Web Socket Server Created");
		}

		private void Listener()
		{
			while (!m_bShutDown)
			{
				try
				{
					TcpClient tcpClient = m_tcpListener.AcceptTcpClient();
					string outdata;
					if (IsHandShake(tcpClient, out outdata))
					{
						FbFManager.print("Handshake");

						byte[] response = Encoding.UTF8.GetBytes(HandShake(outdata));
						tcpClient.GetStream().Write(response, 0, response.Length);

						lock (m_clientsLock)
						{
							m_tcpClients.Add(tcpClient);
						}

						ClientConnected(tcpClient);
					}
					else
					{
						tcpClient.Close();
					}
				}
				catch (SocketException)
				{
					if (!m_bShutDown)
					{
						FbFManager.print("Frame by Frame websocket listener stopped unexpectedly");
					}
				}
				catch (ObjectDisposedException)
				{
					return;
				}
				catch (IOException)
				{
					if (!m_bShutDown)
					{
						FbFManager.print("Frame by Frame websocket handshake failed");
					}
				}
			}
		}

		public void Update()
		{
			INetworkWebSocket[] sockets;
			lock (m_networkWebSocketsLock)
			{
				sockets = m_networkWebSockets.ToArray();
			}

			foreach (var networkWebSocket in sockets)
			{
				networkWebSocket.Update();
			}
		}

		private void Reader()
		{
			while (!m_bShutDown)
			{
				TcpClient[] clients;
				lock (m_clientsLock)
				{
					clients = m_tcpClients.ToArray();
				}

				foreach (TcpClient client in clients)
				{
					if (!client.Connected)
					{
						DisconnectClient(client);
						continue;
					}

					try
					{
						NetworkStream stream = client.GetStream();
						if (stream.DataAvailable && !Read(stream))
						{
							Write(client, (byte)WebSocketOpCode.ConnectionCloseFrame, new byte[0], true);
							DisconnectClient(client);
						}
					}
					catch (IOException)
					{
						DisconnectClient(client);
					}
					catch (ObjectDisposedException)
					{
						DisconnectClient(client);
					}
					catch (SocketException)
					{
						DisconnectClient(client);
					}
				}

				Thread.Sleep(Config.readInterval);
			}
		}

		private bool IsHandShake(TcpClient client, out string data)
		{
			DateTime timeout = DateTime.UtcNow.AddMilliseconds(Config.ExecutionTimeout);
			while (!m_bShutDown && client.Available < 3 && DateTime.UtcNow < timeout)
			{
				Thread.Sleep(1);
			}

			if (m_bShutDown || client.Available < 3)
			{
				data = string.Empty;
				return false;
			}

			byte[] bytes = new byte[client.Available];
			client.GetStream().Read(bytes, 0, bytes.Length);

			data = Encoding.UTF8.GetString(bytes);
			return Regex.IsMatch(data, "^GET");
		}

		private static string HandShake(string data)
		{
			Regex webSocketKeyRegex = new Regex("Sec-WebSocket-Key: (.*)");

			string socketKey = webSocketKeyRegex.Match(data).Groups[1].Value.Trim();

			string response = "HTTP/1.1 101 Switching Protocols" + Environment.NewLine
				+ "Connection: Upgrade" + Environment.NewLine
				+ "Upgrade: websocket" + Environment.NewLine
				+ "Sec-WebSocket-Accept: " + ComputeWebSocketHandshakeSecurityHash09(socketKey) + Environment.NewLine
				+ "Sec-WebSocket-Protocol: " + Config.protocol
				+ Environment.NewLine
				+ Environment.NewLine;

			return response;
		}

		public static string ComputeWebSocketHandshakeSecurityHash09(string secWebSocketKey)
		{
			const string MagicKEY = "258EAFA5-E914-47DA-95CA-C5AB0DC85B11";
			string ret = secWebSocketKey + MagicKEY;

			using (SHA1 sha = new SHA1CryptoServiceProvider())
			{
				byte[] sha1Hash = sha.ComputeHash(Encoding.UTF8.GetBytes(ret));
				return Convert.ToBase64String(sha1Hash);
			}
		}

		public void ShutDown()
		{
			m_bShutDown = true;

			if (m_tcpListener != null)
			{
				m_tcpListener.Stop();
				m_tcpListener = null;
			}

			TcpClient[] clients;
			lock (m_clientsLock)
			{
				clients = m_tcpClients.ToArray();
				m_tcpClients.Clear();
			}

			foreach (TcpClient client in clients)
			{
				try
				{
					client.Close();
				}
				catch (SocketException)
				{
				}
			}

			JoinThread(m_tListener);
			JoinThread(m_tReader);
		}

		private static void JoinThread(Thread thread)
		{
			if (thread != null && thread.IsAlive)
			{
				thread.Join(250);
			}
		}

		public bool Read(Stream stream)
		{
			int firstByte = stream.ReadByte();
			if (firstByte < 0)
			{
				return false;
			}

			byte byte1 = (byte)firstByte;
			byte opCodeFlag = 0x0F;
			WebSocketOpCode opCode = (WebSocketOpCode)(byte1 & opCodeFlag);

			if (opCode == WebSocketOpCode.ConnectionCloseFrame)
			{
				return false;
			}

			int secondByte = stream.ReadByte();
			if (secondByte < 0)
			{
				return false;
			}

			byte byte2 = (byte)secondByte;
			byte maskFlag = 0x80;
			bool isMaskBitSet = (byte2 & maskFlag) == maskFlag;
			ulong len = ReadLength(byte2, stream);
			if (len > int.MaxValue)
			{
				throw new IOException("Websocket payload is too large");
			}

			int payloadLength = (int)len;
			byte[] payload;

			if (isMaskBitSet)
			{
				byte[] maskKey = new byte[4];
				ReadExact(stream, maskKey, maskKey.Length);

				payload = new byte[payloadLength];
				ReadExact(stream, payload, payloadLength);

				for (int i = 0; i < payloadLength; i++)
				{
					payload[i] = (byte)(payload[i] ^ maskKey[i % 4]);
				}
			}
			else
			{
				payload = new byte[payloadLength];
				ReadExact(stream, payload, payloadLength);
			}

			if (opCode == WebSocketOpCode.PingFrame || opCode == WebSocketOpCode.PongFrame)
			{
				return true;
			}

			INetworkWebSocket[] sockets;
			lock (m_networkWebSocketsLock)
			{
				sockets = m_networkWebSockets.ToArray();
			}

			string message = Encoding.UTF8.GetString(payload);
			foreach (var networkWebSocket in sockets)
			{
				networkWebSocket.OnMessage(message);
			}

			return true;
		}

		private static void ReadExact(Stream stream, byte[] buffer, int length)
		{
			int offset = 0;
			while (offset < length)
			{
				int read = stream.Read(buffer, offset, length - offset);
				if (read <= 0)
				{
					throw new IOException("Unexpected end of websocket stream");
				}
				offset += read;
			}
		}

		public static ulong ReadLength(byte byte2, Stream stream)
		{
			byte lenFlag = 0x7F;
			int lenght = byte2 & lenFlag;

			if (lenght <= 125)
			{
				return (ulong)lenght;
			}
			else if (lenght == 126)
			{
				byte[] bSize = new byte[2];
				ReadExact(stream, bSize, 2);

				if (BitConverter.IsLittleEndian)
				{
					Array.Reverse(bSize);
				}

				return BitConverter.ToUInt16(bSize, 0);
			}
			else
			{
				byte[] bSize = new byte[8];
				ReadExact(stream, bSize, 8);

				if (BitConverter.IsLittleEndian)
				{
					Array.Reverse(bSize);
				}

				return BitConverter.ToUInt64(bSize, 0);
			}
		}

		public static void WriteUShort(ushort value, Stream stream, bool isLittleEndian)
		{
			byte[] buffer = BitConverter.GetBytes(value);
			if (BitConverter.IsLittleEndian && !isLittleEndian)
			{
				Array.Reverse(buffer);
			}

			stream.Write(buffer, 0, buffer.Length);
		}

		public static void WriteULong(ulong value, Stream stream, bool isLittleEndian)
		{
			byte[] buffer = BitConverter.GetBytes(value);
			if (BitConverter.IsLittleEndian && !isLittleEndian)
			{
				Array.Reverse(buffer);
			}

			stream.Write(buffer, 0, buffer.Length);
		}

		public void SendData(TcpClient tcpClient, string data)
		{
			if (tcpClient == null || !tcpClient.Connected)
			{
				return;
			}

			byte[] payload = Encoding.UTF8.GetBytes(data);

			try
			{
				Write(tcpClient, (byte)WebSocketOpCode.TextFrame, payload, true);
			}
			catch (IOException)
			{
				DisconnectClient(tcpClient);
			}
			catch (ObjectDisposedException)
			{
				DisconnectClient(tcpClient);
			}
			catch (SocketException)
			{
				DisconnectClient(tcpClient);
			}
		}

		public void Write(TcpClient tcpClient, byte opCode, byte[] payload, bool isLastFrame)
		{
			using (MemoryStream memoryStream = new MemoryStream())
			{
				byte finBitSetAsByte = isLastFrame ? (byte)0x80 : (byte)0x00;
				byte byte1 = (byte)(finBitSetAsByte | opCode);
				memoryStream.WriteByte(byte1);

				byte maskBitSetAsByte = 0x00;

				if (payload.Length < 126)
				{
					byte byte2 = (byte)(maskBitSetAsByte | (byte)payload.Length);
					memoryStream.WriteByte(byte2);
				}
				else if (payload.Length <= ushort.MaxValue)
				{
					byte byte2 = (byte)(maskBitSetAsByte | 126);
					memoryStream.WriteByte(byte2);
					WriteUShort((ushort)payload.Length, memoryStream, false);
				}
				else
				{
					byte byte2 = (byte)(maskBitSetAsByte | 127);
					memoryStream.WriteByte(byte2);
					WriteULong((ulong)payload.Length, memoryStream, false);
				}

				memoryStream.Write(payload, 0, payload.Length);
				byte[] buffer = memoryStream.ToArray();

				tcpClient.GetStream().Write(buffer, 0, buffer.Length);
			}
		}

		public void RegisterNetworkWebSocket(INetworkWebSocket networkWebSocket)
		{
			lock (m_networkWebSocketsLock)
			{
				if (!m_networkWebSockets.Contains(networkWebSocket))
				{
					m_networkWebSockets.Add(networkWebSocket);
				}
			}
		}

		public void UnRegisterNetworkWebSocket(INetworkWebSocket networkWebSocket)
		{
			lock (m_networkWebSocketsLock)
			{
				m_networkWebSockets.Remove(networkWebSocket);
			}
		}

		public void ClientConnected(TcpClient client)
		{
			INetworkWebSocket[] sockets;
			lock (m_networkWebSocketsLock)
			{
				sockets = m_networkWebSockets.ToArray();
			}

			foreach (var networkWebSocket in sockets)
			{
				networkWebSocket.OnClientConnected(client);
			}
		}

		public void ClientDisconnected(TcpClient client)
		{
			INetworkWebSocket[] sockets;
			lock (m_networkWebSocketsLock)
			{
				sockets = m_networkWebSockets.ToArray();
			}

			foreach (var networkWebSocket in sockets)
			{
				networkWebSocket.OnClientDisconnected(client);
			}
		}

		private void DisconnectClient(TcpClient client)
		{
			bool removed;
			lock (m_clientsLock)
			{
				removed = m_tcpClients.Remove(client);
			}

			if (removed)
			{
				ClientDisconnected(client);
			}

			try
			{
				client.Close();
			}
			catch (SocketException)
			{
			}
		}
	}
}

namespace FbF
{
	internal enum WebSocketOpCode
	{
		ContinuationFrame = 0,
		TextFrame = 1,
		BinaryFrame = 2,
		ConnectionCloseFrame = 8,
		PingFrame = 9,
		PongFrame = 10
	}
}
