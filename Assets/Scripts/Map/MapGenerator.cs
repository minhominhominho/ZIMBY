using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.ConstrainedExecution;
using System.Runtime.InteropServices;
using Unity.Mathematics;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Pool;
using UnityEngine.Rendering;
using UnityEngine.UIElements;
using UnityEngine.XR;


public enum MapType
{
    None,
    City,
    Countryside,
}

[Flags]
public enum MapFlag : int
{
    None = 0,
    ClearLand = 1,
    BuildingPoint = 2,
    Building = 4,
    WideRoad = 8,
    MiddleRoad = 16,
    NarrowRoad = 32,
    Environment = 64,
}

public enum BuildingType : int
{
    BuildingA,
    BuildingB,
    BuildingC,
    BuildingD,
    BuildingE,
    BuildingCount,
}

public enum EnvironmentType : int
{
    EnvironmentA,
    EnvironmentB,
    EnvironmentC,
    EnvironmentD,
    EnvironmentE,
    EnvironmentF,
    EnvironmentG,
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

[Serializable]
public struct EnvironmentComponet
{
    public EnvironmentType type;
    public MapFlag mapflag;
    public int[] dx;
    public int[] dy;
    public GameObject environmentObject;

    public bool CheckValid()
    {
        if (dx == null || dy == null) return false;
        if (dx[0] != 0 || dy[0] != 0) return false;
        if (dx.Length != dy.Length) return false;
        return true;
    }
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
    public MapType mapType;
    public string seed;
    private System.Random pseudoRandom;

    [Header("Map")]
    public int mapSize;
    private MapFlag[,] generatedMap;            // All roads and buildings are drawn from left down corner.
    private List<int> possilbeBuildingPoints;

    [Space]

    [Header("Building")]
    public int minBuildingSize;
    public int maxBuildingSize;
    public int minBuildingNum;
    public int maxBuildingNum;
    public int buildingGap;
    private Queue<Vector3Int> buildingInfo;

    private int buildingAreaSize;               // = buildingSize + 2 * minBuildingGap

    [Space]

    [Header("Road")]
    public int wideRoadWidth;
    public int middleRoadWidth;
    public int narrowRoadWidth;
    [Space]
    public int minWideRoadsNumPerDir;
    public int maxWideRoadsNumPerDir;
    public int minMiddleRoadsNumPerDir;
    public int maxMiddleRoadsNumPerDir;
    public int minNarrowRoadsTotalNum;
    public int maxNarrowRoadsTotalNum;

    [Space]

    [Header("Environment")]
    public int minEnvironmentNum;
    public int maxEnvironmentNum;
    private Queue<Vector3Int> environmnetInfo;

    [Space]

    [Header("Objects & Sprites")]
    public List<GameObject> wideRoadObjects;
    public List<GameObject> middleRoadObjects;
    public GameObject narrowRoadObjects;
    public GameObject crossWalkObject;
    public GameObject intersectionObject;

    public List<BuildingComponent> buildingComponents;
    public List<EnvironmentComponet> environmentComponets;

    public GameObject land;
    public GameObject roadParent;
    public GameObject buildingParent;
    public GameObject environmentParent;



    public void LoadFiles()
    {
        Debug.Log("Load \"MapGenerator\" files");

        // to be changed : read all numeric values?
        // Read MapType file
        MapInfoObject mapInfo = JsonReader.ReadMapType(mapType);
        if (mapInfo.MapType != mapType.ToString())
        {
            Debug.LogError("MapType doesn't match");
            return;
        }

        mapSize = mapInfo.MapSize;

        minBuildingSize = mapInfo.MinBuildingSize;
        maxBuildingSize = mapInfo.MaxBuildingSize;
        minBuildingNum = mapInfo.MinBuildingNum;
        maxBuildingNum = mapInfo.MaxBuildingNum;
        buildingGap = mapInfo.BuildingGap;

        wideRoadWidth = mapInfo.WideRoadWidth;
        middleRoadWidth = mapInfo.MiddleRoadWidth;
        narrowRoadWidth = mapInfo.NarrowRoadWidth;

        minWideRoadsNumPerDir = mapInfo.MinWideRoadsNumPerDir;
        maxWideRoadsNumPerDir = mapInfo.MaxWideRoadsNumPerDir;
        minMiddleRoadsNumPerDir = mapInfo.MinMiddleRoadsNumPerDir;
        maxMiddleRoadsNumPerDir = mapInfo.MaxMiddleRoadsNumPerDir;
        minNarrowRoadsTotalNum = mapInfo.MinNarrowRoadsTotalNum;
        maxNarrowRoadsTotalNum = mapInfo.MaxNarrowRoadsTotalNum;

        minEnvironmentNum = mapInfo.MinEnvironmentNum;
        maxEnvironmentNum = mapInfo.MaxEnvironmentNum;


        land = transform.Find("land").gameObject;
        roadParent = transform.Find("roadParent").gameObject;
        buildingParent = transform.Find("buildingParent").gameObject;
        environmentParent = transform.Find("environmentParent").gameObject;


        // to be changed : Road set per Maptype
        // Road Objects
        wideRoadObjects = new List<GameObject>();
        for (int i = 0; i < wideRoadWidth; i++) wideRoadObjects.Add(Resources.Load<GameObject>($"Map/Road/wideRoad{i}"));
        middleRoadObjects = new List<GameObject>();
        for (int i = 0; i < middleRoadWidth; i++) middleRoadObjects.Add(Resources.Load<GameObject>($"Map/Road/wideRoad{i * 2}"));
        narrowRoadObjects = Resources.Load<GameObject>($"Map/Road/narrowRoad");
        crossWalkObject = Resources.Load<GameObject>($"Map/Road/crosswalk");
        intersectionObject = Resources.Load<GameObject>($"Map/Road/intersection");


        // Building Objects
        buildingComponents = new List<BuildingComponent>();
        for (int i = 0; i < mapInfo.BuildingTypes.Length; i++)
        {
            BuildingComponent bc = new BuildingComponent();
            bc.type = JsonReader.StringToEnum<BuildingType>(mapInfo.BuildingTypes[i]);
            string type = mapInfo.BuildingTypes[i];

            bc.In = Resources.Load<GameObject>($"Map/Building/{type}/In");
            bc.UR = Resources.Load<GameObject>($"Map/Building/{type}/UR");
            bc.UL = Resources.Load<GameObject>($"Map/Building/{type}/UL");
            bc.DR = Resources.Load<GameObject>($"Map/Building/{type}/DR");
            bc.DL = Resources.Load<GameObject>($"Map/Building/{type}/DL");
            bc.U = Resources.Load<GameObject>($"Map/Building/{type}/U");
            bc.D = Resources.Load<GameObject>($"Map/Building/{type}/D");
            bc.R = Resources.Load<GameObject>($"Map/Building/{type}/R");
            bc.L = Resources.Load<GameObject>($"Map/Building/{type}/L");
            bc.Door = Resources.Load<GameObject>($"Map/Building/{type}/Door");

            buildingComponents.Add(bc);
        }


        // Environments Objects
        environmentComponets = new List<EnvironmentComponet>();

        // Read all EnvironmentObjects
        EnvironmentObject[] environmentObjects = JsonReader.ReadEnvironmentObjects();

        // Find all EnvironmentObjects which are in MapInfo
        // and then fill environmentComponets
        for (int i = 0; i < mapInfo.EnvironmentTypes.Length; i++)
        {
            EnvironmentComponet environmentComponent = new EnvironmentComponet();
            environmentComponent.type = JsonReader.StringToEnum<EnvironmentType>(mapInfo.EnvironmentTypes[i]);

            bool isFound = false;
            for (int j = 0; j < environmentObjects.Length; j++)
            {
                // Set EnvironmentComponet
                if (environmentObjects[j].EnvironmentType == mapInfo.EnvironmentTypes[i])
                {
                    for (int k = 0; k < environmentObjects[j].mapflag.Length; k++)
                    {
                        environmentComponent.mapflag |= JsonReader.StringToEnum<MapFlag>(environmentObjects[j].mapflag[k]);
                    }
                    environmentComponent.dx = environmentObjects[j].dx;
                    environmentComponent.dy = environmentObjects[j].dy;

                    environmentComponent.environmentObject = Resources.Load<GameObject>($"Map/Environment/{mapInfo.EnvironmentTypes[i]}");

                    isFound = true;
                    break;
                }
            }

            if (!isFound)
            {
                Debug.LogError("Cannot find EnvironmentObject in Map");
                return;
            }

            environmentComponets.Add(environmentComponent);
        }

        int idx = 0;
        while (idx < environmentComponets.Count)
        {
            if (!environmentComponets[idx].CheckValid())
            {
                Debug.LogError("Invalid environmentComponets!");
                return;
            }
            idx++;
        }
    }


    void Update()
    {
        if (Input.GetKeyDown(KeyCode.H))
        {
            land.SetActive(true);
            Init();
            GenerateMap();
        }
    }

    void Init()
    {
        if (seed.Length <= 0) seed = Time.time.ToString();
        pseudoRandom = new System.Random(seed.GetHashCode());

        buildingAreaSize = maxBuildingSize + 2 * buildingGap;

        land.transform.localScale = new Vector3(mapSize, mapSize, mapSize);
        land.transform.position = new Vector3(mapSize / 2 - 0.5f, mapSize / 2 - 0.5f, 0);

        generatedMap = new MapFlag[mapSize, mapSize];
    }

    void GenerateMap()
    {
        DrawRoads();

        MarkBuildings();

        MarkEnvironments();

        DrawObjects();
    }


    #region Draw Roads

    void DrawRoads()
    {
        DrawWideRoads();
        DrawMiddleRoads();
        DrawNarrowRoads();
    }

    void DrawWideRoads()
    {
        List<int> possibleWideRoadPoints = new List<int>();
        List<int> intersections = new List<int>();

        int minWideRoadGap = 2 * buildingAreaSize;

        for (int i = minWideRoadGap; i <= mapSize - minWideRoadGap - wideRoadWidth; i++) possibleWideRoadPoints.Add(i);

        for (int isHor = 0; isHor <= 1; isHor++)
        {
            int roadsNum = pseudoRandom.Next(minWideRoadsNumPerDir, maxWideRoadsNumPerDir + 1);
            List<int> coopiedPossibleWideRoadPoints = possibleWideRoadPoints.ToList();

            while (roadsNum-- > 0)
            {
                if (coopiedPossibleWideRoadPoints.Count == 0) break;

                // Mark Wide Road
                int wideRoadIdx = pseudoRandom.Next(0, coopiedPossibleWideRoadPoints.Count);
                for (int i = 0; i < mapSize; i++)
                {
                    for (int j = 0; j < wideRoadWidth; j++)
                    {
                        int px, py;
                        Quaternion q;
                        if (isHor == 1)
                        {
                            px = i;
                            py = coopiedPossibleWideRoadPoints[wideRoadIdx] + j;
                            q = Quaternion.Euler(new Vector3(0, 0, 90));

                        }
                        else
                        {
                            px = coopiedPossibleWideRoadPoints[wideRoadIdx] + j;
                            py = i;
                            q = Quaternion.identity;
                        }

                        if (generatedMap[px, py] == MapFlag.WideRoad && generatedMap[px - 1, py - 1] == MapFlag.None)
                        {
                            intersections.Add(px * mapSize + py);
                        }

                        if (generatedMap[px, py] != MapFlag.WideRoad)
                        {
                            Instantiate(wideRoadObjects[j], new Vector3(px, py, 0), q, roadParent.transform);
                            generatedMap[px, py] |= MapFlag.WideRoad;
                        }
                    }
                }

                // Remove impossible points
                int removedIdxVal = coopiedPossibleWideRoadPoints[wideRoadIdx];
                for (int i = -minWideRoadGap - wideRoadWidth + 1; i < minWideRoadGap + wideRoadWidth; i++)
                {
                    coopiedPossibleWideRoadPoints.Remove(removedIdxVal + i);
                }
            }
        }

        DrawWideIntersections(intersections);
    }

    // to be changed : same rule at all map types?
    void DrawWideIntersections(List<int> intersections)
    {
        for (int i = 0; i < intersections.Count; i++)
        {
            int x = intersections[i] / mapSize;
            int y = intersections[i] % mapSize;

            for (int j = 0; j < wideRoadWidth; j++)
            {
                Instantiate(crossWalkObject, new Vector3(x + j, y - 1, 0), Quaternion.identity, roadParent.transform);
                Instantiate(crossWalkObject, new Vector3(x + j, y + wideRoadWidth, 0), Quaternion.identity, roadParent.transform);
                Instantiate(crossWalkObject, new Vector3(x - 1, y + j, 0), Quaternion.Euler(new Vector3(0, 0, 90)), roadParent.transform);
                Instantiate(crossWalkObject, new Vector3(x + wideRoadWidth, y + j, 0), Quaternion.Euler(new Vector3(0, 0, 90)), roadParent.transform);
            }

            for (int p = 0; p < wideRoadWidth; p++)
            {
                for (int q = 0; q < wideRoadWidth; q++)
                {
                    Instantiate(intersectionObject, new Vector3(x + p, y + q, 0), Quaternion.identity, roadParent.transform);
                }
            }
        }
    }

    void DrawMiddleRoads()
    {
        int minMiddleRoadGap = buildingAreaSize;
        List<int> intersections = new List<int>();

        for (int isHor = 0; isHor <= 1; isHor++)
        {
            List<int> possibleMiddleRoadPoints = new List<int>();

            // Fill initial possible list
            for (int i = minMiddleRoadGap; i <= mapSize - minMiddleRoadGap - middleRoadWidth; i++)
            {
                bool isPossible = true;

                for (int j = -minMiddleRoadGap - middleRoadWidth + 1; j < minMiddleRoadGap + middleRoadWidth; j++)
                {
                    if (i + j < 0 || i + j >= mapSize) continue;

                    if (isHor == 1)
                    {
                        if (generatedMap[0, i + j] != MapFlag.None)
                        {
                            isPossible = false;
                            break;
                        }
                    }
                    else
                    {
                        if (generatedMap[i + j, 0] != MapFlag.None)
                        {
                            isPossible = false;
                            break;
                        }
                    }
                }

                if (isPossible) possibleMiddleRoadPoints.Add(i);
            }

            int roadsNum = pseudoRandom.Next(minMiddleRoadsNumPerDir, maxMiddleRoadsNumPerDir + 1);
            while (roadsNum-- > 0)
            {
                if (possibleMiddleRoadPoints.Count == 0) break;

                // Mark Middle Road
                int middleRoadIdx = pseudoRandom.Next(0, possibleMiddleRoadPoints.Count);

                bool dir = pseudoRandom.Next(0, 2) == 1 ? true : false;

                for (int i = (dir ? 0 : mapSize - 1); dir ? (i < mapSize) : (i >= 0); i += (dir ? 1 : -1))
                {
                    if (isHor == 1 && (generatedMap[i, 0] & MapFlag.WideRoad) == MapFlag.WideRoad)
                    {
                        intersections.Add(i * mapSize + possibleMiddleRoadPoints[middleRoadIdx]);
                        break;
                    }
                    else if (isHor == 0 && (generatedMap[0, i] & MapFlag.WideRoad) == MapFlag.WideRoad)
                    {
                        intersections.Add(possibleMiddleRoadPoints[middleRoadIdx] * mapSize + i);
                        break;
                    }

                    for (int j = 0; j < middleRoadWidth; j++)
                    {
                        int px, py;
                        Quaternion q;

                        if (isHor == 1)
                        {
                            px = i;
                            py = possibleMiddleRoadPoints[middleRoadIdx] + j;
                            q = Quaternion.Euler(new Vector3(0, 0, 90));

                        }
                        else
                        {
                            px = possibleMiddleRoadPoints[middleRoadIdx] + j;
                            py = i;
                            q = Quaternion.identity;
                        }

                        if (generatedMap[px, py] != MapFlag.None && generatedMap[px - 1, py - 1] == MapFlag.None)
                        {
                            intersections.Add(px * mapSize + py);
                        }

                        if (generatedMap[px, py] == MapFlag.None)
                        {
                            Instantiate(middleRoadObjects[j], new Vector3(px, py, 0), q, roadParent.transform);
                            generatedMap[px, py] |= MapFlag.MiddleRoad;
                        }
                    }
                }

                // Remove impossible points
                int removedIdxVal = possibleMiddleRoadPoints[middleRoadIdx];
                for (int i = -minMiddleRoadGap - middleRoadWidth + 1; i < minMiddleRoadGap + middleRoadWidth; i++)
                {
                    possibleMiddleRoadPoints.Remove(removedIdxVal + i);
                }
            }
        }

        DrawMiddleIntersections(intersections);
    }

    // to be changed : same rule at all map types?
    void DrawMiddleIntersections(List<int> intersections)
    {
        for (int i = 0; i < intersections.Count; i++)
        {
            int x = intersections[i] / mapSize;
            int y = intersections[i] % mapSize;

            // Middle Road meets Wide Road
            if ((generatedMap[x, y] & MapFlag.WideRoad) == MapFlag.WideRoad)
            {
                // Middle Road is horizontal & Middle Road is left
                if (generatedMap[x - 1, y - 1] == MapFlag.None && generatedMap[x - 1, y] == MapFlag.MiddleRoad)
                {
                    for (int j = 0; j < middleRoadWidth; j++)
                    {
                        Instantiate(crossWalkObject, new Vector3(x - 1, y + j, 0), Quaternion.Euler(new Vector3(0, 0, 90)), roadParent.transform);
                        Instantiate(intersectionObject, new Vector3(x, y + j, 0), Quaternion.Euler(new Vector3(0, 0, 90)), roadParent.transform);
                    }
                }
                // Middle Road is horizontal & Middle Road is right
                else if (generatedMap[x + 1, y - 1] == MapFlag.None)
                {
                    for (int j = 0; j < middleRoadWidth; j++)
                    {
                        Instantiate(crossWalkObject, new Vector3(x + 1, y + j, 0), Quaternion.Euler(new Vector3(0, 0, 90)), roadParent.transform);
                        Instantiate(intersectionObject, new Vector3(x, y + j, 0), Quaternion.Euler(new Vector3(0, 0, 90)), roadParent.transform);
                    }
                }
                // Middle Road is verical & Middle Road is up
                else if (generatedMap[x - 1, y + 1] == MapFlag.None)
                {
                    for (int j = 0; j < middleRoadWidth; j++)
                    {
                        Instantiate(crossWalkObject, new Vector3(x + j, y + 1, 0), Quaternion.identity, roadParent.transform);
                        Instantiate(intersectionObject, new Vector3(x + j, y, 0), Quaternion.identity, roadParent.transform);
                    }
                }
                // Middle Road is verical & Middle Road is down
                else
                {
                    for (int j = 0; j < middleRoadWidth; j++)
                    {
                        Instantiate(crossWalkObject, new Vector3(x + j, y - 1, 0), Quaternion.identity, roadParent.transform);
                        Instantiate(intersectionObject, new Vector3(x + j, y, 0), Quaternion.identity, roadParent.transform);
                    }
                }
            }
            // Middle Road meets Middle Road
            else
            {
                for (int j = 0; j < middleRoadWidth; j++)
                {
                    Instantiate(crossWalkObject, new Vector3(x + j, y - 1, 0), Quaternion.identity, roadParent.transform);
                    Instantiate(crossWalkObject, new Vector3(x + j, y + middleRoadWidth, 0), Quaternion.identity, roadParent.transform);
                    Instantiate(crossWalkObject, new Vector3(x - 1, y + j, 0), Quaternion.Euler(new Vector3(0, 0, 90)), roadParent.transform);
                    Instantiate(crossWalkObject, new Vector3(x + middleRoadWidth, y + j, 0), Quaternion.Euler(new Vector3(0, 0, 90)), roadParent.transform);
                }

                for (int p = 0; p < middleRoadWidth; p++)
                {
                    for (int q = 0; q < middleRoadWidth; q++)
                    {
                        Instantiate(intersectionObject, new Vector3(x + p, y + q, 0), Quaternion.identity, roadParent.transform);
                    }
                }
            }
        }
    }

    void DrawNarrowRoads()
    {
        List<int> possiblePoints = new List<int>();

        // Fill initial possible list
        for (int i = 0; i < mapSize; i++)
        {
            for (int j = 0; j < mapSize; j++)
            {
                if (generatedMap[i, j] == MapFlag.None)
                {
                    bool chk = true;
                    for (int p = -buildingAreaSize; p <= buildingAreaSize; p++)
                    {
                        for (int q = -buildingAreaSize; q <= buildingAreaSize; q++)
                        {
                            int px = i + p;
                            int py = j + q;

                            if (px < 0 || py < 0 || px >= mapSize || py >= mapSize || generatedMap[px, py] != MapFlag.None)
                            {
                                chk = false;
                                break;
                            }
                        }
                        if (!chk) break;
                    }

                    if (chk) possiblePoints.Add(mapSize * i + j);
                }
            }
        }

        while (maxNarrowRoadsTotalNum-- > 0)
        {
            if (possiblePoints.Count == 0) break;

            int idx = pseudoRandom.Next(0, possiblePoints.Count);
            int cx = possiblePoints[idx] / mapSize;
            int cy = possiblePoints[idx] % mapSize;

            // to be changed : Set probability of intersection branch num
            List<int> dir = new List<int>();
            for (int i = 0; i < MapValues.dx.Count(); i++) dir.Add(i);

            int branchNum;
            int ranNum = pseudoRandom.Next(0, 100);
            if (ranNum < 50) branchNum = 2;
            else if (ranNum < 80) branchNum = 3;
            else branchNum = 4;

            for (int j = 0; j < 4 - branchNum; j++) dir.RemoveAt(pseudoRandom.Next(0, dir.Count));

            bool chkPossible = true;

            // Check Narrow Road
            for (int i = 0; i < dir.Count; i++)
            {
                int px = cx + MapValues.dx[dir[i]];
                int py = cy + MapValues.dy[dir[i]];

                while (px >= 0 && py >= 0 && px < mapSize && py < mapSize && generatedMap[px, py] == MapFlag.None)
                {
                    for (int j = -buildingAreaSize; j <= buildingAreaSize; j++)
                    {
                        int nx = px + j * MapValues.dy[dir[i]];
                        int ny = py + j * MapValues.dx[dir[i]];
                        if (generatedMap[nx, ny] != MapFlag.None)
                        {
                            chkPossible = false;
                            break;
                        }
                    }

                    if (!chkPossible) break;
                    px += MapValues.dx[dir[i]];
                    py += MapValues.dy[dir[i]];
                }
                if (!chkPossible) break;
            }

            if (!chkPossible) continue;

            // Mark Narrow Road
            for (int i = 0; i < dir.Count; i++)
            {
                int px = cx + MapValues.dx[dir[i]];
                int py = cy + MapValues.dy[dir[i]];

                while (px >= 0 && py >= 0 && px < mapSize && py < mapSize && generatedMap[px, py] == MapFlag.None)
                {
                    generatedMap[px, py] = MapFlag.NarrowRoad;
                    Quaternion q = Quaternion.Euler(new Vector3(0, 0, (dir[i] >= 2 ? 90 : 0)));
                    Instantiate(narrowRoadObjects, new Vector3(px, py, 0), q, roadParent.transform);
                    px += MapValues.dx[dir[i]];
                    py += MapValues.dy[dir[i]];
                }

                if(px >= 0 && py >= 0 && px < mapSize && py < mapSize)
                {
                    Instantiate(intersectionObject, new Vector3(px, py, 0), Quaternion.identity, roadParent.transform);
                }
            }

            // Mark Initial point (intersection)
            generatedMap[cx, cy] = MapFlag.NarrowRoad;
            Instantiate(intersectionObject, new Vector3(cx, cy, 0), Quaternion.identity, roadParent.transform);

            // Remove impossible points
            Stack<int> deleteNum = new Stack<int>();
            for (int i = 0; i < possiblePoints.Count; i++)
            {
                int px = possiblePoints[i] / mapSize;
                int py = possiblePoints[i] % mapSize;
                bool chk = true;

                for (int p = -buildingAreaSize; p <= buildingAreaSize; p++)
                {
                    for (int q = -buildingAreaSize; q <= buildingAreaSize; q++)
                    {
                        int nx = px + p;
                        int ny = py + q;

                        if (nx < 0 || ny < 0 || nx >= mapSize || ny >= mapSize) continue;
                        if (generatedMap[nx, ny] != MapFlag.None)
                        {
                            chk = false;
                            break;
                        }
                    }
                    if (!chk) break;
                }

                if (!chk) deleteNum.Push(possiblePoints[i]);
            }

            while (deleteNum.Count > 0) possiblePoints.Remove(deleteNum.Pop());
        }
    }

    #endregion


    #region Mark Buildings

    // Update possilbeBuildingPoints which includes all possible Mapflag.BuildingPoint [x,y] in generatedMap
    int UpdatePossibleBuildingPoints()
    {
        possilbeBuildingPoints = new List<int>();

        for (int i = 0; i < mapSize; i++)
        {
            for (int j = 0; j < mapSize; j++)
            {
                if (generatedMap[i, j] > MapFlag.None) continue;

                bool chk = true;

                for (int p = -buildingGap; p < maxBuildingSize + buildingGap; p++)
                {
                    for (int q = -buildingGap; q < maxBuildingSize + buildingGap; q++)
                    {
                        int nx = i + p;
                        int ny = j + q;
                        if (nx < 0 || ny < 0 || nx >= mapSize || ny >= mapSize || generatedMap[nx, ny] > MapFlag.None)
                        {
                            chk = false;
                            break;
                        }
                    }
                    if (!chk) break;
                }

                if (chk) possilbeBuildingPoints.Add(i * mapSize + j);
            }
        }

        return possilbeBuildingPoints.Count;
    }

    // Mark [minBuildingNum, maxBuildingNum] Mapflag.BuildingPoints in generatedMap by randomly picking points in possilbeBuildingPoints
    // with randomBuildingSize [minBuildingSize, maxBuildingSize]
    void MarkBuildings()
    {
        buildingInfo = new Queue<Vector3Int>();
        int buildingCount = 0;
        int buildingNum = pseudoRandom.Next(minBuildingNum, maxBuildingNum + 1);

        while (UpdatePossibleBuildingPoints() > 0 && ++buildingCount <= buildingNum)
        {
            int bs = pseudoRandom.Next(minBuildingSize, maxBuildingSize + 1);
            int idx = pseudoRandom.Next(0, possilbeBuildingPoints.Count);
            int px = possilbeBuildingPoints[idx] / mapSize + pseudoRandom.Next(0, maxBuildingSize - bs + 1);
            int py = possilbeBuildingPoints[idx] % mapSize + pseudoRandom.Next(0, maxBuildingSize - bs + 1);

            generatedMap[px, py] |= MapFlag.BuildingPoint;
            buildingInfo.Enqueue(new Vector3Int(px, py, bs));

            for (int i = 0; i < bs; i++)
            {
                for (int j = 0; j < bs; j++)
                {
                    generatedMap[px + i, py + j] |= MapFlag.Building;
                }
            }
        }

        if (possilbeBuildingPoints.Count > 0) MarkBuildingsAtBlankSpace();
    }

    // Mark additional buildings if there is blank space
    void MarkBuildingsAtBlankSpace()
    {
        int[,] visited = new int[mapSize, mapSize];
        List<int> emptyAreaNums = new List<int>();
        int areaNumCnt = 0;

        // Bfs
        for (int i = 0; i < mapSize; i++)
        {
            for (int j = 0; j < mapSize; j++)
            {
                if (generatedMap[i, j] == MapFlag.None && visited[i, j] == 0)
                {
                    bool chk = true;
                    Queue<int> q = new Queue<int>();
                    q.Enqueue(i * mapSize + j);
                    visited[i, j] = ++areaNumCnt;

                    while (q.Count > 0)
                    {
                        int px = q.Dequeue();
                        int py = px % mapSize;
                        px /= mapSize;

                        for (int k = 0; k < MapValues.dx.Count(); k++)
                        {
                            int nx = px + MapValues.dx[k];
                            int ny = py + MapValues.dy[k];
                            if (nx < 0 || ny < 0 || nx >= mapSize || ny >= mapSize || visited[nx, ny] > 0 || generatedMap[nx, ny] >= MapFlag.WideRoad) continue;

                            // if generatedMap[nx,ny] is Building
                            if (generatedMap[nx, ny] != MapFlag.None) chk = false;

                            visited[nx, ny] = areaNumCnt;
                            q.Enqueue(nx * mapSize + ny);
                        }
                    }

                    if (chk) emptyAreaNums.Add(areaNumCnt);
                }
            }
        }

        // Mark one building for one emptyAreaNums values
        for (int k = 0; k < emptyAreaNums.Count; k++)
        {
            List<int> possibleBuildingArea = new List<int>();

            UpdatePossibleBuildingPoints();

            for (int i = 0; i < possilbeBuildingPoints.Count; i++)
            {
                int px = possilbeBuildingPoints[i] / mapSize;
                int py = possilbeBuildingPoints[i] % mapSize;
                if (visited[px, py] == emptyAreaNums[k]) possibleBuildingArea.Add(possilbeBuildingPoints[i]);
            }

            if (possibleBuildingArea.Count == 0) continue;

            int bs = pseudoRandom.Next(minBuildingSize, maxBuildingSize + 1);
            int aidx = pseudoRandom.Next(0, possibleBuildingArea.Count);
            int apx = possibleBuildingArea[aidx] / mapSize + pseudoRandom.Next(0, maxBuildingSize - bs + 1);
            int apy = possibleBuildingArea[aidx] % mapSize + pseudoRandom.Next(0, maxBuildingSize - bs + 1);
            generatedMap[apx, apy] |= MapFlag.BuildingPoint;
            buildingInfo.Enqueue(new Vector3Int(apx, apy, bs));

            for (int i = 0; i < bs; i++)
            {
                for (int j = 0; j < bs; j++)
                {
                    generatedMap[apx + i, apy + j] |= MapFlag.Building;
                }
            }
        }
    }

    #endregion


    #region Mark Environments

    public void MarkClearLands()
    {
        for (int i = 0; i < mapSize; i++)
        {
            for (int j = 0; j < mapSize; j++)
            {
                if (generatedMap[i, j] == MapFlag.None) generatedMap[i, j] = MapFlag.ClearLand;
            }
        }
    }

    public void MarkEnvironments()
    {
        MarkClearLands();

        environmnetInfo = new Queue<Vector3Int>();
        int environmentNum = pseudoRandom.Next(minEnvironmentNum, maxEnvironmentNum + 1);

        while (environmentNum-- > 0)
        {
            int environmentType = pseudoRandom.Next(0, environmentComponets.Count);
            EnvironmentComponet ec = environmentComponets[environmentType];
            List<int> possiblePoints = new List<int>();
            UpdatePossibleEnvironmentPoints(ref ec, possiblePoints, environmentType);

            if (possiblePoints.Count == 0) continue;

            int idx = pseudoRandom.Next(0, possiblePoints.Count);
            int px = possiblePoints[idx] / mapSize;
            int py = possiblePoints[idx] % mapSize;

            for (int i = 0; i < ec.dx.Length; i++)
            {
                int nx = px + ec.dx[i];
                int ny = py + ec.dy[i];
                generatedMap[nx, ny] |= MapFlag.Environment;
            }

            environmnetInfo.Enqueue(new Vector3Int(px, py, environmentType));
        }
    }

    public void UpdatePossibleEnvironmentPoints(ref EnvironmentComponet ec, List<int> possiblePoints, int environmentType)
    {
        for (int i = 0; i < mapSize; i++)
        {
            for (int j = 0; j < mapSize; j++)
            {
                if ((generatedMap[i, j] & ec.mapflag) == ec.mapflag)
                {
                    bool chk = true;
                    for (int k = 0; k < ec.dx.Length; k++)
                    {
                        int px = i + ec.dx[k];
                        int py = j + ec.dy[k];
                        if (px < 0 || py < 0 || px >= mapSize || py >= mapSize)
                        {
                            chk = false;
                            break;
                        }

                        if ((generatedMap[px, py] & MapFlag.Environment) == MapFlag.Environment)
                        {
                            chk = false;
                            break;
                        }

                        if ((generatedMap[px, py] & ec.mapflag) != ec.mapflag)
                        {
                            chk = false;
                            break;
                        }

                        // apply min gap only with building
                        if (ec.mapflag == MapFlag.Building)
                        {
                            for (int d = 0; d < MapValues.dx.Count(); d++)
                            {
                                int nx = px + MapValues.dx[d];
                                int ny = py + MapValues.dy[d];
                                if ((generatedMap[nx, ny] & MapFlag.Building) != MapFlag.Building) chk = false;
                            }
                        }

                        if (!chk) break;
                    }

                    if (chk) possiblePoints.Add(i * mapSize + j);
                }
            }
        }
    }

    #endregion


    #region Mark Items

    #endregion


    #region Draw Objects

    public void DrawObjects()
    {
        DrawBuildings();
        DrawEnvironments();
        DrawItems();
    }

    // Draw Building at all Mapflag.BuildingPoint
    public void DrawBuildings()
    {
        if (generatedMap != null)
        {
            while (buildingInfo.Count > 0)
            {
                Vector3Int bp = buildingInfo.Dequeue();
                DrawRandomBuildingWithDoor(bp.x, bp.y, bp.z);
            }
        }
    }

    // To Be Changed : Door Rotation
    // Set Random Door position & Draw Random Building
    public void DrawRandomBuildingWithDoor(int x, int y, int bs)
    {
        // Building Range
        int minX = x;
        int maxX = x + bs - 1;
        int minY = y;
        int maxY = y + bs - 1;

        // Set Random Door position
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

        // To Be Changed : Door Rotation
        // Draw Building
        int randomBuilding = pseudoRandom.Next(0, buildingComponents.Count);
        BuildingComponent bt = buildingComponents[randomBuilding];
        GameObject g;

        for (int i = minX; i <= maxX; i++)
        {
            for (int j = minY; j <= maxY; j++)
            {
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

    public void DrawEnvironments()
    {
        while (environmnetInfo.Count > 0)
        {
            Vector3Int env = environmnetInfo.Dequeue();
            Instantiate(environmentComponets[env.z].environmentObject, new Vector3(env.x, env.y, 0), Quaternion.identity, environmentParent.transform);
        }
    }

    public void DrawItems()
    {

    }

    #endregion
}
