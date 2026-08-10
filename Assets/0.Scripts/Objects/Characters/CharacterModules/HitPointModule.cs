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

public class HitPointModule : CharacterModule
{
	public FillValue fill;

	public sealed override System.Type RegistrationType => typeof(HitPointModule);

	public float	Percent		 => fill.Percent;
	public int		Max			 => fill.Max;
    public int      Fillable     => fill.Max - GetCurrent();
	public string 	FillString	 => $"{GetCurrent()}/{fill.Max}";
	public bool		IsFullHealth => fill.IsMax;
	public bool		IsOut		 => fill.IsUnderZero;
	public bool		IsAlive		 => !fill.IsUnderZero;
	public bool		IsDamaged	 => Fillable > 0;

    public int SetCurrent(int value, bool isAnimation)  => fill.SetCurrent(value, isAnimation);
    public int GetCurrent()                             => fill.GetCurrent();

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
}
