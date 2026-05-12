using UnityEngine;

public class UI_Button_PlayAction : CharacterTargetUIBase, IControllerConnectable
{
	ControllerBase _connectedController;
	public ControllerBase ConnectedController => _connectedController;

	public event System.Action OnButtonActivated;

	public override void Refresh(){}
	protected override void OnConnected(CharacterBase target){}
	protected override void OnDisconnected(CharacterBase target){}

	protected virtual void OnConnected(ControllerBase target){}
	protected virtual void OnDisconnect(ControllerBase target){}
	public void Connect(ControllerBase target) => this.GeneralConnect(ref _connectedController, target, OnConnected);
	public void Disconnect(ControllerBase target) => this.GeneralDisconnect(ref _connectedController, OnDisconnect);

	public virtual bool IsActivatable() => true;

	protected virtual void OnActivated() { }
	public void Activate()
	{
		OnActivated();
		if(OnButtonActivated is not null && IsActivatable()) OnButtonActivated.Invoke();
	}

}
