using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using FbF;

public class AIRecordingScript : MonoBehaviour
{
    public bool RecordPath = true;

    NavMeshAgent agent;

    // Start is called before the first frame update
    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
    }

    // Update is called once per frame
    void Update()
    {
        if (RecordPath)
        {
            if (FbFManager.IsRecordingOptionEnabled("Navigation"))
            {
                EntityData entity = FbFManager.RecordEntity(this.gameObject);
                PropertyGroup group = entity.AddPropertyGroup("Navigation");
                NavMeshPath path = agent.path;

                for (int i = 0; i < path.corners.Length; ++i)
                {
                    group.AddSphere("Corner " + i, path.corners[i], 0.1f, Color.red, "Pathfinding");

                    if (i > 0)
                    {
                        group.AddLine("Segment " + i, path.corners[i - 1], path.corners[i], Color.red, "Pathfinding");
                    }
                }
            }
        }
    }
}
