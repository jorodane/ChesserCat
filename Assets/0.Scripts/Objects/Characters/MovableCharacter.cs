using UnityEngine;

//Pawn : 조종할 수 있지만 이동할 수 없는 캐릭터
//Character : 조종하고 이동 기능이 있는 캐릭터

public class MovableCharacter : CharacterBase, IRunnable, IFunctionable
{
	[SerializeField] Animator anim;

	protected Vector3? targetDirection   = null;
	protected Vector3? targetDestination = null;
	protected float	  targetTolerance;

	public void RegistrationFunctions()
	{
		//매 프레임마다 작동하는 것 Update => 그냥 프레임
		//                                         물리 계산을 할 때에 시간이 들쭉날쭉하면
		//                                         조금 갔다가 멀리 갔다가 그러면 중간에 벽을 뚫어버릴 수도 있음
		//                                         컴퓨터 연산이 늦어지는 경우 예를 들어 13초동안 컴퓨터가 자동을 안했다!
		//                                         13초를 보정해줘야 하는데 기준이 있어야 해요!
		//                                         0.02초마다 한다는 가정이 있다면 650번을 몰아서 하면 된다!
		//물리를 작동시키는 용도로 사용하는 Update => FixedUpdate
		GameManager.OnPhysicsCharacter -= MovementUpdate;
		GameManager.OnPhysicsCharacter += MovementUpdate;
	}

	public void UnregistrationFunctions()
	{
		GameManager.OnPhysicsCharacter -= MovementUpdate;
	}

	public void MovementUpdate(float deltaTime)
	{
		Vector3 originPosition = transform.position;				//이동하기 전에 제 위치를 저장!
		PhysicsUpdate(deltaTime);									//물리 업데이트를 하고
		Vector3 positionDelta = transform.position - originPosition;//이동한 위치의 차이를 계산!
		AnimationUpdate(positionDelta);								//이동한 양에 따라서 애니메이션을 업데이트!
	}

	public void AnimationUpdate(Vector3 moveDelta)
	{
		if (!anim) return;
		anim.SetFloat("MoveX",		LookRotation.x);
		anim.SetFloat("MoveY",		LookRotation.y);
		anim.SetFloat("MoveSpeed",  moveDelta.magnitude / Time.fixedDeltaTime);
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
		//움직일 때에 해당 방향을 바라보도록!
		if(delta.sqrMagnitude > 0) _lookRotation = delta.normalized;
	}

	public void UpdateToDirection(float deltaTime)
	{
		if (targetDirection is null) return;
		float currentMoveSpeed = GetMoveSpeed(deltaTime);
		Translate(currentMoveSpeed * targetDirection.Value);
	}

	public void UpdateToDestination(float deltaTime)
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

	public void MoveToDestination(Vector3 destination, float tolerance)
	{
		targetDirection = null; //방향으로는 움직이지 않겠다!
		targetDestination = destination;
		targetTolerance = tolerance;
	}

	public void MoveToDirection(Vector3 direction)
	{
		targetDestination = null; //목적지를 제거한다!
		targetDirection = direction.normalized;
	}

	public void StopMovement()
	{
		targetDestination = null; //목적지를 제거한다!
		targetDirection = null; //방향으로는 움직이지 않겠다!
	}


}
