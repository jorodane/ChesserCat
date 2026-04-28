using UnityEngine;

public class MovementModule : CharacterModule, IRunnable
{
	protected Vector3? targetDirection = null;
	protected Vector3? targetDestination = null;
	protected float targetTolerance;

	//이런 거대한 모듈을 만들 때에 한 번 "대분류"로 분류하기!
	//이후 세대 여기에 손을 못대게 처리하는 방법!
	//옛날의 어떤 용사가 "봉인"
	public sealed override System.Type RegistrationType => typeof(MovementModule);

	public override void OnRegistration(CharacterBase newOwner)
	{
		base.OnRegistration(newOwner);
		GameManager.OnPhysicsCharacter -= MovementUpdate;
		GameManager.OnPhysicsCharacter += MovementUpdate;
	}
	public override void OnUnregistration(CharacterBase oldOwner)
	{
		base.OnUnregistration(oldOwner);
		GameManager.OnPhysicsCharacter -= MovementUpdate;
	}
	public void MovementUpdate(float deltaTime)
	{
		Vector3 originPosition = transform.position;                //이동하기 전에 제 위치를 저장!
		PhysicsUpdate(deltaTime);                                   //물리 업데이트를 하고
		Vector3 positionDelta = transform.position - originPosition;//이동한 위치의 차이를 계산!
	 	Owner.MovementNotify(positionDelta);                             //이동한 양에 따라서 애니메이션을 업데이트!
	}
	public void PhysicsUpdate(float deltaTime)
	{
		UpdateToDirection(deltaTime);
		UpdateToDestination(deltaTime);
	}
	public virtual float GetMoveSpeed() => 5.0f;
	public virtual float GetMoveSpeed(float deltaTime) => GetMoveSpeed() * deltaTime;
	public virtual void Translate(Vector3 delta)
	{
		transform.position += delta;
	}
	public virtual void UpdateToDirection(float deltaTime)
	{
		if (targetDirection is null) return;
		float currentMoveSpeed = GetMoveSpeed(deltaTime);
		Translate(currentMoveSpeed * targetDirection.Value);
	}
	public virtual void UpdateToDestination(float deltaTime)
	{
		// 목적지가 없으면 리턴
		if (targetDestination is null) return;

		//여길 넘어서면 목적지가 있는 상태인 것!

		//해당 위치로 조금씩 가는 법!
		//목적지 - 출발지
		Vector3 currentMoveDirection = (targetDestination.Value - transform.position);
		//일단 얼마나 더 가야 해요?
		float distance = currentMoveDirection.magnitude;
		// 거리가        인정범위 밖
		if (distance > targetTolerance)
		{
			//방향을 잡아봅시다!
			currentMoveDirection.Normalize();

			//1.문제 정의
			//  한 번 이동할 때 거리가 정해져 있음 => 그 보다 작은 거리를 움직일 수가 없다!
			float currentMoveSpeed = GetMoveSpeed(deltaTime);
			//2.거리를 구해야 하는데, 언제 그 보다 작은 거리를 움직여야 하는가? 
			//  제가 지금 이동하는 거리가 남은 거리보다 클 때에!
			//               0.1          0.05
			//이동할 거리랑 남은 거리 중에서 더 짧은 거리를 가면 된다!
			//    0.1       10         =      0.1
			//    10        0.5        =      0.5
			float resultMoveSpeed = Mathf.Min(currentMoveSpeed, distance);

			//지금 이 프레임에 나는 몇m를 갈 수 있을까?
			//        2    30km/h = 60km
			//       0.1          = 3km
			//거리 = 시간 * 속력           거리      * 방향
			Translate(resultMoveSpeed * currentMoveDirection);
		}
	}

	public virtual void MoveToDestination(Vector3 destination, float tolerance)
	{
		targetDirection = null; //방향으로는 움직이지 않겠다!
		targetDestination = destination;
		targetTolerance = tolerance;
	}
	public virtual void MoveToDirection(Vector3 direction)
	{
		targetDestination = null; //목적지를 제거한다!
		targetDirection = direction.normalized;
	}
	public virtual void StopMovement()
	{
		targetDestination = null; //목적지를 제거한다!
		targetDirection = null; //방향으로는 움직이지 않겠다!
	}

}
