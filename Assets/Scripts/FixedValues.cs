using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class FixedValues
{
    public static readonly int MAX_FURNITURE = 10000;
}

public static class MapValues
{
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