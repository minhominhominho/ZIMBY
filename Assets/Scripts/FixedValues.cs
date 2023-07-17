using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class FixedValues
{
    public static readonly int MAX_FURNITURE = 10000;
}

public static class MapValues
{
    // Standard Map Values
    public static readonly int MAP_SIZE = 100;
    public static readonly int MAP_BUILDINGSIZE = 10;
    public static readonly int MINBUILDINGGAP = 40;
    public static readonly int MAP_WIDEROADWIDTH = 5;
    public static readonly int MAP_MIDDLEROADWIDTH = 3;
    public static readonly int MAP_NARROWROADWIDTH = 1;
    public static readonly int[] dx = new int[] { 0, 0, 1, -1 };
    public static readonly int[] dy = new int[] { 1, -1, 0, 0 };
    public enum Dir : int { Up, Down, Right, Left }
}

//public static class HTTPSource
//{
//    public static readonly string QUESTIONNAIRE_SURVEY = "/questionnaire/answers/";
//    public static readonly string LOGIN = "/login";
//    public static readonly string ACCOUNT = "/account-management/my-account/";
//}