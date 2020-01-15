using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;

public class Generator : MonoBehaviour
{
    const int RealSizeRoomX = 11;
    const int RealSizeRoomY = 9;

    private int _instanciateRoom = 0;

    public List<GameObject> allRoomsPrefabs;
    public List<GameObject> PrefabsOneDoor;
    public List<GameObject> PrefabsTwoDoors;
    public List<GameObject> PrefabsThreeDoors;
    public List<GameObject> PrefabsFourDoors;

    private void Awake()
    {
        allRoomsPrefabs.AddRange(PrefabsOneDoor);
        allRoomsPrefabs.AddRange(PrefabsTwoDoors);
        allRoomsPrefabs.AddRange(PrefabsThreeDoors);
        allRoomsPrefabs.AddRange(PrefabsFourDoors);
    }
    // Start is called before the first frame update
    void Start()
    {
        SpawnRoom();
    }

    // Update is called once per frame
    void Update()
    {

    }

    private void SpawnRoom()
    {
        foreach (Node _node in GraphManager.Instance.adj)
        {
            int nbDoor = _node.adjacents.Count;
            
            GameObject roomGO = Instantiate(SelectPrefab(_node), new Vector3(_node.location.x * RealSizeRoomX, _node.location.y * RealSizeRoomY, 0), Quaternion.identity);

            Room _room = roomGO.GetComponent<Room>();
            SetRoomDoors(_room, _node);
            _room.position = _node.location;

            if (_node.name == "Start")
            {
                _room.isStartRoom = true;
                _room.name = "Start";
            }

            if (_node.name == "End")
                _room.name = "End";

        }
    }
    GameObject SelectPrefab(Node _node)
    {
        List<Vector2Int> allDoorsDirections = new List<Vector2Int>();

        // store all the doors directions
        foreach (Edge edge in _node.adjacents)
        {
            allDoorsDirections.Add(edge.orientation);
        }

        var matchingRooms = new List<GameObject>();
        switch (_node.roomType)
        {
            case Node.ROOM_TYPE.DEFAULT: case Node.ROOM_TYPE.START: case Node.ROOM_TYPE.END:
                matchingRooms = allRoomsPrefabs.FindAll(i => MatchDoors(i, _node));
                break;
            case Node.ROOM_TYPE.KEYROOM:
                matchingRooms = allRoomsPrefabs.FindAll(i => i.tag == "KeyRoom");
                break;
        }

        return matchingRooms[Random.Range(0, matchingRooms.Count)];
    }
    bool MatchDoors(GameObject roomPrefab, Node node)
    {
        if (node.roomType == Node.ROOM_TYPE.KEYROOM) // current patch for not choosing a Keyroom
            return false;

        var roomDoors = roomPrefab.GetComponent<Room>().connectedDoors;

        Utils.ORIENTATION orientation = Utils.ORIENTATION.NONE;
        for (int i = 0; i < 4; i++)
        {
            switch (i)
            {
                case 0:
                    orientation = Utils.ORIENTATION.NORTH;
                    break;
                case 1:
                    orientation = Utils.ORIENTATION.SOUTH;
                    break;
                case 2:
                    orientation = Utils.ORIENTATION.EAST;
                    break;
                case 3:
                    orientation = Utils.ORIENTATION.WEST;
                    break;
            }

            var edge = node.adjacents.SingleOrDefault(y => y.orientation == Utils.OrientationToDir(orientation));
            var door = roomDoors.SingleOrDefault(y => y.Orientation == orientation);
            bool isDoor = door != null && door.State != Door.STATE.WALL;
            bool isEdge = edge != null && edge.doorState != Door.STATE.WALL;
            if (isDoor != isEdge)
                return false;
        }
        return true;

        /*
        foreach(Door door in roomDoors)
        {
            var edge = node.adjacents.SingleOrDefault(i => i.orientation == Utils.OrientationToDir(door.Orientation));
            if (edge == null || edge.doorState == Door.STATE.WALL)
                return false;
        }
        return true;
        */
    }
    void SetRoomDoors(Room room, Node node)
    {
        foreach(Door door in room.connectedDoors)
        {
            var correspondingEdge = node.adjacents.SingleOrDefault(i => i.orientation == Utils.OrientationToDir(door.Orientation));
            if (correspondingEdge == null)
                door.SetState(Door.STATE.WALL);
            else
                door.SetState(correspondingEdge.doorState);
        }
    }

    /*   void SpawnRoomLetRec(Node _node, bool _isStart = false )
   {

       if (_node != null)
       {
           int nbDoor = _node.adjacents.Count;
           GameObject roomGO = Instantiate(SelectPrefab(nbDoor), new Vector3(_node.location.x * RealSizeRoomX, _node.location.y * RealSizeRoomY, 0), Quaternion.identity);
           Room _room = roomGO.GetComponent<Room>();
           _room.position = _node.location;
           if (_isStart)
           {
               _room.isStartRoom = true;
           }

           foreach (Edge _edge in _node.adjacents)
           {
               SpawnRoomLetRec(_edge.node1);
           }

       }
       return;


   }*/

}
