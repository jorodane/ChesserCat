using UnityEngine;

public abstract class CharacterTargetUIBase : UIBase, ICharacterConnectable
{
	public CharacterBase _connectedCharacter { get; set; }
	public CharacterBase ConnectedCharacter => _connectedCharacter;

	protected abstract void OnConnected(CharacterBase target);
	public void Connect(CharacterBase target)
	{
		if(ConnectedCharacter) Disconnect();
		_connectedCharacter = target;
		if(target) OnConnected(target);
	}

	protected abstract void OnDisconnect(CharacterBase target);
	public void Disconnect()
	{
		if(ConnectedCharacter) OnDisconnect(ConnectedCharacter);
		_connectedCharacter = null;
	}

	public abstract void Refresh();
}
