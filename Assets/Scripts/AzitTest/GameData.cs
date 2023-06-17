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
    private long day = 1;
    private int[] inventory = new int[10000];
    private bool[] learnedRecipes = new bool[10000];
    private List<FurnitureLocation> azitInterior = new List<FurnitureLocation>();

    public int[] GetInventory()
    {
        return inventory;
    }

    public bool[] GetLearnedRecipes()
    {
        return learnedRecipes;
    }

    public List<FurnitureLocation> GetAzitInterior()
    {
        return azitInterior;
    }

    private void InitItem()
    {
        items[1000] = new Furniture("Wood Chair", 1, 1, new(.9f, .9f, 0f, -.5f, .9f, .9f, 0f, -.5f));
        recipes[1000] = new(6001, 5, 6000, 3);
        items[1001] = new Furniture("Wood Table", 3, 2, new(2.9f, .9f, 0f, -.5f, 1.9f, 1.9f, 0f, -.5f));
        recipes[1001] = new(6000, 5, 6001, 3);
        items[1002] = new Furniture("Crafting Table 1", 3, 2, new(2.9f, .9f, 0f, -.5f, 1.9f, 1.9f, 0f, -.5f));
        recipes[1002] = new(6000, 5, 6001, 5);
        items[1003] = new Furniture("Storage 1", 2, 2, new(1.9f, .9f, 0f, -.5f, .9f, .9f, 0f, -.5f));
        recipes[1003] = new(6000, 3, 6001, 3);
        items[6000] = new("Wood");
        items[6001] = new("Iron");
    }

    private void ForTest() {
        inventory[1000]++;
        inventory[6000] += 20;
        inventory[6001] += 20;

        learnedRecipes[1000] = true;
        learnedRecipes[1001] = true;
        learnedRecipes[1002] = true;
        learnedRecipes[1003] = true;

        // azit interior
        azitInterior.Add(new(1002, new(-3f, 3f, 0f), 0));
        azitInterior.Add(new(1003, new(0f, 5f, 0f), 0));
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

    public void LocateFurniture(FurnitureLocation fl)
    {
        this.azitInterior.Add(fl);
    }
}
