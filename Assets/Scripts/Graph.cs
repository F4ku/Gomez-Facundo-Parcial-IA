using System.Collections.Generic;
using UnityEngine;

public class Graph : MonoBehaviour
{
    public static Graph Instance { get; private set; }

    [Header("Graph Settings")]
    public float MaxConnectionDistance = 30f;
    public LayerMask ObstacleLayer;

    [Header("Node Positions")]
    public Transform[] NormalNodes;
    public Transform[] SafeZoneNodes;

    public List<Node> AllNodes { get; private set; }

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        GenerateGraph();
    }

    private void GenerateGraph()
    {
        AllNodes = new List<Node>();

        foreach (Transform t in NormalNodes)
        {
            if (t != null) AllNodes.Add(new Node(t.position, false));
        }

        foreach (Transform t in SafeZoneNodes)
        {
            if (t != null) AllNodes.Add(new Node(t.position, true));
        }

        for (int i = 0; i < AllNodes.Count; i++)
        {
            for (int j = i + 1; j < AllNodes.Count; j++)
            {
                float distance = Vector3.Distance(AllNodes[i].Position, AllNodes[j].Position);

                if (distance <= MaxConnectionDistance)
                {
                    Vector3 origin = AllNodes[i].Position + Vector3.up * 0.5f;
                    Vector3 direction = (AllNodes[j].Position - AllNodes[i].Position).normalized;

                    if (!Physics.Raycast(origin, direction, distance, ObstacleLayer))
                    {
                        AllNodes[i].Neighbors.Add(AllNodes[j]);
                        AllNodes[j].Neighbors.Add(AllNodes[i]);
                    }
                }
            }
        }
    }

    public Node GetClosestNode(Vector3 position, bool ignoreSafeZones = false)
    {
        Node closest = null;
        float minDistance = Mathf.Infinity;


        Vector3 rayOrigin = position + Vector3.up * 0.5f;

        foreach (Node node in AllNodes)
        {
            if (ignoreSafeZones && node.IsSafeZone) continue;

            float dist = Vector3.Distance(position, node.Position);
            if (dist < minDistance)
            {
                Vector3 dir = (node.Position - position).normalized;

                if (!Physics.Raycast(rayOrigin, dir, dist, ObstacleLayer))
                {
                    minDistance = dist;
                    closest = node;
                }
            }
        }


        if (closest == null)
        {
            minDistance = Mathf.Infinity;
            foreach (Node node in AllNodes)
            {
                if (ignoreSafeZones && node.IsSafeZone) continue;
                float dist = Vector3.Distance(position, node.Position);
                if (dist < minDistance)
                {
                    minDistance = dist;
                    closest = node;
                }
            }
        }
        return closest;
    }


    private void OnDrawGizmos()
    {
        if (AllNodes == null || AllNodes.Count == 0) return;

        foreach (Node node in AllNodes)
        {
            Gizmos.color = node.IsSafeZone ? Color.green : Color.cyan;
            Gizmos.DrawSphere(node.Position, 0.5f);

            Gizmos.color = Color.white;
            foreach (Node neighbor in node.Neighbors)
            {
                Gizmos.DrawLine(node.Position, neighbor.Position);
            }
        }
    }
}