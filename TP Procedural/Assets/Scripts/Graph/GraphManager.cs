using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Edge
{
    public Node node0;
    public Node node1;
    public bool isOpen = true;

    public Edge(Node n1, Node n2, bool open = true)
    {
        node0 = n1;
        node1 = n2;
        isOpen = open;
    }
}

public class GraphManager : MonoBehaviour
{
    public int primaryPathSize = 5;

    List<Node> adj = new List<Node>();

    Vector2Int[] directions = new Vector2Int[] { Vector2Int.up, Vector2Int.right, Vector2Int.left, Vector2Int.down };
    List<Vector2Int> allLocations = new List<Vector2Int>();


    void Awake(){
        InitGraph();
    }

    void InitGraph()
    {
        adj.Add(new Node("Start"));
        CreateNodeToNode(adj[adj.Count - 1], "Generic0");
        CreateNodeToNode(adj[adj.Count - 1], "Generic1", false);
        CreateNodeToNode(adj[adj.Count - 1], "Generic2");
        CreateNodeToNode(adj[adj.Count - 1], "End");

        PrintGraph();
    }

    void PrintGraph()
    {
        foreach (Node node in adj)
        {
            var toLog = node.name + "(" + node.location.x + "," + node.location.y + ")" + " : connected to ";
            foreach(Edge edge in node.adjacents)
            {
                toLog += edge.node1.name;
                if (!edge.isOpen) toLog += "(locked)";
                toLog += " ";
            }
            Debug.Log(toLog);
        }
    }

    void CreateNodeToNode(Node fromNode, string nodeName, bool open = true)
    {
        var node = new Node(nodeName);
        InitNodeFrom(fromNode, node);
        ConnectNodes(fromNode, node, open);
        adj.Add(node);
    }

    void InitNodeFrom(Node fromNode, Node node)
    {
        var possNextMoves = PossibleNextLocations(fromNode);
        node.location = possNextMoves[Random.Range(0, possNextMoves.Count)]; ;
        allLocations.Add(node.location);
    }

    List<Vector2Int> PossibleNextLocations(Node fromNode)
    {
        List<Vector2Int> poss = new List<Vector2Int>();
        foreach(Vector2Int vec in directions)
        {
            if (!allLocations.Contains(fromNode.location + vec))
                poss.Add(fromNode.location + vec);
        }
        return poss;
    }

    public void ConnectNodes(Node A, Node B, bool open = true)
    {
        A.adjacents.AddLast(new Edge(A, B, open));
        B.adjacents.AddLast(new Edge(B, A, open));
    }
}
