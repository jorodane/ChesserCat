using UnityEngine;

[CreateAssetMenu(fileName = "BoardBase", menuName = "Boards/BoardBase")]
public class BoardBase : ScriptableObject, ISavable<BoardSaveData>
{
	public Vector3Int size;
	public TileSaveData[] tiles;

	public void LoadData(in BoardSaveData data)
	{
		size = data.boardSize;
		tiles = data.tileList;
	}

	public BoardSaveData MakeSaveData() => new()
	{
		boardSize = size,
		tileList = tiles
	};
}
