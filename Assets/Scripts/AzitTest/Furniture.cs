using System;

public class Furniture : Item
{
    private int x;
    private int y;

    public Furniture(string name, int x, int y) : base(name)
    {
        this.x = x;
        this.y = y;
    }

    public int[] GetSize()
    {
        int[] size = { x, y };
        return size;
    }
}

