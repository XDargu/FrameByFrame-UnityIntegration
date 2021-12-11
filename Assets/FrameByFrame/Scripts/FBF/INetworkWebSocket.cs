using System.Net.Sockets;

namespace FbF
{
	public abstract class INetworkWebSocket
	{
		public abstract void Init(WebSocketServer server);
		public abstract void Shutdown();
		public abstract void OnClientConnected(TcpClient client);
		public abstract void OnClientDisconnected(TcpClient client);
		public abstract void OnMessage(string data);
		public abstract void Update();

		protected WebSocketServer m_server;
	}
}
