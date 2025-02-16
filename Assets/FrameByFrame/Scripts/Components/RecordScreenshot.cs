using FbF;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class RecordScreenshot : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
		FbFManager.RegisterRecordingOption("ShapeHelpers");
	}

	// Update is called once per frame
	void Update()
    {
		Record();
	}

	[System.Diagnostics.Conditional("DEBUG")]
	void Record()
    {
		if (Application.isPlaying)
		{
			// Add screenshot
			string screenshotName =
									"Screenshot_" +
									(UInt32)Time.frameCount +
									".png";

			Texture2D tex = ScreenCapture.CaptureScreenshotAsTexture();
			byte[] jpgTex = tex.EncodeToJPG(35);
			FbFManager.RecordResource(screenshotName, "image/jpg", "{\"blob\":\"data:image/jpg;base64," + Convert.ToBase64String(jpgTex) + "\" }");

			PropertyGroup entity = FbFManager.RecordProperties(this.gameObject, "Screenshot");
			entity.AddPlane("Plane", gameObject.transform.position, gameObject.transform.up, gameObject.transform.forward, tex.width / 1000.0f, tex.height / 1000.0f, new Color(1, 1, 1, 1), screenshotName, "ShapeHelpers");
		}
	}
}
