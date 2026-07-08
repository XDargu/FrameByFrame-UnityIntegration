# Examples

`[IMAGE PLACEHOLDER: Frame by Frame timeline showing a custom Combat event selected.]`

## Record a Custom Property Group

```csharp
using FbF;
using UnityEngine;

[FrameByFrameRecordingOption("Combat", "Records combat state.")]
public class RecordCombatState : MonoBehaviour
{
	public int health;
	public GameObject target;

	private void Update()
	{
		if (!FbFManager.IsRecordingOptionEnabled("Combat"))
		{
			return;
		}

		PropertyGroup combat = FbFManager.RecordProperties(gameObject, "Combat");
		combat.AddProperty("Health", health);

		if (target != null)
		{
			combat.AddEntityRef("Target", target);
		}
	}
}
```

## Record an Event

```csharp
using FbF;
using UnityEngine;

[FrameByFrameRecordingOption("Doors", "Records door interactions.")]
public class DoorDebugRecorder : MonoBehaviour
{
	public void RecordOpened(GameObject instigator)
	{
		if (!FbFManager.IsRecordingOptionEnabled("Doors"))
		{
			return;
		}

		PropertyGroup evt = FbFManager.RecordEvent(gameObject, "Door Opened", "Doors");
		evt.AddEntityRef("Instigator", instigator);
		evt.AddProperty("Position", transform.position);
	}
}
```

## Record Nearby Objects Only

Use recording options for data channels and your own filters for object eligibility.

```csharp
using FbF;
using UnityEngine;

[FrameByFrameRecordingOption("NearbyEnemies", "Records enemies near the player.")]
public class NearbyEnemyRecorder : MonoBehaviour
{
	public Transform player;
	public float radius = 30.0f;

	private void Update()
	{
		if (!FbFManager.IsRecordingOptionEnabled("NearbyEnemies") || player == null)
		{
			return;
		}

		float distance = Vector3.Distance(transform.position, player.position);
		if (distance > radius)
		{
			return;
		}

		PropertyGroup group = FbFManager.RecordProperties(gameObject, "Nearby Enemy");
		group.AddProperty("Distance To Player", distance);
	}
}
```

## Register an Option Manually

Use this for dynamic systems where an attribute is not practical.

```csharp
FbFRecordingOptions.Register("Streaming", "Records streaming system state.");
FbFRecordingOptions.SetEnabled("Streaming", true);
```

Prefer attributes for recorder components because they are discoverable before Play Mode.
