using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;


public class MapGenerator : MonoBehaviour
{
    private System.Random pseudoRandom;
    private int[,] placedBuildingMap;
    private int[,] reservedBuildingMap;
    private int buildingWidth;
    private int buildingHeight;

    public string seed;
    public int width;
    public int height;
    public int buildingSize;
    public int buildingOffset;
    [Range(0, 100)] public int buildingFrequency;



    void Update()
    {
        SetMapGenerator();
        GenerateMap();
    }

    void SetMapGenerator()
    {
        if (seed.Length <= 0) seed = Time.time.ToString();
        pseudoRandom = new System.Random(seed.GetHashCode());

        if (width <= 0) width = MapValues.MAP_WIDTH;
        if (height <= 0) width = MapValues.MAP_HEIGHT;
        if (buildingSize <= 0) buildingSize = MapValues.MAP_BUILDINGSIZE;

        buildingWidth = width / buildingSize;
        buildingHeight = height / buildingSize;

        reservedBuildingMap = new int[buildingWidth, buildingHeight];
        placedBuildingMap = new int[width, height];

        if (buildingFrequency <= 0) buildingFrequency = MapValues.MAP_BUILDINGFREQUENCY;
    }

    void GenerateMap()
    {
        ReserveBuildings();
        PlaceBuildings();
    }

    void ReserveBuildings()
    {
        for (int x = 0; x < buildingWidth; x++)
        {
            for (int y = 0; y < buildingHeight; y++)
            {
                reservedBuildingMap[x, y] = (pseudoRandom.Next(0, 100) < buildingFrequency) ? 1 : 0;
            }
        }
    }

    void PlaceBuildings()
    {
        for (int x = 0; x < buildingWidth; x++)
        {
            for (int y = 0; y < buildingHeight; y++)
            {
                if (reservedBuildingMap[x, y] == 1)
                {
                    int xOffset = pseudoRandom.Next(-buildingOffset, buildingOffset + 1);
                    int yOffset = pseudoRandom.Next(-buildingOffset, buildingOffset + 1);

                    // 안겹치나, 안나가나 테스트
                    bool test = true;
                    for (int i = xOffset; i < buildingSize + xOffset; i++)
                    {
                        for (int j = yOffset; j < buildingSize + yOffset; j++)
                        {
                            int px = x * buildingSize + i;
                            int py = y * buildingSize + j;

                            if (width <= px || 0 > px || height <= py || 0 > py)
                            {
                                test = false;
                                break;
                            }

                            if(placedBuildingMap[px, py] == 1)
                            {
                                test = false;
                                break;
                            }

                            placedBuildingMap[px, py] = 1;
                        }

                        if (!test) break;
                    }

                    // 테스트 통과했으면 집그리기
                    if (test)
                    {
                        for (int i = xOffset; i < buildingSize + xOffset; i++)
                        {
                            for (int j = yOffset; j < buildingSize + yOffset; j++)
                            {
                                int px = x * buildingSize + i;
                                int py = y * buildingSize + j;
                                placedBuildingMap[px, py] = 2;
                            }
                        }
                    }
                }
            }
        }
    }



    void OnDrawGizmos()
    {
        if (placedBuildingMap != null)
        {
            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    Gizmos.color = (placedBuildingMap[x, y] == 2) ? Color.blue : Color.black;
                    Vector3 pos = new Vector3(x + .5f, y + .5f, 0);
                    Gizmos.DrawCube(pos, Vector3.one);
                }
            }
        }
    }
}
