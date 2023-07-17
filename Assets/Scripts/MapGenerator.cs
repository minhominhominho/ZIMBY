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

[Flags]
public enum MapFlag : byte
{
    None = 0,
    BuildingPoint = 1,
    Building = 2,
    WideRoad = 4,
    MiddleRoad = 8,
    NarrowRoad = 16,
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
    // 아이템과 상호작용할 때, key로 private Dictionary<int, MapItem> mapItems = new Dictionary<int, MapItem>()에 접근
}

public class MapGenerator : MonoBehaviour
{
    public string seed;
    private System.Random pseudoRandom;

    [Header("Map")]
    public int mapSize;
    private MapFlag[,] generatedMap;            // All roads and buildings are drawn from left down corner.
    private List<int> possilbeBuildingPoints;

    [Header("Building")]
    public int buildingSize;
    public int minBuildingNum;
    public int maxBuildingNum;
    public int minBuildingGap;

    private int buildingAreaSize;               // = buildingSize + 2 * minBuildingGap
    [Header("Road")]
    public int wideRoadWidth;
    public int middleRoadWidth;
    public int narrowRoadWidth;

    public int minWideRoadsNumPerDir;
    public int maxWideRoadsNumPerDir;
    public int minMiddleRoadsNumPerDir;
    public int maxMiddleRoadsNumPerDir;
    public int minNarrowRoadsTotalNum;
    public int maxNarrowRoadsTotalNum;

    [Header("Objects & Sprites")]
    public GameObject land;
    public BuildingComponent[] buildingComponents = new BuildingComponent[(int)BuildingType.BuildingCount];

    public GameObject roadObject;
    public GameObject roadParent;
    public GameObject buildingParent;


    void Update()
    {
        if (Input.GetKeyDown(KeyCode.H))
        {
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

        buildingAreaSize = buildingSize + 2 * minBuildingGap;

        land.transform.localScale = new Vector3(mapSize, mapSize, mapSize);
        land.transform.position = new Vector3(mapSize / 2 - 0.5f, mapSize / 2 - 0.5f, 0);

        generatedMap = new MapFlag[mapSize, mapSize];

        if (wideRoadWidth <= 0) wideRoadWidth = MapValues.MAP_MIDDLEROADWIDTH;
        if (middleRoadWidth <= 0) middleRoadWidth = MapValues.MAP_MIDDLEROADWIDTH;
        if (narrowRoadWidth <= 0) narrowRoadWidth = MapValues.MAP_NARROWROADWIDTH;



        // Building & Roads Min, Max
    }

    void GenerateMap()
    {
        MarkWideRoads();
        MarkMiddleRoads();
        MarkNarrowRoads();
        MarkBuildings();
        DrawBuildings();
        DrawRoads();
    }


    #region Mark Roads

    void MarkWideRoads()
    {
        List<int> possibleWideRoadPoints = new List<int>();

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
                wideRoadIdx = coopiedPossibleWideRoadPoints.Count - 1;
                for (int i = 0; i < mapSize; i++)
                {
                    for (int j = 0; j < wideRoadWidth; j++)
                    {
                        if (isHor == 1) generatedMap[i, coopiedPossibleWideRoadPoints[wideRoadIdx] + j] |= MapFlag.WideRoad;
                        else generatedMap[coopiedPossibleWideRoadPoints[wideRoadIdx] + j, i] |= MapFlag.WideRoad;
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
    }

    void MarkMiddleRoads()
    {
        int minMiddleRoadGap = buildingAreaSize;

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
                    if (isHor == 1 && (generatedMap[i, 0] & MapFlag.WideRoad) == MapFlag.WideRoad) break;
                    else if (isHor == 0 && (generatedMap[0, i] & MapFlag.WideRoad) == MapFlag.WideRoad) break;

                    for (int j = 0; j < middleRoadWidth; j++)
                    {
                        if (isHor == 1) generatedMap[i, possibleMiddleRoadPoints[middleRoadIdx] + j] |= MapFlag.MiddleRoad;
                        else generatedMap[possibleMiddleRoadPoints[middleRoadIdx] + j, i] |= MapFlag.MiddleRoad;
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
    }

    void MarkNarrowRoads()
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
                    px += MapValues.dx[dir[i]];
                    py += MapValues.dy[dir[i]];
                }
            }

            // Mark Initial point (intersection)
            generatedMap[cx, cy] = MapFlag.NarrowRoad;

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

                for (int p = -minBuildingGap; p < buildingSize + minBuildingGap; p++)
                {
                    for (int q = -minBuildingGap; q < buildingSize + minBuildingGap; q++)
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
    void MarkBuildings()
    {
        int buildingCount = 0;
        int buildingNum = pseudoRandom.Next(minBuildingNum, maxBuildingNum + 1);

        while (UpdatePossibleBuildingPoints() > 0 && ++buildingCount <= buildingNum)
        {
            int idx = pseudoRandom.Next(0, possilbeBuildingPoints.Count);
            int px = possilbeBuildingPoints[idx] / mapSize;
            int py = possilbeBuildingPoints[idx] % mapSize;

            generatedMap[px, py] |= MapFlag.BuildingPoint;

            for (int i = 0; i < buildingSize; i++)
            {
                for (int j = 0; j < buildingSize; j++)
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

            int aidx = pseudoRandom.Next(0, possibleBuildingArea.Count);
            int apx = possibleBuildingArea[aidx] / mapSize;
            int apy = possibleBuildingArea[aidx] % mapSize;
            generatedMap[apx, apy] |= MapFlag.BuildingPoint;

            for (int i = 0; i < buildingSize; i++)
            {
                for (int j = 0; j < buildingSize; j++)
                {
                    generatedMap[apx + i, apy + j] |= MapFlag.Building;
                }
            }
        }
    }

    #endregion


    #region Draw Objects

    // Draw Building at all Mapflag.BuildingPoint
    public void DrawBuildings()
    {
        if (generatedMap != null)
        {
            for (int x = 0; x < mapSize; x++)
            {
                for (int y = 0; y < mapSize; y++)
                {
                    if ((generatedMap[x, y] & MapFlag.BuildingPoint) == MapFlag.BuildingPoint)
                        DrawRandomBuildingWithDoor(x, y);
                }
            }
        }
    }

    // To Be Changed : Door Rotation
    // Set Random Door position & Draw Random Building
    public void DrawRandomBuildingWithDoor(int x, int y)
    {
        // Building Range
        int minX = x;
        int maxX = x + buildingSize - 1;
        int minY = y;
        int maxY = y + buildingSize - 1;

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
        int randomBuilding = pseudoRandom.Next(0, (int)BuildingType.BuildingCount);
        BuildingComponent bt = buildingComponents[randomBuilding];
        GameObject g = bt.In;

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

    // To Be Changed : Road Object
    public void DrawRoads()
    {
        for (int i = 0; i < mapSize; i++)
        {
            for (int j = 0; j < mapSize; j++)
            {
                if (generatedMap[i, j] >= MapFlag.WideRoad) Instantiate(roadObject, new Vector3(i, j, 0), Quaternion.identity, roadParent.transform);
            }
        }
    }

    #endregion
}
