using System;
using UnityEngine;

public class Furniture : Item
{
    private int x;
    private int y;
    private ColliderSize colliderSize;

    public Furniture(string name, string description, int x, int y, ColliderSize colliderSize) : base(name, description)
    {
        this.x = x;
        this.y = y;
        this.colliderSize = colliderSize;
    }

    public int[] GetSize()
    {
        int[] size = { x, y };
        return size;
    }

    //
    public Vector2 GetColliderSize(int isPerpendicular)
    {
        return colliderSize.GetSize(isPerpendicular);
    }

    public Vector2 GetColliderOffset(int isPerpendicular)
    {
        return colliderSize.GetOffset(isPerpendicular);
    }
}

