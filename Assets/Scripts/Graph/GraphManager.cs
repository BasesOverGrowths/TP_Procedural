using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Node
{
    public string name;
    public Vector2Int location = Vector2Int.zero;
    public LinkedList<Edge> adjacents = new LinkedList<Edge>();

    public Node(string name)
    {
        this.name = name;
    }
}
public class Edge
{
    public Node node0;
    public Node node1;
    public Door.STATE doorState = Door.STATE.OPEN;
    public Door door = null;

    public Edge(Node n1, Node n2,  Door.STATE state = Door.STATE.OPEN)
    {
        node0 = n1;
        node1 = n2;
        doorState = state;
    }
}

public class GraphManager : MonoBehaviour
{
    [SerializeField]
    public int primaryPathSize = 5;

    List<Node> adj = new List<Node>();

    Vector2Int[] directions = new Vector2Int[] { Vector2Int.up, Vector2Int.right, Vector2Int.left, Vector2Int.down };
    List<Vector2Int> allLocations = new List<Vector2Int>() { Vector2Int.zero };

    List<int> currentSecPathPoss = new List<int>();
    void Awake(){
        InitGraph(); // graph main path

        for (int i = 1; i < primaryPathSize; i++)
        {
            currentSecPathPoss.Add(i);
        }

        CreateSubPath();
        CreateSubPath(1);
        /*randomBranchRoom = Random.Range(2, primaryPathSize - 2);
        CreatePathAt(Random.Range(2, primaryPathSize - 2), randomAvailableBranch(), true);
        randomBranchRoom = Random.Range(2, primaryPathSize - 2);
        CreatePathAt(Random.Range(2, primaryPathSize - 2), randomAvailableBranch(), true);*/

        PrintGraph();
    }

    void CreatePathAt(int length, int adjIndex = 0, bool isSecondary = false, uint offset = 0)
    {
        if (isSecondary)
        {
            adj[adjIndex].adjacents.Last.Value.doorState = Door.STATE.CLOSED; // closes the door to the next main path room
            adj[adjIndex + 1].adjacents.First.Value.doorState = Door.STATE.CLOSED; // closes the door at the other end (next room)
            CreateNodeToNode(adj[adjIndex - (int)offset], "Sec" + (adjIndex - offset) + "_Generic0"); // creates the first room next to the chosen branch (at the end of the adj list)
        }

        // creates a path from the room (default is the latest room created)
        for (int i = 0; i < length; i++)
        {
            var nodeName = "Generic" + (i + 1);
            if (isSecondary) nodeName = "Sec" + adjIndex + "_" + nodeName;
            CreateNodeToNode(adj[adj.Count - 1], nodeName);
        }
    }
    void CreateSubPath(uint _offset = 0)
    {
        var randomBranchRoom = Random.Range(2, primaryPathSize - 2);
        CreatePathAt(randomBranchRoom, randomAvailableBranch(), true, _offset);
    }

    int randomAvailableBranch()
    {
        var randSubPathStart = currentSecPathPoss[Random.Range(0, currentSecPathPoss.Count)];
        currentSecPathPoss.Remove(randSubPathStart);
        return randSubPathStart;
    }

    void InitGraph()
    {
        adj.Add(new Node("Start"));

        CreatePathAt(primaryPathSize);

        CreateNodeToNode(adj[adj.Count - 1], "End");
    }

    void PrintGraph()
    {
        foreach (Node node in adj)
        {
            var toLog = node.name + "(" + node.location.x + "," + node.location.y + ")" + " : connected to ";
            foreach(Edge edge in node.adjacents)
            {
                toLog += edge.node1.name;
                switch (edge.doorState){
                    case Door.STATE.CLOSED: toLog += "(locked)";
                        break;
                    case Door.STATE.SECRET: toLog += "(secret)";
                        break;
                }
                toLog += " ";
            }
            Debug.Log(toLog);
        }
    }

    void CreateNodeToNode(Node fromNode, string nodeName, Door.STATE state = Door.STATE.OPEN)
    {
        var node = new Node(nodeName);
        InitNodeFrom(fromNode, node);
        ConnectNodes(fromNode, node, state);
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

    public void ConnectNodes(Node A, Node B, Door.STATE state = Door.STATE.OPEN)
    {
        A.adjacents.AddLast(new Edge(A, B, state));
        B.adjacents.AddLast(new Edge(B, A, state));
    }
}
