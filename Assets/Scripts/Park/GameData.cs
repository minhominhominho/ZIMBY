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

    private int[] inventory = new int[10000];

    public int[] GetInventory()
    {
        return inventory;
    }

    private void InitItem()
    {
        items[1000] = new("나무의자");
        items[6000] = new("나무조각");
        items[6001] = new("철조각");
    }

    private void ForTest() {
        inventory[1000]++;
        inventory[6000]++;
        inventory[6001] += 3;
    }

}
