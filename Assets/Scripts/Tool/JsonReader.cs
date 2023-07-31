using JetBrains.Annotations;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class JsonReader : MonoBehaviour
{
    public static MapInfoObject ReadMapType(MapType mapType)
    {
        TextAsset t = (TextAsset)Resources.Load($"Data/{mapType.ToString() + "Map"}");
        MapInfoObject temp = JsonUtility.FromJson<MapInfoObject>(t.ToString());
        return temp;
    }

    public static EnvironmentObject[] ReadEnvironmentObjects()
    {
        TextAsset t = (TextAsset)Resources.Load("Data/EnvironmentObjects");
        EnvironmentObject[] temp = JsonUtility.FromJson<EnvironmentObjectsReader>(t.ToString()).EnvironmentObjects;
        return temp;
    }

    public static T StringToEnum<T>(string e)
    {
        return (T)Enum.Parse(typeof(T), e);
    }
}

[System.Serializable]
public class MapInfoObject
{
    public int MapSize;

    public int MinBuildingSize;
    public int MaxBuildingSize;
    public int MinBuildingNum;
    public int MaxBuildingNum;
    public int BuildingGap;

    public int WideRoadWidth;
    public int MiddleRoadWidth;
    public int NarrowRoadWidth;

    public int MinWideRoadsNumPerDir;
    public int MaxWideRoadsNumPerDir;
    public int MinMiddleRoadsNumPerDir;
    public int MaxMiddleRoadsNumPerDir;
    public int MinNarrowRoadsTotalNum;
    public int MaxNarrowRoadsTotalNum;

    public int MinEnvironmentNum;
    public int MaxEnvironmentNum;

    public string MapType;
    public string[] BuildingTypes;
    public string[] EnvironmentTypes;
}


    [System.Serializable]
public class EnvironmentObjectsReader
{
    public EnvironmentObject[] EnvironmentObjects;
}


[System.Serializable]
public class EnvironmentObject
{
    public string EnvironmentType;
    public string[] mapflag;
    public int[] dx;
    public int[] dy;
}

