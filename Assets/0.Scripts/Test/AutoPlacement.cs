using UnityEngine;

public class AutoPlacement : MonoBehaviour, IFunctionable
{
	public Vector3Int location;
	public void RegistrationFunctions()
	{
		GameManager.OnInitializeCharacter += Place;
	}

	public void UnregistrationFunctions()
	{

	}

	void Place()
	{
		TileManager.PlaceObjectOnTile(gameObject, location);
	}
}
