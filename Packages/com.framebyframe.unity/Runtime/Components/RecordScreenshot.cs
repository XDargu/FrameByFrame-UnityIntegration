using FbF;
using System;
using System.Collections;
using UnityEngine;

public class RecordScreenshot : MonoBehaviour
{
	public int jpgQuality = 35;

	// Cached texture object to reuse memory across frames
	private Texture2D cachedTex;
	// Cached WaitForEndOfFrame to eliminate garbage generation in the loop
	private WaitForEndOfFrame waitFrame = new WaitForEndOfFrame();

	void Start()
	{
		FbFManager.RegisterRecordingOption("ShapeHelpers");
		// Start the loop to run every single frame
		StartCoroutine(RecordLoop());
	}

	void OnDestroy()
	{
		// Clean up the texture asset safely when the object is destroyed
		if (cachedTex != null)
		{
			Destroy(cachedTex);
		}
	}

	private IEnumerator RecordLoop()
	{
		// Continuous loop that runs for the lifetime of this component
		while (true)
		{
			// Yields and safely waits inside the "drawing frame" window
			yield return waitFrame;

			if (Application.isPlaying && FbFManager.IsRecordingOptionEnabled("ShapeHelpers"))
			{
				Record();
			}
		}
	}

	void Record()
	{
		int width = Screen.width;
		int height = Screen.height;

		// Initialize or resize the cached texture only when screen resolution changes
		if (cachedTex == null || cachedTex.width != width || cachedTex.height != height)
		{
			if (cachedTex != null) Destroy(cachedTex);
			cachedTex = new Texture2D(width, height, TextureFormat.RGB24, false);
		}

		// Read screen pixels directly into the cached texture (safe here)
		Rect rect = new Rect(0, 0, width, height);
		cachedTex.ReadPixels(rect, 0, 0);
		cachedTex.Apply();

		try
		{
			string screenshotName = "Screenshot_" + (uint)Time.frameCount + ".jpg";
			byte[] jpgTex = cachedTex.EncodeToJPG(Mathf.Clamp(jpgQuality, 1, 100));
			if (jpgTex == null || jpgTex.Length == 0)
			{
				return;
			}

			FbFManager.RecordResource(
				screenshotName,
				"image/jpg",
				"{\"blob\":\"data:image/jpg;base64," + Convert.ToBase64String(jpgTex) + "\" }");

			PropertyGroup entity = FbFManager.RecordProperties(gameObject, "Screenshot");
			entity.AddPlane(
				"Plane",
				gameObject.transform.position,
				gameObject.transform.up,
				gameObject.transform.forward,
				cachedTex.width / 1000.0f,
				cachedTex.height / 1000.0f,
				new Color(1, 1, 1, 1),
				screenshotName,
				"ShapeHelpers");
		}
		catch (Exception e)
		{
			Debug.LogError($"Error encoding or recording frame: {e.Message}");
		}
	}
}