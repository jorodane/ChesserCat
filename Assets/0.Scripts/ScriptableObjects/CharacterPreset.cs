using System;
using UnityEngine;
using UnityEngine.U2D.Animation;

[Serializable]
public struct CharacterBaseSetting
{
	public string initial;
	public string displayName;
	public Sprite icon;
	public SpriteLibraryAsset Visual;
	public Vector3 scale;
	public int damage;
	public int health;

	public MoveTypeInfo move;
	public MoveTypeInfo attack;
}


[CreateAssetMenu(fileName = "CharacterPreset", menuName = "Characters/CharacterPreset")]
public class CharacterPreset : ScriptableObject
{
	public CharacterBaseSetting masterInfo;
	public CharacterBaseSetting pawnInfo;

	public CharacterBaseSetting GetSetting(bool isPawn) => isPawn ? pawnInfo : masterInfo;
}
