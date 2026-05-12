using UnityEngine;

public abstract class CharacterTargetUIBase : UIBase, ICharacterConnectable
{
	protected CharacterBase _connectedCharacter;
	public CharacterBase ConnectedCharacter => _connectedCharacter;

	protected abstract void OnConnected(CharacterBase target);
	public void Connect(CharacterBase target) => this.GeneralConnect(ref _connectedCharacter, target, OnConnected);

	protected abstract void OnDisconnected(CharacterBase target);
	public void Disconnect(CharacterBase target) => this.GeneralDisconnect(ref _connectedCharacter, OnDisconnected);

	public abstract void Refresh();
}
