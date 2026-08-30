using System;
using System.Collections.Generic;
using System.Linq;
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

    public static implicit operator KeyValuePair<string, string>(in CustomSaveData origin) => new(origin.key, origin.value);
}

[Serializable]
public struct CharacterSaveData
{
    public CustomSaveData[] saveDataList;
	public int[] pawnIDList;
    public Vector3Int startPosition;
    public string presetName;
	public int controllerID;
	public int selfID;
	public int masterID;
    public bool isPawn;
}

[Serializable]
public struct ControllerSaveData
{
    public CustomSaveData[] saveDataList;
	public Color teamColor;
    public Vector3Int oppositeDirection;
	public int id;
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
public struct BoardSaveData
{
    public CustomSaveData[] saveDataList;
    public TileSaveData[] tileList;
    public Vector3Int boardSize;
}


[Serializable]
public struct GuideSaveData
{
    public int index;
    public Vector3IntDirection[] guides;

    public GuideSaveData(int wantIndex, IEnumerable<Vector3IntDirection> wantGuides) 
    {
        index = wantIndex;
        if(wantGuides is null)  guides = new Vector3IntDirection[0];
        else                    guides = wantGuides.ToArray(); 
    }
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
    public TurnSaveData[] turnList;
    public GuideSaveData[] guideList;
    public CharacterSaveData[] characterList;

    public ControllerSaveData playerSave;
    public StageSaveData stage;
}

[Serializable]
public struct StageSaveData
{
    public CustomSaveData[] saveDataList;
    public ControllerSaveData[] controllerList;
    public BoardSaveData fieldData;
    public string stageName;
}