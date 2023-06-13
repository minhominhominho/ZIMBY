using System;
using UnityEngine;

public class Item
{
	private string name;
	public Sprite sprite;

	public Item(string name, Sprite sprite)
	{
		this.name = name;
		this.sprite = sprite;
	}
}
