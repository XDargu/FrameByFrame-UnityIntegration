using System.Collections.Generic;
using FbF;
using UnityEditor;
using UnityEngine;

namespace FbF.Editor
{
	[FilePath("ProjectSettings/FrameByFrameSettings.asset", FilePathAttribute.Location.ProjectFolder)]
	public class FrameByFrameProjectSettings : ScriptableSingleton<FrameByFrameProjectSettings>
	{
		private const string RuntimeSettingsFolder = "Assets/FrameByFrame";
		private const string RuntimeSettingsResourcesFolder = "Assets/FrameByFrame/Resources";
		private const string RuntimeSettingsAssetPath = "Assets/FrameByFrame/Resources/FrameByFrameRuntimeSettings.asset";

		[SerializeField] private bool autoStartInEditor = true;
		[SerializeField] private bool keepConnectionAliveAcrossPlayMode = true;
		[SerializeField] private bool enableInBuilds = true;
		[SerializeField] private bool developmentBuildsOnly = true;
		[SerializeField] private int websocketPort = 23001;
		[SerializeField] private string websocketProtocol = "frameByframe";
		[SerializeField] private int readIntervalMs = 100;
		[SerializeField] private int handshakeTimeoutMs = 10000;
		[SerializeField] private string rawRecordingDefaultPath = "rawRecording";
		[SerializeField] private List<FrameByFrameRecordingOptionState> recordingOptions = new List<FrameByFrameRecordingOptionState>();

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
			Config.enableInBuilds = enableInBuilds;
			Config.developmentBuildsOnly = developmentBuildsOnly;
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
			SaveRuntimeSettingsAsset();
		}

		public void ApplyRecordingOptionsToRegistry()
		{
			SyncRecordingOptions();
			FbFRecordingOptions.ApplyStates(recordingOptions);
		}

		public void SetRecordingOptionDefault(string id, bool enabled)
		{
			FrameByFrameRecordingOptionState state = GetOrCreateRecordingOptionState(id);
			state.enabled = enabled;
			FbFRecordingOptions.SetEnabled(id, enabled);
			Save(true);
			SaveRuntimeSettingsAsset();
		}

		private bool SyncRecordingOptions()
		{
			FbFRecordingOptions.DiscoverOptions();

			bool changed = false;
			List<FrameByFrameRecordingOptionDefinition> definitions = FbFRecordingOptions.GetDefinitions();
			foreach (FrameByFrameRecordingOptionDefinition definition in definitions)
			{
				if (FindRecordingOptionState(definition.id) == null)
				{
					FrameByFrameRecordingOptionState state = new FrameByFrameRecordingOptionState();
					state.id = definition.id;
					state.enabled = FbFRecordingOptions.IsEnabled(definition.id);
					recordingOptions.Add(state);
					changed = true;
				}
			}

			return changed;
		}

		private FrameByFrameRecordingOptionState GetOrCreateRecordingOptionState(string id)
		{
			FrameByFrameRecordingOptionState state = FindRecordingOptionState(id);
			if (state != null)
			{
				return state;
			}

			state = new FrameByFrameRecordingOptionState();
			state.id = id;
			state.enabled = FbFRecordingOptions.IsEnabled(id);
			recordingOptions.Add(state);
			return state;
		}

		private FrameByFrameRecordingOptionState FindRecordingOptionState(string id)
		{
			foreach (FrameByFrameRecordingOptionState state in recordingOptions)
			{
				if (state != null && state.id == id)
				{
					return state;
				}
			}

			return null;
		}

		private void SaveRuntimeSettingsAsset()
		{
			SyncRecordingOptions();
			EnsureRuntimeSettingsFolder();

			FrameByFrameRuntimeSettings runtimeSettings = AssetDatabase.LoadAssetAtPath<FrameByFrameRuntimeSettings>(RuntimeSettingsAssetPath);
			if (runtimeSettings == null)
			{
				runtimeSettings = ScriptableObject.CreateInstance<FrameByFrameRuntimeSettings>();
				AssetDatabase.CreateAsset(runtimeSettings, RuntimeSettingsAssetPath);
			}

			runtimeSettings.Configure(
				enableInBuilds,
				developmentBuildsOnly,
				Mathf.Clamp(websocketPort, 1, 65535),
				string.IsNullOrEmpty(websocketProtocol) ? "frameByframe" : websocketProtocol,
				Mathf.Max(1, readIntervalMs),
				Mathf.Max(100, handshakeTimeoutMs),
				string.IsNullOrEmpty(rawRecordingDefaultPath) ? "rawRecording" : rawRecordingDefaultPath,
				recordingOptions);

			EditorUtility.SetDirty(runtimeSettings);
			AssetDatabase.SaveAssets();
		}

		private static void EnsureRuntimeSettingsFolder()
		{
			if (!AssetDatabase.IsValidFolder(RuntimeSettingsFolder))
			{
				AssetDatabase.CreateFolder("Assets", "FrameByFrame");
			}

			if (!AssetDatabase.IsValidFolder(RuntimeSettingsResourcesFolder))
			{
				AssetDatabase.CreateFolder(RuntimeSettingsFolder, "Resources");
			}
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
			EditorGUILayout.PropertyField(serializedSettings.FindProperty("enableInBuilds"), new GUIContent("Enable In Builds"));
			EditorGUILayout.PropertyField(serializedSettings.FindProperty("developmentBuildsOnly"), new GUIContent("Development Builds Only"));
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
			EditorGUILayout.LabelField("Recording Options", EditorStyles.boldLabel);
			DrawRecordingOptions(settings);

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

		private static void DrawRecordingOptions(FrameByFrameProjectSettings settings)
		{
			if (settings.SyncRecordingOptions())
			{
				settings.Save(true);
			}

			List<FrameByFrameRecordingOptionDefinition> definitions = FbFRecordingOptions.GetDefinitions();
			if (definitions.Count == 0)
			{
				EditorGUILayout.HelpBox("No recording options have been discovered yet.", UnityEditor.MessageType.Info);
				return;
			}

			foreach (FrameByFrameRecordingOptionDefinition definition in definitions)
			{
				FrameByFrameRecordingOptionState state = settings.GetOrCreateRecordingOptionState(definition.id);
				bool nextValue = EditorGUILayout.Toggle(definition.id, state.enabled);
				if (nextValue != state.enabled)
				{
					settings.SetRecordingOptionDefault(definition.id, nextValue);
				}

				if (!string.IsNullOrEmpty(definition.description))
				{
					EditorGUILayout.LabelField(definition.description, EditorStyles.miniLabel);
				}
			}
		}
	}
}
