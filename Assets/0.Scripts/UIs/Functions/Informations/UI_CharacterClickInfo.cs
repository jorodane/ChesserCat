using UnityEngine;

public class UI_CharacterClickInfo : OpenableCharacterTargetUIBase
{
	public Transform anchorTransform;

	public override void Refresh()
	{
		if(ConnectedCharacter)
		{
			anchorTransform.position = CameraManager.GetScreenPosition(ConnectedCharacter.transform.position);
			gameObject.SetActive(true);
		}
		else
		{
			gameObject.SetActive(false);
		}
	}

	protected override void OnConnected(CharacterBase target)
	{
		anchorTransform.position = CameraManager.GetScreenPosition(target.transform.position);
		gameObject.SetActive(true);
	}

	protected override void OnDisconnect(CharacterBase target)
	{
		gameObject.SetActive(false);
	}
}
