using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;
using System.IO;

namespace FbF
{
	public class WebSocketServer
	{
		private TcpListener m_tcpListener;
		private Thread m_tListener;
		private Thread m_tReader;
		private List<TcpClient> m_tcpClients;

		private List<INetworkWebSocket> m_networkWebSockets;

		private bool m_bShutDown;

		public WebSocketServer(IPAddress ipAdress, int port)
		{
			if (m_tcpListener == null)
			{
				m_networkWebSockets = new List<INetworkWebSocket>();
				m_tcpClients = new List<TcpClient>();
				m_bShutDown = false;

				m_tcpListener = new TcpListener(ipAdress, port);
				m_tcpListener.Start();

				m_tListener = new Thread(Listener);
				m_tReader = new Thread(Reader);
				m_tListener.Start();
				m_tReader.Start();

				FbFManager.print("Web Socket Server Created");
			}
		}

		private void Listener()
		{
			//while (!m_bShutDown)
			while (true)
			{
				TcpClient tcpClient = m_tcpListener.AcceptTcpClient();
				string outdata;
				if (IsHandShake(tcpClient, out outdata))
				{
					FbFManager.print("Handshake");

					Byte[] response = Encoding.UTF8.GetBytes(HandShake(outdata));
					tcpClient.GetStream().Write(response, 0, response.Length);

					m_tcpClients.Add(tcpClient);

					ClientConnected(tcpClient);
				}
			}
		}

		public void Update()
		{
			foreach (var networkWebSocket in m_networkWebSockets)
			{
				networkWebSocket.Update();
			}
		}

		private void Reader( )
		{
			while(!m_bShutDown)
			{
				for (int i = m_tcpClients.Count - 1; i >= 0; i--)
				{
					if (m_tcpClients[i].Connected)
					{
						if (m_tcpClients[i].GetStream().DataAvailable)
						{
							if (!Read(m_tcpClients[i].GetStream()))
							{
								Write(m_tcpClients[i], (byte)WebSocketOpCode.ConnectionCloseFrame, new byte[0], true);

								ClientDisconnected(m_tcpClients[i]);
								m_tcpClients.RemoveAt(i);
							}
						}
					}
					else
                    {
						ClientDisconnected(m_tcpClients[i]);
						m_tcpClients.RemoveAt(i);
					}
				}

				Thread.Sleep(Config.readInterval);
			}
		}

		private bool IsHandShake(TcpClient client , out string data)
		{
			while (client.Available < 3) ;

			Byte[] bytes = new Byte[client.Available];

			client.GetStream().Read(bytes, 0, bytes.Length);

			//translate bytes of request to string
			data = Encoding.UTF8.GetString(bytes);
			return Regex.IsMatch(data, "^GET");
		}

		private static string HandShake(string data)
		{
			Regex webSocketKeyRegex = new Regex("Sec-WebSocket-Key: (.*)");
			Regex webSocketVersionRegex = new Regex("Sec-WebSocket-Version: (.*)");

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

		public static String ComputeWebSocketHandshakeSecurityHash09(String secWebSocketKey)
		{
			const String MagicKEY = "258EAFA5-E914-47DA-95CA-C5AB0DC85B11";
			String secWebSocketAccept = String.Empty;

			String ret = secWebSocketKey + MagicKEY;

			SHA1 sha = new SHA1CryptoServiceProvider();
			byte[] sha1Hash = sha.ComputeHash(Encoding.UTF8.GetBytes(ret));

			secWebSocketAccept = Convert.ToBase64String(sha1Hash);

			return secWebSocketAccept;
		}

		public void ShutDown()
		{
			m_bShutDown = true;
		}

		public bool Read(Stream stream)
		{
			byte byte1;

			byte1 = (byte)stream.ReadByte();
			
			byte finBitFlag = 0x80;
			byte opCodeFlag = 0x0F;
			bool isFinBitSet = (byte1 & finBitFlag) == finBitFlag;
			WebSocketOpCode opCode = (WebSocketOpCode)(byte1 & opCodeFlag);

			if (opCode == WebSocketOpCode.ConnectionCloseFrame)
			{
				return false;
			}

			byte byte2 = (byte)stream.ReadByte();
			byte maskFlag = 0x80;
			bool isMaskBitSet = (byte2 & maskFlag) == maskFlag;
			UInt64 len = ReadLength(byte2, stream);
			byte[] payload;

			if (isMaskBitSet)
			{
				byte[] maskKey = new byte[4];
				stream.Read(maskKey, 0, 4);

				payload = new byte[len];
				stream.Read(payload, 0, (int)len);

				for (UInt64 i = 0; i < len; i++)
				{
					payload[i] = (Byte)(payload[i] ^ maskKey[i % 4]);
				}
			}
			else
			{
				payload = new byte[len];
				stream.Read(payload, 0, (int)len);
			}

			foreach (var networkWebSocket in m_networkWebSockets)
			{
				networkWebSocket.OnMessage(Encoding.UTF8.GetString(payload));
			}

			return true;
		}

		public static UInt64 ReadLength(byte byte2, Stream stream)
		{
			byte lenFlag = 0x7F;
			int lenght = byte2 & lenFlag;

			if (lenght <= 125)
			{
				return (UInt64)lenght;
			}
			else if (lenght == 126)
            {
				byte[] bSize = new byte[2];
				stream.Read(bSize, 0, 2);

				if (BitConverter.IsLittleEndian)
					Array.Reverse(bSize);

				return (UInt64)BitConverter.ToUInt16(bSize, 0);
			}
			else
			{
				byte[] bSize = new byte[8];
				stream.Read(bSize, 0, 8);

				if (BitConverter.IsLittleEndian)
					Array.Reverse(bSize);

				return (UInt64)BitConverter.ToUInt64(bSize, 0);
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
			if (tcpClient.Connected)
			{
				byte[] payload = Encoding.UTF8.GetBytes(data);

				Write(tcpClient, (byte)WebSocketOpCode.TextFrame, payload, true);
			}
		}

		public void Write(TcpClient tcpClient, byte opCode, byte[] payload, bool isLastFrame)
		{
			using (MemoryStream memoryStream = new MemoryStream())
			{
				byte finBitSetAsByte = isLastFrame ? (byte)0x80 : (byte)0x00;
				byte byte1 = (byte)(finBitSetAsByte | opCode);
				memoryStream.WriteByte(byte1);

				byte maskBitSetAsByte = (byte)0x00;

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

		public void RegisterNetworkWebSocket( INetworkWebSocket networkWebSocket )
		{
			m_networkWebSockets.Add(networkWebSocket);
		}

		public void UnRegisterNetworkWebSocket( INetworkWebSocket networkWebSocket )
		{
			m_networkWebSockets.Remove(networkWebSocket);
		}

		public void ClientConnected(TcpClient client)
		{
			foreach(var networkWebSocket in m_networkWebSockets)
			{
				networkWebSocket.OnClientConnected(client);
			}
		}

		public void ClientDisconnected(TcpClient client)
		{
			foreach (var networkWebSocket in m_networkWebSockets)
			{
				networkWebSocket.OnClientDisconnected( client);
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