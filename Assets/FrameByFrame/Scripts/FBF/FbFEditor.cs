using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class FrameByFrameWindow : EditorWindow
{
    bool isAdvancedConfigEnabled = false;
    bool areRecordingOptionsEnabled = true;
    bool myBool = true;
    float myFloat = 1.23f;

    // Add menu item named "My Window" to the Window menu
    [MenuItem("Window/Frame by Frame")]
    public static void ShowWindow()
    {
        //Show existing window instance. If one doesn't exist, make one.
        EditorWindow.GetWindow(typeof(FrameByFrameWindow));
    }

    public void OnGUI()
    {
        areRecordingOptionsEnabled = EditorGUILayout.Foldout(areRecordingOptionsEnabled, "Recording options");

        if (areRecordingOptionsEnabled)
        {
            EditorGUI.indentLevel++;
            Dictionary<string, bool> options = FbF.FbFManager.GetRecordingOptions();
            Dictionary<string, bool> tmpOptions = new Dictionary<string, bool>();

            foreach (KeyValuePair<string, bool> entry in options)
            {
                tmpOptions[entry.Key] = EditorGUILayout.Toggle(entry.Key, entry.Value);
            }

            foreach (KeyValuePair<string, bool> entry in tmpOptions)
            {
                FbF.FbFManager.SetRecordingOption(entry.Key, entry.Value);
            }
            EditorGUI.indentLevel--;
        }

        EditorGUILayout.Separator();

        FbF.Config.rawRecordingDefaultPath = EditorGUILayout.TextField("Raw Recording File", FbF.Config.rawRecordingDefaultPath);

        bool isRawRecordingActive = FbF.FbFManager.recorder.GetRecordingMode() == FbF.RecordingMode.RawData;

        if (isRawRecordingActive)
        {
            if (GUILayout.Button("Stop Raw Recording"))
            {
                FbF.FbFManager.recorder.StopRawRecording();
            }
        }
        else
        {
            if (GUILayout.Button("Start Raw Recording"))
            {
                FbF.FbFManager.recorder.StartRawRecording(FbF.Config.rawRecordingDefaultPath);
            }
        }

        EditorGUILayout.Separator();

        isAdvancedConfigEnabled = EditorGUILayout.BeginToggleGroup("Advanced config", isAdvancedConfigEnabled);
        FbF.Config.protocol = EditorGUILayout.TextField("Protocol", FbF.Config.protocol);
        EditorGUILayout.EndToggleGroup();
    }

    public void OnInspectorUpdate()
    {
        this.Repaint();
    }
}

public class FoldoutUsage : EditorWindow
{
    bool showPosition = true;
    string status = "Select a GameObject";

    [MenuItem("Examples/Foldout Usage")]
    static void Init()
    {
        FoldoutUsage window = (FoldoutUsage)GetWindow(typeof(FoldoutUsage));
        window.Show();
    }

    public void OnGUI()
    {
        showPosition = EditorGUILayout.Foldout(showPosition, status);
        if (showPosition)
        {
            if (Selection.activeTransform)
            {
                Selection.activeTransform.position =
                    EditorGUILayout.Vector3Field("Position", Selection.activeTransform.position);
                status = Selection.activeTransform.name;

                EditorGUILayout.IntField("Id", Selection.activeTransform.gameObject.GetInstanceID());

                BoxCollider boxCollider = Selection.activeTransform.gameObject.GetComponent<BoxCollider>();
                if (boxCollider)
                {
                    

                    Vector3 size = Selection.activeTransform.lossyScale;
                    size.x *= boxCollider.size.x;
                    size.y *= boxCollider.size.y;
                    size.z *= boxCollider.size.z;

                    Vector3 pos = boxCollider.center;
                    Vector3 forward = Selection.activeTransform.forward;
                    Vector3 right = Selection.activeTransform.right;
                    Vector3 up = Selection.activeTransform.up;
                    Vector3 halfSize = size * 0.5f;

                    Vector3 corner00 = pos + halfSize.z * forward + halfSize.x * right + halfSize.y * up;
                    Vector3 corner01 = pos - halfSize.z * forward + halfSize.x * right + halfSize.y * up;


                    Debug.DrawLine(corner00, corner01, Color.red);

                    EditorGUILayout.Vector3Field("Box Col Size", boxCollider.bounds.size);
                    EditorGUILayout.Vector3Field("Scaled Size", size);
                    EditorGUILayout.Vector3Field("Center", boxCollider.bounds.center);
                }

                if (!Selection.activeTransform)
                {
                    status = "Select a GameObject";
                    showPosition = false;
                }
            }
        }
    }

    public void OnInspectorUpdate()
    {
        this.Repaint();
    }
}