using System.Collections.Generic;
using FbF;
using UnityEditor;
using UnityEngine;

[InitializeOnLoad]
public static class FrameByFrameEditorBootstrap
{
	static FrameByFrameEditorBootstrap()
	{
		FbFManager.InitializeForEditor();
	}
}

public class FrameByFrameWindow : EditorWindow
{
	bool isAdvancedConfigEnabled = false;
	bool areRecordingOptionsEnabled = true;

	[MenuItem("Window/Frame by Frame")]
	public static void ShowWindow()
	{
		GetWindow<FrameByFrameWindow>("Frame by Frame");
	}

	public void OnGUI()
	{
		FbFManager.EnsureInitialized();

		EditorGUILayout.LabelField("Connection", EditorStyles.boldLabel);
		bool isServerRunning = FbFManager.server != null && FbFManager.server.IsRunning;
		EditorGUILayout.LabelField("Server", isServerRunning ? "Running" : "Stopped");
		EditorGUILayout.LabelField("Port", Config.port.ToString());

		EditorGUILayout.BeginHorizontal();
		if (GUILayout.Button("Start"))
		{
			FbFManager.StartServer();
		}
		if (GUILayout.Button("Stop"))
		{
			FbFManager.StopServer();
		}
		EditorGUILayout.EndHorizontal();

		EditorGUILayout.Separator();

		areRecordingOptionsEnabled = EditorGUILayout.Foldout(areRecordingOptionsEnabled, "Recording options");

		if (areRecordingOptionsEnabled)
		{
			EditorGUI.indentLevel++;
			Dictionary<string, bool> options = FbFManager.GetRecordingOptions();
			Dictionary<string, bool> tmpOptions = new Dictionary<string, bool>();

			foreach (KeyValuePair<string, bool> entry in options)
			{
				tmpOptions[entry.Key] = EditorGUILayout.Toggle(entry.Key, entry.Value);
			}

			foreach (KeyValuePair<string, bool> entry in tmpOptions)
			{
				FbFManager.SetRecordingOption(entry.Key, entry.Value);
			}
			EditorGUI.indentLevel--;
		}

		EditorGUILayout.Separator();

		Config.rawRecordingDefaultPath = EditorGUILayout.TextField("Raw Recording File", Config.rawRecordingDefaultPath);

		bool isRawRecordingActive = FbFManager.recorder != null && FbFManager.recorder.GetRecordingMode() == RecordingMode.RawData;

		if (isRawRecordingActive)
		{
			if (GUILayout.Button("Stop Raw Recording"))
			{
				FbFManager.recorder.StopRawRecording();
			}
		}
		else
		{
			if (GUILayout.Button("Start Raw Recording"))
			{
				FbFManager.recorder.StartRawRecording(Config.rawRecordingDefaultPath);
			}
		}

		EditorGUILayout.Separator();

		isAdvancedConfigEnabled = EditorGUILayout.BeginToggleGroup("Advanced config", isAdvancedConfigEnabled);
		Config.protocol = EditorGUILayout.TextField("Protocol", Config.protocol);
		EditorGUILayout.EndToggleGroup();
	}

	public void OnInspectorUpdate()
	{
		Repaint();
	}
}
