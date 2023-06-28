using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.ConstrainedExecution;
using System.Runtime.InteropServices;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Pool;

[Flags]
public enum MapFlag : byte
{
    None = 0,
    BuildingPlace = 1,
    BuildingPoint = 2,
    Building = 4,
    WideRoad = 8,
    MiddleRoad = 16,
}

public enum BuildingType : byte
{
    BuildingA,
    BuildingB,
    BuildingC,
    BuildingD,
    BuildingE,
    BuildingCount,
}

[Serializable]
public struct BuildingComponent
{
    public BuildingType type;
    public GameObject In;
    public GameObject UR;
    public GameObject UL;
    public GameObject DR;
    public GameObject DL;
    public GameObject U;
    public GameObject D;
    public GameObject R;
    public GameObject L;
    public GameObject Door;
}

public struct MapItem
{
    public Vector3 pos;
    public bool isActive;

    public Item item;
    public GameObject gameObject;

    public int key;
    // �����۰� ��ȣ�ۿ��� ��, key�� private Dictionary<int, MapItem> mapItems = new Dictionary<int, MapItem>()�� ����
}

public class MapGenerator : MonoBehaviour
{
    public string seed;
    private System.Random pseudoRandom;


    [Header("Map & Building")]
    public int mapSize;                             // �� ũ�� (mapSize * mapSize)
    public int minBuildingGap;                      // �ǹ� �� �ּ� ����
    public int buildingSize;                        // �ǹ� ũ��

    private int extendedMapSize;                    // �ǹ� �� �ּ� ���ݿ� ���� Ȯ��� �� ũ��
    private int extendedBuildingSize;               // �ǹ� �� �ּ� ���ݿ� ���� �������� ������ ���� �ǹ� ũ��

    private int[,] buildingMap;                     // �ǹ��� ��ġ�ϴ� ��
    private int buildingMapSize;                    // = mapSize / buildingSize

    private MapFlag[,] generatedMap;                // ���� ��

    [Range(0, 1)] public float buildingOffset;      // �ǹ��� �׸��忡�� ����� ����
    [Range(0, 100)] public int buildingFrequency;   // �ǹ� ���� �� (%)


    public int wideRoadWidth;                       // ���� ���� ��
    public int middleRoadWidth;                     // �߰� ���� ��
    public int narrowRoadWidth;                     // �ּ� ���� ��
    [Range(1, 5)] public int maxWideRoadFrequency;  // ���� ���� �ִ� ��
    [Range(1, 5)] public int maxMiddleRoadFrequency;// �߰� ���� �ִ� ��

    private Stack<Vector2Int> roads;
    private Stack<MapValues.Dir> roadsDir;

    [Header("Objects & Sprites")]
    public GameObject land;
    public BuildingComponent[] buildingComponents = new BuildingComponent[(int)BuildingType.BuildingCount];

    public GameObject roadObject;
    public GameObject roadParent;
    public GameObject buildingParent;
    public GameObject buildingPlaceParent;
    public GameObject buildingPlaceObject;
    // private List<GameObject> roads = new List<GameObject>();
    // private Dictionary<int, MapItem> mapItems = new Dictionary<int, MapItem>();



    void Update()
    {
        if (Input.GetKeyDown(KeyCode.H))
        {
            Debug.Log("Map");
            Init();
            GenerateMap();
        }
    }

    void Init()
    {
        if (seed.Length <= 0) seed = Time.time.ToString();
        pseudoRandom = new System.Random(seed.GetHashCode());

        if (mapSize <= 0) mapSize = MapValues.MAP_SIZE;
        if (buildingSize <= 0) buildingSize = MapValues.MAP_BUILDINGSIZE;
        if (minBuildingGap < 0) minBuildingGap = MapValues.MINBUILDINGGAP;

        extendedBuildingSize = buildingSize + 2 * minBuildingGap;
        buildingMapSize = mapSize / buildingSize;
        extendedMapSize = buildingMapSize * extendedBuildingSize;

        land.transform.localScale = new Vector3(extendedMapSize, extendedMapSize, extendedMapSize);
        land.transform.position = new Vector3(extendedMapSize / 2 - 0.5f, extendedMapSize / 2 - 0.5f, 0);


        buildingMap = new int[buildingMapSize, buildingMapSize];
        generatedMap = new MapFlag[extendedMapSize, extendedMapSize];

        if (buildingOffset < 0) buildingOffset = 0;
        if (buildingFrequency <= 0) buildingFrequency = MapValues.MAP_BUILDINGFREQUENCY;

        // maxWideRoadFrequency
        // wideRoadWidth;
        // middleRoadWidth
        // narrowRoadWidth
    }

    void GenerateMap()
    {
        PlaceWideRoads();
        PlaceMiddleRoads();
        ReserveBuildings();
        PlaceBuildings();
        DrawBuildings();
        DrawRoads();
    }


    // ū �� ǥ��
    void PlaceWideRoads()
    {
        int offset = extendedBuildingSize;
        int wideRoadsSlotNum = (extendedMapSize - 2 * extendedBuildingSize) / wideRoadWidth;

        for (int ver = 0; ver < 2; ver++)
        {
            int roadsNum = pseudoRandom.Next(1, maxWideRoadFrequency + 1);
            List<int> possibleWideRoads = new List<int>(wideRoadsSlotNum);
            for (int i = 0; i < wideRoadsSlotNum; i++) possibleWideRoads.Add(i);

            for (int i = 0; i < roadsNum; i++)
            {
                if (possibleWideRoads.Count == 0) break;
                int wideRoadIdx = pseudoRandom.Next(0, possibleWideRoads.Count);

                for (int j = offset + possibleWideRoads[wideRoadIdx] * wideRoadWidth; j < offset + (possibleWideRoads[wideRoadIdx] + 1) * wideRoadWidth; j++)
                {
                    for (int k = 0; k < extendedMapSize; k++)
                    {
                        if (ver == 0) generatedMap[j, k] |= MapFlag.WideRoad;
                        else generatedMap[k, j] |= MapFlag.WideRoad;
                    }
                }

                int pickedValue = possibleWideRoads[wideRoadIdx];

                for (int j = possibleWideRoads.Count - 1; j >= 0; j--)
                {
                    if (pickedValue - (extendedBuildingSize + wideRoadWidth - 1) / wideRoadWidth <= possibleWideRoads[j] && possibleWideRoads[j] <= pickedValue + (extendedBuildingSize + wideRoadWidth - 1) / wideRoadWidth)
                        possibleWideRoads.RemoveAt(j);
                }
            }
        }
    }

    // �߰� �� ǥ��
    void PlaceMiddleRoads()
    {
        List<int> possibleVer = new List<int>();
        List<int> possibleHor = new List<int>();

        // ������ �� ����Ʈ ����
        for (int i = 0; i < extendedMapSize; i++)
        {
            bool isVerPossible = true;
            bool isHorrPossible = true;

            for (int j = i - extendedBuildingSize; j < i + middleRoadWidth + extendedBuildingSize; j++)
            {
                if (j < 0 || j >= extendedMapSize || generatedMap[j, 0] == MapFlag.WideRoad)
                {
                    isVerPossible = false;
                    if (!isVerPossible && !isHorrPossible) break;
                }

                if (j < 0 || j >= extendedMapSize || generatedMap[0, j] == MapFlag.WideRoad)
                {
                    isHorrPossible = false;
                    if (!isVerPossible && !isHorrPossible) break;
                }
            }

            if (isVerPossible) possibleVer.Add(i);
            if (isHorrPossible) possibleHor.Add(i);
        }

        // ������ �� �������� ǥ��
        for (int dir = 0; dir < 4; dir++)
        {
            List<int> copiedPossible;
            if (dir == (int)MapValues.Dir.Up || dir == (int)MapValues.Dir.Down) copiedPossible = possibleVer;
            else copiedPossible = possibleHor;


            int roadsNum = pseudoRandom.Next(0, maxMiddleRoadFrequency + 1);
            for (int i = 0; i < roadsNum; i++)
            {
                if (copiedPossible.Count == 0) break;
                int middleRoadIdx = pseudoRandom.Next(0, copiedPossible.Count);

                // �� ǥ��
                for (int j = copiedPossible[middleRoadIdx]; j < copiedPossible[middleRoadIdx] + middleRoadWidth; j++)
                {
                    if (dir == (int)MapValues.Dir.Up || dir == (int)MapValues.Dir.Right)
                    {
                        for (int k = 0; k < extendedMapSize; k++)
                        {
                            if (dir == (int)MapValues.Dir.Up)
                            {
                                if (generatedMap[j, k] > MapFlag.None) break;
                                generatedMap[j, k] |= MapFlag.MiddleRoad;
                            }
                            if (dir == (int)MapValues.Dir.Right)
                            {
                                if (generatedMap[k, j] > MapFlag.None) break;
                                generatedMap[k, j] |= MapFlag.MiddleRoad;
                            }
                        }
                    }
                    else if (dir == (int)MapValues.Dir.Down || dir == (int)MapValues.Dir.Left)
                    {
                        for (int k = extendedMapSize - 1; k >= 0; k--)
                        {
                            if (dir == (int)MapValues.Dir.Down)
                            {
                                if (generatedMap[j, k] > MapFlag.None) break;
                                generatedMap[j, k] |= MapFlag.MiddleRoad;
                            }
                            if (dir == (int)MapValues.Dir.Left)
                            {
                                if (generatedMap[k, j] > MapFlag.None) break;
                                generatedMap[k, j] |= MapFlag.MiddleRoad;
                            }
                        }
                    }
                }

                int pickedValue = copiedPossible[middleRoadIdx];

                // ǥ���� �ε���, �� �� �� ũ�⸸ŭ ������ �濡�� ����
                for (int j = copiedPossible.Count - 1; j >= 0; j--)
                {
                    if (pickedValue - extendedBuildingSize - middleRoadWidth < copiedPossible[j] && copiedPossible[j] < pickedValue + middleRoadWidth + extendedBuildingSize)
                        copiedPossible.RemoveAt(j);
                }
            }
        }
    }

    // ���� �ǹ� ���� Ȯ��
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

    // Ȯ���� �ǹ� ������ �����°� �Բ� �ǹ� ��ġ �õ�, �ǹ� ��ü�� �ǹ� ������ ǥ��
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

                    // �ٸ� �ǹ��� ��ġ����, ���� �������� �׽�Ʈ
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

                    // �׽�Ʈ ��������� �ǹ� ��ü ǥ��
                    if (!isOverllaped)
                    {
                        for (int i = 0; i < extendedBuildingSize; i++)
                        {
                            for (int j = 0; j < extendedBuildingSize; j++)
                            {
                                int px = x * extendedBuildingSize + xOffset + i;
                                int py = y * extendedBuildingSize + yOffset + j;

                                generatedMap[px, py] = MapFlag.BuildingPlace;
                            }
                        }

                        // �ǹ� ������(BuildingPoint) ǥ��
                        generatedMap[x * extendedBuildingSize + xOffset, y * extendedBuildingSize + yOffset] |= MapFlag.BuildingPoint;
                    }
                }
            }
        }
    }

    // �� �ǹ� �߾ӿ��� �ǹ� �׸��� ȣ��
    public void DrawBuildings()
    {
        ///////////////////////////////////////////////
        /// To be deleted
        for (int i = 0; i < extendedMapSize; i++)
        {
            for (int j = 0; j < extendedMapSize; j++)
            {
                if ((generatedMap[i, j] & MapFlag.BuildingPlace) == MapFlag.BuildingPlace)
                    Instantiate(buildingPlaceObject, new Vector3(i, j, 0), Quaternion.identity, buildingPlaceParent.transform);
            }
        }
        //////////////////////////////////////////////////////


        if (generatedMap != null)
        {
            // Draw Buildings
            for (int x = 0; x < extendedMapSize; x++)
            {
                for (int y = 0; y < extendedMapSize; y++)
                {
                    // Gap�� ���� �׷��ߵ�
                    if ((generatedMap[x, y] & MapFlag.BuildingPoint) == MapFlag.BuildingPoint)
                        DrawRandomBuilding(x, y);
                }
            }
        }
    }

    // �� ��ġ �������� ���ؼ� �ǹ� �׸��� & ���� �ǹ� ǥ��
    public void DrawRandomBuilding(int x, int y)
    {
        // �ǹ� ���� ����
        int minX = x + minBuildingGap;
        int maxX = x + minBuildingGap + buildingSize - 1;
        int minY = y + minBuildingGap;
        int maxY = y + minBuildingGap + buildingSize - 1;

        // Door ��ġ �������� ���ϱ�
        int doorUDRL = pseudoRandom.Next(0, 4);
        int doorX = 0, doorY = 0;
        if (doorUDRL == 0)
        {
            doorY = maxY;
            doorX = pseudoRandom.Next(minX + 1, maxX);
        }
        else if (doorUDRL == 1)
        {
            doorY = minY;
            doorX = pseudoRandom.Next(minX + 1, maxX);
        }
        else if (doorUDRL == 2)
        {
            doorX = maxX;
            doorY = pseudoRandom.Next(minY + 1, maxY);
        }
        else if (doorUDRL == 3)
        {
            doorX = minX;
            doorY = pseudoRandom.Next(minY + 1, maxY);
        }

        // �ǹ� �׸���
        int randomBuilding = pseudoRandom.Next(0, (int)BuildingType.BuildingCount);
        randomBuilding = 0; // To Be Changed
        BuildingComponent bt = buildingComponents[randomBuilding];
        GameObject g;

        for (int i = minX; i <= maxX; i++)
        {
            for (int j = minY; j <= maxY; j++)
            {
                // Gap�� ������ ���� �ǹ� ǥ��
                generatedMap[i, j] |= MapFlag.Building;

                if (i == maxX && j == maxY) g = bt.UR;
                else if (i == minX && j == maxY) g = bt.UL;
                else if (i == maxX && j == minY) g = bt.DR;
                else if (i == minX && j == minY) g = bt.DL;
                else if (i == doorX && j == doorY) g = bt.Door;
                else if (j == maxY) g = bt.U;
                else if (j == minY) g = bt.D;
                else if (i == maxX) g = bt.R;
                else if (i == minX) g = bt.L;
                else g = bt.In;

                Instantiate(g, new Vector3(i, j, 0), Quaternion.identity, buildingParent.transform);
            }
        }
    }

    public void DrawRoads()
    {
        for (int i = 0; i < extendedMapSize; i++)
        {
            for (int j = 0; j < extendedMapSize; j++)
            {
                if (generatedMap[i, j] >= MapFlag.WideRoad) Instantiate(roadObject, new Vector3(i, j, 0), Quaternion.identity, roadParent.transform);
            }
        }
    }
}
