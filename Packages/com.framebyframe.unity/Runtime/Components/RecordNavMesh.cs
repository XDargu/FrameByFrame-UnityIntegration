using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using FbF;

public class RecordNavMesh : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        FbFManager.RegisterRecordingOption("NavMesh");
    }

    // Update is called once per frame
    void Update()
    {
        RecordData();
    }

    [System.Diagnostics.Conditional("DEBUG")]
    void RecordData()
    {
        if (FbFManager.IsRecordingOptionEnabled("NavMesh"))
        {
            NavMeshTriangulation meshData = NavMesh.CalculateTriangulation();

            PropertyGroup group = FbFManager.RecordProperties(this.gameObject, "Navigation");
            group.AddMesh("NavMesh", FlattenPositions(meshData.vertices), ReverseIndices(meshData.indices), true, new Color(Color.cyan.r, Color.cyan.g, Color.cyan.b, 0.5f), "NavMesh");
        }
    }

    static int[] ReverseIndices(int[] indices)
    {
        int[] reversed = new int[indices.Length];
        for (int i = 0; i < indices.Length; i++)
        {
            reversed[i] = indices[indices.Length - i - 1];
        }

        return reversed;
    }

    static float[] FlattenPositions(Vector3[] input)
    {
        int size = input.Length * 3;
        float[] result = new float[size];

        for (int i = 0; i < input.Length; i++)
        {
            int resultIdx = i * 3;
            result[resultIdx] = input[i].x;
            result[resultIdx + 1] = input[i].y;
            result[resultIdx + 2] = input[i].z;
        }
        return result;
    }
}
