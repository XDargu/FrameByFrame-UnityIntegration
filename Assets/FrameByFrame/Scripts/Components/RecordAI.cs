using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using FbF;

public class RecordAI : MonoBehaviour
{
    public bool RecordPath = true;

    NavMeshAgent agent;

    // Start is called before the first frame update
    void Start()
    {
        FbFManager.RegisterRecordingOption("Navigation");
        agent = GetComponent<NavMeshAgent>();
    }

    // Update is called once per frame
    void Update()
    {
        Record();
    }

    [System.Diagnostics.Conditional("DEBUG")]
    void Record()
    {
        if (RecordPath)
        {
            if (FbFManager.IsRecordingOptionEnabled("Navigation"))
            {
                EntityData entity = FbFManager.RecordEntity(this.gameObject);
                PropertyGroup group = entity.AddGroup("NavMeshAgent");
                NavMeshPath path = agent.path;

                group.AddProperty("Enabled", agent.enabled, agent.enabled ? new Icon("check-circle", "green") : new Icon("times-circle", "red"));
                group.AddProperty("Path Status", path.status.ToString());

                if (path.corners.Length > 0)
                {
                    group.AddPath("Path", path.corners, Color.red, "Pathfinding");

                    for (int i = 0; i < path.corners.Length; ++i)
                    {
                        group.AddSphere("Corner " + i, path.corners[i], 0.1f, Color.red, "Pathfinding", null, PropertyFlags.Hidden);

                        if (i > 0)
                        {
                            group.AddLine("Segment " + i, path.corners[i - 1], path.corners[i], Color.red, "Pathfinding", null, PropertyFlags.Hidden);
                        }
                    }
                }
            }
        }
    }
}
