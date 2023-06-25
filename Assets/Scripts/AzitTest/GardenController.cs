using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GardenController : MonoBehaviour
{
    public SpriteRenderer seedSprite;
    public GameObject dropPrefab;

    private ResourceManager rm = ResourceManager.GetInstance();
    private GameData gameData = GameData.GetInstance();
    private GardenLocation location;

    public void PlantSeed(int seedId)
    {
        this.location.seedId = seedId;
        this.location.age = 0;
        RefreshSeedSprite();
    }

    public void SetGarden(GardenLocation location)
    {
        this.location = location;
        RefreshSeedSprite();
    }

    public void ResetGarden()
    {
        this.location.seedId = -1;
        this.location.age = -1;
        RefreshSeedSprite();
    }

    private void RefreshSeedSprite()
    {
        if (location.seedId == -1) seedSprite.sprite = null;
        else seedSprite.sprite = rm.GetSeed(location.seedId.ToString())[location.age];
    }

    public int GetPlantAge()
    {
        return location.age;
    }

    public void Harvest()
    {
        gameData.AddItem(location.seedId + 1000, 1);
        SpawnDrop();
        ResetGarden();
    }

    private void SpawnDrop()
    {
        GameObject drop = Instantiate<GameObject>(dropPrefab, transform);
        drop.transform.GetChild(0).GetComponent<SpriteRenderer>().sprite = rm.GetIcon((location.seedId + 1000).ToString());
        drop.transform.GetChild(1).GetComponent<SpriteRenderer>().sprite = rm.GetIcon((location.seedId + 1000).ToString());
    }
}