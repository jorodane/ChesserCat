using System.Collections;
using UnityEngine;

public class DataManager : ManagerBase
{
	//프로퍼티는 변수모양이지만 함수
	//					int GetLoadCount();
	public override int LoadCount => 100;

	protected override IEnumerator OnConnected(GameManager newManager)
	{
		//나는 로딩 스크린이 어떻게 생겼는지 모른다.
		//하지만 로딩 스크린을 업데이트해주고싶다.
		UIBase loading = UIManager.ClaimGetUI(UIType.Loading);
		IProgress<int> progressUI = loading as IProgress<int>;
		IStatus<string> statusUI = loading as IStatus<string>;

		//Interface : 연결고리 => 무엇이 무엇을 사용할 수 있도록 열어주는 기능
		//            GUI : 그래픽 보여줌, 마우스 움직임, 누르 떼기, 클릭하기, 드래그
		//윈도우를 하다가, 맥으로 넘어간다! => 클릭하기 어려울까요?
		//이게 "클릭"이야 => GUI는 클릭이 가능하구나! => GUI이기만 하면 클릭을 지원하겠구나!
		//"어떤 기능이 있을 거야"라는 [약속]이 바로 Interface
		//IOpenable => 열기, 닫기, 토글, 열렸는지 확인도 가능하다!

		//로딩 진행율 => 최대 몇 개인지, 현재 몇 개까지 했는지
		//              현재 / 최대      1 / 100 = 0.01
		//10개
		for(int i = 0; i < LoadCount; i+= 7)
		{
			//반복할 때 17을 좋아하는 이유가 뭘까?
			//00  0
			//07  7
			//14  4
			//21  1
			//28  8
			//35  5
			//42  2
			//49  9
			//56  6
			//63  3
			progressUI?.AddCurrent(7);
			statusUI?.SetCurrentStatus($"Load Data ({i + 1}/{LoadCount})");
			yield return new WaitForSeconds(0.05f);
		}
		yield return null;
	}

	protected override void OnDisconnected()
	{

	}

	//파일을 가지고 올 건데, "경로"로 가져오는 것이 중요한 이유!
	//Resources => 유니티에서 Resources폴더를 만들고 나면 사용할 수 있다!
	// Resources/Prefabs/Square
	//드래그 - 드롭으로 넣는게 아니라 파일 경로로 찾는 이유는 무엇일까요?
	//파일이 많으면 드래그하는 데에 한 세월 걸림
	//폴더 째로 로드가 가능하다
	//폴더 내부에 있는 파일을 다른 사람(프로그래머 외)이 수정해도 괜찮다.
	// => 원래 지정되었던 게 전부 풀리고 => 새로 들어온 건 그냥 멀뚱멀뚱 있음
	//기획 문서를 가지고 사람들이 무언가를 찾을 수 있습니다.
	//프로그래밍 팀 따로, 아트 팀 따로, 사운드 팀 따로 ..
	//프로그래밍 팀은 아트가 아직 안들어와도 진행해도 된다.
	//프로그래밍 팀이 그냥 "경로"를 설정해놓고 (예외처리만) 담날 왔습니다.
	//근데 원래 이미지가 없었는데 오늘 켜봤더니 이미지가 적용되어있다!
	bool TryGetFileFromResources<T>(string path, out T result) where T : Object
	{
		//Resources.LoadAll<T>(path);
		result = Resources.Load<T>(path);
		return result != null;
	}

	//1. 경로로 찾는 건 좋은 거라서
	//2. 경로로 찾을 수밖에 없어서
	//파일을.. 클라이언트가 모두 가지고 있을 수 있는가 여부
	//모바일 애플리케이션 => 플레이스토어에서 200mb까지
	//컨텐츠 추가 다운로드 중...
	//Asset Bundle => 경로 (제가 임의로 지정한 카테고리)
	//DLC => 특정 카테고리에 있는 요소를 다운로드 하게 할 것인가 말 것인가?
	//Addressable
	bool TryGetFileFromAssetBundle()
	{
		return false;
	}
}