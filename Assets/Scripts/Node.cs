using System.Collections.Generic;
using UnityEngine;

public class Node
{
    public Vector3 Position;
    public List<Node> Neighbors;
    public bool IsSafeZone;

    public int GCost;
    public int HCost;
    public Node Parent;

    public int FCost { get { return GCost + HCost; } }

    public Node(Vector3 position, bool isSafeZone)
    {
        Position = position;
        IsSafeZone = isSafeZone;
        Neighbors = new List<Node>();
    }
}