using UnityEngine;
using UnityEngine.AI;
using FbF;

public class MoveToClickPoint : MonoBehaviour
{
    NavMeshAgent agent;
    void Start()
    {
        FbFManager.RegisterRecordingOption("Navigation");

        agent = GetComponent<NavMeshAgent>();
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            RaycastHit hit;
            if (Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out hit, 100))
            {
                RecordDestChanged(hit.point);
                agent.destination = hit.point;
            }
        }
    }

    [System.Diagnostics.Conditional("DEBUG")]
    private void RecordDestChanged(Vector3 position)
    {
        if (FbFManager.IsRecordingOptionEnabled("Navigation"))
        {
            PropertyGroup eventPath = FbFManager.RecordEvent(this.gameObject, "Destination changed", "Navigation");
            eventPath.AddSphere("Destination", position, 0.2f, Color.yellow, "Pathfinding");
            eventPath.AddComment("Destination was set to something");
            eventPath.AddLine("Marker", position, position + Vector3.up * 5, Color.yellow, "Pathfinding");
        }
    }
}