using System.Collections.Generic;
using Unity.VectorGraphics;
using UnityEngine;

public struct DamageStruct
{
	public GameObject from;
	public ControllerBase instigator;
	public int damageAmount;
	public bool critical;
	public ElementType damageType;
	public MoveCheckType moveType;
}

public struct RestoreStruct
{
	public GameObject from;
	public ControllerBase instigator;
	public int restoreAmount;
}

public class HitPointModule : CharacterModule, ISavable
{
	public FillValue fill;

	public sealed override System.Type RegistrationType => typeof(HitPointModule);

	public float	Percent		 => fill.Percent;
	public int		Current		 => fill.GetCurrent();
	public int		Max			 => fill.Max;
    public int      Fillable     => Max - Current;
	public string 	FillString	 => $"{Current}/{Max}";
	public bool		IsFullHealth => fill.IsMax;
	public bool		IsOut		 => fill.IsEmpty;
	public bool		IsAlive		 => !IsOut;
	public bool		IsDamaged	 => Fillable > 0;

    public int SetCurrent(int value, bool isAnimation)  => fill.SetCurrent(value, isAnimation);

	public int TakeDamage(in DamageStruct damageInfo)
	{
		fill.DecreaseCurrent(damageInfo.damageAmount);
		return damageInfo.damageAmount;
	}

	public int TakeRestore(in RestoreStruct restoreInfo)
	{
		fill.IncreaseCurrent(restoreInfo.restoreAmount);
		return restoreInfo.restoreAmount;
	}

	public override void ApplySetting(CharacterBaseSetting setting)
	{
		base.ApplySetting(setting);
		fill.SetMax(setting.health);
		fill.SetCurrent(setting.health, false);
	}

    public void ConstructCustomSaveData(Dictionary<string, string> result)
    {
        result["Base.CurrentHealth"] = Current.ToString();
        result["Base.MaxHealth"] = Max.ToString();
    }
}
