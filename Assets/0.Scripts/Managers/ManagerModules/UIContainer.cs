using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class UIContainer<UIKeyType, UIClassType> where UIClassType : UIBase
{
	//어떤 창을 열어주세요!
	//         이 타입  어떤 오브젝트!
	Dictionary<UIKeyType, UIClassType> contains = new();
	UIManager owner;

	public UIContainer(UIManager from)
	{
		owner = from;
	}

	public UIClassType CreateUI(UIKeyType wantType, string wantName, Transform parent)
	{
		GameObject instance = ObjectManager.CreateObject(wantName, parent);
		UIClassType result = instance?.GetComponent<UIClassType>();
		return SetUI(result, wantType);
	}

	public UIClassType CreateUI(UIKeyType wantType, string wantName, Transform targetParent, Transform fallBackParent, UIBase movableScreen)
	{
		UIClassType result = CreateUI(wantType, wantName, targetParent ? targetParent : fallBackParent);
		//만약 Draggable이 가능한 친구라면 이동 스크린으로 가라!
		if (result?.GetComponentInChildren<UI_DraggableWindow>()) movableScreen?.SetChild(result.gameObject);
		return result;
	}

	
	public UIClassType CreateUI(string wantName, Transform parent)
	{
		GameObject created = ObjectManager.CreateObject(wantName, parent);
		if (!created) return null;
		if (created.TryGetComponent(out UIClassType result))
		{
			result.Registration(owner);
			return result;
		}
		else return null;
	}
	

	public UIClassType CreateUI(string wantName, Transform targetParent, Transform fallBackParent, UIBase movableScreen)
	{
		GameObject created = ObjectManager.CreateObject(wantName, targetParent ? targetParent : fallBackParent);
		if (!created) return null;
		if (created.TryGetComponent(out UI_DraggableWindow draggable))
		{
			draggable.Registration(owner);
			if (movableScreen) movableScreen.SetChild(created);
			return draggable as UIClassType;
		}
		else if (created.TryGetComponent(out UIClassType result))
		{
			result.Registration(owner);
			return result;
		}
		else return null;
	}
	

	public void UnSetAllUI() // 싹 다 해고야
	{
		foreach (UIClassType ui in contains.Values) //애들 전부 돌면서
		{
			UnsetUI(ui);//나가라고 해주기!
						//여기에서 나가라고 할 때마다 Dictionary에서 빼려고 하시는 분들이 있어요!
						//안되는 이유!
						//contains.Remove(wantType);
						//제거를 하는 경우 contains의 모양이 달라져서 모두를 돌다가...?
						//A,B,C,D,E,F
						//0 1 2 3 4 5 : 6명

			//A,B,C,D,E,F
			//0
			//B,C,D,E,F

			//B,C,D,E,F
			//  1
			//B, ,D,E,F

			//B,D,E,F
			//    2
			//B,D, ,F

			//B,D,F
			//      3
		}
		//다 나갔으니까 직원 명부를 버려버림!
		contains.Clear();
	}
	public void UnsetUI(UIKeyType wantType) //담당 공무원의 부서랑 직책만 알고 있는 경우
	{
		//그 직원을 찾아야 함
		//담당 공무원의 이름을 알고 있는 경우로 이동하시오.
		if (contains.TryGetValue(wantType, out UIClassType found))
		{
			//처리하고
			UnsetUI(found);
			//너 해고야.
			contains.Remove(wantType);
		}
	}
	public void UnsetUI(UIClassType wantUI) //담당 공무원의 이름을 알고 있는 경우
	{
		if (!wantUI) return;

		wantUI.Unregistration(owner);
	}
	
	public UIClassType SetUI(UIClassType wantUI)
	{
		wantUI?.Registration(owner);
		return wantUI;
	}

	public UIClassType SetUI(UIClassType wantUI, UIKeyType wantType)
	{
		//Set UI를 하려고 하는데 문제가 무엇일까!
		//InventoryType, InventoryInstance
		if (wantUI == null) return null; // 승상께서 나를 더 필요로 하시지 않는구나

		//어? 뭐야? 이미 Inventory는 있는데? 너는 누구냐! => 서생원
		//일단 문전박대 => 프로그래밍에서는요? 똑같은 기능을 하는 친구면
		//음.. 너가 원본인 건 무슨 상관인데?
		//뒤이어서 들어온 친구는 치워버리겠다!
		if (contains.TryGetValue(wantType, out UIClassType origin)) return origin;

		//두 가지의 시련을 모두 통과하다니. 너는 등록될 수 있는 자격을 갖추었다.
		contains.Add(wantType, wantUI);
		//등록 완!
		return SetUI(wantUI);
	}
	
	
	

	public UIClassType GetUI(UIKeyType wantType)
	{
		if (contains.TryGetValue(wantType, out UIClassType result)) return result; //있으면 result반환
		else return null; //없으면 null
	}
	

	public bool IsOpen(UIKeyType wantType)
	{
		IOpenable resultOpenable = default;
		UIClassType target = GetUI(wantType);
		if (!target) return false;
		resultOpenable = target as IOpenable;
		if (resultOpenable is not null) return resultOpenable.IsOpen;
		return target.gameObject.activeSelf;
	}
	public bool IsOpen(UIKeyType wantType, out IOpenable resultOpenable)
	{
		resultOpenable = default;
		UIClassType target = GetUI(wantType);
		if (!target) return false;
		resultOpenable = target as IOpenable;
		if (resultOpenable is not null) return resultOpenable.IsOpen;
		return target.gameObject.activeSelf;
	}

	public bool CloseUI(UIToggleEvent<UIKeyType> CallBack, params UIKeyType[] wantTypes)
	{
		foreach (UIKeyType wantType in wantTypes)
		{
			if (IsOpen(wantType, out IOpenable resultOpenable))
			{
				if (resultOpenable is null || !resultOpenable.IsNeedClose) continue;
				resultOpenable.Close(true);
				CallBack?.Invoke(wantType, false);
				return true;
			}
		}
		return false;
	}

	

	public UIClassType OpenUI(UIKeyType wantType, UIToggleEvent<UIKeyType> CallBack, bool isActiveByKey = false)
	{
		//Result가 누군지 전혀 모름!  리스코프 치환 원칙
		//IOpenable이면 열게 해준다! 세부 요소는 모르겠는데, 상위 요소만으로 실행하기
		UIClassType result = GetUI(wantType);
		//이게 "열 수 있는"인 건 어떻게 확인할까요?
		//IOpenable인지 체크해보면 열 수 있는지 알 수 있습니다.
		//IOpenable로서 활동 할 수 있으면 IOpenable
		//result는 IOpenable인 opener인가?
		if (result is IOpenable asOpenable)
		{
			asOpenable.Open(isActiveByKey);
			CallBack?.Invoke(wantType, true);
		}

		if (result) EventSystem.current.SetSelectedGameObject(result.gameObject);

		//아랫줄이랑 같은 의미예요!
		//IOpenable opener = result as IOpenable;
		//if(opener != null) opener.Open();
		return result;
	}

	public UIClassType CloseUI(UIKeyType wantType, UIToggleEvent<UIKeyType> CallBack, bool isActiveByKey = false)
	{
		UIClassType result = GetUI(wantType);
		//             자료형    이름   =>  변수 생성
		if (result is IOpenable asOpenable)
		{
			asOpenable.Close(isActiveByKey);
			CallBack?.Invoke(wantType, false);
		}
		return result;
	}
	

	public UIClassType ToggleUI(UIKeyType wantType, UIToggleEvent<UIKeyType> CallBack, bool isActiveByKey = false)
	{
		UIClassType result = GetUI(wantType);
		if (result is IOpenable asOpenable)
		{
			bool isOpened = asOpenable.Toggle(isActiveByKey);
			CallBack?.Invoke(wantType, isOpened);
		}
		return result;
	}
	

}
