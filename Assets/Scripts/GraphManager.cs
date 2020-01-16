using System.Collections;
using System.Collections.Generic;
using System.Linq;
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
    public static GraphManager Instance;

    public int primaryPathSize = 5;

    private int subPathsCount;

    public List<Node> adj { get; set; } = new List<Node>();

    Vector2Int[] directions = new Vector2Int[] { Vector2Int.up, Vector2Int.right, Vector2Int.left, Vector2Int.down };
    List<Vector2Int> allLocations = new List<Vector2Int>() { Vector2Int.zero };

    List<int> currentSecPathPoss = new List<int>();
    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
            Destroy(this);

        for (int i = 1; i < primaryPathSize - 1; ++i)
        {
            currentSecPathPoss.Add(i);
        }

        InitGraph(); // graph main path

        subPathsCount = Mathf.FloorToInt(primaryPathSize / 3);

        // creates sub paths automatically depending on the main path length
        while (currentSecPathPoss.Count > 2)
        {
            CreateSubPath();
        }
        // manual input
        /*for (int i = 0; i < subPathsCount; i++)
        {
            CreateSubPath();
        }*/

        PrintGraph();
    }

    void CreatePathAt(int length, int adjIndex = 0, bool isSecondary = false, uint offset = 0)
    {
        int formerRoomCount = adj.Count();

        if (isSecondary)
        {
            var nextBranchTry = adj[adjIndex - (int)offset];
            if (CheckIfNodeIsSurrounded(nextBranchTry)) // temporary fix to try to find another possible branch (once only)
                --offset;

            if (offset != 0) {
                if (CheckIfNodeIsSurrounded(nextBranchTry) || nextBranchTry.roomType == Node.ROOM_TYPE.START) // can't do a sub branch if no free path, nor if the node is the start (rule)
                {
                    Debug.Log("Couldn't do an offset of -" + offset + " on " + nextBranchTry.name + "(" + nextBranchTry.location.x + "," + nextBranchTry.location.y + ")");
                    offset = 0;
                }
            }

            CreateNodeToNode(adj[adjIndex - (int)offset], "Sec" + (adjIndex - offset) + "_Generic0"); // creates the first room next to the chosen branch (at the end of the adj list)
            if (adj.Count() > formerRoomCount) // create locked doors only if node really created
            {
                adj[adjIndex].adjacents.ElementAt(1).doorState = Door.STATE.CLOSED; // closes the door to the next main path room
                adj[adjIndex + 1].adjacents.First.Value.doorState = Door.STATE.CLOSED; // closes the door at the other end (next room)
            }
            else
                return; // tempo => couldn't create a path there (nor just before) anyway
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

            // create the node connected to the previous node in adj
            CreateNodeToNode(adj[adj.Count - 1], nodeName, roomType);


            // forces the room to be a key room and return if the length couldn't be reached (dead end that forces to create a key room)
            var createdNode = adj[adj.Count - 1];

            // checks if there are rooms all around the created room, set it as the end of the sub path (as the key room)
            if (CheckIfNodeIsSurrounded(createdNode))
            {
                createdNode.roomType = Node.ROOM_TYPE.KEYROOM;
                return;
            }
        }
    }
    void CreateSubPath(uint _offset = 0)
    {
        var randomBranchLength = Random.Range(2, primaryPathSize - 2);
        int nextRandomBranch = randomAvailableBranch();
        CreatePathAt(randomBranchLength, nextRandomBranch, true, _offset);
    }
    bool CheckIfNodeIsSurrounded(Node node)
    {
        var roomsAroundNodePossLoc = new List<Vector2Int>();
        foreach (Vector2Int dir in directions)
        {
            roomsAroundNodePossLoc.Add(node.location + dir);
        }
        // checks if there are rooms all around the room
        return !roomsAroundNodePossLoc.Except(allLocations).Any();
    }

    int currentMinimumBranchIndex = 0;
    // create a random branch between the current minimum branch (default = 0) and the median value in the current branch starting node possibilities
    int randomAvailableBranch()
    {
        var randSubPathStart = currentSecPathPoss[Random.Range(currentMinimumBranchIndex, (currentSecPathPoss.Count - currentMinimumBranchIndex) / 2)];
        //currentMinimumBranchIndex = randSubPathStart;
        currentSecPathPoss.RemoveRange(0, currentSecPathPoss.FindIndex(i => i == randSubPathStart));
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
        // check exception : no path found (rooms all around) [Shouldn't ever be called]
        if (!InitNodeFrom(fromNode, node))
        {
            Debug.LogWarning("cannot create a room next to " + fromNode.name + " because it has rooms all around already");
            return;
        }
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
