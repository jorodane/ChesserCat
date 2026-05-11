using UnityEngine;

public class UI_Button_PlayAction : CharacterTargetUIBase
{
	public event System.Action OnButtonActivated;

	public override void Refresh()
	{

	}

	protected override void OnConnected(CharacterBase target)
	{

	}

	protected override void OnDisconnect(CharacterBase target)
	{

	}

	public virtual void Activate()
	{
		OnButtonActivated?.Invoke();
	}
}
