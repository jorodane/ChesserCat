using System;
using System.Reflection;

[AttributeUsage(AttributeTargets.Class, Inherited = false)]
public sealed class SaveNameSet : Attribute
{
    public string Value { get; }

    public SaveNameSet(string wantValue) => Value = wantValue;
}

public struct SaveRegister
{
    public Type instanceType;
    public Type dataType;
    public SaveNameSet saveSetter;
    public ConstructorInfo Constructor;

    public SaveRegister(Type wantInstanceType, Type wantDataType, SaveNameSet wantSaveSetter)
    {
        instanceType = wantInstanceType;
        dataType = wantDataType;
        saveSetter = wantSaveSetter;
        Constructor = instanceType.GetConstructor(new[] { dataType });
    }

    public readonly object CreateInstance(in object data)
    {
        try
        {
            return Constructor?.Invoke(new object[] { data });
        }
        catch
        {
            return null;
        }
    }
}
