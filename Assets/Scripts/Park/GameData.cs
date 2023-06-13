using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameData
{
    private static GameData instance = new GameData();

    private GameData() {}

    public static GameData GetInstance() { return instance; }

    public Item[] items = new Item[10000];

    int[] inventory = new int[10000];
}
