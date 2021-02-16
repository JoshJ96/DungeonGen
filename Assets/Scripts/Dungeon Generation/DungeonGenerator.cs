using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DungeonGenerator : MonoBehaviour
{
    List<Room> roomsList = new List<Room>();
    Tile[,] map;
    public GameObject
        wallObject,
        playerObject;
    public int roomMaxSize, roomMinSize;
    public int mapWidth, mapHeight;
    public int maxRooms;
    public int playerStartX, playerStartY;
    int justincase = 43300;

    void Start()
    {
        GenerateDungeon(mapWidth, mapHeight);
    }

    private void GenerateDungeon(int roomMaxSize, int roomMinSize)
    {
        InitializeMap();
        PlaceRooms();
        CarveTunnels();
        PrintMap();
    }
    void InitializeMap()
    {
        map = new Tile[mapWidth - 1, mapHeight - 1];

        for (int x = 0; x < mapWidth - 1; x++)
        {
            for (int y = 0; y < mapHeight - 1; y++)
            {
                map[x, y] = new Tile { };
                map[x, y].tileType = Tile.State.Wall;
            }
        }
    }
    void PlaceRooms()
    {
        int numRooms = 0;

        while (numRooms != maxRooms)
        {
            justincase--;
            if (justincase == 0)
            {
                return;
            }
            //Random width and height
            int w = UnityEngine.Random.Range(roomMinSize, roomMaxSize);
            int h = UnityEngine.Random.Range(roomMinSize, roomMaxSize);
            //Random Position
            int x = UnityEngine.Random.Range(0, mapWidth - w - 1);
            int y = UnityEngine.Random.Range(0, mapHeight - h - 1);
            //Create the room
            Room newRoom = new Room(x, y, w, h);
            //Check for overlap
            bool failed = false;
            foreach (var room in roomsList)
            {
                if (RoomsOverlap(newRoom, room))
                {
                    failed = true;
                    //break;
                }
            }
            //If no overlap, create the room
            if (!failed)
            {
                CreateRoom(newRoom);

                //Reserve player spawn if last room created
                if (numRooms == 0)
                {
                    playerStartX = (int)newRoom.center.x;
                    playerStartY = (int)newRoom.center.y;
                }
                else
                {
                    //Center coordinates of previous room
                    Vector2 previousCoordinates = roomsList[numRooms - 1].center;

                    //Random chance for horizontal/vertical passage
                    int coinToss = UnityEngine.Random.Range(0, 2);

                    //if (coinToss == 0)
                    {
                        CarveHorizontalTunnel((int)previousCoordinates.x, (int) newRoom.center.x, (int) previousCoordinates.y);
                        CarveVerticalTunnel((int)previousCoordinates.y, (int) newRoom.center.y, (int)newRoom.center.x);
                    }
                    //else if (coinToss == 1)
                    //{
                    //    CarveVerticalTunnel((int)previousCoordinates.y, (int)newRoom.center.y, (int)previousCoordinates.x);
                    //    CarveHorizontalTunnel((int)previousCoordinates.x, (int)newRoom.center.x, (int)newRoom.center.y);
                    //}
                }
                numRooms++;
                roomsList.Add(newRoom);
            }
        }
    }

    void CarveTunnels()
    {

    }


    void PrintMap()
    {
        GameObject player = Instantiate(playerObject, new Vector3(transform.position.x + playerStartX + 0.5f, -.5f, transform.position.z + playerStartY + 0.5f), Quaternion.identity);
        player.GetComponent<PlayerUnit>().walkSpeed = 7;

        for (int x = 0; x < mapWidth - 1; x++)
        {
            for (int y = 0; y < mapHeight - 1; y++)
            {
                if (map[x,y].tileType == Tile.State.Wall)
                {
                    GameObject wall = Instantiate(wallObject, new Vector3(transform.position.x + x + 0.5f,0, transform.position.z + y + 0.5f), Quaternion.identity);
                    wall.transform.parent = this.transform;
                }
            }
        }
    }

    /*
    ██╗░░██╗███████╗██╗░░░░░██████╗░███████╗██████╗░░██████╗
    ██║░░██║██╔════╝██║░░░░░██╔══██╗██╔════╝██╔══██╗██╔════╝
    ███████║█████╗░░██║░░░░░██████╔╝█████╗░░██████╔╝╚█████╗░
    ██╔══██║██╔══╝░░██║░░░░░██╔═══╝░██╔══╝░░██╔══██╗░╚═══██╗
    ██║░░██║███████╗███████╗██║░░░░░███████╗██║░░██║██████╔╝
    ╚═╝░░╚═╝╚══════╝╚══════╝╚═╝░░░░░╚══════╝╚═╝░░╚═╝╚═════╝░
    */

    void CreateRoom(Room room)
    {
        for (int x = room.mapX + 1; x < room.mapX + room.width; x++)
        {
            for (int y = room.mapY + 1; y < room.mapY + room.height; y++)
            {
                map[x, y].tileType = Tile.State.Floor;
                map[x, y].blockSight = false;
            }
        }
    }

    void CarveHorizontalTunnel(int x1, int x2, int y)
    {
        if (x1 < x2)
        {
            for (int x = x1; x < x2 + 1; x++)
            {
                map[x, y].tileType = Tile.State.Floor;
                map[x, y].blockSight = false;
            }
        }
        else if (x1 > x2)
        {
            for (int x = x1 + 1; x > x2; x--)
            {
                map[x, y].tileType = Tile.State.Floor;
                map[x, y].blockSight = false;
            }
        }
    }

    void CarveVerticalTunnel(int y1, int y2, int x)
    {
        if (y1 < y2)
        {
            for (int y = y1; y < y2 + 1; y++)
            {
                map[x, y].tileType = Tile.State.Floor;
                map[x, y].blockSight = false;
            }
        }
        else if (y1 > y2)
        {
            for (int y = y1 + 1; y > y2; y--)
            {
                map[x, y].tileType = Tile.State.Floor;
                map[x, y].blockSight = false;
            }
        }
    }

    bool RoomsOverlap(Room room1, Room room2)
    {
        return (
            room1.mapX                <= room2.mapX + room2.width
            &&
            room1.mapX + room1.width  >= room2.mapX
            &&
            room1.mapY                <= room2.mapY + room2.height
            &&
            room1.mapY + room1.height >= room2.mapY
            );
    }
}