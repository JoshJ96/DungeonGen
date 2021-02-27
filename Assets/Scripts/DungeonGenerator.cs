using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

//Dungeon data
public class DungeonData
{
    public int mapWidth, mapHeight;
    public string dungeonName;
    public int floorNumber;
    public Node[,] map;
    public Room playerSpawnRoom;
}

//Divider class definition
public class Divider
{
    public int depth, id;
    public Divider leftChild, rightChild;
    public int x1, x2, y1, y2;
    public Room roomWithin;
    public List<Room> FindRoomsWithin(List<Room> roomList)
    {
        List<Room> toReturn = new List<Room>();
        foreach (var room in roomList)
        {
            if (room.x1 >= this.x1 && room.x2 <= this.x2 && room.y1 >= this.y1 && room.y2 < this.y2)
            {
                toReturn.Add(room);
            }
        }
        return toReturn;
    }
}

public class Room
{
    public enum Type
    {
        Room,
        Hallway
    }

    public Type type;
    public bool visibleOnMap;

    public Divider dividerParent;
    public int x1, x2, y1, y2;

    public Vector2 V2Center => new Vector2((x2 + x1) / 2, (y2 + y1) / 2);

    public Vector3 V3Center => new Vector3((x2 + x1) / 2, 0, (y2 + y1) / 2);

    public List<Node> NodesWithinRoom(Node[,] map)
    {
        List<Node> toReturn = new List<Node>();
        for (int x = x1; x < x2; x++)
        {
            for (int y = y1; y < y2; y++)
            {
                toReturn.Add(map[x, y]);
            }
        }

        return toReturn;
    }

}

public class DungeonGenerator : MonoBehaviour
{
    #region Singleton
    public static DungeonGenerator instance;
    private void Awake() => instance = this;
    #endregion

    enum Axis
    {
        X,
        Y
    }

    public string dungeonName;
    public int floorNumber;

    //Map size properties
    [Range(20, 500)]
    public int mapWidth;
    [Range(20, 500)]
    public int mapHeight;

    //Room size properties
    [Range(5, 100)]
    public int roomMinSize;
    [Range(5, 100)]
    public int roomMaxSize;

    //BSP Divider Root
    Divider root;
    public List<Divider> dividerList = new List<Divider>();
    public List<Room> roomList = new List<Room>();
    int finalIteration = 0;

    public Node[,] map;

    [Range(0,3)]
    public float roomOverlapPreventionFactor;
    public int numberDivides = 6;
    int divides;

    public GameObject wallObj, floorObj, playerObj, camObj;

    //The % of random coords from middle to be chosen (0.05 = 5%)
    [Range(0, 0.1f)]
    public float bspDeviation = 0.05f;

    public bool testMode = true;
    public bool drawWalls = false;

    private void Update()
    {
        if (Input.GetKey(KeyCode.Space))
        {
            if (!testMode)
            {
                return;
            }

            dividerList.Clear();
            roomList.Clear();
            finalIteration = 0;
            GenerateDungeon();
        }
    }

    private void Start()
    {
        GenerateDungeon();
    }

    private void GenerateDungeon()
    {
        Initialize_Node_Map();
        Initialize_BSP_Root();
        Recursive_BSP_Split(0);
        Random_Place_Rooms();
        Generate_Hallways();
        Update_Node_Map();
        Pass_Map_To_Grid();
        Instantiate_Terrain_Objects();
        Event_Push_Dungeon_Data();
    }

    private void Initialize_Node_Map()
    {
        map = new Node[mapWidth, mapHeight];

        for (int x = 0; x < mapWidth; x++)
        {
            for (int y = 0; y < mapHeight; y++)
            {
                map[x,y] = new Node
                {
                    gridX = x,
                    gridY = y,
                    walkable = false
                };
            }
        }
    }

    private void Initialize_BSP_Root()
    {
        divides = numberDivides;

        //Create the initial root divider
        root = new Divider
        {
            id = 0,
            depth = 0,
            x1 = roomMaxSize,
            y1 = roomMaxSize,
            x2 = mapWidth - roomMaxSize,
            y2 = mapHeight - roomMaxSize
        };

        dividerList.Add(root);
    }

    private void Recursive_BSP_Split(int currentDepth)
    {
        divides--;
        if (divides <= 0)
        {
            return;
        }

        //Loop through every divider at the current depth
        List<Divider> toCarve = dividerList.Where(x => x.depth == currentDepth).ToList();

        if (toCarve.Count != 0)
        {
            int count = 0;
            foreach (var item in toCarve)
            {
                //Pick a random axis
                int rng = UnityEngine.Random.Range(0, 2);

                //X
                if (rng == 0)
                {
                    //If it can't be carved along the X axis, switch and try Y axis
                    if (DividerCanBeCarvedX(item))
                    {
                        CarveDivider(item, Axis.X);
                    }
                    else if (DividerCanBeCarvedY(item))
                    {
                        CarveDivider(item, Axis.Y);
                    }
                }

                //Y
                else if (rng == 1)
                {
                    //If it can't be carved along the Y axis, switch and try X axis
                    if (DividerCanBeCarvedY(item))
                    {
                        CarveDivider(item, Axis.Y);
                    }
                    else if (DividerCanBeCarvedX(item))
                    {
                        CarveDivider(item, Axis.X);
                    }
                }
                count++;
            }
        }
        else
        {
            return;
        }
        finalIteration++;
        Recursive_BSP_Split(currentDepth + 1);
    }

    private void Random_Place_Rooms()
    {
        foreach (var divider in dividerList.Where(x => x.depth == finalIteration))
        {
            int _x1 = UnityEngine.Random.Range(divider.x1, divider.x2 - roomMaxSize);
            int _y1 = UnityEngine.Random.Range(divider.y1, divider.y2 - roomMaxSize);
            int _x2 = UnityEngine.Random.Range(_x1 + roomMinSize, _x1 + roomMaxSize);
            int _y2 = UnityEngine.Random.Range(_y1 + roomMinSize, _y1 + roomMaxSize);

            Room room = new Room {
                type = Room.Type.Room,
                visibleOnMap = false,
                x1 = _x1,
                x2 = _x2,
                y1 = _y1,
                y2 = _y2,
            };

            divider.roomWithin = room;
            roomList.Add(room);
        }
    }

    private void Generate_Hallways()
    {

        //Get dividers at finalIteration - 1 (to access child nodes)
        foreach (var divider in dividerList.Where(x => x.depth == finalIteration - 1))
        {
            if (divider.leftChild != null && divider.rightChild != null)
            {
                Create_Hallway(divider.leftChild.roomWithin, divider.rightChild.roomWithin);
            }
        }

        int count = 2;
        while (count < finalIteration+1)
        {
            //Get dividers at finalIteration - 2 (to access child nodes)
            foreach (var divider in dividerList.Where(x => x.depth == finalIteration - count))
            {
                if (divider.leftChild != null && divider.rightChild != null)
                {
                    var room2 = divider.leftChild.FindRoomsWithin(roomList);
                    var room1 = divider.rightChild.FindRoomsWithin(roomList);

                    if (room1 != null && room2 != null)
                    {
                        if (room1.Count != 0 && room2.Count != 0)
                        {
                            Create_Hallway(room1[0], room2[0]);
                        }
                    }
                }
            }
            count++;
        }
    }

    private void Update_Node_Map()
    {
        //Update rooms and hallways
        foreach (var room in roomList.Where(x => x.type == Room.Type.Hallway))
        {
            for (int x = room.x1; x < room.x2; x++)
            {
                for (int y = room.y1; y < room.y2; y++)
                {
                    map[x, y].roomPartOf = room;
                    map[x, y].walkable = true;
                }
            }
        }

        foreach (var room in roomList.Where(x => x.type == Room.Type.Room))
        {
            for (int x = room.x1; x < room.x2; x++)
            {
                for (int y = room.y1; y < room.y2; y++)
                {
                    map[x, y].roomPartOf = room;
                    map[x, y].walkable = true;
                }
            }
        }
    }

    private void Instantiate_Terrain_Objects()
    {
        if (testMode)
        {
            return;
        }

        for (int x = 0; x < mapWidth; x++)
        {
            for (int y = 0; y < mapHeight; y++)
            {
                if (map[x,y].walkable)
                {
                    Instantiate(floorObj, new Vector3(x, -0.5f, y), Quaternion.identity, this.transform);
                }
                else
                {
                    if (drawWalls)
                    {
                        Instantiate(wallObj, new Vector3(x, 0.5f, y), Quaternion.identity, this.transform);
                    }
                }
            }
        }
    }

    private void Pass_Map_To_Grid()
    {
        if (testMode)
        {
            return;
        }
        Grid.instance.grid = map;
        Grid.instance.gridSizeX = mapWidth;
        Grid.instance.gridSizeY = mapHeight;
    }

    private void Event_Push_Dungeon_Data()
    {
        if (testMode)
        {
            return;
        }

        //Get random spawn room for player
        List<Room> listFullRooms = roomList.Where(x => x.type == Room.Type.Room).ToList();
        int randomRoomIndex = UnityEngine.Random.Range(0, listFullRooms.Count - 1);
        listFullRooms[randomRoomIndex].visibleOnMap = true;
        Room playerSpawnRoom = listFullRooms[randomRoomIndex];

        DungeonData data = new DungeonData {
            mapWidth = this.mapWidth,
            mapHeight = this.mapHeight,
            dungeonName = this.dungeonName,
            floorNumber = this.floorNumber,
            map = this.map,
            playerSpawnRoom = playerSpawnRoom
        };
        GameEvents.instance.PushDungeonData(data);
    }

    
    /*
    ██╗░░██╗███████╗██╗░░░░░██████╗░███████╗██████╗░░██████╗
    ██║░░██║██╔════╝██║░░░░░██╔══██╗██╔════╝██╔══██╗██╔════╝
    ███████║█████╗░░██║░░░░░██████╔╝█████╗░░██████╔╝╚█████╗░
    ██╔══██║██╔══╝░░██║░░░░░██╔═══╝░██╔══╝░░██╔══██╗░╚═══██╗
    ██║░░██║███████╗███████╗██║░░░░░███████╗██║░░██║██████╔╝
    ╚═╝░░╚═╝╚══════╝╚══════╝╚═╝░░░░░╚══════╝╚═╝░░╚═╝╚═════╝░
    */

    private void CarveDivider(Divider divider, Axis axis)
    {
        if (axis == Axis.X)
        {
            //Deviate a random point from carve center
            int lowerBounds = (int)((divider.x2 + divider.x1) * (0.5f - bspDeviation));
            int upperBounds = (int)((divider.x2 + divider.x1) * (0.5f + bspDeviation));
            int carveCoordinate = UnityEngine.Random.Range(lowerBounds, upperBounds);

            //Create left and right child dividers
            Divider left = new Divider
            {
                id = divider.id + 1,
                x1 = divider.x1,
                x2 = carveCoordinate,
                y1 = divider.y1,
                y2 = divider.y2,
                depth = divider.depth + 1
            };
            Divider right = new Divider
            {
                id = divider.id + 1,
                x1 = carveCoordinate,
                x2 = divider.x2,
                y1 = divider.y1,
                y2 = divider.y2,
                depth = divider.depth + 1
            };

            dividerList.Add(right);
            dividerList.Add(left);
            divider.leftChild = left;
            divider.rightChild = right;
        }
        else if (axis == Axis.Y)
        {
            //Deviate a random point from carve center
            int lowerBounds = (int)((divider.y2 + divider.y1) * (0.5f - bspDeviation));
            int upperBounds = (int)((divider.y2 + divider.y1) * (0.5f + bspDeviation));
            int carveCoordinate = UnityEngine.Random.Range(lowerBounds, upperBounds);

            //Create left and right child dividers
            Divider left = new Divider
            {
                id = divider.id + 1,
                y1 = divider.y1,
                y2 = carveCoordinate,
                x1 = divider.x1,
                x2 = divider.x2,
                depth = divider.depth + 1
            };
            Divider right = new Divider
            {
                id = divider.id + 1,
                y1 = carveCoordinate,
                y2 = divider.y2,
                x1 = divider.x1,
                x2 = divider.x2,
                depth = divider.depth + 1
            };

            dividerList.Add(right);
            dividerList.Add(left);
            divider.leftChild = left;
            divider.rightChild = right;
        }
    }

    private void Create_Hallway(Room roomWithin1, Room roomWithin2)
    {

        Vector2 center1 = roomWithin1.V2Center;
        Vector2 center2 = roomWithin2.V2Center;

        //Random corridor direction
        int rng = UnityEngine.Random.Range(0, 2);

        //X then Y
        if (rng == 0)
        {
            //X Left to Right
            if (center2.x > center1.x)
            {
                Room hallwayX = new Room
                {
                    type = Room.Type.Hallway,
                    visibleOnMap = false,
                    x1 = (int)center1.x,
                    x2 = (int)center2.x + 2,
                    y1 = (int)center1.y,
                    y2 = (int)center1.y + 2
                };
                roomList.Add(hallwayX);
            }
            //X Right to Left
            else if (center2.x < center1.x)
            {
                Room hallwayX = new Room
                {
                    type = Room.Type.Hallway,
                    visibleOnMap = false,
                    x1 = (int)center2.x,
                    x2 = (int)center1.x - 2,
                    y1 = (int)center1.y,
                    y2 = (int)center1.y + 2
                };
                roomList.Add(hallwayX);
            }

            //Y Down to Up
            if (center2.y > center1.y)
            {
                Room hallwayY = new Room
                {
                    type = Room.Type.Hallway,
                    visibleOnMap = false,
                    x1 = (int)center2.x,
                    x2 = (int)center2.x + 2,
                    y1 = (int)center1.y,
                    y2 = (int)center2.y
                };
                roomList.Add(hallwayY);
            }
            else if (center2.y < center1.y)
            {
                Room hallwayY = new Room
                {
                    type = Room.Type.Hallway,
                    visibleOnMap = false,
                    x1 = (int)center2.x,
                    x2 = (int)center2.x + 2,
                    y1 = (int)center2.y,
                    y2 = (int)center1.y
                };
                roomList.Add(hallwayY);
            }
        }
        else if (rng == 1)
        {
            //Y Down to Up
            if (center2.y > center1.y)
            {
                Room hallwayY = new Room
                {
                    type = Room.Type.Hallway,
                    visibleOnMap = false,
                    x1 = (int)center2.x,
                    x2 = (int)center2.x + 2,
                    y1 = (int)center1.y,
                    y2 = (int)center2.y
                };
                roomList.Add(hallwayY);
            }
            else if (center2.y < center1.y)
            {
                Room hallwayY = new Room
                {
                    type = Room.Type.Hallway,
                    visibleOnMap = false,
                    x1 = (int)center2.x,
                    x2 = (int)center2.x + 2,
                    y1 = (int)center2.y,
                    y2 = (int)center1.y
                };
                roomList.Add(hallwayY);
            }

            //X Left to Right
            if (center2.x > center1.x)
            {
                Room hallwayX = new Room
                {
                    type = Room.Type.Hallway,
                    visibleOnMap = false,
                    x1 = (int)center1.x,
                    x2 = (int)center2.x + 2,
                    y1 = (int)center1.y,
                    y2 = (int)center1.y + 2
                };
                roomList.Add(hallwayX);
            }
            //X Right to Left
            else if (center2.x < center1.x)
            {
                Room hallwayX = new Room
                {
                    type = Room.Type.Hallway,
                    visibleOnMap = false,
                    x1 = (int)center2.x,
                    x2 = (int)center1.x - 2,
                    y1 = (int)center1.y,
                    y2 = (int)center1.y + 2
                };
                roomList.Add(hallwayX);
            }
        }
        
    }

    private bool DividerCanBeCarvedX(Divider divider)
    {
        return !((divider.x2 - divider.x1) < (roomMaxSize * roomOverlapPreventionFactor));
    }

    private bool DividerCanBeCarvedY(Divider divider)
    {
        return !((divider.y2 - divider.y1) < (roomMaxSize * roomOverlapPreventionFactor));
    }

    private void OnDrawGizmos()
    {
        if (!testMode)
        {
            return;
        }

        foreach (var divider in dividerList.Where(x => x.depth == finalIteration - 1))
        {
            if (divider.leftChild != null && divider.rightChild != null)
            {
                Gizmos.color = Color.cyan;
                Gizmos.DrawCube(new Vector3(divider.leftChild.x1 + 3, 0, divider.leftChild.y1 + 3), Vector3.one);
                Gizmos.DrawCube(new Vector3(divider.rightChild.x1 + 3, 0, divider.rightChild.y1 + 3), Vector3.one);
            }
        }


        foreach (var room in roomList)
        {
            for (int x = room.x1; x < room.x2; x++)
            {
                for (int y = room.y1; y < room.y2; y++)
                {
                    Gizmos.color = Color.green;
                    Gizmos.DrawCube(new Vector3(x, 0, y), Vector3.one);
                }
            }
        }



        for (int x = 0; x < mapWidth; x++)
        {
            for (int y = 0; y < mapHeight; y++)
            {
                if (x == 0 || y == 0 || x == mapWidth-1 || y == mapHeight-1)
                {
                    Gizmos.color = Color.black;
                    Gizmos.DrawCube(new Vector3(x, 0, y), Vector3.one);
                }
            }
        }


        foreach (var item in dividerList.Where(x => x.depth == numberDivides-1).ToList())
        {
            {
                Gizmos.color = Color.red;
                Gizmos.DrawCube(new Vector3((item.x2 + item.x1) / 2, 0, (item.y2 + item.y1) / 2), Vector3.one);

                for (int x = item.x1; x < item.x2 - 1; x++)
                {
                    for (int y = item.y1; y < item.y2 - 1; y++)
                    {
                        if (x == item.x1 || x == item.x2 - 2 || y == item.y1 || y == item.y2 - 2)
                        {
                            Gizmos.color = Color.white;
                            Gizmos.DrawCube(new Vector3(x, 0, y), Vector3.one);
                        }
                    }
                }
            }
        }
    }
}