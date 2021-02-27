using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

public class Grid : MonoBehaviour
{
	#region Singleton
	public static Grid instance;
	void Awake() => instance = this;
	#endregion

	public Node[,] grid;
	public int gridSizeX, gridSizeY;
	public bool gizmo = false;
	public List<Node> path;
	public Transform seeker, target;
	public Color gridColorNormal, gridColorPlayer;

    private void Start()
    {
		GameEvents.instance.pushDungeonData += PushDungeonData;
	}

    private void PushDungeonData(DungeonData data)
    {
		grid = data.map;
    }

    public void FindPath(Node start, Node target)
	{
		List<Node> openSet = new List<Node>();
		HashSet<Node> closedSet = new HashSet<Node>();
		openSet.Add(start);

		while (openSet.Count > 0)
		{
			Node node = openSet[0];
			for (int i = 1; i < openSet.Count; i++)
			{
				if (openSet[i].fCost < node.fCost || openSet[i].fCost == node.fCost)
				{
					if (openSet[i].hCost < node.hCost)
						node = openSet[i];
				}
			}

			openSet.Remove(node);
			closedSet.Add(node);

			if (node == target)
			{
				RetracePath(start, target);
				return;
			}

			foreach (Node neighbour in GetNeighbours(node))
			{
				if (!neighbour.walkable || closedSet.Contains(neighbour))
				{
					continue;
				}

				int newCostToNeighbour = node.gCost + GetDistance(node, neighbour);
				if (newCostToNeighbour < neighbour.gCost || !openSet.Contains(neighbour))
				{
					neighbour.gCost = newCostToNeighbour;
					neighbour.hCost = GetDistance(neighbour, target);
					neighbour.parent = node;

					if (!openSet.Contains(neighbour))
						openSet.Add(neighbour);
				}
			}
		}
	}

	void RetracePath(Node startNode, Node endNode)
	{
		List<Node> retracedPath = new List<Node>();
		Node currentNode = endNode;

		while (currentNode != startNode)
		{
			retracedPath.Add(currentNode);
			currentNode = currentNode.parent;
		}
		retracedPath.Reverse();

		path = retracedPath;
	}

	int GetDistance(Node nodeA, Node nodeB)
	{
		int dstX = Mathf.Abs(nodeA.gridX - nodeB.gridX);
		int dstY = Mathf.Abs(nodeA.gridY - nodeB.gridY);

		if (dstX > dstY)
			return 14 * dstY + 10 * (dstX - dstY);
		return 14 * dstX + 10 * (dstY - dstX);
	}

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

	static Material lineMaterial;
	static void CreateLineMaterial()
	{
		if (!lineMaterial)
		{
			// Unity has a built-in shader that is useful for drawing
			// simple colored things.
			Shader shader = Shader.Find("Hidden/Internal-Colored");
			lineMaterial = new Material(shader);
			lineMaterial.hideFlags = HideFlags.HideAndDontSave;
			// Turn on alpha blending
			lineMaterial.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
			lineMaterial.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
			// Turn backface culling off
			lineMaterial.SetInt("_Cull", (int)UnityEngine.Rendering.CullMode.Off);
			// Turn off depth writes
			lineMaterial.SetInt("_ZWrite", 0);
		}
	}

	public void OnRenderObject()
	{
		CreateLineMaterial();
		// Apply the line material
		lineMaterial.SetPass(0);

		GL.PushMatrix();
		//Set transformation matrix for drawing to
		//match our transform
		GL.MultMatrix(transform.localToWorldMatrix);
		//Draw grid cells
		for (float x = (int)PlayerUnit.instance.transform.position.x - 15; x < (int)PlayerUnit.instance.transform.position.x + 15; x++)
		{
			for (float y = (int)PlayerUnit.instance.transform.position.z - 15; y < (int)PlayerUnit.instance.transform.position.z + 15; y++)
			{
                if (NodeFromWorldPoint(new Vector3(x,0,y)).walkable)
                {
					GL.Begin(GL.LINES);
					GL.Color(gridColorNormal);
					GL.Vertex3(x + 0.49f, 0, y + 0.49f);
					GL.Vertex3(x + 0.49f, 0, y - 0.49f);
					GL.Vertex3(x + 0.49f, 0, y - 0.49f);
					GL.Vertex3(x - 0.49f, 0, y - 0.49f);
					GL.Vertex3(x - 0.49f, 0, y - 0.49f);
					GL.Vertex3(x - 0.49f, 0, y + 0.49f);
					GL.Vertex3(x - 0.49f, 0, y + 0.49f);
					GL.Vertex3(x + 0.49f, 0, y + 0.49f);
				}
				if (PlayerUnit.instance.GetCurrentNode().GetWorldPoint() == new Vector3(x, 0, y))
				{
					GL.Begin(GL.LINES);
					GL.Color(gridColorPlayer);
					GL.Vertex3(x + 0.49f, 0, y + 0.49f);
					GL.Vertex3(x + 0.49f, 0, y - 0.49f);
					GL.Vertex3(x + 0.49f, 0, y - 0.49f);
					GL.Vertex3(x - 0.49f, 0, y - 0.49f);
					GL.Vertex3(x - 0.49f, 0, y - 0.49f);
					GL.Vertex3(x - 0.49f, 0, y + 0.49f);
					GL.Vertex3(x - 0.49f, 0, y + 0.49f);
					GL.Vertex3(x + 0.49f, 0, y + 0.49f);
				}
			}
		}
		GL.End();
		GL.PopMatrix();
	}
}