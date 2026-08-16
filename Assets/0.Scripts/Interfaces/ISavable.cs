using System;
using System.Collections.Generic;

public interface ISavable
{
    public void ConstructCustomSaveData(Dictionary<string, string> result);
}
public interface ISavable<T> : ISavable
{
    public T MakeSaveData();
    public void LoadData(in T data);

    void ISavable.ConstructCustomSaveData(Dictionary<string, string> result) { }
}