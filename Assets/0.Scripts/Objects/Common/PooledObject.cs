using UnityEngine;

public delegate void PoolEnqueueEvent(GameObject target);
public delegate void PoolDequeueEvent(GameObject target);

public class PooledObject : MonoBehaviour
{
	//event는 왜 써야 하죠?
	//   delegate와       event의 차이!  => event는 delegate의 일종!
	//남들이 구독/실행   남들이 구독
	//     행동           이벤트
	public event PoolEnqueueEvent OnEnqueueEvent;
	public event PoolDequeueEvent OnDequeueEvent;

	//오브젝트 풀링..이라고 하는 게 뭐였더라?
	//생성 / 삭제를 하는 대신 => 켜고 끄는 걸로 대체!
	//안좋아지는 게 있는 거 아닐까요?
	//삭제하는 것의 미학 => 정보의 유지
	//눈 앞에 몬스터가 있습니다.
	//몬스터는 유저를 공격한다.
	//회색늑대A가 아스트리드를 공격하고 있었다!
	//그 회색늑대A가 죽었다!
	//회색 늑대들이 스폰하다보니까 차례가 회색늑대A차례가 되었다!
	//눈을 뜬 회색 늑대는.. 뭘 할까?
	//아스트리드를 공격하러 갑니다.
	//회귀했다! => 생명력은 초기화되었을까? ㅎㅎ
	//죽은 상태로 부활해서 아스트리드를 잡으러 가니까 => 망겜

	//큐로 돌아갈 때 할 일
	public void OnEnqueue()
	{
		//제가 이거.. 이벤트가 없을 수 있다!
		//집으로 돌아가는 기능을 넣었단 말이죠?
		//집으로 돌아가는 기능이 없는 친구는 어떻게 될까?
		//1시간 안에 일을 마치지 못하고 서성이던 벌은.. 죽음만이 기다릴 뿐
		if(OnEnqueueEvent != null)	OnEnqueueEvent.Invoke(gameObject);
		else Destroy(gameObject);
	}

	//큐에서 나올 때 할 일
	public void OnDequeue()
	{
		OnDequeueEvent?.Invoke(gameObject);
	}
}
