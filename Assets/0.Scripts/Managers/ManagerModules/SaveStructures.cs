using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public struct CustomSaveData
{
    public string key;
    public string value;

    public CustomSaveData(string wantKey, string wantValue) 
    {
        key = wantKey;
        value = wantValue;
    }

    public CustomSaveData(KeyValuePair<string, string> wantPair)
    {
        key = wantPair.Key;
        value = wantPair.Value;
    }

    public static CustomSaveData[] GetCustomSaveDatas(Dictionary<string, string> from)
    {
        if (from is null) return null;
        CustomSaveData[] result = new CustomSaveData[from.Count];
        int progress = 0;
        foreach(KeyValuePair<string, string> currentPair in from)
        {
            result[progress] = new(currentPair);
            ++progress;
        }
        return result;
    }
}

[Serializable]
public struct CharacterSaveData
{
    public CustomSaveData[] saveDataList;
    public string prefabName;
    public Vector3Int startPosition;
    public Vector3Int finalPosition;
    public int startHealth;
    public int finalHealth;
    public bool isAlive;
}

[Serializable]
public struct ControllerSaveData
{
    public CustomSaveData[] saveDataList;
    public CharacterSaveData[] characterList;
    public CharacterSaveData[] pawnList;
    public Vector3Int oppositeDirection;
    public string prefabName;
}