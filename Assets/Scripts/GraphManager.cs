using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Node
{
    public string name;
    public Vector2Int location = Vector2Int.zero;
    public LinkedList<Edge> adjacents = new LinkedList<Edge>();
    public enum ROOM_TYPE
    {
        DEFAULT = 0,
        START,
        END,
        KEYROOM
    }
    public ROOM_TYPE roomType = 0;

    public Node(string name, ROOM_TYPE roomType)
    {
        this.name = name;
        this.roomType = roomType;
    }
}
public class Edge
{
    public Node node0;
    public Node node1;
    public Door.STATE doorState = Door.STATE.OPEN;
    public Vector2Int orientation;

    public Edge(Node n1, Node n2, Door.STATE state = Door.STATE.OPEN)
    {
        node0 = n1;
        node1 = n2;
        doorState = state;
        orientation = node1.location - node0.location;
    }
}

public class GraphManager : MonoBehaviour
{
    [SerializeField]
    public int primaryPathSize = 5;

    public List<Node> adj { get; set; } = new List<Node>();
    public static GraphManager Instance;

    Vector2Int[] directions = new Vector2Int[] { Vector2Int.up, Vector2Int.right, Vector2Int.left, Vector2Int.down };
    List<Vector2Int> allLocations = new List<Vector2Int>() { Vector2Int.zero };

    List<int> currentSecPathPoss = new List<int>();
    void Awake()
    {
        if(Instance == null)

        {
            Instance = this;
        }

        InitGraph(); // graph main path

        for (int i = 1; i < primaryPathSize; i++)
        {
            currentSecPathPoss.Add(i);
        }

        CreateSubPath();
        CreateSubPath();

        PrintGraph();
    }

    void CreatePathAt(int length, int adjIndex = 0, bool isSecondary = false, uint offset = 0)
    {
        if (isSecondary)
        {
            adj[adjIndex].adjacents.Last.Value.doorState = Door.STATE.CLOSED; // closes the door to the next main path room
            adj[adjIndex + 1].adjacents.First.Value.doorState = Door.STATE.CLOSED; // closes the door at the other end (next room)

            if (offset != 0) {
                var nextBranchTry = adj[adjIndex - (int)offset];
                if (nextBranchTry.adjacents.Count == 4 || nextBranchTry.name == "Start") // can't do a sub branch if no free path, nor if the node is the start (rule)
                {
                    Debug.Log("Couldn't do an offset of -" + offset + " on " + nextBranchTry.name + "(" + nextBranchTry.location.x + "," + nextBranchTry.location.y + ")");
                    offset = 0;
                }
            }
            CreateNodeToNode(adj[adjIndex - (int)offset], "Sec" + (adjIndex - offset) + "_Generic0"); // creates the first room next to the chosen branch (at the end of the adj list)
        }

        // creates a path from the room (default is the latest room created)
        for (int i = 0; i < length; i++)
        {
            var nodeName = "";
            var roomType = Node.ROOM_TYPE.DEFAULT;
            if (isSecondary && i == length - 1)
            {
                nodeName = "KeyRoom";
                roomType = Node.ROOM_TYPE.KEYROOM;
            }
            else nodeName = "Generic" + (i + 1);
            if (isSecondary) nodeName = "Sec" + adjIndex + "_" + nodeName;
            CreateNodeToNode(adj[adj.Count - 1], nodeName, roomType);
        }
    }
    void CreateSubPath(uint _offset = 0)
    {
        var randomBranchLength = Random.Range(2, primaryPathSize - 2);
        CreatePathAt(randomBranchLength, randomAvailableBranch(), true, _offset);
    }

    int currentMinimumBranchIndex = 0;
    // create a random branch between the current minimum branch (default = 0) and the median value in the current branch starting node possibilities
    int randomAvailableBranch()
    {
        var randSubPathStart = currentSecPathPoss[Random.Range(currentMinimumBranchIndex, currentSecPathPoss.Count / 2)];
        currentSecPathPoss.Remove(randSubPathStart);
        return randSubPathStart;
    }

    void InitGraph()
    {
        adj.Add(new Node("Start", Node.ROOM_TYPE.START));

        CreatePathAt(primaryPathSize);

        CreateNodeToNode(adj[adj.Count - 1], "End", Node.ROOM_TYPE.END);
    }

    void PrintGraph()
    {
        foreach (Node node in adj)
        {
            var toLog = node.name + "(" + node.location.x + "," + node.location.y + ")" + " : connected to ";
            foreach (Edge edge in node.adjacents)
            {
                toLog += edge.node1.name;
                switch (edge.doorState)
                {
                    case Door.STATE.CLOSED:
                        toLog += "(locked)";
                        break;
                    case Door.STATE.SECRET:
                        toLog += "(secret)";
                        break;
                }
                toLog += " ";
            }
            Debug.Log(toLog);
        }
    }

    void CreateNodeToNode(Node fromNode, string nodeName, Node.ROOM_TYPE roomType = Node.ROOM_TYPE.DEFAULT, Door.STATE state = Door.STATE.OPEN)
    {
        var node = new Node(nodeName, roomType);
        // check exception : no path found (rooms all around)
        if (!InitNodeFrom(fromNode, node))
            return;
        ConnectNodes(fromNode, node, state);
        adj.Add(node);
    }

    bool InitNodeFrom(Node fromNode, Node node)
    {
        var possNextMoves = PossibleNextLocations(fromNode);
        if (possNextMoves.Count == 0)
            return false;
        node.location = possNextMoves[Random.Range(0, possNextMoves.Count)];
        allLocations.Add(node.location);
        return true;
    }

    List<Vector2Int> PossibleNextLocations(Node fromNode)
    {
        List<Vector2Int> poss = new List<Vector2Int>();
        foreach (Vector2Int vec in directions)
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
