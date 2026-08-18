using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using UnityEngine;

public class SaveManager : ManagerBase
{
    static readonly Dictionary<string, SaveRegister> registeredData = new();
    static readonly Dictionary<Type, string> registeredName = new();
    static readonly string quickSaveDirectory = "C:/Test.Json";

    protected override IEnumerator OnConnected(GameManager newManager)
	{
        yield return Initialize();
        InputManager.OnQuickSave -= QuickSave;
        InputManager.OnQuickSave += QuickSave;
        InputManager.OnQuickLoad -= QuickLoad;
        InputManager.OnQuickLoad += QuickLoad;
        InputManager.OnClassicLoad -= LoadClassicChess;
        InputManager.OnClassicLoad += LoadClassicChess;
        yield return null;
	}

	protected override void OnDisconnected()
	{
        InputManager.OnQuickSave -= QuickSave;
        InputManager.OnQuickLoad -= QuickSave;
        InputManager.OnClassicLoad -= LoadClassicChess;
    }

    IEnumerator Initialize()
    {
        yield return SaveSubClassTypeRegistration(typeof(TurnActionInfo)).WaitForTask();
    }

    public async Task SaveSubClassTypeRegistration(Type wantType)
    {
        await Task.Run(() =>
        {
            foreach (Type currentType in wantType.GetSubClasses())
            {
                if (currentType is null) continue;
                SaveNameSet currentSetting = currentType.GetCustomAttribute<SaveNameSet>();

                Type savableInterface = currentType.GetInterfaces().FirstOrDefault
                (x => x.IsGenericType && x.GetGenericTypeDefinition() == typeof(ISavable<>));

                if (currentSetting is null)
                {
                    Debug.LogError($"{currentType} has not SaveNameSet Attribute");
                }
                else if (savableInterface is null)
                {
                    Debug.LogError($"{currentType} is not Savable Type");
                }
                else
                {
                    Type[] genericArguments = savableInterface.GetGenericArguments();
                    Type dataType = genericArguments[0];
                    SaveTypeRegistration(currentType, dataType, currentSetting);
                }
            }
        });
    }

    public void SaveTypeRegistration(Type wantType, Type wantDataType, SaveNameSet wantSetting)
    {
        string wantName = wantSetting.Value;
        registeredData[wantName] = new(wantType, wantDataType, wantSetting);
        registeredName[wantType] = wantName;
    }

    public static SaveRegister? GetRegisteredData(string registeredName)
    {
        if (registeredData is null) return null;
        if (registeredData.TryGetValue(registeredName, out SaveRegister data)) return data;
        return null;
    }

    void LoadData(BattleSaveData data)
    {
        GameManager.Tile?.LoadData(data.stage.fieldData);
        GameManager.Battle?.LoadData(data);
    }

    void LoadFromDirectory(string directory)
    {
        try
        {
            LoadData(JsonUtility.FromJson<BattleSaveData>(File.ReadAllText(directory)));
        }
        catch
        {

        }
    }

    void QuickLoad(bool value)
    {
        LoadFromDirectory(quickSaveDirectory);
    }

    void LoadClassicChess(bool value)
    {
        LoadFromDirectory("C:/ClassicChess.Json");
    }

    void QuickSave(bool value)
    {
        FileStream testSaveStream = File.Create(quickSaveDirectory);
        testSaveStream.Close();
        string jsonData = JsonUtility.ToJson(GameManager.Battle?.MakeSaveData());
        if(!string.IsNullOrEmpty(jsonData))
        {
            File.WriteAllText(quickSaveDirectory, jsonData);
        }
    }
}