using UnityEngine;

public interface ICharacterConnectable : ITargetConnectable<CharacterBase>
{
	public CharacterBase ConnectedCharacter { get; }
}
