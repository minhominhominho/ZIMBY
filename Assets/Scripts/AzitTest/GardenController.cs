using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GardenController : MonoBehaviour
{
    public SpriteRenderer seedSprite;

    private ResourceManager rm = ResourceManager.GetInstance();
    private GardenLocation location;

    public void PlantSeed(int seedId)
    {
        this.location.seedId = seedId;
        this.location.age = 0;
        seedSprite.sprite = rm.GetSeed(seedId.ToString())[0];
    }

    public void SetGarden(GardenLocation location)
    {
        this.location = location;
        if(location.seedId != -1)
        {
            seedSprite.sprite = rm.GetSeed(location.seedId.ToString())[location.age];
        }
    }
}