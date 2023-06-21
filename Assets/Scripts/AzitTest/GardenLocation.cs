using System;

public class GardenLocation : FurnitureLocation
{
    public int seedId { get; set; } = -1;
    public int age { get; set; } = -1;

    public GardenLocation(int locationId, int itemId, int direction) : base(locationId, itemId, direction)
    {
        this.isGarden = true;
    }

    public void GetOld()
    {
        if (this.age == -1 || this.age == 3) return;
        this.age++;
    }
}

