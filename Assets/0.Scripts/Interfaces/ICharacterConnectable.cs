using UnityEngine;

public interface ICharacterConnectable
{
	public CharacterBase ConnectedCharacter { get; }
    public void Connect(CharacterBase target);
	public void Disconnect();
	public void Refresh();
}
