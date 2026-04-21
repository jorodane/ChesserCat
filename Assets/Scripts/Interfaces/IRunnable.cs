using UnityEngine;

//Locomotion
//이동할 수 있는 친구들!
public interface IRunnable
{
	//                                      도착이라고 인정하는 거리!
	public void MoveToDestination(Vector3 destination, float tolerance);
	public void MoveToDirection(Vector3 direction);
	public void StopMovement();
}
