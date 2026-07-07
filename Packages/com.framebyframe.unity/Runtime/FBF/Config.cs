using System;
using System.IO;
using System.Reflection;

namespace FbF
{
	public static class Config
	{
		public static string version = "0.0.0";
		public static string rawRecordingDefaultPath = "rawRecording";
		public static int port = 23001;
		public static int portQuery = 23000;
		public static int readInterval = 100;
		public static int writeInterval = 16;
		public static int pingEachInterval = 10;
		public static int packageTimeOut = 2000;
		public static int clientTimeOut = 15000;
		public static string protocol = "frameByframe";
		public static int ExecutionTimeout = 10000;
    }
}
