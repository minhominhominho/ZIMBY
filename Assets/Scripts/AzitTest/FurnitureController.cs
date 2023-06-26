using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FurnitureController : MonoBehaviour
{
    private Furniture furniture;
    private FurnitureLocation location;
    private SpriteRenderer sr;
    private BoxCollider2D col;
    private Sprite[] sheet;
    private ResourceManager rm = ResourceManager.GetInstance();

    private void Awake()
    {
        int id = int.Parse(name.Substring(0, 4));
        furniture = GameData.GetInstance().items[id] as Furniture;
        sr = GetComponent<SpriteRenderer>();
        col = GetComponent<BoxCollider2D>();
        sheet = rm.GetSpriteSheet("Furnitures/" + id);
    }

    public FurnitureLocation GetLocation()
    {
        return this.location;
    }

    public void SetLocation(FurnitureLocation location)
    {
        this.location = location;
    }

    public void Rotate()
    {
        SetDirection((location.Direction + 1) % 4);
    }

    public void SetDirection(int direction)
    {
        Debug.Assert((direction >= 0 && direction < 4), "invalid funitrue direction");
        this.location.Direction = direction;

        // sprite
        sr.sprite = sheet[direction];

        // collider
        int isPerpendicular = direction % 2;

        col.size = furniture.GetColliderSize(isPerpendicular);
        col.offset = furniture.GetColliderOffset(isPerpendicular);
    }

    public int GetDirection()
    {
        return location.Direction;
    }

    public void OnHoverEnter()
    {
        sr.color = Color.green;
    }

    public void OnHoverExit()
    {
        Debug.Log("HOVER EXIT");
        sr.color = Color.white;
    }
}
