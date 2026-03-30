using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//오브젝트를 생성하고 제거하는 것은 오래 걸리는 작업이 맞습니다!
//제거하고 난 뒤에는 문제가 크게 생깁니다
//무언가를 제거하는 건 항상 신중하게!
//C#의 오브젝트는 삭제 되는 기능이 존재하지 않는다!
//사람이 언제 죽는지 아나? => 잊혀졌을 때 : 이 친구를 저장하고 있는 오브젝트가 없을 때
//기억하는 사람이 아무도 없어지면 => 쓰레기장으로 갑니다
//Garbage가 됩니다. => Garbage Collector => 폐품 수집가 => 주기적으로 순찰을 해요!
//나 이제 없어졌어! 라고 주장하는 친구들을 학살합니다.
//내가 삭제되었어! 라고 주장하는 애들이 많아지면 많아질수록 이 친구의 일은 많아진다!
//프리렌 "남부의 용사" => 내가 잊혀지더라도 힘멜의 길을 열어주겠다!
//남부의 용사를 기리는 마을이 남아있었다! => 쓰레기가 아니었습니다!
//가비지컬렉터의 역할 : 얘가 쓰레기인지 판별도 해야해요!
//                     어떻게 할 것 같은가요? => 이 세상에 얘를 기억하고 있는 애가 있는지 체크
//                     걔를 알고 있을 법한 모두한테 가서 "기억하고 있어?"라고 물어봐야 해요!
//                     성능을 오지게 잡아먹는다!
//만들어진다 없어진다 하는 과정이 있으면 힘드니까 => 안 할 수 있는 방법!
//없애지 않으면 됩니다. => 오브젝트를 껐다 켰다로 대체한다!
//오브젝트 풀링
//만드는 과정을 인게임중에는 안 하고 로딩할 때 해버리고 싶다!
//매번 만들기 싫으니까 한 캐릭터 50000개 만들어두면 잘 쓸 수 있지 않을까요?
//웬만하면은 이 친구가 "일반적인 상황"에서 나올 수 있는 최대 개수
//리그 오브 레전드에는 "대포 미니언"이라는 것이 있습니다.
//1웨이브당 한번 나온다 => 잡히는 데에 걸리는 시간, 조건 => 10마리
//"마법사 미니언" => 60마리
//없을 수 없으면 struct
//Pooling을 위한 설정!
//                         직렬연결
//                       Serial Number는 숫자가 연속적으로 나열되어 있는 것
//                      직렬화할수있는   직렬화      직렬
[System.Serializable] // Serializable Serialize => Serial
                      // 데이터를 한줄로 쭉 뽑아볼 수 있다! => 저장, 전송, 해석
public struct PoolSetting
{
	public string poolName;	   // 이 풀링 정보를 어떤 이름으로 보고 싶은가?
	public GameObject target;  // 풀링할 대상 원거리 미니언
	public int countInitial;   // 처음에 준비할 개수 60마리
	public int countAdditional;// 부족하면 추가할 개수
}

public class ObjectManager : ManagerBase
{
	//직렬화가능한 => 유니티에서 보기 위해서 쓴 것!
	//public이라고 하는 건 사실 필요 없고 직렬화만 되면 유니티에서 볼 수 있다!
	//직렬화 변수
	//[SerializeField] PoolSetting[] testSettings;

	//PoolRequest가 있고, 그것을 위한 풀링을 준비하기
	//PoolRequest를 가져와서 저장하려면 어떤 자료구조가 필요할까?
	//리스트 : 배열과 비슷한데 추가 제거가 쉬움	, 용량△, 찾는 속도가 느리다
	//추가 제거가 많고, 전체를 도는 일이 적은

	//배열 : 리스트와 비슷한데 추가 제거가 어려움, 용량▽, 찾는 속도가 빠르다
	//추가 제거가 적고, 전체를 도는 일이 많은

	//PoolRequest는.. 얼마나 자주 추가될까? => 로딩할 때 즈음?
	//로딩되는 횟수보다 대상이 개수가 부족하면 새로 추가하거나 하는 일!
	List<PoolRequest> loadedPoolRequests = new();

	//해당하는 이름의 대상으로 불러주기 위해서
	//[이름 - 게임오브젝트] 자료구조
	Dictionary<string, ObjectPoolModule> poolDictionary = new();

	//PollRequst에서 그 안에서 string으로 찾아서 그 이름에 맞는 GameObject를 찾으면 되니까!
	//만약 같은 이름으로 똑같은 오브젝트를 만들려고 했는데..
	protected override IEnumerator OnConnected(GameManager newManager)
	{
		RegistrationPool("GlobalCharacterPool");
		RegistrationPool("GlobalControllerPool");
		RegistrationPool("GlobalEffectPool");
		RegistrationPool("GlobalObjectPool");
		RegistrationPool("GlobalUIPool");

		InitializePool();

		yield return null;
	}

	protected override void OnDisconnected()
	{

	}

	//                                                      부모게임오브젝트는 "Transform"으로 저장함!
	public static GameObject CreateObject(GameObject prefab, Transform parent = null)
	{
		if (prefab == null) return null;

		//                                      누가 주인인가
		GameObject result = Instantiate(prefab, parent); //만들고
		RegistrationObject(result); //등록함
		return result;
	}

	public static GameObject CreateObject(GameObject prefab, Vector3 position)
	{
		GameObject result = CreateObject(prefab);
		if(result) result.transform.position = position;
		return result;
	}

	public static GameObject CreateObject(GameObject prefab, Vector3 position, Quaternion rotation)
	{
		GameObject result = CreateObject(prefab);
		if (result)
		{
			result.transform.position = position;
			result.transform.rotation = rotation;
		}
		return result;
	}

	public static GameObject CreateObject(GameObject prefab, Vector3 position, Quaternion rotation, Vector3 scale)
	{
		GameObject result = CreateObject(prefab);
		if (result)
		{
			result.transform.position = position;
			result.transform.rotation = rotation;
			result.transform.localScale = scale;
		}
		return result;
	}

	public static GameObject CreateObject(GameObject prefab, Transform parent, Vector3 position, Space space = Space.Self)
	{
		GameObject result = CreateObject(prefab, parent);
		if (result)
		{
			switch(space)
			{
				case Space.World:
					result.transform.position = position; //절대값을 기준으로
					break;
				case Space.Self:
					result.transform.localPosition = position; //부모를 기준으로
					break;
			}
		}
		return result;
	}

	public static GameObject CreateObject(GameObject prefab, Transform parent, Vector3 position, Quaternion rotation, Space space = Space.Self)
	{
		GameObject result = CreateObject(prefab, parent);
		if (result)
		{
			switch (space)
			{
				case Space.World:
					result.transform.position = position; //절대값을 기준으로
					result.transform.rotation = rotation;
					break;
				case Space.Self:
					result.transform.localPosition = position; //부모를 기준으로
					result.transform.localRotation = rotation; //부모를 기준으로
					break;
			}
		}
		return result;
	}

	public static GameObject CreateObject(GameObject prefab, Transform parent, Vector3 position, Quaternion rotation, Vector3 scale, Space space = Space.Self)
	{
		GameObject result = CreateObject(prefab, parent);
		if (result)
		{
			switch (space)
			{
				case Space.World:
					result.transform.position = position; //절대값을 기준으로
					result.transform.rotation = rotation;
					result.transform.localScale	= scale; //부모를 기준으로
					//"진짜 월드 스케일" 을 본 적 있습니다.
					// lossyScale
					// 저는 "진짜"크기는 (1,1,1)
					// 근데 "부모"크기는 (2,2,2)
					// 이때 "로컬"크기는 (0.5, 0.5, 0.5)
					// 저의 "진짜" 크기를 => (3,3,3)으로 하고 싶다고 해보죠
					// 설정할 "로컬"크기는 몇일까요? (1.5, 1.5, 1.5)
					// 물론 회전하는 순간 끔살
					// 어떻게 제가 원하는 "진짜" 크기가 되기 위한 "로컬"크기를 구할 수 있을까?
					// "진짜 크기"를 원한다 "부모 크기"랑 비교를 해서 값을 가져주면 좋겠다!
					// 여기서 문제! 부모의 부모는 어떻게 해요?
					// 조부모 부모  로컬    월드크기       월드크기
					// 0.5    1.5  1.2  => 0.9        =>  3
					// 0.5    1.5  4    => 3
					// 로컬 => 월드  로컬 * (월드 / 로컬) = 월드
					//               1.2 *  (0.9 / 1.2) = 0.9
					// 월드 => 로컬  월드 * (로컬 / 월드) = 로컬
					//               0.9 *  (1.2 / 0.9) = 1.2
					//               3   *  (4/3)       = 4
					//벡터는 행렬이기 때문에, 행렬 곱을 할 수 있는 조건은 => xy행렬이라면 yx행렬이어야 한다
					//           (0)
					// (1,2,3) x (1)
 					//           (2)
					//Vector3 originLocalScale = result.transform.localScale;
					//Vector3 originLossyScale = result.transform.lossyScale;
					//float scaledScaleX = scale.x * (originLocalScale.x / originLossyScale.x);
					//float scaledScaleY = scale.y * (originLocalScale.y / originLossyScale.y);
					//float scaledScaleZ = scale.z * (originLocalScale.z / originLossyScale.z);
					//result.transform.localScale = new Vector3(scaledScaleX, scaledScaleY, scaledScaleZ);
					break;
				case Space.Self:
					result.transform.localPosition  = position; //부모를 기준으로
					result.transform.localRotation  = rotation; 
					result.transform.localScale		= scale; 
					break;
			}
		}
		return result;
	}

	public static void RegistrationObject(GameObject target) //실제로 등록하는 기능
	{
		if (target)
		{
			//이 친구가 등록 가능한지를 어떻게 체크할까?
			//저희가 만드는 건 "컴포넌트"를 만드는 것이지
			//"게임 오브젝트"를 만드는 것이 아니기 때문에
			//IFunctionable이 들어간 곳은 "컴포넌트"다!
			//GetComponent : 컴포넌트를 가져옴 (제일 첫번째 컴포넌트)
			//GetComponent<IFunctionable>() => IFunctionable 하나
			//GetComponents<IFunctionable>() => IFunctionable을 상속받는 모든 컴포넌트
			//GetComponentsInChild<IFunctionable>() => (나포함) 자식한테 있는 IFunctionable을 상속받는 모든 컴포넌트
			//GetComponentsInChildren<IFunctionable>() => (나포함)자식들한테 있는 IFunctionable을 상속받는 모든 컴포넌트
			foreach (var current in target.GetComponentsInChildren<IFunctionable>())
			{
				current.RegistrationFunctions();
			}
		}
	}

	public static void DestroyObject(GameObject target)
	{
		if (!target) return;
		UnregistrationObject(target);
		Destroy(target);
	}

	public static void UnregistrationObject(GameObject target)
	{
		if (!target) return;

		foreach (var current in target.GetComponentsInChildren<IFunctionable>())
		{
			current.UnregistrationFunctions();
		}
	}

	public void RegistrationPool(string poolName)
	{
		//명령!
		PoolRequest currentRequest = DataManager.LoadDataFile<PoolRequest>(poolName);
		if (currentRequest == null) return;
		loadedPoolRequests.Add(currentRequest);
		//애들마다 하나씩!
		//        학생          다음학생    in   3학년 4반
		foreach (PoolSetting currentSetting in currentRequest.settings)
		{
			string currentName = currentSetting.poolName;
			GameObject currentPrefab = currentSetting.target;
			//다음학생이.. 오늘 학교 안왔대요!
			//=> 헌혈차를 접어버리면 안되고
			//=> 다음 학생을 불러야 한다!
			if (currentPrefab == null) continue;
			//문제가 생길 여지가 하나 더 있다!
			//프리팹을 찾아봤으니까, 이름에서 문제가 생길 수 있는 여지!
			//딕셔너리에는 같은 키값을 두 개 넣을 수 없다!
			if (poolDictionary.ContainsKey(currentName)) continue;
			//나의 시련을 모두 통과하다니. 너를 정식 기사로 임명해주마
			poolDictionary.Add(currentName, new(currentSetting));
		}
	}

	public void InitializePool()
	{
		foreach(ObjectPoolModule currentPool in poolDictionary.Values)
		{
			currentPool?.Initialize();
		}
	}
}
