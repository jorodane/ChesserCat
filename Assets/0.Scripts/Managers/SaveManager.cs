using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

public struct SaveRegister
{
    public Type instanceType;
    public Type dataType;
    public SaveNameSet saveSetter;

    public SaveRegister(Type wantInstanceType, Type wantDataType, SaveNameSet wantSaveSetter)
    {
        instanceType = wantInstanceType;
        dataType = wantDataType;
        saveSetter = wantSaveSetter;
    }
}

public class SaveManager : ManagerBase
{
    static readonly Dictionary<string, SaveRegister> registeredData = new();
    static readonly Dictionary<Type, string> registeredName = new();

	protected override IEnumerator OnConnected(GameManager newManager)
	{
        Initialize();
		yield return null;
	}

	protected override void OnDisconnected()
	{

	}

    void Initialize()
    {
        SaveSubClassTypeRegistration(typeof(TurnActionInfo));
    }

    public void SaveSubClassTypeRegistration(Type wantType)
    {
        foreach (Type currentType in wantType.GetSubClasses())
        {
            if (currentType is null) continue;
            SaveNameSet currentSetting = currentType.GetCustomAttribute<SaveNameSet>();
            if (currentSetting is not null)
            {
                SaveTypeRegistration(currentType, currentSetting);
            }
            else
            {
                Debug.LogError($"{currentType} has not SaveNameSet Attribute");
            }
        }
    }

    public void SaveTypeRegistration(Type wantType, SaveNameSet wantSetting)
    {
        string wantName = wantSetting.Value;
        registeredData[wantName] = new(wantType);
        registeredName[wantType] = wantName;
    }
}

[AttributeUsage(AttributeTargets.Class, Inherited = false)]
public sealed class SaveNameSet : Attribute
{
    public string Value { get; }

    public SaveNameSet(string wantValue) => Value = wantValue;
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
        foreach (KeyValuePair<string, string> currentPair in from)
        {
            result[progress] = new(currentPair);
            ++progress;
        }
        return result;
    }

    public static CustomSaveData[] MakeCustomSaveData(this ISavable<CustomSaveData> savable)
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
        List<GuideSaveData> result = new();
        int index = 0;
        foreach (List<Vector3IntDirection> current in targets)
        {
            if (current is not null && current.Count > 0) result.Add(new(index, current));
            ++index;
        }
        return result.ToArray();
    }
}