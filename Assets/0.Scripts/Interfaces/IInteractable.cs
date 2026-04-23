using UnityEngine;

public enum InteractType
{ 
	None,
	Talk, Take, Trade, Move, Work, 
	Length
}

public interface IInteractable
{
	public bool IsInteractable(GameObject from);
	public string GetInteractText(GameObject from);
	public InteractType GetInteractType();

	public void Interact(GameObject from);
	public void StopInteract(GameObject from);
}
