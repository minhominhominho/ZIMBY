using UnityEngine;
using System.Collections;
using static UnityEditor.Experimental.AssetDatabaseExperimental.AssetDatabaseCounters;

public class Recipe
{
	private int[,] materials;

	public Recipe(params int[] materialArr)
	{
        materials = new int[materialArr.Length / 2, 2];

        for (int i=0; i<materialArr.Length; i += 2)
		{
			materials[i / 2, 0] = materialArr[i];
			materials[i / 2, 1] = materialArr[i + 1];
		}
	}

	public int[,] getMaterials() { return materials; }
}

