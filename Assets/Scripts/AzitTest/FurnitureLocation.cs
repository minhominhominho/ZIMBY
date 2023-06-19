using System;
using UnityEngine;

public class FurnitureLocation
{
	public int id { get; }
	public Vector3 pos { get; }
	public int direction { get; }

	public FurnitureLocation(int id, Vector3 pos, int direction)
	{
		this.id = id;
		this.pos = pos;
		this.direction = direction;
	}
}