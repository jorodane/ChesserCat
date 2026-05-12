using UnityEngine;

public abstract class OpenableCharacterTargetUIBase : CharacterTargetUIBase, IOpenable
{
	public virtual bool IsOpen => gameObject.activeSelf;
	public virtual bool IsNeedClose => IsOpen;
	public virtual void Open(bool isActiveByKey) => gameObject.SetActive(true);
	public virtual void Close(bool isActiveByKey) => gameObject.SetActive(false);
	public virtual void Toggle(bool isActiveByKey) => gameObject.SetActive(!IsOpen);
}
