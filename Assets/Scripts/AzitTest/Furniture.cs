using System;
using UnityEngine;

public class Furniture : Item
{
    private int x;
    private int y;
    private ColliderSize colliderSize;
    private float satRate;
    public float SatRate
    {
        get { return satRate; }
    }

    public Furniture(string name, string description, float satRate, int x, int y, ColliderSize colliderSize) : base(name, description)
    {
        this.satRate = satRate;
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

