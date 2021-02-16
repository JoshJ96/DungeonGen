using UnityEngine;

public class Tile
{
    public bool blockSight;
    public State tileType;
    public enum State
    {
        Wall,
        Floor
    }
}
