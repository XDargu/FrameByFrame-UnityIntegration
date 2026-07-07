using System;

namespace FbF
{
	[AttributeUsage(AttributeTargets.Class, AllowMultiple = true, Inherited = true)]
	public sealed class FbFRecordingOptionAttribute : Attribute
	{
		public string Id { get; private set; }
		public string Description { get; private set; }

		public FbFRecordingOptionAttribute(string id, string description)
		{
			Id = id;
			Description = description;
		}
	}
}
