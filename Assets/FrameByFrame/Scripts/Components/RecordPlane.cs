using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FbF;
using System.IO;
using System;

public class RecordPlane : MonoBehaviour {

	public float width = 1.0f;
	public float length = 1.0f;

    [Range(0.0f, 1.0f)]
    public float opacity = 1.0f;

	// Use this for initialization
	void Start () {
		FbFManager.RegisterRecordingOption("ShapeHelpers");
	}
	
	// Update is called once per frame
	void Update () {

		if (FbFManager.IsRecordingOptionEnabled("ShapeHelpers"))
		{
			PropertyGroup entity = FbFManager.RecordProperties(this.gameObject, "ShapeHelpers");

			float maxScale = Mathf.Max(Mathf.Max(gameObject.transform.lossyScale.x, gameObject.transform.lossyScale.y), gameObject.transform.lossyScale.z);
			entity.AddPlane("Plane", gameObject.transform.position, gameObject.transform.up, gameObject.transform.forward, width, length, new Color(1, 0.5f, 0.5f, opacity), "", "ShapeHelpers");
		}
	}
}