using System;
using UnityEngine;

public class Food : Item
{
    private float satisfaction;
    public float Satisfaction
    {
        get { return satisfaction; }
    }

    public Food(string name, string description, float satisfaction) : base(name, description)
    {
        this.satisfaction = satisfaction;
    }
}

