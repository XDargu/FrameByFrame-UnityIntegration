using System;
using System.Collections.Generic;
using FbF;
using UnityEditor;
using UnityEngine;

namespace FbF.Editor
{
	[InitializeOnLoad]
	public static class FrameByFrameEditorBootstrap
	{
		static FrameByFrameEditorBootstrap()
		{
			if (ShouldSkipAutoStart())
			{
				return;
			}

			FrameByFrameProjectSettings.instance.ApplyToConfig();
			if (FrameByFrameProjectSettings.instance.AutoStartInEditor)
			{
				FbFManager.InitializeForEditor();
			}
		}

		private static bool ShouldSkipAutoStart()
		{
			if (Application.isBatchMode)
			{
				return true;
			}

			string[] args = Environment.GetCommandLineArgs();
			foreach (string arg in args)
			{
				if (arg.IndexOf("AssetImportWorker", StringComparison.OrdinalIgnoreCase) >= 0)
				{
					return true;
				}
			}

			return false;
		}
	}

	public class FrameByFrameWindow : EditorWindow
	{
		private Vector2 scrollPosition;
		private bool showRecordingOptions = true;

		[MenuItem("Window/Frame by Frame")]
		public static void ShowWindow()
		{
			GetWindow<FrameByFrameWindow>("Frame by Frame");
		}

		public void OnGUI()
		{
			FrameByFrameProjectSettings.instance.ApplyToConfig();

			scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);
			DrawHeader();
			EditorGUILayout.Space(8);
			DrawConnectionPanel();
			EditorGUILayout.Space(8);
			DrawRawRecordingPanel();
			EditorGUILayout.Space(8);
			DrawRecordingOptionsPanel();
			EditorGUILayout.EndScrollView();
		}

		public void OnInspectorUpdate()
		{
			Repaint();
		}

		private void DrawHeader()
		{
			using (new EditorGUILayout.HorizontalScope())
			{
				EditorGUILayout.LabelField("Frame by Frame", EditorStyles.boldLabel);
				if (GUILayout.Button("Project Settings", GUILayout.Width(120)))
				{
					SettingsService.OpenProjectSettings("Project/Frame by Frame");
				}
			}
		}

		private void DrawConnectionPanel()
		{
			bool isRunning = FbFManager.server != null && FbFManager.server.IsRunning;
			int clientCount = FbFManager.server != null ? FbFManager.server.ClientCount : 0;
			string lastError = GetLastError();

			EditorGUILayout.BeginVertical(EditorStyles.helpBox);
			EditorGUILayout.LabelField("Connection", EditorStyles.boldLabel);

			DrawStatusRow("Server", isRunning ? "Running" : "Stopped");
			DrawStatusRow("Clients", clientCount.ToString());
			DrawStatusRow("Port", Config.port.ToString());
			DrawStatusRow("Protocol", Config.protocol);
			DrawStatusRow("Play Mode", Config.keepConnectionAliveAcrossPlayMode ? "Keep connection alive" : "Stop on exit");

			if (!string.IsNullOrEmpty(lastError))
			{
				EditorGUILayout.HelpBox(lastError, UnityEditor.MessageType.Warning);
			}

			using (new EditorGUILayout.HorizontalScope())
			{
				GUI.enabled = !isRunning;
				if (GUILayout.Button("Start"))
				{
					FrameByFrameProjectSettings.instance.SaveAndApply();
					FbFManager.StartServer();
				}

				GUI.enabled = isRunning;
				if (GUILayout.Button("Stop"))
				{
					FbFManager.StopServer();
				}

				GUI.enabled = true;
				if (GUILayout.Button("Restart"))
				{
					FrameByFrameProjectSettings.instance.SaveAndApply();
					FbFManager.StopServer();
					FbFManager.StartServer();
				}
			}

			EditorGUILayout.EndVertical();
		}

		private void DrawRawRecordingPanel()
		{
			bool hasRecorder = FbFManager.recorder != null;
			bool isRawRecordingActive = hasRecorder && FbFManager.recorder.GetRecordingMode() == RecordingMode.RawData;

			EditorGUILayout.BeginVertical(EditorStyles.helpBox);
			EditorGUILayout.LabelField("Raw Recording", EditorStyles.boldLabel);
			DrawStatusRow("Path", Config.rawRecordingDefaultPath + ".fbf");
			DrawStatusRow("Mode", isRawRecordingActive ? "Recording raw frames" : "Network stream");

			using (new EditorGUILayout.HorizontalScope())
			{
				GUI.enabled = hasRecorder && !isRawRecordingActive;
				if (GUILayout.Button("Start Raw Recording"))
				{
					FbFManager.EnsureInitialized();
					FbFManager.recorder.StartRawRecording(Config.rawRecordingDefaultPath);
				}

				GUI.enabled = hasRecorder && isRawRecordingActive;
				if (GUILayout.Button("Stop Raw Recording"))
				{
					FbFManager.recorder.StopRawRecording();
				}

				GUI.enabled = true;
			}

			EditorGUILayout.EndVertical();
		}

		private void DrawRecordingOptionsPanel()
		{
			EditorGUILayout.BeginVertical(EditorStyles.helpBox);
			using (new EditorGUILayout.HorizontalScope())
			{
				showRecordingOptions = EditorGUILayout.Foldout(showRecordingOptions, "Recording Options", true);
				if (GUILayout.Button("Refresh", GUILayout.Width(80)))
				{
					Repaint();
				}
			}

			if (showRecordingOptions)
			{
				Dictionary<string, bool> options = FbFManager.GetRecordingOptions();
				if (options.Count == 0)
				{
					EditorGUILayout.HelpBox("No recording options have been registered yet. Options appear after recorder components initialize.", UnityEditor.MessageType.Info);
				}
				else
				{
					foreach (KeyValuePair<string, bool> entry in options)
					{
						bool nextValue = EditorGUILayout.Toggle(entry.Key, entry.Value);
						if (nextValue != entry.Value)
						{
							FbFManager.SetRecordingOption(entry.Key, nextValue);
						}
					}
				}
			}

			EditorGUILayout.EndVertical();
		}

		private static void DrawStatusRow(string label, string value)
		{
			using (new EditorGUILayout.HorizontalScope())
			{
				EditorGUILayout.LabelField(label, GUILayout.Width(110));
				EditorGUILayout.SelectableLabel(value, GUILayout.Height(EditorGUIUtility.singleLineHeight));
			}
		}

		private static string GetLastError()
		{
			if (!string.IsNullOrEmpty(FbFManager.LastError))
			{
				return FbFManager.LastError;
			}

			if (FbFManager.server != null && !string.IsNullOrEmpty(FbFManager.server.LastError))
			{
				return FbFManager.server.LastError;
			}

			return string.Empty;
		}
	}
}
