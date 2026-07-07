using FbF;
using UnityEditor;
using UnityEngine;

namespace FbF.Editor
{
	[FilePath("ProjectSettings/FrameByFrameSettings.asset", FilePathAttribute.Location.ProjectFolder)]
	public class FrameByFrameProjectSettings : ScriptableSingleton<FrameByFrameProjectSettings>
	{
		[SerializeField] private bool autoStartInEditor = true;
		[SerializeField] private bool keepConnectionAliveAcrossPlayMode = true;
		[SerializeField] private int websocketPort = 23001;
		[SerializeField] private string websocketProtocol = "frameByframe";
		[SerializeField] private int readIntervalMs = 100;
		[SerializeField] private int handshakeTimeoutMs = 10000;
		[SerializeField] private string rawRecordingDefaultPath = "rawRecording";

		public bool AutoStartInEditor
		{
			get { return autoStartInEditor; }
		}

		public bool KeepConnectionAliveAcrossPlayMode
		{
			get { return keepConnectionAliveAcrossPlayMode; }
		}

		public int WebsocketPort
		{
			get { return websocketPort; }
		}

		public string WebsocketProtocol
		{
			get { return websocketProtocol; }
		}

		public string RawRecordingDefaultPath
		{
			get { return rawRecordingDefaultPath; }
		}

		public void ApplyToConfig()
		{
			Config.autoStartInEditor = autoStartInEditor;
			Config.keepConnectionAliveAcrossPlayMode = keepConnectionAliveAcrossPlayMode;
			Config.port = Mathf.Clamp(websocketPort, 1, 65535);
			Config.protocol = string.IsNullOrEmpty(websocketProtocol) ? "frameByframe" : websocketProtocol;
			Config.readInterval = Mathf.Max(1, readIntervalMs);
			Config.ExecutionTimeout = Mathf.Max(100, handshakeTimeoutMs);
			Config.rawRecordingDefaultPath = string.IsNullOrEmpty(rawRecordingDefaultPath) ? "rawRecording" : rawRecordingDefaultPath;
		}

		public void SaveAndApply()
		{
			ApplyToConfig();
			Save(true);
		}

		[SettingsProvider]
		public static SettingsProvider CreateSettingsProvider()
		{
			SettingsProvider provider = new SettingsProvider("Project/Frame by Frame", SettingsScope.Project)
			{
				label = "Frame by Frame",
				guiHandler = DrawSettings,
				keywords = new[]
				{
					"Frame",
					"Frame by Frame",
					"FbF",
					"WebSocket",
					"Recording",
					"Raw Recording"
				}
			};

			return provider;
		}

		private static void DrawSettings(string searchContext)
		{
			FrameByFrameProjectSettings settings = instance;
			SerializedObject serializedSettings = new SerializedObject(settings);

			EditorGUILayout.LabelField("Connection", EditorStyles.boldLabel);
			EditorGUILayout.PropertyField(serializedSettings.FindProperty("autoStartInEditor"), new GUIContent("Auto Start In Editor"));
			EditorGUILayout.PropertyField(serializedSettings.FindProperty("keepConnectionAliveAcrossPlayMode"), new GUIContent("Keep Alive Across Play Mode"));
			EditorGUILayout.PropertyField(serializedSettings.FindProperty("websocketPort"), new GUIContent("WebSocket Port"));
			EditorGUILayout.PropertyField(serializedSettings.FindProperty("websocketProtocol"), new GUIContent("WebSocket Protocol"));

			EditorGUILayout.Space();
			EditorGUILayout.LabelField("Timing", EditorStyles.boldLabel);
			EditorGUILayout.PropertyField(serializedSettings.FindProperty("readIntervalMs"), new GUIContent("Read Interval (ms)"));
			EditorGUILayout.PropertyField(serializedSettings.FindProperty("handshakeTimeoutMs"), new GUIContent("Handshake Timeout (ms)"));

			EditorGUILayout.Space();
			EditorGUILayout.LabelField("Recording", EditorStyles.boldLabel);
			EditorGUILayout.PropertyField(serializedSettings.FindProperty("rawRecordingDefaultPath"), new GUIContent("Raw Recording Path"));

			if (serializedSettings.ApplyModifiedProperties())
			{
				settings.SaveAndApply();
			}

			EditorGUILayout.Space();
			EditorGUILayout.HelpBox("Restart the Frame by Frame server after changing port or protocol settings.", UnityEditor.MessageType.Info);

			using (new EditorGUILayout.HorizontalScope())
			{
				if (GUILayout.Button("Apply"))
				{
					settings.SaveAndApply();
				}

				if (GUILayout.Button("Apply and Restart Server"))
				{
					settings.SaveAndApply();
					FbFManager.StopServer();
					FbFManager.StartServer();
				}
			}
		}
	}
}
