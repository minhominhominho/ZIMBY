using System;
using UnityEngine;

/// <summary>
/// 가구 회전 시 Collider Size
/// 오직 D, R 방향만 가지고 있음
/// </summary>
public class ColliderSize
{
	// 0 = D, 1 = R
	private Vector2[] sizes = new Vector2[2];
	private Vector2[] offsets = new Vector2[2];

	public ColliderSize(params float[] nums)
	{
		Debug.Assert(nums.Length == 8, "Invalid params in ColliderSize");

		for(int i=0; i<2; i++)
		{
			int bases = i * 4;
			sizes[i] = new(nums[bases], nums[bases + 1]);
			offsets[i] = new(nums[bases + 2], nums[bases + 3]);
		}
	}

	public Vector2 GetSize(int isPerpendicular)
	{
		return sizes[isPerpendicular];
	}

	public Vector2 GetOffset(int isPerpendicular)
	{
		return offsets[isPerpendicular];
	}
}

