using System;
using UnityEngine;

public class FurnitureLocation
{
	protected bool isGarden = false;
	public bool IsGarden() { return isGarden; }
	public int locationId { get; }
	public int itemId { get; }
	public int direction { get; }

	public FurnitureLocation(int locationId, int itemId, int direction)
	{
		this.locationId = locationId;
		this.itemId = itemId;
		this.direction = direction;
	}
}