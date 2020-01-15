using CreativeSpore.SuperTilemapEditor;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Room : MonoBehaviour {

	public Vector2Int position = Vector2Int.zero;

	private TilemapGroup _tilemapGroup;

	public static List<Room> allRooms = new List<Room>();
    [HideInInspector]
    public List<Door> connectedDoors = new List<Door>();

    public Node.ROOM_TYPE roomType = 0;

    void Awake()
    {
		_tilemapGroup = GetComponentInChildren<TilemapGroup>();
		allRooms.Add(this);
        connectedDoors.AddRange(GetComponentsInChildren<Door>());
	}

	private void OnDestroy()
	{
		allRooms.Remove(this);
	}

	void Start () {
        if (roomType == Node.ROOM_TYPE.START)
        {
            OnEnterRoom();
        }
    }
	
	public void OnEnterRoom()
    {
        CameraFollow cameraFollow = Camera.main.GetComponent<CameraFollow>();
        Bounds cameraBounds = _GetWorldRoomBounds();
        cameraFollow.SetBounds(cameraBounds);
		Player.Instance.EnterRoom(this);
    }


    private Bounds _GetLocalRoomBounds()
    {
		Bounds roomBounds = new Bounds(Vector3.zero, Vector3.zero);
		if (_tilemapGroup == null)
			return roomBounds;

		foreach (STETilemap tilemap in _tilemapGroup.Tilemaps)
		{
			Bounds bounds = tilemap.MapBounds;
			roomBounds.Encapsulate(bounds);
		}
		return roomBounds;
    }

    public Bounds _GetWorldRoomBounds()
    {
        Bounds result = _GetLocalRoomBounds();
        result.center += transform.position;
        return result;
    }

	public bool Contains(Vector3 position)
	{
		position.z = 0;
		return (_GetWorldRoomBounds().Contains(position));
	}
}
