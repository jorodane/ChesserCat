using UnityEngine;

public interface IControllerConnectable
{
	public ControllerBase ConnectedController { get; }
    public void Connect(ControllerBase target);
	public void Disconnect();
	public void Refresh();
}
