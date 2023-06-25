using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// ���� �������� �ߴ� ���

public static class FixedValues
{

}

public static class MapValues
{
    // Standard Map Values
    public static readonly int MAP_SIZE = 100;
    public static readonly int MAP_BUILDINGSIZE = 10;
    public static readonly int MINBUILDINGGAP = 1;
    public static readonly int MAP_BUILDINGFREQUENCY = 40;
    public static readonly int[] dx = new int[] { 0, 0, 1, -1 };
    public static readonly int[] dy = new int[] { 1, -1, 0, 0 };
}

// ����
//public static class HTTPSource
//{
//    public static readonly string QUESTIONNAIRE_SURVEY = "/questionnaire/answers/";
//    public static readonly string LOGIN = "/login";
//    public static readonly string ACCOUNT = "/account-management/my-account/";
//}