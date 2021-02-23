using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Grid : MonoBehaviour
{
	//Singleton
	public static Grid instance;
	public LayerMask unwalkableMask;
	public Vector2 gridWorldSize;
	public float nodeRadius;
	public Node[,] grid;

	float nodeDiameter;
	public int gridSizeX, gridSizeY;

	public bool gizmo = false;

	void Awake()
	{
		instance = this;
		//nodeDiameter = nodeRadius * 2;
		//gridSizeX = Mathf.RoundToInt(gridWorldSize.x / nodeDiameter);
		//gridSizeY = Mathf.RoundToInt(gridWorldSize.y / nodeDiameter);
		//CreateGrid();
	}

    //private void LateUpdate()
    //{
	//	CreateGrid();
	//}

	//public void CreateGrid()
	//{
	//	Vector3 worldBottomLeft = transform.position - Vector3.right * gridWorldSize.x / 2 - Vector3.forward * gridWorldSize.y / 2;
	//
	//	for (int x = 0; x < gridSizeX; x++)
	//	{
	//		for (int y = 0; y < gridSizeY; y++)
	//		{
	//			Vector3 worldPoint = worldBottomLeft + Vector3.right * (x * nodeDiameter + nodeRadius) + Vector3.forward * (y * //nodeDiameter + nodeRadius);
	//			bool walkable = !(Physics.CheckSphere(worldPoint, nodeRadius, unwalkableMask));
	//			grid[x, y] = new Node(walkable, worldPoint, x, y);
	//		}
	//	}
	//}

	public List<Node> GetNeighbours(Node node)
	{
		List<Node> neighbours = new List<Node>();

		for (int x = -1; x <= 1; x++)
		{
			for (int y = -1; y <= 1; y++)
			{
				if (x == 0 && y == 0)
					continue;

				int checkX = node.gridX + x;
				int checkY = node.gridY + y;

				if (checkX >= 0 && checkX < gridSizeX && checkY >= 0 && checkY < gridSizeY)
				{
					neighbours.Add(grid[checkX, checkY]);
				}
			}
		}

		return neighbours;
	}

	public Node NodeFromWorldPoint(Vector3 worldPosition)
	{
		return grid[(int)worldPosition.x, (int)worldPosition.z];
	}

	public List<Node> path;

	void OnDrawGizmos()
	{
        if (gizmo)
        {
			for (int x = 0; x < gridSizeX; x++)
			{
				for (int y = 0; y < gridSizeY; y++)
				{
					if (grid[x, y].walkable)
					{
						Gizmos.color = Color.white;
						Gizmos.DrawWireCube(new Vector3(x, 0, y), Vector3.one);
					}
					else
					{
						Gizmos.color = Color.black;
						Gizmos.DrawWireCube(new Vector3(x, 0, y), Vector3.one);
					}
				}
			}
		}


		/*
		Gizmos.DrawWireCube(transform.position, new Vector3(gridWorldSize.x, 1, gridWorldSize.y));

		if (grid != null)
		{
			foreach (Node n in grid)
			{
				if (path != null)
                {
					if (path.Contains(n))
					{
						Gizmos.color = Color.green;
						Gizmos.DrawCube(n.worldPosition, Vector3.one * (nodeDiameter));
					}
				}
			}
		}
		*/
	}
}