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

	public bool Check() => true;
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
	public float scaleBase = 1.0f;
	public float scaleMin = 0.5f;
	public float scaleMax = 2.0f;
	public Sprite worldImage;
	public StageLocator[] stages;
}
