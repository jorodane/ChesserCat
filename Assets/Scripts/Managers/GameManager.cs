using UnityEngine;

public class GameManager : MonoBehaviour
{
	//Static : 프로그램에서 단 하나, 어디서든지 접근 가능!
	static GameManager _instance;
	public static GameManager Instance => _instance;

	UIManager _ui;
	public UIManager		UI => _ui;

	DataManager _data;
	public DataManager		Data => _data;

	SaveManager _save;
	public SaveManager		Save => _save;

	SettingManager _setting;
	public SettingManager	Setting => _setting;

	LanguageManager _language;
	public LanguageManager	Language => _language;

	AudioManager _audio;
	public AudioManager	Audio => _audio;

	CameraManager _camera;
	public CameraManager	Camera => _camera;

	InputManager _input;
	public InputManager	Input => _input;

	//Awake		: 이 친구가 시작할 때 (아침에 눈을 뜸)
	//OnEnabled : 이 친구가 시작할 때 (정신 차림) => 여러번 실행도 된다
	//OnDisabled: 기절
	//Reset		: 일 시작하기 위해 초기화 준비
	//Start		: 이 친구가 시작할 때 (하루의 시작)
	void Awake()
    {
        //게임매니저가 일어나서 제일 처음에 할 일
		//진정한 게임 매니저를 가르는 목숨을 건 사투
		//게임 매니저가.. 둘이에요
		//둘 중 하나만 진정한 게임 매니저!
		//하늘 아래 두 개의 태양은 있을 수 없습니다
		//먼저 온 애를 인정한다.
		//먼저 온 애를 죽이고 간다.
		//하던 놈을 그대로 유지하는 것이 더 좋음!
		//예를 들어서, 국가에 회군이 생겼음
		//게임하는 중간에 갑자기 새로운 세력이 생겼음!
		//만약 원래 있던 애가 죽게 만들면 => 새로운 국가가 생김
		//=>국가 정비를 처음부터 다시 해야 함
		if(Instance == null) //지금 왕이 없음
		{
			//내가 바로 이 시대의 새로운 왕이다!
			_instance = this;
		}
		else //지금 왕이 있음
		{
			//역모를 일으킨 죄인을 참수하라
			Destroy(this);
		}
		//세상에 단 하나만 있도록 유지하는 패턴 => 싱글턴 패턴 (Singleton Pattern)

		//제가 이걸 "왕"이라고 불렀잖아요?
		//"왕"이 농사/장사...등등을 할까요?
		//왕은 신하들에게 일을 진행하라고 관리하는 역할이 될 겁니다!
		//왕이 농사 마스터가 될 수 있을까?
		//모든 것을 다 할 수 있는 사람이어야 왕이 되는 것이 아니라
		//시킬 수 있고, 관리하는 방법을 알아야 된다!
		//하부 조직을 만들 것이다
		//게임에서 "관리"되어야 하는 것들이 무엇일까?

		//UI		UIManager
		//데이터파일 DataManager
		//세이브		SaveManager
		//세팅		SettingManager
		//언어		LanguageManager
		//오디오		AudioManager
		//카메라		CameraManager
		//유저입력	InputManager
	}

	void InitializeManagers()
	{
		//UI를 만들어서 로딩창이라던지, 다른 유저에게 보여줄 수 있는 공간
		//데이터 불러오기
		//유저 세이브 불러오기
		//설정값을 찾아서 세팅
		//언어도 세팅
		//사운드도 세팅
		//카메라 초기화
		//유저 입력 받기 시작
		CreateManager(ref _ui);
		CreateManager(ref _data);
		CreateManager(ref _save);
		CreateManager(ref _setting);
		CreateManager(ref _language);
		CreateManager(ref _audio);
		CreateManager(ref _camera);
		CreateManager(ref _input);
	}

	//달라지는 것이 "자료형"뿐이라면
	//"자료형"에 따라 변수로 작용하는 함수를 만들 수 있지 않을까?
	//"Generic Method" => 범용 함수
	//반환값 이름<자료형>(매개변수) where 자료형 : 부모

	//_input에다가 값을 넣고 싶은데
	//다른 데에서는 _audio에다가 값을 넣는다!
	//대상이 되는 변수를 가져오긴 해야 함!
	//원본 값을 바꿔야 함!
	//								원본 값을 "참조"한다
	//                              원본 값이랑 연결되는 변수로 만들어주기!
	//                              Reference => ref
	ManagerType CreateManager<ManagerType>(ref ManagerType targetVariable) where ManagerType : ManagerBase
	{
		if (targetVariable == null)
		{
			//컴포넌트는 어떻게 추가해야 할 것인가?
			//게임 오브젝트를 누르면 => Inspector창에 [Add Component]
			//버튼을 눌렀다라고 하는 것은 => 기능이 실행된다는 거고 => 함수가 있다는 뜻
			targetVariable = gameObject.AddComponent<ManagerType>();
			targetVariable.Connect(this);
		}
		return targetVariable;
	}

	void Update()
    {
        
    }
}
