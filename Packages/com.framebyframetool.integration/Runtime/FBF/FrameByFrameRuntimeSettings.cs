using System.Collections.Generic;
using UnityEngine;

namespace FbF
{
	public class FrameByFrameRuntimeSettings : ScriptableObject
	{
		public const string ResourceName = "FrameByFrameRuntimeSettings";

		[SerializeField] private bool enableInBuilds = true;
		[SerializeField] private bool developmentBuildsOnly = true;
		[SerializeField] private int websocketPort = 23001;
		[SerializeField] private string websocketProtocol = "frameByframe";
		[SerializeField] private int readIntervalMs = 100;
		[SerializeField] private int handshakeTimeoutMs = 10000;
		[SerializeField] private string rawRecordingDefaultPath = "rawRecording";
		[SerializeField] private List<FrameByFrameRecordingOptionState> recordingOptions = new List<FrameByFrameRecordingOptionState>();

		public bool EnableInBuilds
		{
			get { return enableInBuilds; }
		}

		public bool DevelopmentBuildsOnly
		{
			get { return developmentBuildsOnly; }
		}

		public void Configure(bool enableInBuilds, bool developmentBuildsOnly, int websocketPort, string websocketProtocol, int readIntervalMs, int handshakeTimeoutMs, string rawRecordingDefaultPath, List<FrameByFrameRecordingOptionState> recordingOptions)
		{
			this.enableInBuilds = enableInBuilds;
			this.developmentBuildsOnly = developmentBuildsOnly;
			this.websocketPort = websocketPort;
			this.websocketProtocol = websocketProtocol;
			this.readIntervalMs = readIntervalMs;
			this.handshakeTimeoutMs = handshakeTimeoutMs;
			this.rawRecordingDefaultPath = rawRecordingDefaultPath;
			this.recordingOptions = CloneStates(recordingOptions);
		}

		public void Apply()
		{
			Config.enableInBuilds = enableInBuilds;
			Config.developmentBuildsOnly = developmentBuildsOnly;
			Config.port = Mathf.Clamp(websocketPort, 1, 65535);
			Config.protocol = string.IsNullOrEmpty(websocketProtocol) ? "frameByframe" : websocketProtocol;
			Config.readInterval = Mathf.Max(1, readIntervalMs);
			Config.ExecutionTimeout = Mathf.Max(100, handshakeTimeoutMs);
			Config.rawRecordingDefaultPath = string.IsNullOrEmpty(rawRecordingDefaultPath) ? "rawRecording" : rawRecordingDefaultPath;
			FbFRecordingOptions.ApplyStates(recordingOptions);
		}

		public static FrameByFrameRuntimeSettings Load()
		{
			return Resources.Load<FrameByFrameRuntimeSettings>(ResourceName);
		}

		private static List<FrameByFrameRecordingOptionState> CloneStates(List<FrameByFrameRecordingOptionState> source)
		{
			List<FrameByFrameRecordingOptionState> result = new List<FrameByFrameRecordingOptionState>();
			if (source == null)
			{
				return result;
			}

			foreach (FrameByFrameRecordingOptionState sourceState in source)
			{
				if (sourceState == null)
				{
					continue;
				}

				FrameByFrameRecordingOptionState state = new FrameByFrameRecordingOptionState();
				state.id = sourceState.id;
				state.enabled = sourceState.enabled;
				result.Add(state);
			}

			return result;
		}
	}
}
