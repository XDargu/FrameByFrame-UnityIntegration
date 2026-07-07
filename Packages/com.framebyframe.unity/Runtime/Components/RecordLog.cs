using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FbF;

public class RecordLog : MonoBehaviour
{
    struct LogMsg
    {
        public LogMsg(string logString, string stackTrace, LogType type)
        {
            this.logString = logString;
            this.stackTrace = stackTrace;
            this.type = type;
        }

        public string logString;
        public string stackTrace;
        public LogType type;
    }

    List<LogMsg> logHistory = new List<LogMsg>();

    public bool logErrorsAsEvents = false;

    void Start()
    {
        FbFManager.RegisterRecordingOption("Log");
    }

    void OnEnable()
    {
        Application.logMessageReceived += HandleLog;
    }

    void OnDisable()
    {
        Application.logMessageReceived -= HandleLog;
    }

    private void Update()
    {
        if (FbFManager.IsRecordingOptionEnabled("Log"))
        {
            PropertyGroup log = FbFManager.RecordProperties(gameObject, "Log");
            PropertyTable messages = log.AddTable("Messages", "Time", "Type", "Message");

            foreach (LogMsg logMsg in logHistory)
            {
                messages.AddRow(System.DateTime.Now.ToString("[HH:mm:ss]"), logMsg.type.ToString(), logMsg.logString);

                if (logMsg.type == LogType.Error || logMsg.type == LogType.Exception || logMsg.type == LogType.Assert)
                {
                    PropertyGroup logEvent = FbFManager.RecordEvent(gameObject, "Log " + logMsg.type.ToString(), "Log");
                    logEvent.AddComment(logMsg.logString, GetIconType(logMsg.type));
                    logEvent.AddProperty("Stack Trace", logMsg.stackTrace);
                }
            }

            logHistory.Clear();
        }
    }

    void HandleLog(string logString, string stackTrace, LogType type)
    {
        if (FbFManager.IsRecordingOptionEnabled("Log"))
        {
            logHistory.Add(new LogMsg(logString, stackTrace, type));
        }
    }

    FbF.Icon? GetIconType(LogType type)
    {
        switch(type)
        {
            case LogType.Log:       return null;
            case LogType.Assert:    return new Icon("exclamation-circle", "red");
            case LogType.Error:     return new Icon("exclamation-circle", "red");
            case LogType.Exception: return new Icon("exclamation-circle", "red");
            case LogType.Warning:   return new Icon("exclamation-triangle", "yellow");
            default:                return null;
        }
    }
}
