using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

//Divider class definition
public class Divider
{
    public int depth;
    public Divider leftChild, rightChild;
    public int x1, x2, y1, y2;
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
    [Range(5, 30)]
    public int roomMinSize;
    [Range(5, 30)]
    public int roomMaxSize;

    //BSP Divider Root
    Divider root;
    List<Divider> dividerList = new List<Divider>();

    //The % of random coords from middle to be chosen (0.05 = 5%)
    [Range(0, 1)]
    public float bspDeviation = 0.05f;

    private void Start()
    {
        GenerateDungeon();
    }

    private void GenerateDungeon()
    {
        Initialize_BSP_Root();
        BSP_Split(0);
    }

    private void Initialize_BSP_Root()
    {
        //Create the initial root divider
        root = new Divider
        {
            depth = 0,
            x1 = 0,
            y1 = 0,
            x2 = mapWidth,
            y2 = mapHeight
        };

        dividerList.Add(root);
    }

    int deleteThis = 400;

    private void BSP_Split(int currentDepth)
    {
        deleteThis--;
        if (deleteThis <= 0)
        {
            return;
        }

        //Loop through every divider at the current depth
        List<Divider> toCarve = dividerList.Where(x => x.depth == currentDepth).ToList();

        if (toCarve.Count != 0)
        {
            foreach (var item in dividerList.Where(x => x.depth == currentDepth).ToList())
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
                if (rng == 1)
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
            }
        }
        else
        {
            return;
        }
        print(currentDepth);
        BSP_Split(currentDepth + 1);
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
                x1 = divider.x1,
                x2 = carveCoordinate,
                y1 = divider.y1,
                y2 = divider.y2,
                depth = divider.depth + 1
            };
            Divider right = new Divider
            {
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
                y1 = divider.y1,
                y2 = carveCoordinate,
                x1 = divider.x1,
                x2 = divider.x2,
                depth = divider.depth + 1
            };
            Divider right = new Divider
            {
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

    private bool DividerCanBeCarvedX(Divider divider)
    {
        return !((divider.x2 - divider.x1) < (roomMaxSize * 2));
    }

    private bool DividerCanBeCarvedY(Divider divider)
    {
        return !((divider.y2 - divider.y1) < (roomMaxSize * 2));
    }

    private void OnDrawGizmos()
    {

        foreach (var item in dividerList)
        {
            for (int x = item.x1; x < item.x2 - 1; x++)
            {
                for (int y = item.y1; y < item.y2 - 1; y++)
                {
                    if (x == item.x1 || x == item.x2 - 2 || y == item.y1 || y == item.y2 - 2)
                    {
                        Gizmos.DrawCube(new Vector3(x, 0, y), Vector3.one);
                    }
                }
            }
        }
    }
}



