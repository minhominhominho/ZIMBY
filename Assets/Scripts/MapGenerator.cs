using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Pool;
using static UnityEditor.Searcher.SearcherWindow.Alignment;

[Flags]
public enum MapFlag : byte
{
    None = 0,
    Center = 1,
    BuildingPlace = 2,
    BuildingPlaceEdge = 4,
    Building = 8,
    Road = 16,
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

    Stack<Vector2Int> doorsStack;


    [Header("Objects & Sprites")]
    public GameObject land;
    public BuildingComponent[] buildingComponents = new BuildingComponent[(int)BuildingType.BuildingCount];

    public GameObject roadObject;
    public GameObject dummyParent;
    public GameObject dummy;
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

        doorsStack = new Stack<Vector2Int>();
    }

    void GenerateMap()
    {
        ReserveBuildings();
        PlaceBuildings();
        DrawBuildings();
        PlaceRoads();
        DrawRoads();
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

    // Ȯ���� �ǹ� ������ �����°� �Բ� �ǹ� ��ġ �õ�, �ǹ� �� �ǹ� �߾� ǥ��
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

                    // �׽�Ʈ ��������� �ǹ� ǥ��
                    if (!isOverllaped)
                    {
                        for (int i = 0; i < extendedBuildingSize; i++)
                        {
                            for (int j = 0; j < extendedBuildingSize; j++)
                            {
                                int px = x * extendedBuildingSize + xOffset + i;
                                int py = y * extendedBuildingSize + yOffset + j;


                                generatedMap[px, py] = MapFlag.BuildingPlace;
                                if (i == 0 || i == extendedBuildingSize - 1 || j == 0 || j == extendedBuildingSize - 1) generatedMap[px, py] = MapFlag.BuildingPlaceEdge;
                            }
                        }

                        // �ǹ� �߾� ǥ��
                        generatedMap[x * extendedBuildingSize + extendedBuildingSize / 2 + xOffset, y * extendedBuildingSize + extendedBuildingSize / 2 + yOffset] |= MapFlag.Center;
                    }
                }
            }
        }
    }

    // �� �ǹ� �߾ӿ��� �ǹ� �׸��� ȣ��
    public void DrawBuildings()
    {
        ///////////////////////////////////////////////
        ///To be deleted
        ////////////////////////////
        for (int i = 0; i < extendedMapSize; i++)
        {
            for (int j = 0; j < extendedMapSize; j++)
            {
                if ((generatedMap[i, j] & MapFlag.BuildingPlace) == MapFlag.BuildingPlace)
                    Instantiate(dummy, new Vector3(i, j, 0), Quaternion.identity, dummyParent.transform);
            }
        }

        if (generatedMap != null)
        {
            // Draw Buildings
            for (int x = 0; x < extendedMapSize; x++)
            {
                for (int y = 0; y < extendedMapSize; y++)
                {
                    // Gap�� ���� �׷��ߵ�
                    if ((generatedMap[x, y] & MapFlag.Center) == MapFlag.Center)
                        DrawRandomBuilding(x, y);
                }
            }
        }
    }

    // �� ��ġ �������� ���ؼ� �ǹ� �׸��� & ���� �ǹ� ǥ��
    public void DrawRandomBuilding(int x, int y)
    {
        // �ǹ� ũ�� ����
        int minX = x - buildingSize / 2;
        int maxX = x + buildingSize / 2 - 1;
        int minY = y - buildingSize / 2;
        int maxY = y + buildingSize / 2 - 1;

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

        doorsStack.Push(new Vector2Int(doorX, doorY));

        // �ǹ� �׸���
        int randomBuilding = pseudoRandom.Next(0, (int)BuildingType.BuildingCount);
        randomBuilding = 0; // To Be Changed
        BuildingComponent bt = buildingComponents[randomBuilding];
        GameObject g;

        for (int i = minX; i < maxX + 1; i++)
        {
            for (int j = minY; j < maxY + 1; j++)
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

                Instantiate(g, new Vector3(i, j, 0), Quaternion.identity, this.gameObject.transform);

            }
        }
    }

    // �� ��ġ���� �� �׸��� �õ�
    public void PlaceRoads()
    {
        Queue<Vector2Int> posQ = new Queue<Vector2Int>();
        Queue<bool> verQ = new Queue<bool>();

        while (doorsStack.Count > 0)
        {
            Vector2Int door = doorsStack.Pop();

            for (int i = 0; i < 4; i++)
            {
                // ���� ���� ã��
                int roadX = door.x + MapValues.dx[i];
                int roadY = door.y + MapValues.dy[i];
                if (roadX < 0 || roadY < 0 || roadX >= extendedMapSize || roadY >= extendedMapSize) continue;
                if ((generatedMap[roadX, roadY] & MapFlag.Building) == MapFlag.Building) continue;

                // �������� �� ǥ��
                for (int j = 0; j < minBuildingGap; j++)
                {
                    generatedMap[roadX, roadY] = MapFlag.Road;
                    roadX += MapValues.dx[i];
                    roadY += MapValues.dy[i];
                    if (roadX < 0 || roadY < 0 || roadX >= extendedMapSize || roadY >= extendedMapSize) return;
                }

                generatedMap[roadX, roadY] = MapFlag.Road;
                posQ.Enqueue(new Vector2Int(roadX, roadY));
                verQ.Enqueue(MapValues.dy[i] == 0 ? true : false);
            }
        }

        // �������� �� ǥ��
        while (posQ.Count > 0)
        {
            Vector2Int point = posQ.Dequeue();
            int x = point.x;
            int y = point.y;
            bool isVertical = verQ.Dequeue();

            for (int i = 0; i < 4; i++)
            {
                if (isVertical && MapValues.dx[i] != 0) continue;
                if (!isVertical && MapValues.dy[i] != 0) continue;

                int px = x;
                int py = y;
                while (true)
                {
                    // ���� �������� �� �׸��� �õ�
                    px += MapValues.dx[i];
                    py += MapValues.dy[i];
                    if (px < 0 || py < 0 || px >= extendedMapSize || py >= extendedMapSize) break;
                    if ((generatedMap[px, py] & MapFlag.Road) == MapFlag.Road) break;

                    if ((generatedMap[px, py] & MapFlag.BuildingPlace) == MapFlag.BuildingPlace)
                    {
                        posQ.Enqueue(new Vector2Int(px - MapValues.dx[i], py - MapValues.dy[i]));
                        verQ.Enqueue(!isVertical);
                        break;
                    }

                    generatedMap[px, py] = MapFlag.Road;
                }
            }
        }
    }

    public void DrawRoads()
    {
        for (int i = 0; i < extendedMapSize; i++)
        {
            for (int j = 0; j < extendedMapSize; j++)
            {
                if (generatedMap[i, j] == MapFlag.Road) Instantiate(roadObject, new Vector3(i, j, 0), Quaternion.identity, this.gameObject.transform);
            }
        }
    }
}
