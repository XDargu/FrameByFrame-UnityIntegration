using System;

namespace FbF
{
	[AttributeUsage(AttributeTargets.Class, AllowMultiple = true, Inherited = true)]
	public sealed class FrameByFrameRecordingOptionAttribute : Attribute
	{
		public string Id { get; private set; }
		public string Description { get; private set; }

		public FrameByFrameRecordingOptionAttribute(string id, string description)
		{
			Id = id;
			Description = description;
		}
	}
}
