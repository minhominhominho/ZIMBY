using System;
using UnityEngine;

public class Item
{
	private string name;
	public string GetName()
	{
		return name;
	}

	public Item(string name)
	{
		this.name = name;
	}
}
