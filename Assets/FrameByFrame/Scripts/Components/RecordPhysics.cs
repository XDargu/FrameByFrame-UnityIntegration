﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FbF;

public class RecordPhysics : MonoBehaviour
{

	public bool RecordCollider;
	public bool RecordVelocity;

	public bool UseFixedUpdate = false;

	public bool RecordAllChildren = false;
	public bool MergeChildrenRecordings = false;

	bool isOriginalObject = true;
	GameObject targetObject;

	public Color color = Color.red;

	SphereCollider sphereCollider;
	BoxCollider boxCollider;
	CapsuleCollider capsuleCollider;
	Rigidbody rigidBody;

	void Start()
	{
		Init();
	}

	[System.Diagnostics.Conditional("DEBUG")]
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

		if (isOriginalObject)
        {
			targetObject = gameObject;
        }

		if (RecordAllChildren)
		{
			int childCount = gameObject.transform.childCount;

			for (int i = 0; i < childCount; ++i)
			{
				GameObject childObject = gameObject.transform.GetChild(i).gameObject;
				if (childObject)
				{
					RecordPhysics recordingScript = childObject.AddComponent<RecordPhysics>();
					recordingScript.RecordCollider = RecordCollider;
					recordingScript.RecordVelocity = RecordVelocity;
					recordingScript.color = color;
					recordingScript.RecordAllChildren = RecordAllChildren;
					recordingScript.MergeChildrenRecordings = MergeChildrenRecordings;
					recordingScript.targetObject = MergeChildrenRecordings ? targetObject : recordingScript.gameObject;
					recordingScript.isOriginalObject = false;
				}
			}
		}
	}

    private void Update()
    {
        if (!UseFixedUpdate)
        {
			Record();
        }
    }

	private void FixedUpdate()
	{
		if (UseFixedUpdate)
		{
			Record();
		}
	}

	[System.Diagnostics.Conditional("DEBUG")]
	void Record()
	{
		if (FbFManager.IsRecordingOptionEnabled("Colliders"))
		{
			EntityData entity = FbFManager.RecordEntity(targetObject);

			if (RecordCollider && sphereCollider)
			{
				PropertyGroup group = entity.AddGroup("Colliders");
				float maxScale = Mathf.Max(Mathf.Max(gameObject.transform.lossyScale.x, gameObject.transform.lossyScale.y), gameObject.transform.lossyScale.z);
				group.AddSphere("SphereCollider", gameObject.transform.TransformPoint(sphereCollider.center), sphereCollider.radius * maxScale, color, "Colliders", null, PropertyFlags.Collapsed);
			}

			if (RecordCollider && boxCollider)
			{
				Vector3 size = gameObject.transform.lossyScale;
				size.x *= boxCollider.size.x;
				size.y *= boxCollider.size.y;
				size.z *= boxCollider.size.z;
				PropertyGroup group = entity.AddGroup("Colliders");

				group.AddOOBB("BoxCollider", boxCollider.bounds.center, size, gameObject.transform.up, gameObject.transform.forward, color, "Colliders", null, PropertyFlags.Collapsed);
			}

			if (RecordCollider && capsuleCollider)
			{
				Vector3 size = gameObject.transform.lossyScale;
				// TODO: Direction and correct size
				PropertyGroup group = entity.AddGroup("Colliders");
				Vector3 capsuleRef = gameObject.transform.up;
				if (capsuleCollider.direction == 0)
					capsuleRef = gameObject.transform.right;
				if (capsuleCollider.direction == 1)
					capsuleRef = gameObject.transform.up;
				if (capsuleCollider.direction == 2)
					capsuleRef = gameObject.transform.forward;
				
				group.AddCapsule("CapsuleCollider", capsuleCollider.bounds.center, capsuleRef, capsuleCollider.radius, capsuleCollider.height, color, "Colliders", null, PropertyFlags.Collapsed);
			}
		}

		if (FbFManager.IsRecordingOptionEnabled("Physics"))
		{
			EntityData entity = FbFManager.RecordEntity(targetObject);

			if (RecordVelocity && rigidBody)
			{
				PropertyGroup group = entity.AddGroup("RigidBody");
				group.AddLine("Velocity", gameObject.transform.position, gameObject.transform.position + rigidBody.velocity, Color.blue, "Physics", null, PropertyFlags.Collapsed);
			}
		}

		if (RecordAllChildren)
        {
			FbFManager.RecordEntity(targetObject);
		}
	}

	void OnCollisionEnter(Collision collision)
	{
		if (FbFManager.IsRecordingOptionEnabled("Collisions"))
		{
			if (RecordCollider)
			{
				PropertyGroup eventData = FbFManager.RecordEvent(targetObject, "OnCollisionEnter", "Collision");
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
