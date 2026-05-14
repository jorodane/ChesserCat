using System.Collections;
using UnityEngine;

public class SettingManager : ManagerBase
{
	protected override IEnumerator OnConnected(GameManager newManager)
	{
		//스마트폰의 화면 방향은 가로 세로 역가로 역세로 => 허용
		Screen.autorotateToLandscapeLeft	  = true;	// 카메라가 왼쪽
		Screen.autorotateToLandscapeRight	  = true;	// 카메라가 오른쪽
		Screen.autorotateToPortrait			  = false;	// 카메라가 위쪽
		Screen.autorotateToPortraitUpsideDown = false;  // 카메라가 아래쪽

		//화면 기본 설정! 자동 회전이 안되는 경우에는 이걸로 하면 됩니다!
		Screen.orientation = ScreenOrientation.LandscapeLeft;

		//원하는 프레임 레이트도 조정하시면 좋아요!
		Application.targetFrameRate = 60;

		//게임하다가 화면을 클릭을 오래 안하는 게임도 있잖아요?
		//컷씬을 보게 되는 경우도 있잖아요?
		//스크린이 얼마나 오랫동안 터치가 안되면 꺼질지!
		//SleepTimeout.SystemSetting => 시스템 세팅에 따름
		//SleepTimeout.NeverSleep => 이 설정이 되어 있으면 절대 잠을 자지 않음
		Screen.sleepTimeout = SleepTimeout.SystemSetting;
		yield return null;
	}

	protected override void OnDisconnected()
	{

	}
}
