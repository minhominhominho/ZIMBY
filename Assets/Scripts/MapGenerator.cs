using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Pool;




[Flags]
enum MapFlag : int
{
    None = 0,
    Center = 1,
    BuildingA = 2,
    BuildingB = 4,
    BuildingC = 8,
    BuildingD = 16,
    BuildingE = 32,

}

public struct MapItem
{
    public Vector3 pos;
    public bool isActive;

    public Item item;
    public GameObject gameObject;

    public int key;
    // 아이템과 상호작용할 때, key로 private Dictionary<int, MapItem> mapItems = new Dictionary<int, MapItem>()에 접근
}

public class MapGenerator : MonoBehaviour
{
    public string seed;
    private System.Random pseudoRandom;


    [Header("Map & Building")]
    public int mapSize;
    private int extendedMapSize;
    public int buildingSize;
    private int extendedBuildingSize;
    public int minBuildingGap;

    private int[,] buildingMap;
    private int buildingMapSize;

    private MapFlag[,] generatedMap;

    [Range(0, 1)] public float buildingOffset;
    [Range(0, 100)] public int buildingFrequency;


    [Header("Objects & Sprites")]
    public GameObject building;
    public GameObject land;

    private GameObject[] buildings = new GameObject[100];
    private List<GameObject> loads = new List<GameObject>();
    private Dictionary<int, MapItem> mapItems = new Dictionary<int, MapItem>();



    void Update()
    {
        if (Input.GetKeyDown(KeyCode.H))
        {
            Init();
            GenerateMap();
            DrawMap();
        }
    }

    void Init()
    {
        if (seed.Length <= 0) seed = Time.time.ToString();
        pseudoRandom = new System.Random(seed.GetHashCode());

        if (mapSize <= 0) mapSize = MapValues.MAP_SIZE;
        if (buildingSize <= 0) buildingSize = MapValues.MAP_BUILDINGSIZE;
        if (minBuildingGap < 0) minBuildingGap = MapValues.MINBUILDINGGAP;

        extendedBuildingSize = buildingSize + minBuildingGap;
        buildingMapSize = mapSize / buildingSize;
        extendedMapSize = buildingMapSize * extendedBuildingSize;
        land.transform.localScale = new Vector3(extendedMapSize, extendedMapSize, extendedMapSize);
        land.transform.position = new Vector3(extendedMapSize / 2, extendedMapSize / 2, 0);


        buildingMap = new int[buildingMapSize, buildingMapSize];
        generatedMap = new MapFlag[extendedMapSize, extendedMapSize];

        if (buildingOffset < 0) buildingOffset = 0;
        if (buildingFrequency <= 0) buildingFrequency = MapValues.MAP_BUILDINGFREQUENCY;
    }

    #region GenerateMap()
    void GenerateMap()
    {
        ReserveBuildings();
        PlaceBuildings();
    }

    void ReserveBuildings()
    {
        for (int x = 0; x < buildingMapSize; x++)
        {
            for (int y = 0; y < buildingMapSize; y++)
            {
                buildingMap[x, y] = (pseudoRandom.Next(0, 100) <= buildingFrequency) ? 1 : 0;
            }
        }
    }

    void PlaceBuildings()
    {
        for (int x = 0; x < buildingMapSize; x++)
        {
            for (int y = 0; y < buildingMapSize; y++)
            {
                if (buildingMap[x, y] == 1)
                {
                    int maxOffset = (int)(buildingOffset * buildingSize);
                    int xOffset = pseudoRandom.Next(-maxOffset, maxOffset + 1);
                    int yOffset = pseudoRandom.Next(-maxOffset, maxOffset + 1);

                    // 다른 건물과 겹치는지, 맵을 나가는지 확인
                    bool isOverllaped = false;
                    for (int i = 0; i < extendedBuildingSize; i++)
                    {
                        for (int j = 0; j < extendedBuildingSize; j++)
                        {
                            int px = x * extendedBuildingSize + xOffset + i;
                            int py = y * extendedBuildingSize + yOffset + j;

                            if (extendedMapSize <= px || 0 > px || extendedMapSize <= py || 0 > py || generatedMap[px, py] > MapFlag.None)
                            {
                                isOverllaped = true;
                                break;
                            }
                        }

                        if (isOverllaped) break;
                    }

                    // 통과했으면 건물 그리기
                    if (!isOverllaped)
                    {
                        MapFlag buildingFlag = GetRandomBuilding();
                        Debug.Log(buildingFlag);

                        for (int i = 0; i < extendedBuildingSize; i++)
                        {
                            for (int j = 0; j < extendedBuildingSize; j++)
                            {
                                int px = x * extendedBuildingSize + xOffset + i;
                                int py = y * extendedBuildingSize + yOffset + j;
                                generatedMap[px, py] = buildingFlag;
                            }
                        }

                        generatedMap[x * extendedBuildingSize + extendedBuildingSize / 2 + xOffset, y * extendedBuildingSize + extendedBuildingSize / 2 + yOffset] = MapFlag.Center | buildingFlag;
                    }
                }
            }
        }
    }

    MapFlag GetRandomBuilding()
    {
        return (MapFlag)((int)MapFlag.BuildingA << pseudoRandom.Next(0, MapValues.MAP_BUILDINGNUM));
    }

    #endregion


    #region DrawMap()
    void DrawMap()
    {
        for (int i = 0; i < 100; i++)
        {
            if (buildings[i] != null) buildings[i].SetActive(false);
        }

        int buildingCount = 0;

        if (generatedMap != null)
        {
            for (int x = 0; x < extendedMapSize; x++)
            {
                for (int y = 0; y < extendedMapSize; y++)
                {
                    if ((generatedMap[x, y] & MapFlag.Center) == true)
                    {
                        if (buildings[buildingCount] == null)
                        {
                            building.transform.localScale = new Vector3(buildingSize, buildingSize, buildingSize);
                            buildings[buildingCount] = Instantiate(building, new Vector3(x, y, 0), Quaternion.identity, this.gameObject.transform);
                        }
                        else
                        {
                            buildings[buildingCount].transform.position = new Vector3(x, y, 0);
                            buildings[buildingCount].transform.localScale = new Vector3(buildingSize, buildingSize, buildingSize);
                        }

                        buildings[buildingCount].SetActive(true);
                        buildingCount++;
                    }
                }
            }
        }
    }
    #endregion
}
