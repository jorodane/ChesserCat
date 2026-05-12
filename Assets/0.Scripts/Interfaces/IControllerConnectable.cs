using UnityEngine;

public interface IControllerConnectable : ITargetConnectable<ControllerBase>
{
	public ControllerBase ConnectedController { get; }
}
