using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Generator : MonoBehaviour
{
    const int RealSizeRoomX = 11;
    const int RealSizeRoomY = 8;

    private int _instanciateRoom = 0;

    public List<GameObject> PrefabsOneDoor;
    public List<GameObject> PrefabsTwoDoors;
    public List<GameObject> PrefabsThreeDoors;
    public List<GameObject> PrefabsFourDoors;

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
            {
                _room.isStartRoom = true;
                _room.name = "End";
            }
        }
    }
    GameObject SelectPrefab(Node _node)
    {
        List<Vector2Int> allDoorsDirections = new List<Vector2Int>();

        // store all the doors directions
        foreach (Edge edge in _node.adjacents)
        {
            allDoorsDirections.Add(edge.node1.location - _node.location);
        }

        // NOPE
        switch (_node.adjacents.Count)
        {
            case 1:
                return PrefabsOneDoor[UnityEngine.Random.Range(0, PrefabsOneDoor.Count - 1)];
            case 2:
                return PrefabsTwoDoors[UnityEngine.Random.Range(0, PrefabsTwoDoors.Count - 1)];
            case 3:
                return PrefabsThreeDoors[UnityEngine.Random.Range(0, PrefabsThreeDoors.Count - 1)];
            case 4:
                return PrefabsFourDoors[UnityEngine.Random.Range(0, PrefabsFourDoors.Count - 1)];

            default:
                return null;
        }
    }
    void SetRoomDoors(Room room, Node node)
    {
        foreach(Door door in room.connectedDoors)
        {
            door.transform.LookAt(door.transform.position - room.transform.position);
            var correspondingEdge = node.adjacents.SingleOrDefault(i => i.orientation == Utils.OrientationToDir(door.Orientation));
            if (correspondingEdge == null)
            {
                door.SetState(Door.STATE.WALL);
                return;
            }
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
