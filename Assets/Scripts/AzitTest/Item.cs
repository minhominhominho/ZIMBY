using System;
using UnityEngine;

public class Item
{
	private string name;
	public string GetName()
	{
		return name;
	}

	public string description { get; }

	public Item(string name, string description)
	{
		this.name = name;
		this.description = description;
	}
}
