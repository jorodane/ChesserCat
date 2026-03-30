using System.Collections;
using System.Threading.Tasks;
using UnityEngine;

//확장 메소드들을 가지고 있을 친구들!
//영토 확장
//확장판
//=>추가 컨텐츠
//원본 오리지널 게임에서는 없었던 것을 추가해주는 개념!
//내가 원하는 클래스에 내가 원하는 기능을 추가해줄 수 있다!
//=>객체화가 필요한 친구일까?
public static class Extensions
{
	//숫자놀이!
	//연산을 하는 놀이!
	//Normalize => 정규화 => 방향을 유지한 채 크기를 1로 만드는 것
	//벡터에만 있잖아요?
	//Float에도 만들어보면 어떨까?
	//Normalize는 본인을 1로 바꿔버리는 것이고
	//normalized는 크기가 1인 값을 돌려주는 것
	//float를 정규화하면 어떤 모양이 될까?
	// 13.0f =>  1.0f
	// -2.0f => -1.0f
	// 0.0f  =>  0.0f
	// float => float
	//                             내가 함수를 넣고 싶은 대상
	//                             this
	public static float normalized(this float target)
	{
		if (target > 0)		 return 1.0f;
		else if (target < 0) return -1.0f;
		else				 return 0.0f;
	}

	//Try Add Component => 추가를 시도 => 있는지 확인 => 없으면 추가
	public static T TryAddComponent<T>(this GameObject target) where T : Component
	{
		T result = null;
		if (target == null) return result; //RVO

		result = target.GetComponent<T>() ?? target.AddComponent<T>();

		//result = target.GetComponent<T>();
		//result ??= target.AddComponent<T>();

		//result = target.GetComponent<T>();
		//if (result is null) result = target.AddComponent<T>();

		return result;
	}

	public static T TryAddComponent<T>(this Component target) where T : Component
	{
		if(target == null)  return null;
		else				return target.gameObject.TryAddComponent<T>(); //NRVO
		//						   TryAddComponent<T>(target.gameObject);
	}

	public static IEnumerator WaitForTask(this Task targetTask)
	{
		//WaitWhile : true인 동안 작동함!
		//WaitUntil : false인 동안 작동함! => true가 될 때까지 기다림!
		//             기다린다 전까지         타겟작업이 끝나기
		//              Wait Until Target Task Is Completed
		yield return new WaitUntil(() => targetTask.IsCompleted);
		//작업을 제거하다
		targetTask.Dispose();
	}
}
