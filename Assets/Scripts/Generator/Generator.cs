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
    public List<GameObject> PrefabsTwoDoor;
    public List<GameObject> PrefabsThreeDoor;
    public List<GameObject> PrefabsFourthDoor;

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
            GameObject roomGO = Instantiate(SelectPrefab(nbDoor), new Vector3(_node.location.x * RealSizeRoomX, _node.location.y * RealSizeRoomY, 0), Quaternion.identity);
            Room _room = roomGO.GetComponent<Room>();
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

    GameObject SelectPrefab(int _nbRoom)
    {
        switch (_nbRoom)
        {
            case 1:
                return PrefabsOneDoor[UnityEngine.Random.Range(0, PrefabsOneDoor.Count - 1)];

            case 2:
                return PrefabsTwoDoor[UnityEngine.Random.Range(0, PrefabsTwoDoor.Count - 1)];
            case 3:
                return PrefabsThreeDoor[UnityEngine.Random.Range(0, PrefabsThreeDoor.Count - 1)];
            case 4:
                return PrefabsFourthDoor[UnityEngine.Random.Range(0, PrefabsFourthDoor.Count - 1)];

            default:
                return null;
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
