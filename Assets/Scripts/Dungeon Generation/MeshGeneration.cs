using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MeshGeneration : MonoBehaviour
{
    MeshFilter meshFilter;
    Vector2[] UVArray;
    Mesh mesh;

    //For this, your GameObject this script is attached to would have a
    //Transform Component, a Mesh Filter Component, and a Mesh Renderer
    //component. You will also need to assign your texture atlas / material
    //to it. 

    void Start()
    {
        meshFilter = GetComponent<MeshFilter>();
    }

    public void BuildMesh(int mapSizeX, int mapSizeY)
    {
        int numTiles = mapSizeX * mapSizeY;
        int numTriangles = numTiles * 6;
        int numVerts = numTiles * 8;

        Vector3[] vertices = new Vector3[numVerts];
        UVArray = new Vector2[numVerts];

        int x, y, iVertCount = 0;
        for (x = 0; x < mapSizeX; x++)
        {
            for (y = 0; y < mapSizeY; y++)
            {
                if (!Grid.instance.grid[x,y].walkable)
                {
                    vertices[iVertCount + 0] = new Vector3(x, 0, y);
                    vertices[iVertCount + 1] = new Vector3(x + 1, 0, y);
                    vertices[iVertCount + 2] = new Vector3(x + 1, 0, y + 1);
                    vertices[iVertCount + 3] = new Vector3(x, 0, y + 1);

                    vertices[iVertCount + 4] = new Vector3(x, 1, y + 1);
                    vertices[iVertCount + 5] = new Vector3(x + 1, 1, y + 1);
                    vertices[iVertCount + 6] = new Vector3(x + 1, 1, y);
                    vertices[iVertCount + 7] = new Vector3(x, 1, y);

                    iVertCount += 8;
                }


            }
        }

        int[] triangles = new int[numTriangles];

        int iIndexCount = 0; iVertCount = 0;
        for (int i = 0; i < numTiles; i++)
        {
            triangles[iIndexCount + 0] += (iVertCount + 0);
            triangles[iIndexCount + 1] += (iVertCount + 1);
            triangles[iIndexCount + 2] += (iVertCount + 2);
            triangles[iIndexCount + 3] += (iVertCount + 0);
            triangles[iIndexCount + 4] += (iVertCount + 2);
            triangles[iIndexCount + 5] += (iVertCount + 3);

            iVertCount += 4; iIndexCount += 6;
        }

        mesh = new Mesh();
        //mesh.MarkDynamic(); if you intend to change the vertices a lot, this will help.
        mesh.vertices = vertices;
        mesh.triangles = triangles;
        meshFilter.mesh = mesh;

        UpdateMesh(mapSizeX, mapSizeY); //I put this in a separate method for my own purposes.
    }


    //Note, the example UV entries I have are assuming a tile atlas 
    //with 16 total tiles in a 4x4 grid.

    public void UpdateMesh(int mapSizeX, int mapSizeY)
    {
        int iVertCount = 0;

        for (int x = 0; x < mapSizeX; x++)
        {
            for (int y = 0; y < mapSizeY; y++)
            {
                UVArray[iVertCount + 0] = new Vector2(0, 0); //Top left of tile in atlas
                UVArray[iVertCount + 1] = new Vector2(.25f, 0); //Top right of tile in atlas
                UVArray[iVertCount + 2] = new Vector2(.25f, .25f); //Bottom right of tile in atlas
                UVArray[iVertCount + 3] = new Vector2(0, .25f); //Bottom left of tile in atlas
                iVertCount += 4;
            }
        }

        meshFilter.mesh.uv = UVArray;
    }
}
