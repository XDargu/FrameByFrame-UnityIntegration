using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Runtime.InteropServices;
using UnityEditor;
using UnityEngine;

namespace FbF
{
	class FbFManager
	{
		public static RecorderNetworkWebSocket recorder;
		private static Dictionary<string, bool> m_recordingOptions;

		static FbFManager _singleton;
		public static FbFManager singleton
		{
			get
			{
				if (_singleton == null)
					_singleton = new FbFManager();
				return _singleton;
			}
		}

		static FbFManager()
        {
			server = new WebSocketServer(IPAddress.Any, Config.port);
			recorder = new RecorderNetworkWebSocket();
			m_recordingOptions = new Dictionary<string, bool>();
			recorder.Init(server);

			Debug.Log("Initializing FBF Manager");

			EditorApplication.playModeStateChanged += OnPlayModechanged;
			EditorApplication.update += Update;
		}

		private static void OnPlayModechanged(PlayModeStateChange state)
		{
			if (state == PlayModeStateChange.ExitingPlayMode)
            {
				Debug.Log("Shuttind down FBF Manager server");

				server.ShutDown();

				Debug.Log("Storing recording options");

				foreach (KeyValuePair<string, bool> entry in m_recordingOptions)
				{
					EditorPrefs.SetBool(entry.Key, entry.Value);
				}

			}
		}

		public static EntityData RecordEntity(GameObject entity)
		{
			return recorder.RecordEntity(entity);
		}

		public static PropertyGroup RecordProperties(GameObject entity, string group)
        {
			EntityData entitydata = recorder.RecordEntity(entity);
			return entitydata.AddPropertyGroup(group);
			
		}

		public static PropertyGroup RecordEvent(GameObject entity, string name, string tag)
		{
			EntityData entitydata = recorder.RecordEntity(entity);
			EventData eventData = entitydata.AddEvent(name, tag);
			return eventData.properties;
		}

		public static void RecordResource(string path, string type, string content)
        {
			recorder.RecordResource(path, type, content);
		}

		public static bool IsRecordingOptionEnabled(string option)
		{
			bool isEnabled = false;
			m_recordingOptions.TryGetValue(option, out isEnabled);
			return isEnabled;
		}

		public static void RegisterRecordingOption(string option)
		{
			if (!m_recordingOptions.ContainsKey(option))
			{
				// Try to load it from the prefs
				bool value = false;
				if (EditorPrefs.HasKey(option))
				{
					value = EditorPrefs.GetBool(option);
				}

				m_recordingOptions[option] = value;
			}
		}
		public static void SetRecordingOption(string option, bool isEnabled)
		{
			m_recordingOptions[option] = isEnabled;
		}

		public static void FillRecordingOptions(List<RecordingOption> target)
        {
			foreach (KeyValuePair<string, bool> entry in m_recordingOptions)
			{
				RecordingOption option = new RecordingOption();
				option.name = entry.Key;
				option.enabled = entry.Value;
				target.Add(option);
			}
		}

		public static Dictionary<string, bool> GetRecordingOptions()
        {
			return m_recordingOptions;
        }

		public static WebSocketServer server;

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
			server.ShutDown();
		}

		static private void Update()
		{
			server.Update();
		}
	}
}
