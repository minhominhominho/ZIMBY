using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FurnitureController : MonoBehaviour
{
    private GameData gameData = GameData.GetInstance();
    private int id;
    // DLUR 
    private int direction = 0;
    private SpriteRenderer sr;
    private BoxCollider2D col;
    private Sprite[] sheet;

    private void Awake()
    {
        id = int.Parse(name.Substring(0, 4));
        sr = GetComponent<SpriteRenderer>();
        col = GetComponent<BoxCollider2D>();
        sheet = Resources.LoadAll<Sprite>("Furnitures/" + id);
    }

    public void Rotate()
    {
        SetDirection((direction + 1) % 4);
    }

    public void SetDirection(int direction)
    {
        Debug.Assert((direction >= 0 && direction < 4), "invalid funitrue direction");
        this.direction = direction;

        // sprite
        sr.sprite = sheet[direction];

        // collider
        Furniture furniture = gameData.items[id] as Furniture;
        int[] size = furniture.GetSize();

        bool isPerpendicular = direction % 2 == 1;
        Vector2 newSize = isPerpendicular ? new(size[1], size[0]) : new(size[0], size[1]);
        Vector2 newColSize = new(newSize.x - 0.1f, newSize.y - 0.1f);
        Vector2 newColOffset = new((newSize.x - 1f) / 2, ((newSize.y - 1f) / 2) * -1);

        col.size = newColSize;
        col.offset = newColOffset;
    }

    public int GetDirection()
    {
        return direction;
    }
}
