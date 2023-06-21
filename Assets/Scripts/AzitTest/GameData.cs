using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameData
{
    // Singleton
    private static GameData instance = new GameData();

    private GameData() {
        InitItem();
        ForTest();
    }

    public static GameData GetInstance() { return instance; }

    // DB
    public Item[] items = new Item[10000];
    public Recipe[] recipes = new Recipe[10000];

    // InGame Data
    private long day = 0;
    private float satisfaction = 0f;
    private int[] inventory = new int[10000];
    private bool[] learnedRecipes = new bool[10000];

    private FurnitureLocation[] furnitureList = new FurnitureLocation[FixedValues.MAX_FURNITURE];
    private int furnitureLocationId = 0;
    private FurnitureLocation[,] interior = new FurnitureLocation[60, 40];
    private List<GardenLocation> gardenList = new List<GardenLocation>();

    public int[] GetInventory()
    {
        return inventory;
    }

    public bool[] GetLearnedRecipes()
    {
        return learnedRecipes;
    }

    public void GoodMorning()
    {
        day++;

        // Garden
        foreach(GardenLocation location in gardenList) {
            location.GetOld();
        }
    }

    public void DoAllFurnitureLocation(Action<int, int, FurnitureLocation> action)
    {
        for(int i=0; i<interior.GetLength(0); i++)
        {
            for(int j=0; j < interior.GetLength(1); j++)
            {
                if (interior[i,j] != null)
                {
                    action(i, j, interior[i, j]);
                }
            }
        }
    }

    private void InitItem()
    {
        items[1000] = new Furniture("Wood Chair", "Wood Chair", 1, 1, new(.9f, .9f, 0f, -.5f, .9f, .9f, 0f, -.5f));
        recipes[1000] = new(6001, 5, 6000, 3);
        items[1001] = new Furniture("Wood Table", "Wood Table", 3, 2, new(2.9f, .9f, 0f, -.5f, 1.9f, 1.9f, 0f, -.5f));
        recipes[1001] = new(6000, 5, 6001, 3);
        items[1002] = new Furniture("Crafting Table 1", "", 3, 2, new(2.9f, .9f, 0f, -.5f, 1.9f, 1.9f, 0f, -.5f));
        recipes[1002] = new(6000, 5, 6001, 5);
        items[1003] = new Furniture("Storage 1", "", 2, 2, new(1.9f, .9f, 0f, -.5f, .9f, .9f, 0f, -.5f));
        recipes[1003] = new(6000, 3, 6001, 3);
        items[1004] = new Furniture("Stove 1", "", 2, 3, new(1.8f, 1.8f, 0f, -.5f, 1.8f, 1.8f, 0f, -.5f));
        recipes[1004] = new(6007, 3, 6008, 3, 6009, 1);
        items[1005] = new Furniture("Garden 1", "Let's Nongsa", 3, 3, new(2.8f, 2.8f, 0f, 0f, 2.8f, 2.8f, 0f, 0f));
        recipes[1005] = new(6008, 3, 6000, 5);

        items[3000] = new("French Fries", "Delicious");
        recipes[3000] = new(5000, 3);

        items[4000] = new("Potato Seed", "");

        items[5000] = new("Potato", "");

        items[6000] = new("Wood", "");
        items[6001] = new("Iron", "");
        items[6007] = new("Iron Plate", "");
        recipes[6007] = new(6001, 3);
        items[6008] = new("Wood Plate", "");
        recipes[6008] = new(6000, 3);
        items[6009] = new("Aluminum", "");
    }

    private void ForTest() {
        inventory[1000]++;
        inventory[1004]++;
        inventory[6000] += 20;
        inventory[6001] += 20;

        learnedRecipes[1000] = true;
        learnedRecipes[1001] = true;
        learnedRecipes[1002] = true;
        learnedRecipes[1003] = true;
        learnedRecipes[1004] = true;
        learnedRecipes[1005] = true;

        learnedRecipes[3000] = true;

        learnedRecipes[6007] = true;
        learnedRecipes[6008] = true;

        // azit interior
        LocateFurniture(new(10f, 19f), 1002, 0);
        LocateFurniture(new(10f, 15.5f), 1003, 1);
    }

    public void AddItem(int what, int count)
    {
        if(count > 0)
        {
            inventory[what] = (int.MaxValue - count < inventory[what]) ? int.MaxValue : inventory[what] + count;
        } else
        {
            inventory[what] = (inventory[what] < (-1 * count)) ? 0 : inventory[what] + count;
        } 
    }

    public string GetItemType(int itemId)
    {
        string result = "";
        if (itemId / 1000 == 1) result = "Furniture";
        else if (itemId / 1000 == 2) result = "Tool";
        else if (itemId / 1000 == 3) result = "Food";
        else if (itemId / 1000 == 4) result = "Seed";
        else if (itemId / 1000 == 5) result = "Plant";
        else if (itemId / 1000 == 6) result = "Material";

        return result;
    }

    public FurnitureLocation LocateFurniture(Vector2 pos, int itemId, int direction)
    {
        int locationId = GetAvailableFurnitureLocationId();

        // if no more furniture
        if (locationId == -1) return null;

        FurnitureLocation location;
        if (IsGarden(itemId))
        {
            location = new GardenLocation(locationId, itemId, direction);
            gardenList.Add(location as GardenLocation);
        }
        else location = new(locationId, itemId, direction);

        interior[Mathf.RoundToInt(pos.x * 2), Mathf.RoundToInt(pos.y * 2)] = location;
        furnitureList[locationId] = location;
        return location;
    }

    public void removeFurniture(int locationId)
    {
        bool isFound = false;
        for (int i = 0; i < interior.GetLength(0); i++)
        {
            for (int j = 0; j < interior.GetLength(1); j++)
            {
                if (interior[i, j] != null && interior[i, j].locationId == locationId)
                {
                    if(furnitureList[locationId].IsGarden())
                    {
                        gardenList.Remove(furnitureList[locationId] as GardenLocation);
                    }

                    interior[i, j] = null;
                    furnitureList[locationId] = null;
                    isFound = true;
                    break;
                }
            }

            if (isFound) break;
        }


    }

    private int GetAvailableFurnitureLocationId()
    {
        for(int i=0; i<FixedValues.MAX_FURNITURE; i++)
        {
            if (furnitureList[i] == null) return i;
        }

        return -1;
    }

    private bool IsGarden(int itemId)
    {
        // TODO: what is garden code?
        return itemId == 1005;
    }
}
