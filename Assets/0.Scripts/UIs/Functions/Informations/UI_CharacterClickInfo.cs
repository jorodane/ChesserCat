using UnityEngine;

public class UI_CharacterClickInfo : OpenableCharacterTargetUIBase
{
	public Transform anchorTransform;

	[SerializeField] UI_CharacterHoverInfo targetInfo;
	[SerializeField] UI_Button_PlayAction attackButton;
	[SerializeField] UI_Button_PlayAction moveButton;
	[SerializeField] UI_Button_PlayAction infoButton;
	[SerializeField] UI_Button_PlayAction cancelButton;

	public override void Registration(UIManager manager)
	{
		base.Registration(manager);
		attackButton.OnButtonActivated -= Close;
		attackButton.OnButtonActivated += Close;
		moveButton.OnButtonActivated -= Close;
		moveButton.OnButtonActivated += Close;
		infoButton.OnButtonActivated -= Close;
		infoButton.OnButtonActivated += Close;
		cancelButton.OnButtonActivated -= Close;
		cancelButton.OnButtonActivated += Close;
	}

	public override void Unregistration(UIManager manager)
	{
		base.Unregistration(manager);
		attackButton.OnButtonActivated -= Close;
		moveButton.OnButtonActivated -= Close;
		infoButton.OnButtonActivated -= Close;
		cancelButton.OnButtonActivated -= Close;
	}

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
		if (ConnectedCharacter)
		{
			targetInfo.OpenWithCharacter(ConnectedCharacter);
			attackButton.Connect(ConnectedCharacter);
			moveButton.Connect(ConnectedCharacter);
			infoButton.Connect(ConnectedCharacter);
			cancelButton.Connect(ConnectedCharacter);
		}
		gameObject.SetActive(true);
	}

	protected override void OnDisconnect(CharacterBase target)
	{
		gameObject.SetActive(false);
		targetInfo.Close();
		attackButton.Disconnect();
		moveButton.Disconnect();
		infoButton.Disconnect();
		cancelButton.Disconnect();
	}

	public override void Open()
	{
		base.Open();
		UIManager.ClaimCloseUI(UIType.CharacterHoverInfo);
	}
}
