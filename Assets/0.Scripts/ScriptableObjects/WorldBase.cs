using UnityEngine;

public enum ConditionChecker
{
	None, Defined, Undefined, Greater, GreaterEqual, Less, LessEqual, Equal, NotEqual
}

[System.Serializable]
public struct StageCondition
{
	public string conditionName;
	public ConditionChecker checker;
	public int value;
}

[System.Serializable]
public struct StageLocator
{
	public string label;
	public StageBase stage;
	public Vector3 location;
	public StageCondition[] unlockCondition;
}

[CreateAssetMenu(fileName = "WorldBase", menuName = "Worlds/WorldBase")]
public class WorldBase : ScriptableObject
{
	public string worldName;
	public Sprite worldImage;
	public StageLocator[] stages;
}
