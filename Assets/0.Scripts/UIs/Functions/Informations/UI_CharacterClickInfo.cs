using UnityEngine;
using static UnityEngine.GraphicsBuffer;

public class UI_CharacterClickInfo : OpenableCharacterTargetUIBase, IControllerConnectable
{
	public Transform anchorTransform;

	[SerializeField] UI_CharacterHoverInfo targetInfo;
	[SerializeField] UI_Button_PlayAction[] actionButtons;

	ControllerBase _connectedController;
	public ControllerBase ConnectedController => _connectedController;

	public override bool IsNeedClose => IsOpen && ConnectedController == PlayerController.Instance;

	public override void Refresh()
	{
		if(ConnectedCharacter)
		{
			gameObject.SetActive(true);
		}
		else
		{
			gameObject.SetActive(false);
		}
	}

	protected override void OnConnected(CharacterBase target)
	{
		if (target)
		{
			targetInfo.OpenWithCharacter(target);
			if(actionButtons is not null) foreach (UI_Button_PlayAction currentAction in actionButtons) currentAction.Connect(target);
		}
		gameObject.SetActive(true);
	}

	protected override void OnDisconnected(CharacterBase target)
	{
		gameObject.SetActive(false);
		targetInfo.Close(false);
		if (actionButtons is not null) foreach (UI_Button_PlayAction currentAction in actionButtons) currentAction.Disconnect(target);
	}

	protected virtual void OnConnected(ControllerBase target) 
	{
		if (!target) return;
		if (actionButtons is not null) foreach (UI_Button_PlayAction currentAction in actionButtons) currentAction.Connect(target);
	}
	protected virtual void OnDisconnected(ControllerBase target) 
	{
		if (!target) return;
		if (actionButtons is not null) foreach (UI_Button_PlayAction currentAction in actionButtons) currentAction.Disconnect(target);
	}


	public void Connect(ControllerBase target) => this.GeneralConnect(ref _connectedController, target, OnConnected);

	public void Disconnect(ControllerBase target) => this.GeneralDisconnect(ref _connectedController, OnDisconnected);


	public override void Open(bool isActiveByKey)
	{
		base.Open(isActiveByKey);
		UIManager.ClaimCloseUI(UIType.CharacterHoverInfo); 
        GameManager.OnUpdateUI -= MoveToTarget;
        GameManager.OnUpdateUI += MoveToTarget;
    }
	public override void Close(bool isActiveByKey) 
	{
		base.Close(isActiveByKey);
        GameManager.OnUpdateUI -= MoveToTarget;
		if (isActiveByKey) PlayerController.Instance.UnselectCurrentCharacter(true);
	}

    private void MoveToTarget(float deltaTime)
    {
        if (ConnectedCharacter == null) return;
        transform.position = Camera.main.WorldToScreenPoint(ConnectedCharacter.transform.position);
    }
}
