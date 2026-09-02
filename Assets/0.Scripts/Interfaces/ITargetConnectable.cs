using UnityEngine;

public interface ITargetConnectable<T>
{
    public void Connect(T target);
	public void Disconnect(T target);
	public void Disconnect();
	public void Refresh();
}
