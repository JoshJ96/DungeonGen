﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

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
    public Divider dividerParent;
    public int x1, x2, y1, y2;
    public Vector2 GetCenter()
    {
        return new Vector2((x2 + x1) / 2, (y2 + y1) / 2);
    }
}

public class Hallway
{
    public int x1, x2, y1, y2;
    public Vector2 GetCenter()
    {
        return new Vector2((x2 + x1) / 2, (y2 + y1) / 2);
    }
}

public class DungeonGenerator : MonoBehaviour
{
    enum Axis
    {
        X,
        Y
    }

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

    public int[,] map;

    //BSP Divider Root
    Divider root;
    List<Divider> dividerList = new List<Divider>();
    List<Room> roomList = new List<Room>();
    List<Hallway> hallwayList = new List<Hallway>();
    int finalIteration = 0;

    //Map objects
    public GameObject floor;
    public GameObject wall;

    //The % of random coords from middle to be chosen (0.05 = 5%)
    [Range(0, 0.1f)]
    public float bspDeviation = 0.05f;

    private void Start()
    {
        GenerateDungeon();
    }

    private void GenerateDungeon()
    {
        Initialize_BSP_Root();
        Recursive_BSP_Split(0);
        Random_Place_Rooms();
        Generate_Hallways();
        Instantiate_Terrain();
    }

    private void Instantiate_Terrain()
    {
        map = new int[mapWidth, mapHeight];

        for (int x = 0; x < mapWidth; x++)
        {
            for (int y = 0; y < mapHeight; y++)
            {
                map[x, y] = 1;
            }
        }

        //Rooms
        foreach (var room in roomList)
        {
            for (int x = room.x1; x < room.x2; x++)
            {
                for (int y = room.y1; y < room.y2; y++)
                {
                    map[x,y] = 0;
                }
            }
        }

        //Cooridors
        foreach (var item in hallwayList)
        {
            for (int x = item.x1; x < item.x2; x++)
            {
                for (int y = item.y1; y < item.y2; y++)
                {
                    map[x,y] = 0;
                }
            }
        }

        for (int x = 0; x < mapWidth; x++)
        {
            for (int y = 0; y < mapHeight; y++)
            {
                if (map[x, y] == 1)
                {
                    GameObject wallObj = Instantiate(wall, new Vector3(x, 1, y), Quaternion.identity);
                    wallObj.transform.parent = this.transform;
                }
                else if (map[x, y] == 0)
                {
                    GameObject floorObj = Instantiate(floor, new Vector3(x, 0, y), Quaternion.identity);
                    floorObj.transform.parent = this.transform;
                }
            }
        }
    }

    private void Random_Place_Rooms()
    {
        foreach (var divider in dividerList.Where(x => x.depth == finalIteration))
        {
            //Generate a random room
            int width = UnityEngine.Random.Range(roomMinSize, roomMaxSize);
            int height = UnityEngine.Random.Range(roomMinSize, roomMaxSize);

            int _x1 = UnityEngine.Random.Range(divider.x1, divider.x2 - width);
            int _y1 = UnityEngine.Random.Range(divider.y1, divider.y2 - height);
            int _x2 = _x1 + width;
            int _y2 = _y1 + height;

            Room room = new Room
            {
                x1 = _x1,
                x2 = _x2,
                y1 = _y1,
                y2 = _y2
            };

            divider.roomWithin = room;
            roomList.Add(room);
        }
    }

    private void Generate_Hallways()
    {

        //Get dividers at finalIteration - 1 (to access child nodes)
        foreach (var divider in dividerList.Where(x => x.depth == finalIteration-1))
        {
            if (divider.leftChild != null && divider.rightChild != null)
            {
                Create_Hallway(divider.leftChild.roomWithin, divider.rightChild.roomWithin);
            }
        }

        int count = 2;
        while (count < finalIteration + 1)
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

    private void Create_Hallway(Room roomWithin1, Room roomWithin2)
    {

        Vector2 center1 = roomWithin1.GetCenter();
        Vector2 center2 = roomWithin2.GetCenter();

        //Random corridor direction
        int rng = UnityEngine.Random.Range(0, 2);

        //X then Y
        if (rng == 0)
        {
            //X Left to Right
            if (center2.x > center1.x)
            {
                Hallway hallwayX = new Hallway
                {
                    x1 = (int)center1.x,
                    x2 = (int)center2.x+1,
                    y1 = (int)center1.y,
                    y2 = (int)center1.y + 1
                };
                hallwayList.Add(hallwayX);
            }
            //X Right to Left
            else if (center2.x < center1.x)
            {
                Hallway hallwayX = new Hallway
                {
                    x1 = (int)center2.x,
                    x2 = (int)center1.x-1,
                    y1 = (int)center1.y,
                    y2 = (int)center1.y + 1
                };
                hallwayList.Add(hallwayX);
            }

            //Y Down to Up
            if (center2.y > center1.y)
            {
                Hallway hallwayY = new Hallway
                {
                    x1 = (int)center2.x,
                    x2 = (int)center2.x + 1,
                    y1 = (int)center1.y,
                    y2 = (int)center2.y
                };
                hallwayList.Add(hallwayY);
            }
            else if (center2.y < center1.y)
            {
                Hallway hallwayY = new Hallway
                {
                    x1 = (int)center2.x,
                    x2 = (int)center2.x + 1,
                    y1 = (int)center2.y,
                    y2 = (int)center1.y
                };
                hallwayList.Add(hallwayY);
            }
        }
        else if (rng == 1)
        {
            //Y Down to Up
            if (center2.y > center1.y)
            {
                Hallway hallwayY = new Hallway
                {
                    x1 = (int)center2.x,
                    x2 = (int)center2.x + 1,
                    y1 = (int)center1.y,
                    y2 = (int)center2.y
                };
                hallwayList.Add(hallwayY);
            }
            else if (center2.y < center1.y)
            {
                Hallway hallwayY = new Hallway
                {
                    x1 = (int)center2.x,
                    x2 = (int)center2.x + 1,
                    y1 = (int)center2.y,
                    y2 = (int)center1.y
                };
                hallwayList.Add(hallwayY);
            }

            //X Left to Right
            if (center2.x > center1.x)
            {
                Hallway hallwayX = new Hallway
                {
                    x1 = (int)center1.x,
                    x2 = (int)center2.x + 1,
                    y1 = (int)center1.y,
                    y2 = (int)center1.y + 1
                };
                hallwayList.Add(hallwayX);
            }
            //X Right to Left
            else if (center2.x < center1.x)
            {
                Hallway hallwayX = new Hallway
                {
                    x1 = (int)center2.x,
                    x2 = (int)center1.x - 1,
                    y1 = (int)center1.y,
                    y2 = (int)center1.y + 1
                };
                hallwayList.Add(hallwayX);
            }
        }
    }

    private void Initialize_BSP_Root()
    {
        //Create the initial root divider
        root = new Divider
        {
            id = 0,
            depth = 0,
            x1 = 0,
            y1 = 0,
            x2 = mapWidth,
            y2 = mapHeight
        };

        dividerList.Add(root);
    }

    public int numberDivides = 6;

    private void Recursive_BSP_Split(int currentDepth)
    {
        numberDivides--;
        if (numberDivides <= 0)
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

    public int layer;

    private bool DividerCanBeCarvedX(Divider divider)
    {
        return !((divider.x2 - divider.x1) < (roomMaxSize * 2));
    }

    private bool DividerCanBeCarvedY(Divider divider)
    {
        return !((divider.y2 - divider.y1) < (roomMaxSize * 2));
    }
}