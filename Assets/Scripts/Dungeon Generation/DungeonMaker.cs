using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class DungeonDivider
{
    public DungeonDivider(DungeonDivider _parent, int _x1, int _x2, int _y1, int _y2, int _depth)
    {
        parent = _parent;
        x1 = _x1;
        x2 = _x2;
        y1 = _y1;
        y2 = _y2;
        depth = _depth;
    }
    public DungeonDivider()
    {

    }

    public DungeonDivider parent;
    public DungeonDivider leftChildNode, rightChildNode;
    public int depth;
    public int x1, x2, y1, y2;
    public Color gizmoColor;
}

public class DungeonMaker : MonoBehaviour
{
    DungeonDivider root;

    //Room size properties
    [Range(20, 1000)]
    public int mapWidth;
    [Range(20, 1000)]
    public int mapHeight;

    [Range(5, 30)]
    public int roomMinSize;
    [Range(5, 30)]
    public int roomMaxSize;



    //Split position percentages when dividing areas
    [Range(0, 1)]
    public float splitPercentMin = 0.45f;
    [Range(0, 1)]
    public float splitPercentMax = 0.55f;

    //Number of BSP divides
    public int numberOfDivides = 4;
    private List<DungeonDivider> dividers = new List<DungeonDivider>();

    private void Start()
    {
        ////Create the initial divider (full map)
        root = new DungeonDivider(null, 0, mapWidth, 0, mapHeight, 0);
        //DivideRooms(root);
        dividers.Add(root);

        //
        for (int i = 0; i < numberOfDivides; i++)
        {
            List<DungeonDivider> toDivide = dividers.Where(x => x.depth == i).ToList();
        
            if (toDivide.Count == 0)
            {
                continue;
            }
        
            foreach (var item in toDivide)
            {
                DivideRooms(item.leftChildNode);
                DivideRooms(item.rightChildNode);
            }
        }
        
        



    }

    private void Update()
    {

    }

    void DivideRooms(DungeonDivider dungeonDivider)
    {
        //Random axis
        int rng = Random.Range(0, 2);

        //X
        if (rng == 0)
        {
            if ((dungeonDivider.x2-dungeonDivider.x1) <= (roomMinSize * 2))
            {
                return;
            }
            //Find boundaries and pick a random X value
            float minBoundaryX = (dungeonDivider.x1 + dungeonDivider.x2) * splitPercentMin;
            float maxBoundaryX = (dungeonDivider.x1 + dungeonDivider.x2) * splitPercentMax;
            int randomX = (int) Random.Range(minBoundaryX, maxBoundaryX);
            print(randomX);
            //Create the right division
            DungeonDivider childRight = new DungeonDivider
            {
                parent = dungeonDivider,
                x1 = randomX,
                x2 = dungeonDivider.x2,
                y1 = dungeonDivider.y1,
                y2 = dungeonDivider.y2,
                depth = dungeonDivider.depth + 1,
            };
            
            dungeonDivider.rightChildNode = childRight;
            dividers.Add(childRight);

            //Create the left division
            DungeonDivider childLeft = new DungeonDivider
            {
                parent = dungeonDivider,
                x1 = dungeonDivider.x1,
                x2 = randomX,
                y1 = dungeonDivider.y1,
                y2 = dungeonDivider.y2,
                depth = dungeonDivider.depth + 1,
            };

            dungeonDivider.leftChildNode = childLeft;
            dividers.Add(childLeft);

        }

        //Y
        else
        {
            if ((dungeonDivider.y2 - dungeonDivider.y1) <= (roomMinSize * 2))
            {
                return;
            }
            //Find boundaries and pick a random Y value
            float minBoundaryY = (dungeonDivider.y1 + dungeonDivider.y2) * splitPercentMin;
            float maxBoundaryY = (dungeonDivider.y1 + dungeonDivider.y2) * splitPercentMax;
            int randomY = (int)Random.Range(minBoundaryY, maxBoundaryY);
            print(randomY);

            //Create the right division
            DungeonDivider childRight = new DungeonDivider
            {
                parent = dungeonDivider,
                y1 = randomY,
                y2 = dungeonDivider.y2,
                x1 = dungeonDivider.x1,
                x2 = dungeonDivider.x2,
                depth = dungeonDivider.depth + 1,
            };

            dungeonDivider.rightChildNode = childRight;
            dividers.Add(childRight);



            //Create the left division
            DungeonDivider childLeft = new DungeonDivider
            {
                parent = dungeonDivider,
                y1 = dungeonDivider.y1,
                y2 = randomY,
                x1 = dungeonDivider.x1,
                x2 = dungeonDivider.x2,
                depth = dungeonDivider.depth + 1,
            };
            dungeonDivider.leftChildNode = childLeft;
            dividers.Add(childLeft);

        }
    }


    private void OnDrawGizmos()
    {

        foreach (var item in dividers)
        {
            for (int x = item.x1; x < item.x2-1; x++)
            {
                for (int y = item.y1; y < item.y2-1; y++)
                {
                    if (x == item.x1 || x == item.x2-2 || y == item.y1 || y == item.y2-2)
                    {
                        Gizmos.DrawCube(new Vector3(x, 0, y), Vector3.one);
                    }
                }
            }
        }
    }
}