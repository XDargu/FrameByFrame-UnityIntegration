using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;

namespace FbF
{
	public class FbFManager
	{
		public static RecorderNetworkWebSocket recorder;
		public static WebSocketServer server;

		private static readonly object m_stateLock = new object();
		private static readonly Dictionary<string, bool> m_recordingOptions = new Dictionary<string, bool>();
		private static bool m_isInitialized;
		private static bool m_isInitializing;
		private static string m_lastError;

		public static string LastError
		{
			get { return m_lastError; }
		}

		static FbFManager _singleton;
		public static FbFManager singleton
		{
			get
			{
				if (_singleton == null)
				{
					_singleton = new FbFManager();
				}
				return _singleton;
			}
		}

		static FbFManager()
		{
		}

		public static void InitializeForEditor()
		{
			EnsureInitialized();
		}

		public static void EnsureInitialized()
		{
			lock (m_stateLock)
			{
				if (m_isInitialized || m_isInitializing)
				{
					return;
				}

				m_isInitializing = true;
				try
				{
					recorder = new RecorderNetworkWebSocket();
					StartServerLocked();

#if UNITY_EDITOR
					EditorApplication.playModeStateChanged -= OnPlayModeChanged;
					EditorApplication.playModeStateChanged += OnPlayModeChanged;
					EditorApplication.update -= Update;
					EditorApplication.update += Update;
#endif

					m_isInitialized = true;
				}
				finally
				{
					m_isInitializing = false;
				}
			}

			Debug.Log("Initializing FBF Manager");
		}

		public static void StartServer()
		{
			bool shouldInitialize;
			lock (m_stateLock)
			{
				shouldInitialize = !m_isInitialized && !m_isInitializing;
			}

			if (shouldInitialize)
			{
				EnsureInitialized();
				return;
			}

			lock (m_stateLock)
			{
				StartServerLocked();
			}
		}

		private static void StartServerLocked()
		{
			if (server != null && server.IsRunning)
			{
				return;
			}

			if (recorder == null)
			{
				recorder = new RecorderNetworkWebSocket();
			}

			try
			{
				server = new WebSocketServer(IPAddress.Any, Config.port);
				recorder.Init(server);
				m_lastError = string.Empty;
			}
			catch (SocketException exception)
			{
				server = null;
				m_lastError = "Could not start websocket server on port " + Config.port + ": " + exception.Message;
				Debug.LogWarning("Frame by Frame " + m_lastError);
			}
		}

#if UNITY_EDITOR
		private static void OnPlayModeChanged(PlayModeStateChange state)
		{
			if (state == PlayModeStateChange.ExitingEditMode)
			{
				StartServer();
			}

			if (state == PlayModeStateChange.ExitingPlayMode)
			{
				Debug.Log("Persisting FBF recording options");
				PersistRecordingOptions();
				if (!Config.keepConnectionAliveAcrossPlayMode)
				{
					StopServer();
				}
			}

			if (state == PlayModeStateChange.EnteredEditMode || state == PlayModeStateChange.EnteredPlayMode)
			{
				StartServer();
			}
		}

		private static void PersistRecordingOptions()
		{
			lock (m_stateLock)
			{
				foreach (KeyValuePair<string, bool> entry in m_recordingOptions)
				{
					EditorPrefs.SetBool(entry.Key, entry.Value);
				}
			}
		}
#endif

		public static EntityData RecordEntity(GameObject entity)
		{
			EnsureInitialized();
			return recorder.RecordEntity(entity);
		}

		public static PropertyGroup RecordProperties(GameObject entity, string group)
		{
			EnsureInitialized();
			EntityData entityData = recorder.RecordEntity(entity);
			return entityData.AddGroup(group);
		}

		public static PropertyGroup RecordEvent(GameObject entity, string name, string tag)
		{
			EnsureInitialized();
			EntityData entityData = recorder.RecordEntity(entity);
			EventData eventData = entityData.AddEvent(name, tag);
			return eventData.properties;
		}

		public static void RecordResource(string path, string type, string content)
		{
			EnsureInitialized();
			recorder.RecordResource(path, type, content);
		}

		public static bool IsRecordingOptionEnabled(string option)
		{
			lock (m_stateLock)
			{
				bool isEnabled;
				return m_recordingOptions.TryGetValue(option, out isEnabled) && isEnabled;
			}
		}

		public static void RegisterRecordingOption(string option)
		{
			lock (m_stateLock)
			{
				if (m_recordingOptions.ContainsKey(option))
				{
					return;
				}

				bool value = false;
#if UNITY_EDITOR
				if (EditorPrefs.HasKey(option))
				{
					value = EditorPrefs.GetBool(option);
				}
#endif
				m_recordingOptions[option] = value;
			}
		}

		public static void SetRecordingOption(string option, bool isEnabled)
		{
			lock (m_stateLock)
			{
				m_recordingOptions[option] = isEnabled;
			}
		}

		public static void FillRecordingOptions(List<RecordingOption> target)
		{
			lock (m_stateLock)
			{
				foreach (KeyValuePair<string, bool> entry in m_recordingOptions)
				{
					RecordingOption option = new RecordingOption();
					option.name = entry.Key;
					option.enabled = entry.Value;
					target.Add(option);
				}
			}
		}

		public static Dictionary<string, bool> GetRecordingOptions()
		{
			lock (m_stateLock)
			{
				return new Dictionary<string, bool>(m_recordingOptions);
			}
		}

		public static void print(string text)
		{
			Debug.Log(text);
		}

		public static void print(ConsoleColor color, string text)
		{
			Debug.Log(text);
		}

		public static void ShutDown()
		{
			StopServer();
		}

		public static void StopServer()
		{
			lock (m_stateLock)
			{
				if (recorder != null)
				{
					recorder.Shutdown();
				}

				if (server != null)
				{
					server.ShutDown();
					server = null;
				}
			}
		}

		static private void Update()
		{
			WebSocketServer currentServer = server;
			if (currentServer != null && currentServer.IsRunning)
			{
				currentServer.Update();
			}
		}
	}
}
