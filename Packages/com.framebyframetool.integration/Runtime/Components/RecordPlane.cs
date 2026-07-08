using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FbF;
using System.IO;
using System;

[FrameByFrameRecordingOption(FrameByFrameBuiltInRecordingOptions.ShapeHelpers, "Records helper shapes such as planes and screenshots.")]
public class RecordPlane : MonoBehaviour {

	public float width = 1.0f;
	public float length = 1.0f;

    [Range(0.0f, 1.0f)]
    public float opacity = 1.0f;

	// Update is called once per frame
	void Update () {

		if (FbFManager.IsRecordingOptionEnabled(FrameByFrameBuiltInRecordingOptions.ShapeHelpers))
		{
			PropertyGroup entity = FbFManager.RecordProperties(this.gameObject, FrameByFrameBuiltInRecordingOptions.ShapeHelpers);

			float maxScale = Mathf.Max(Mathf.Max(gameObject.transform.lossyScale.x, gameObject.transform.lossyScale.y), gameObject.transform.lossyScale.z);
			entity.AddPlane("Plane", gameObject.transform.position, gameObject.transform.up, gameObject.transform.forward, width, length, new Color(1, 0.5f, 0.5f, opacity), "", FrameByFrameBuiltInRecordingOptions.ShapeHelpers);
		}
	}
}
