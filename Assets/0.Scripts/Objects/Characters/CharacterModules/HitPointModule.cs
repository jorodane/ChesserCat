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

public abstract class HitPointModule : CharacterModule
{
	protected FillValue fill;

	public sealed override System.Type RegistrationType => typeof(HitPointModule);

	public bool IsFullHealth  => fill.IsMax;
	public bool IsOut => fill.IsUnderZero;
	public bool IsAlive => !fill.IsUnderZero;

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
