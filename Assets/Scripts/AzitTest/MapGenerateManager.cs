using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class MapGenerateManager : MonoBehaviour
{
    int[,] generatedMap;

    public int width;
    public int height;

    public string seed;
    private System.Random pseudoRandom;

    [Range(0, 100)]
    public int fillingPercentage;

    void Update()
    {
        CellularAutomata();
    }

    void CellularAutomata()
    {
        seed = (seed.Length <= 0) ? Time.time.ToString() : seed;
        pseudoRandom = new System.Random(seed.GetHashCode());

        GenerateMap();

        for (int i = 0; i < 5; i++)
            SmoothMap();

        RemoveSecludedCells();
        RecoverEdgeCells();
    }

    void GenerateMap()
    {
        generatedMap = new int[width, height];

        for (int x = 1; x < width - 1; x++)
        {
            for (int y = 1; y < height - 1; y++)
            {
                generatedMap[x, y] = (pseudoRandom.Next(0, 100) < fillingPercentage) ? 1 : 0;
            }
        }
    }

    // 참조로 넘겨주면 최적화 여지 있을듯?
    int getNeighboursCellCount(int x, int y, int[,] map)
    {
        int neighbors = 0;
        for (int i = -1; i <= 1; i++)
            for (int j = -1; j <= 1; j++)
                neighbors += map[i + x, j + y];

        neighbors -= map[x, y];

        return neighbors;
    }

    void SmoothMap()
    {
        for (int x = 1; x < width - 1; x++)
        {
            for (int y = 1; y < height - 1; y++)
            {
                int neighbors = getNeighboursCellCount(x, y, generatedMap);

                // enforcing rule for cave generation by Johnson et. al.
                // source: https://www.researchgate.net/publication/228919622_Cellular_automata_for_real-time_generation_of
                // T > 4 => C = true
                // T = 4 => C = C
                // T < 4 => C = false

                if (neighbors > 4)
                    generatedMap[x, y] = 1;
                else if (neighbors < 4)
                    generatedMap[x, y] = 0;
            }
        }
    }

    void RecoverEdgeCells()
    {
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                if (x == 0 || x == width - 1 || y == 0 || y == height - 1)
                    generatedMap[x, y] = 0;
            }
        }
    }

    void RemoveSecludedCells()
    {
        for (int x = 1; x < width - 1; x++)
        {
            for (int y = 1; y < height - 1; y++)
            {
                generatedMap[x, y] = (getNeighboursCellCount(x, y, generatedMap) <= 0) ? 0 : generatedMap[x, y];
            }
        }
    }


    void OnDrawGizmos()
    {
        if (generatedMap != null)
        {
            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    Gizmos.color = (generatedMap[x, y] == 1) ? Color.red : Color.blue;
                    Vector3 pos = new Vector3(x + .5f, y + .5f);
                    Gizmos.DrawCube(pos, Vector3.one);
                }
            }
        }
    }
}