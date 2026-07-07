using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Net.Sockets;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.SceneManagement;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace FbF
{
	public class RecorderNetworkWebSocket : INetworkWebSocket
	{
		private readonly object m_clientsLock = new object();
		private readonly List<TcpClient> m_tcpClients = new List<TcpClient>();
		private readonly ConcurrentQueue<string> m_pendingMessages = new ConcurrentQueue<string>();

		private FrameData frameData = new FrameData();
		private uint eventIdx;
		private RecordingMode recordingMode;
		private bool shouldCloseRawDataFile;
		private string rawRecordingPath;
		private SyncCameraDataMessage lastCameraMessage = null;

		public static uint GetEntityID(GameObject entity)
		{
			if (entity.GetInstanceID() > 0)
			{
				return (uint)((long)entity.GetInstanceID() + int.MaxValue);
			}

			return (uint)((long)entity.GetInstanceID() + int.MaxValue);
		}

		public uint GetNextEventIdx()
		{
			return eventIdx++;
		}

		public RecordingMode GetRecordingMode()
		{
			return recordingMode;
		}

		public void StartRawRecording(string path)
		{
			recordingMode = RecordingMode.RawData;
			rawRecordingPath = path + ".fbf";
			if (File.Exists(rawRecordingPath))
			{
				File.Delete(rawRecordingPath);
			}
		}

		public void StopRawRecording()
		{
			recordingMode = RecordingMode.NetworkConnection;
			CloseRawDataFile();
		}

		public EntityData RecordEntity(GameObject entity)
		{
			uint entityId = GetEntityID(entity);
			foreach (EntityData storedEntity in frameData.entities)
			{
				if (storedEntity.id == entityId)
				{
					return storedEntity;
				}
			}

			EntityData entityData = new EntityData(entityId);
			entityData.position = entity.transform.position;
			entityData.up = entity.transform.up;
			entityData.forward = entity.transform.forward;
			entityData.name = entity.name;

			frameData.entities.Add(entityData);
			if (entity.transform.parent)
			{
				entityData.parentId = GetEntityID(entity.transform.parent.gameObject);
				RecordEntity(entity.transform.parent.gameObject);
			}

			return entityData;
		}

		public void RecordResource(string path, string type, string content)
		{
			Resource resource = new Resource(path, type, content);
			frameData.resources.Add(resource);
		}

		public override void Init(WebSocketServer server)
		{
			if (m_server == server)
			{
				return;
			}

			if (m_server != null)
			{
				m_server.UnRegisterNetworkWebSocket(this);
			}

			m_server = server;
			m_server.RegisterNetworkWebSocket(this);
			frameData = new FrameData();
			eventIdx = 0;
			recordingMode = RecordingMode.NetworkConnection;
			shouldCloseRawDataFile = false;

			lock (m_clientsLock)
			{
				m_tcpClients.Clear();
			}

			FbFManager.print("Recorder Manager Initialized");
		}

		public override void OnClientConnected(TcpClient client)
		{
			lock (m_clientsLock)
			{
				if (!m_tcpClients.Contains(client))
				{
					m_tcpClients.Add(client);
				}
			}

			Debug.Log("Client Connected: " + client);
		}

		public override void OnClientDisconnected(TcpClient client)
		{
			lock (m_clientsLock)
			{
				m_tcpClients.Remove(client);
			}

			Debug.Log("Client Disconnected: " + client);
		}

		public override void OnMessage(string data)
		{
			m_pendingMessages.Enqueue(data);
		}

		public override void Shutdown()
		{
			if (shouldCloseRawDataFile)
			{
				CloseRawDataFile();
			}

			if (m_server != null)
			{
				m_server.UnRegisterNetworkWebSocket(this);
				m_server = null;
			}

			lock (m_clientsLock)
			{
				m_tcpClients.Clear();
			}
		}

		public override void Update()
		{
			ProcessPendingMessages();
			SyncEditorCamera();

#if UNITY_EDITOR
			if (EditorApplication.isPaused)
			{
				return;
			}
#endif
			if (!Application.isPlaying)
			{
				if (shouldCloseRawDataFile)
				{
					CloseRawDataFile();
				}
				return;
			}

			frameData.frameId = (uint)Time.frameCount;
			frameData.scene = SceneManager.GetActiveScene().path;
			frameData.clientId = 0;
			frameData.serverTime = (uint)Math.Round(Time.timeSinceLevelLoadAsDouble * 1000);
			frameData.tag = "Server";
			frameData.elapsedTime = Time.deltaTime;

			foreach (EntityData entity in frameData.entities)
			{
				entity.AddSpecialProperty("name", entity.name);
				entity.AddSpecialProperty("position", entity.position);
				entity.AddSpecialProperty("up", entity.up);
				entity.AddSpecialProperty("forward", entity.forward);
			}

			switch (recordingMode)
			{
				case RecordingMode.NetworkConnection:
					SendFrameDataToClients();
					break;
				case RecordingMode.RawData:
					LogFrameDataToFile();
					break;
			}

			frameData.entities.Clear();
			frameData.resources.Clear();
			eventIdx = 0;
		}

		private void ProcessPendingMessages()
		{
			string data;
			while (m_pendingMessages.TryDequeue(out data))
			{
				try
				{
					ProcessMessage(data);
				}
				catch (JsonException exception)
				{
					Debug.LogWarning("Frame by Frame ignored an invalid websocket message: " + exception.Message);
				}
			}
		}

		private void ProcessMessage(string data)
		{
			JsonSerializerSettings settings = new JsonSerializerSettings
			{
				ReferenceLoopHandling = ReferenceLoopHandling.Error
			};

			FbFMessage result = JsonConvert.DeserializeObject<FbFMessage>(data, settings);
			if (result == null)
			{
				return;
			}

			if (result.type == MessageType.RecordingOptionChanged)
			{
				RecordingOptionChangedMessage message = JsonConvert.DeserializeObject<RecordingOptionChangedMessage>(data, settings);
				if (message != null && message.data != null)
				{
					FbFManager.SetRecordingOption(message.data.name, message.data.enabled);
				}
			}
			else if (result.type == MessageType.SyncCameraData)
			{
				lastCameraMessage = JsonConvert.DeserializeObject<SyncCameraDataMessage>(data, settings);
			}
			else if (result.type == MessageType.SyncVisibleShapesData)
			{
				SyncVisibleShapesDataMessage message = JsonConvert.DeserializeObject<SyncVisibleShapesDataMessage>(data, settings);
				Debug.Log(message);
			}
		}

		private void SyncEditorCamera()
		{
#if UNITY_EDITOR
			if (lastCameraMessage == null)
			{
				return;
			}

			SceneView sceneCam = SceneView.lastActiveSceneView;
			if (sceneCam == null || sceneCam.camera == null)
			{
				lastCameraMessage = null;
				return;
			}

			sceneCam.camera.transform.position = lastCameraMessage.position;
			sceneCam.pivot = lastCameraMessage.position;
			sceneCam.LookAt(
				lastCameraMessage.position + lastCameraMessage.forward,
				Quaternion.LookRotation(lastCameraMessage.forward, lastCameraMessage.up));
			sceneCam.Repaint();

			lastCameraMessage = null;
#endif
		}

		private void CloseRawDataFile()
		{
			shouldCloseRawDataFile = false;

			string path = rawRecordingPath + ".temp";
			string pathFinalFile = rawRecordingPath;
			if (!File.Exists(path))
			{
				return;
			}

			using (StreamWriter writer = new StreamWriter(path, true))
			{
				writer.WriteLine("]}");
			}

			if (File.Exists(pathFinalFile))
			{
				File.Delete(pathFinalFile);
			}
			File.Move(path, pathFinalFile);
		}

		private void LogFrameDataToFile()
		{
			JsonSerializerSettings settings = new JsonSerializerSettings
			{
				ReferenceLoopHandling = ReferenceLoopHandling.Error
			};
			string data = JsonConvert.SerializeObject(frameData, settings);

			string path = rawRecordingPath + ".temp";
			bool doesFileExist = File.Exists(path);

			using (StreamWriter writer = new StreamWriter(path, true))
			{
				if (!doesFileExist)
				{
					writer.WriteLine("{");
					writer.WriteLine("\"type\": 1,");
					writer.WriteLine("\"version\": 1,");
					writer.WriteLine("\"rawFrames\": [");
					writer.WriteLine(data);
				}
				else
				{
					writer.WriteLine(",");
					writer.WriteLine(data);
				}
			}

			shouldCloseRawDataFile = true;
		}

		private void SendFrameDataToClients()
		{
			FrameMessage message = new FrameMessage();
			message.data = frameData;
			message.type = MessageType.FrameData;

			JsonSerializerSettings settings = new JsonSerializerSettings
			{
				ReferenceLoopHandling = ReferenceLoopHandling.Error
			};
			string data = JsonConvert.SerializeObject(message, settings);

			TcpClient[] clients;
			lock (m_clientsLock)
			{
				clients = m_tcpClients.ToArray();
			}

			foreach (TcpClient client in clients)
			{
				m_server.SendData(client, data);
			}

			RecordingOptionsMessage optionsMessage = new RecordingOptionsMessage();
			optionsMessage.data = new List<RecordingOption>();
			optionsMessage.type = MessageType.RecordingOptions;
			FbFManager.FillRecordingOptions(optionsMessage.data);

			string recordingOptionsData = JsonConvert.SerializeObject(optionsMessage, settings);

			foreach (TcpClient client in clients)
			{
				m_server.SendData(client, recordingOptionsData);
			}
		}
	}
}
