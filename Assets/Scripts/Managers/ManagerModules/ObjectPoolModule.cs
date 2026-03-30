using System.Collections.Generic;
using UnityEngine;

public class ObjectPoolModule
{
	//오브젝트 하나를 관리하는 하청 모듈!
	//오브젝트 하나를 담당할 거라면, 어떤 것들이 필요할까?
	//내가 뭘 하면 되는데?
	PoolSetting _setting;
	public PoolSetting Setting => _setting;

	Transform rootTransform;

	//"대기열"을 만들 겁니다!
	//                                          선  입  선  출
	//                                Queue => 먼저 온 애가 먼저 간다
	//"대기열"에 스스로 들어가는 경우! => 큐를 잡는다! / 돌린다!
	//진열대 => 밖에 있는 거를 먼저 가져와요
	//          맨 마지막에
	//          후   입         선   출 => Stack
	Queue<GameObject> prepareQueue = new();

	//작업중인 애들은 왜 Queue나 Stack이 아니죠?
	//가는데 순서 없다
	//List<GameObject> inProgressList = new();

	//구인할 때, "4년제 대학 이상, 슬라임 조련 자격증 2급 이상, 슬라임 배양소 경력 3년 이상"
	//생성할 때 정보를 넣어주기!
	//생성자! 반환값 => 본인, 이름 => 본인
	public ObjectPoolModule(PoolSetting newSetting)
	{
		_setting = newSetting;
	}

	public void Initialize()
	{
		rootTransform = new GameObject(Setting.poolName).transform;

		//게임 하면서 미니언을 30개 쓸 거니까! 그 만큼 준비를 미리 해놓아야지!
		for(int i = 0; i < Setting.countInitial; i++)
		{
			//새로운 오브젝트를 미리 대기시킬 거임!
			//탱커 7명 대기해주세요.
			//드럼통 앞에 불 쬐고 있는 친구 세명
			PrepareObject();
		}
	}

	//대기자! => 관리를 해주려면!
	//대기하고 있는 애들 중에 아무나 데려와도 되는가?
	//새벽 6시 출근, 아침 9시 출근, 아침 10시 출근
	//1명 데려가려고 함 => 아침 9시 너 나와!
	//먼저 온 애는???
	GameObject PrepareObject()
	{
		if(!Setting.target) return null;
		GameObject result = ObjectManager.CreateObject(Setting.target, rootTransform);
		if(result)
		{
			result.SetActive(false);
			//부모 - 자식 관계를 만들기!
			//폴더처럼 쓸 수 있는 것을 만들어볼게요!
			result.name = Setting.poolName;
			//대기열에 넣기!
			prepareQueue.Enqueue(result);
		}
		return result;
	}

	//오브젝트를 생성해달라고 부탁!
	public GameObject CreateObject()
	{
		//어떻게 하는 걸까?
		//대기자 중에서 꺼내보기!
		GameObject result;
		//대기열에 아무도 없을 때
		if(!prepareQueue.TryDequeue(out result))
		{
			//새로 대기자를 뽑아서 가져오면 됩니다!
			PrepareObject();
		}

		return result;
	}

	//오브젝트를 제거해달라고 부탁!
	public void DestroyObject(GameObject target)
	{

	}
}
