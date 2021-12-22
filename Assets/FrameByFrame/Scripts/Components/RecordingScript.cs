using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FbF;

public class RecordingScript : MonoBehaviour
{

	public bool RecordCollider;
	public bool RecordVelocity;

	public bool RecordAllChildren;

	[Range(0.0f, 1.0f)]
	public float opacity = 1.0f;

	SphereCollider sphereCollider;
	BoxCollider boxCollider;
	CapsuleCollider capsuleCollider;
	Rigidbody rigidBody;

	void Start()
	{
		Init();
	}

	void Init()
    {
		Application.targetFrameRate = 60;
		FbFManager.RegisterRecordingOption("Colliders");
		FbFManager.RegisterRecordingOption("Collisions");
		FbFManager.RegisterRecordingOption("Physics");

		sphereCollider = gameObject.GetComponent<SphereCollider>();
		boxCollider = gameObject.GetComponent<BoxCollider>();
		capsuleCollider = gameObject.GetComponent<CapsuleCollider>();
		rigidBody = gameObject.GetComponent<Rigidbody>();

		if (RecordAllChildren)
		{
			int childCount = gameObject.transform.childCount;

			for (int i = 0; i < childCount; ++i)
			{
				GameObject childObject = gameObject.transform.GetChild(i).gameObject;
				if (childObject)
				{
					RecordingScript recordingScript = childObject.AddComponent<RecordingScript>();
					recordingScript.RecordCollider = RecordCollider;
					recordingScript.RecordVelocity = RecordVelocity;
					recordingScript.opacity = opacity;
					recordingScript.RecordAllChildren = RecordAllChildren;
				}
			}
		}

		Debug.Log("Init Recording Script: " + gameObject.name);
	}

	void Update()
	{
		if (FbFManager.IsRecordingOptionEnabled("Colliders"))
		{
			EntityData entity = FbFManager.RecordEntity(this.gameObject);

			if (RecordCollider && sphereCollider)
			{
				PropertyGroup group = entity.AddPropertyGroup("Colliders");
				float maxScale = Mathf.Max(Mathf.Max(gameObject.transform.lossyScale.x, gameObject.transform.lossyScale.y), gameObject.transform.lossyScale.z);
				group.AddSphere("SphereCollider", gameObject.transform.TransformPoint(sphereCollider.center), sphereCollider.radius * maxScale, new Color(1, 0.5f, 0.5f, 0.5f), "Colliders");
			}

			if (RecordCollider && boxCollider)
			{
				Vector3 size = gameObject.transform.lossyScale;
				size.x *= boxCollider.size.x;
				size.y *= boxCollider.size.y;
				size.z *= boxCollider.size.z;
				PropertyGroup group = entity.AddPropertyGroup("Colliders");
				group.AddOOBB("BoxCollider", gameObject.transform.TransformPoint(boxCollider.center), size, gameObject.transform.up, gameObject.transform.forward, new Color(1, 0.5f, 0.5f, opacity), "Colliders");
			}

			if (RecordCollider && capsuleCollider)
			{
				Vector3 size = gameObject.transform.lossyScale;
				// TODO: Direction and correct size
				PropertyGroup group = entity.AddPropertyGroup("Colliders");
				group.AddCapsule("CapsuleCollider", gameObject.transform.TransformPoint(capsuleCollider.center), gameObject.transform.up, capsuleCollider.radius, capsuleCollider.height, new Color(1, 0.5f, 0.5f, opacity), "Colliders");
			}
		}

		if (FbFManager.IsRecordingOptionEnabled("Physics"))
		{
			EntityData entity = FbFManager.RecordEntity(this.gameObject);

			if (RecordVelocity && rigidBody)
			{
				PropertyGroup group = entity.AddPropertyGroup("RigidBody");
				group.AddLine("Velocity", gameObject.transform.position, gameObject.transform.position + rigidBody.velocity, Color.blue, "Physics");
			}
		}
	}

	void OnCollisionEnter(Collision collision)
	{
		if (FbFManager.IsRecordingOptionEnabled("Collisions"))
		{
			if (RecordCollider)
			{
				EntityData entity = FbFManager.RecordEntity(this.gameObject);
				EventData eventData = entity.AddEvent("OnCollisionEnter", "Collision");
				eventData.AddProperty("Collider name", collision.collider.name);

				foreach (ContactPoint contact in collision.contacts)
				{
					eventData.AddSphere("Contact", contact.point, 0.1f, Color.blue, "Collisions");
					eventData.AddEntityRef("Contact Entity", contact.otherCollider.gameObject);
				}
			}
		}
	}
}
