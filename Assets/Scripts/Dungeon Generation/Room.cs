using UnityEngine;

public class Room
{
    public int mapX, mapY, width, height;
    public Vector2 center = new Vector2();
    public Room(int _x, int _y, int _width, int _height)
    {
        this.mapX = _x;
        this.mapY = _y;
        this.width = _width;
        this.height = _height;

        //Construct the center
        this.center.x = _x + (_width / 2);
        this.center.y = _y + (_height / 2);
    }
}
