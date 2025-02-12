using System;
using System.IO;
using System.Collections.Generic;
using System.Net.Sockets;
using Newtonsoft.Json;
using UnityEngine;
using UnityEditor;
using UnityEngine.SceneManagement;

namespace FbF
{
	public class RecorderNetworkWebSocket : INetworkWebSocket
	{
		private List<TcpClient> m_tcpClients;

		private FrameData frameData;

		private UInt32 eventIdx;

		private RecordingMode recordingMode;

		private bool shouldCloseRawDataFile;
		private string rawRecordingPath;

		private SyncCameraDataMessage lastCameraMessage = null;

		public static UInt32 GetEntityID(GameObject entity)
        {
			if (entity.GetInstanceID() > 0)
				return ((UInt32)entity.GetInstanceID()) + Int32.MaxValue;
			else
				return (UInt32)(entity.GetInstanceID() + Int32.MaxValue);
		}

		public UInt32 GetNextEventIdx() { return eventIdx++; }

		public RecordingMode GetRecordingMode() { return recordingMode; }

		public void StartRawRecording(string path)
		{
			recordingMode = RecordingMode.RawData;
			rawRecordingPath = path + ".fbf";
			FileUtil.DeleteFileOrDirectory(rawRecordingPath);
		}

		public void StopRawRecording()
        {
			recordingMode = RecordingMode.NetworkConnection;
			CloseRawDataFile();
		}

		// TODO: Helper for frame data (fast entity access)
		// private Dictionary<UInt32, EntityData> registeredEntities;

		public EntityData RecordEntity(GameObject entity)
		{
			UInt32 entityId = GetEntityID(entity);
			foreach (EntityData storedEntity in frameData.entities)
			{
				if (storedEntity.id == entityId)
				{
					return storedEntity;
				}
			}

			// Add a new one
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

		public override void Init(WebSocketServer server)
		{
			m_server = server;
			m_server.RegisterNetworkWebSocket(this);
			frameData = new FrameData();
			m_tcpClients = new List<TcpClient>();
			eventIdx = 0;
			recordingMode = RecordingMode.NetworkConnection;
			shouldCloseRawDataFile = false;

			// Uncomment to test raw data recording
			// StartRawRecording(Application.persistentDataPath + "/rawRecording");

			FbFManager.print("Recorder Manager Initialized");
		}

		public override void OnClientConnected(TcpClient client)
		{
			bool isFirstClient = m_tcpClients.Count == 0;
			m_tcpClients.Add(client);

			if (isFirstClient)
			{
				// Initialize data
			}

			Debug.Log("Client Connected: " + client.ToString());
		}

		public override void OnClientDisconnected(TcpClient client)
		{
			m_tcpClients.Remove(client);
			bool isLastClient = m_tcpClients.Count == 0;

			if (isLastClient)
			{
				// Initialize data
			}

			Debug.Log("Client Disconnected: " + client.ToString());
		}

		public override void OnMessage(string data)
		{
			Debug.Log("OnMessage");
			Debug.Log(data);

			JsonSerializerSettings settings = new JsonSerializerSettings
			{
				ReferenceLoopHandling = ReferenceLoopHandling.Error
			};
			FbFMessage result = JsonConvert.DeserializeObject<FbFMessage>(data, settings);
			MessageType type = result.type;

			if (type == MessageType.RecordingOptionChanged)
            {
				RecordingOptionChangedMessage message = JsonConvert.DeserializeObject<RecordingOptionChangedMessage>(data, settings);
				FbFManager.SetRecordingOption(message.data.name, message.data.enabled);
            }
			if (type == MessageType.SyncCameraData)
			{
				SyncCameraDataMessage message = JsonConvert.DeserializeObject<SyncCameraDataMessage>(data, settings);
				lastCameraMessage = message;
			}
			if (type == MessageType.SyncVisibleShapesData)
            {
				SyncVisibleShapesDataMessage message = JsonConvert.DeserializeObject<SyncVisibleShapesDataMessage>(data, settings);
				Debug.Log(message);
			}
		}

		public override void Shutdown()
		{
			if (shouldCloseRawDataFile)
			{
				CloseRawDataFile();
			}
			m_server.UnRegisterNetworkWebSocket(this);
		}

		public override void Update()
		{
			if (lastCameraMessage != null)
			{
				SceneView sceneCam = SceneView.lastActiveSceneView;
				sceneCam.camera.transform.position = lastCameraMessage.position;
				sceneCam.pivot = lastCameraMessage.position;
				sceneCam.LookAt(lastCameraMessage.position + lastCameraMessage.forward, Quaternion.LookRotation(lastCameraMessage.forward, lastCameraMessage.up));
				sceneCam.Repaint();

				lastCameraMessage = null;
			}

			if (EditorApplication.isPaused)
            {
				return;
            }
			if (!Application.isPlaying)
            {
				if (shouldCloseRawDataFile)
                {
					CloseRawDataFile();
                }
				return;
            }

			frameData.frameId = (UInt32)Time.frameCount;
			frameData.scene = SceneManager.GetActiveScene().path;
			frameData.clientId = 0; // TODO: Needs to come from the initial config
			frameData.serverTime = (UInt32)Math.Round(Time.timeSinceLevelLoadAsDouble * 1000); // Server time in ms, in a real networked application, it needs to come from the networking system
			frameData.tag = "Server"; // TODO: Needs to come from the initial config
			frameData.elapsedTime = Time.deltaTime;

			// Add special properties
			foreach (EntityData entity in frameData.entities)
			{
				// Properties need to be in this order
				entity.AddSpecialProperty("name", entity.name);
				entity.AddSpecialProperty("position", entity.position);
				entity.AddSpecialProperty("up", entity.up);
				entity.AddSpecialProperty("forward", entity.forward);
			}

			// Send frame data every frame
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
			eventIdx = 0;
		}

		private void CloseRawDataFile()
        {
			shouldCloseRawDataFile = false;

			string path = rawRecordingPath + ".temp";
			string pathFinalFile = rawRecordingPath;
			StreamWriter writer = new StreamWriter(path, true);
			writer.WriteLine("]}");
			writer.Close();

			// Remove .temp extension
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

			bool doesFileExist = System.IO.File.Exists(path);

			StreamWriter writer = new StreamWriter(path, true);
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
			writer.Close();
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

			foreach (TcpClient client in m_tcpClients)
			{
				m_server.SendData(client, data);
			}

			// Send recording options as well. This can probably be done every couple of frames
			RecordingOptionsMessage optionsMessage = new RecordingOptionsMessage();
			optionsMessage.data = new List<RecordingOption>();
			optionsMessage.type = MessageType.RecordingOptions;
			FbFManager.FillRecordingOptions(optionsMessage.data);

			string recordingOptionsData = JsonConvert.SerializeObject(optionsMessage, settings);

			foreach (TcpClient client in m_tcpClients)
			{
				m_server.SendData(client, recordingOptionsData);
			}
		}
	}
}
