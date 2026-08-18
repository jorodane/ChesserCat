using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;

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
        foreach (KeyValuePair<string, string> currentPair in from)
        {
            result[progress] = new(currentPair);
            ++progress;
        }
        return result;
    }

    public static CustomSaveData[] MakeCustomSaveData(this ISavable savable)
    {
        Dictionary<string, string> customSave = new();
        savable.ConstructCustomSaveData(customSave);
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

    public static IEnumerable<CharacterBase> MakeCharacterFromData(this IEnumerable<CharacterSaveData> datas)
    {
        foreach (CharacterSaveData currentData in datas)
        {
            GameObject instance = ObjectManager.CreateObject(currentData.selfPrefabName);
            if (instance && instance.TryGetComponent(out CharacterBase currentCharacter))
            {
                currentCharacter.LoadData(currentData);
                yield return currentCharacter;
            }
        }
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

    static ActionSaveData[] MakeActionSaveDataArray(this IEnumerable<TurnActionInfo> targets, int count)
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
    public static IEnumerable<TurnActionInfo> MakeActionFromData(this IEnumerable<ActionSaveData> datas)
    {
        foreach (ActionSaveData currentData in datas)
        {
            SaveRegister? currentRegister = SaveManager.GetRegisteredData(currentData.actionName);
            if (currentRegister is null) continue;
            TurnActionInfo currentTurn = currentRegister?.CreateInstance(currentData) as TurnActionInfo;
            yield return currentTurn;
        }
    }

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

    public static IEnumerable<TurnBaseInfo> MakeTurnFromData(this IEnumerable<TurnSaveData> datas)
    {
        foreach (TurnSaveData currentData in datas)
        {
            TurnBaseInfo currentTurn = new();
            currentTurn.LoadData(currentData);
            yield return currentTurn;
        }
    }

    public static GuideSaveData[] MakeGuideSaveDataArray(this List<List<Vector3IntDirection>> targets)
    {
        List<GuideSaveData> result = new();
        int index = 0;
        foreach (List<Vector3IntDirection> current in targets)
        {
            if (current is not null && current.Count > 0) result.Add(new(index, current));
            ++index;
        }
        return result.ToArray();
    }

    public static Regex Vector3IntParser = new (@"-?\d+", RegexOptions.Compiled);
    public static Vector3Int GetVector3Int(this string data)
    {
        if (string.IsNullOrEmpty(data)) return default;
        MatchCollection match = Vector3IntParser.Matches(data);
        if(match is null || match.Count != 3) return default;
        return new(int.Parse(match[0].Value), int.Parse(match[1].Value), int.Parse(match[2].Value));
    }
}