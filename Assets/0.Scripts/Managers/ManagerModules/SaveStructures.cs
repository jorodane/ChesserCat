using System;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using static UnityEditor.Experimental.GraphView.GraphView;

public interface ISavable
{
    public void ConstructCustomSaveData(ref Dictionary<string, string> result);
}

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

    public static implicit operator KeyValuePair<string, string>(in CustomSaveData origin) => new(origin.key, origin.value);
}

public static class SaveDataHelper
{ 
    public static Dictionary<string, string> GetDictionary(this IEnumerable<CustomSaveData> originDatas)
    {
        Dictionary<string, string> result = new();
        if (originDatas is null) return result;
        foreach (CustomSaveData data in originDatas) result[data.key] = data.value;
        return result;
    }

    public static CustomSaveData[] GetCustomSaveDatas(this Dictionary<string, string> from)
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

    public static CustomSaveData[] MakeCustomSaveData(this ISavable savable)
    {
        Dictionary<string, string> customSave = new();
        savable.ConstructCustomSaveData(ref customSave);
        return customSave.GetCustomSaveDatas();
    }

    public static CharacterSaveData[] MakeCharacterSaveDataArray(this List<CharacterBase> targets)
    {
        CharacterSaveData[] result = new CharacterSaveData[targets.Count];
        int index = 0;
        foreach (CharacterBase current in targets)
        {
            result[index] = current.MakeSaveData();
            ++index;
        }
        return result;
    }

    public static ControllerSaveData[] MakeControllerSaveDataArray(this List<ControllerBase> targets)
    {
        ControllerSaveData[] result = new ControllerSaveData[targets.Count];
        int index = 0;
        foreach (ControllerBase current in targets)
        {
            result[index] = current.MakeSaveData();
            ++index;
        }
        return result;
    }

    public static TileSaveData[] MakeTileSaveDataArray(this List<TileBase> targets)
    {
        TileSaveData[] result = new TileSaveData[targets.Count];
        int index = 0;
        foreach (TileBase current in targets)
        {
            result[index] = current.MakeSaveData();
            ++index;
        }
        return result;
    }

    public static ActionSaveData[] MakeActionSaveDataArray(this IEnumerable<TurnActionInfo> targets, in int count)
    {
        ActionSaveData[] result = new ActionSaveData[count];
        int index = 0;
        foreach (TurnActionInfo current in targets)
        {
            result[index] = current.MakeSaveData();
            ++index;
        }
        return result;
    }
    public static ActionSaveData[] MakeActionSaveDataArray(this List<TurnActionInfo> targets) => MakeActionSaveDataArray(targets, targets.Count);
    public static ActionSaveData[] MakeActionSaveDataArray(this TurnActionInfo[] targets) => MakeActionSaveDataArray(targets, targets.Length);


    public static TurnSaveData[] MakeTurnSaveDataArray(this List<TurnBaseInfo> targets)
    {
        TurnSaveData[] result = new TurnSaveData[targets.Count];
        int index = 0;
        foreach (TurnBaseInfo current in targets)
        {
            result[index] = current.MakeSaveData();
            ++index;
        }
        return result;
    }

    public static GuideSaveData[] MakeGuideSaveDataArray(this List<List<Vector3IntDirection>> targets)
    {
        GuideSaveData[] result = new GuideSaveData[targets.Count];
        int index = 0;
        foreach (List<Vector3IntDirection> current in targets)
        {
            result[index] = new(current);
            ++index;
        }
        return result;
    }
}

[Serializable]
public struct CharacterSaveData
{
    public CustomSaveData[] saveDataList;
    public string selfPrefabName;
    public string pawnPrefabName;
    public Vector3Int startPosition;
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

[Serializable]
public struct TileSaveData
{
    public CustomSaveData[] saveDataList;
    public Vector3Int location;
    public TileBaseType baseType;
    public TileDecoType decoType;
}


[Serializable]
public struct GuideSaveData
{
    public Vector3IntDirection[] guides;

    public GuideSaveData(IEnumerable<Vector3IntDirection> newGuides) { guides = newGuides.ToArray(); }
}

[Serializable]
public struct ActionSaveData
{
    public CustomSaveData[] saveDataList;
    public string actionName;
}

[Serializable]
public struct TurnSaveData
{
    public CustomSaveData[] saveDataList;
    public ActionSaveData[] actionList;
    public Vector3Int start;
    public Vector3Int destination;
    public string turnContext;
    public int turnIndex;
    public int playerID;
    public int characterID;
}

[Serializable]
public struct BattleSaveData
{
    public CustomSaveData[] saveDataList;
    public ControllerSaveData[] controllerList;
    public CharacterSaveData[] neutralCharacterList;
    public TurnSaveData[] turnList;
    public GuideSaveData[] guideList;
    public TileSaveData[] tileList;
}