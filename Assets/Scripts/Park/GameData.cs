using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameData
{
    private static GameData instance = new GameData();

    private GameData() {
        InitItem();
        ForTest();
    }

    public static GameData GetInstance() { return instance; }

    public Item[] items = new Item[10000];
    public Recipe[] recipes = new Recipe[10000];

    private int[] inventory = new int[10000];
    private bool[] learnedRecipes = new bool[10000];

    public int[] GetInventory()
    {
        return inventory;
    }

    public bool[] GetLearnedRecipes()
    {
        return learnedRecipes;
    }

    private void InitItem()
    {
        int[,] a = { { 1, 3 }, { 2, 4 } };
        items[1000] = new("Wood Chair");
        recipes[1000] = new(6001, 5, 6000, 3);
        items[6000] = new("Wood");
        items[6001] = new("Iron");
    }

    private void ForTest() {
        inventory[1000]++;
        inventory[6000]++;
        inventory[6001] += 3;

        learnedRecipes[1000] = true;
    }

    public void addItem(int what, int count)
    {
        if(count > 0)
        {
            inventory[what] = (int.MaxValue - count < inventory[what]) ? int.MaxValue : inventory[what] + count;
        } else
        {
            inventory[what] = (inventory[what] < (-1 * count)) ? 0 : inventory[what] + count;
        } 
    }
}
