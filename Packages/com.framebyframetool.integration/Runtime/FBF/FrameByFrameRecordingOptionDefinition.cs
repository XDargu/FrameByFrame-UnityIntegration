using System;
using System.Runtime.Serialization;

namespace FbF
{
	[Serializable]
	public class FrameByFrameRecordingOptionDefinition
	{
		[DataMember]
		public string id;
		[DataMember]
		public string description;

		public FrameByFrameRecordingOptionDefinition(string id, string description)
		{
			this.id = id;
			this.description = description;
		}
	}

	[Serializable]
	public class FrameByFrameRecordingOptionState
	{
		public string id;
		public bool enabled;
	}
}
