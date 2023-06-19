using System;
using System.Collections.Generic;
using UnityEngine;

public class ResourceManager
{
    private static ResourceManager instance = new();
    public void Load()
    {
        if (isReady) return;
        // Prefabs
        prefabs.Add("Prefabs/1000", Resources.Load<GameObject>("Prefabs/1000"));
        prefabs.Add("Prefabs/1001", Resources.Load<GameObject>("Prefabs/1001"));
        prefabs.Add("Prefabs/1002", Resources.Load<GameObject>("Prefabs/1002"));
        prefabs.Add("Prefabs/1003", Resources.Load<GameObject>("Prefabs/1003"));
        prefabs.Add("Prefabs/1004", Resources.Load<GameObject>("Prefabs/1004"));

        // Sprites
        sprites.Add("Icons/1000", Resources.Load<Sprite>("Icons/1000"));
        sprites.Add("Icons/1001", Resources.Load<Sprite>("Icons/1001"));
        sprites.Add("Icons/1002", Resources.Load<Sprite>("Icons/1002"));
        sprites.Add("Icons/1003", Resources.Load<Sprite>("Icons/1003"));
        sprites.Add("Icons/1004", Resources.Load<Sprite>("Icons/1004"));
        sprites.Add("Icons/3000", Resources.Load<Sprite>("Icons/3000"));
        sprites.Add("Icons/5000", Resources.Load<Sprite>("Icons/5000"));
        sprites.Add("Icons/6000", Resources.Load<Sprite>("Icons/6000"));
        sprites.Add("Icons/6001", Resources.Load<Sprite>("Icons/6001"));
        sprites.Add("Icons/6007", Resources.Load<Sprite>("Icons/6007"));
        sprites.Add("Icons/6008", Resources.Load<Sprite>("Icons/6008"));
        sprites.Add("Icons/6009", Resources.Load<Sprite>("Icons/6009"));

        // Sprite Sheets
        spriteSheets.Add("Furnitures/1000", Resources.LoadAll<Sprite>("Furnitures/1000"));
        spriteSheets.Add("Furnitures/1001", Resources.LoadAll<Sprite>("Furnitures/1001"));
        spriteSheets.Add("Furnitures/1002", Resources.LoadAll<Sprite>("Furnitures/1002"));
        spriteSheets.Add("Furnitures/1003", Resources.LoadAll<Sprite>("Furnitures/1003"));
        spriteSheets.Add("Furnitures/1004", Resources.LoadAll<Sprite>("Furnitures/1004"));

        isReady = true;
    }
    public static ResourceManager GetInstance() { return instance; }
    private ResourceManager() {}

    private bool isReady = false;

    private Dictionary<string, GameObject> prefabs = new();
    private Dictionary<string, Sprite> sprites = new();
    private Dictionary<string, Sprite[]> spriteSheets = new();

    public Sprite GetSprite(string path)
    {
        return sprites[path];
    }

    public Sprite GetIcon(string path)
    {
        return GetSprite("Icons/" + path);
    }

    public Sprite[] GetSpriteSheet(string path)
    {
        return spriteSheets[path];
    }

    public GameObject GetPrefab(string path)
    {
        return prefabs["Prefabs/" + path];
    }
}

