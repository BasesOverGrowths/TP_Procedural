using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Node
{
    public string name;
    public Vector2Int location = Vector2Int.zero;
    //public List<Node> adjacents = new List<Node>();
    public LinkedList<Edge> adjacents = new LinkedList<Edge>();

    public Node(string name)
    {
        this.name = name;
    }
}
