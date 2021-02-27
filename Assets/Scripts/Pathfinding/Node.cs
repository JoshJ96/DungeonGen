using UnityEngine;
using System.Collections;

public class Node
{
	public bool visibleOnMap;
	public Room roomPartOf;
	public bool walkable;
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

	public bool NodeRoomIsVisible() => roomPartOf.visibleOnMap;
}
