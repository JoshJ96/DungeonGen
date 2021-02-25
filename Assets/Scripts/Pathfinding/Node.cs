using UnityEngine;
using System.Collections;

public class Node
{

	public bool walkable;
	public Vector3 worldPosition;
	public int gridX;
	public int gridY;

	public int gCost;
	public int hCost;
	public Node parent;

	public int fCost
	{
		get
		{
			return gCost + hCost;
		}
	}

	public Vector3 GetWorldPoint() => new Vector3(gridX, 0, gridY);
}
