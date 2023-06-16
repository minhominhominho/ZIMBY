using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Furniture : MonoBehaviour
{
    private int id;
    // DLUR 
    private int direction = 0;
    private SpriteRenderer sr;
    private Sprite[] sheet;

    private void Awake()
    {
        id = int.Parse(name.Substring(0, 4));
        sr = GetComponent<SpriteRenderer>();
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

        Debug.Log(sheet == null);
        sr.sprite = sheet[direction];
        Debug.Log(sr.sprite);
    }

    public int GetDirection()
    {
        return direction;
    }
}
