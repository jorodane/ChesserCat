using UnityEngine;

[CreateAssetMenu(fileName = "StageBase", menuName = "Stages/StageBase")]
public class StageBase : ScriptableObject, ISavable<StageSaveData>
{
	public string stageName;
	[Multiline]
	public string stageDetail;

	public int engageMin = 1;
	public int engageMax = 4;

	public string boardID;

	public ControllerSaveData playerData;

	public ControllerSaveData[] controllerDatas;
	public string[] objectiveList;

	public CharacterSaveData[] characterList;
	public ObjectPlacementSaveData[] objectPlacementList;


	public void LoadData(in StageSaveData data)
	{

	}

	public StageSaveData MakeSaveData() => new ()
	{
		
	};

	public string GetStageName() => stageName;
	public string GetStageDetail() => stageDetail;
}
