using System.Collections;
using UnityEngine;

public delegate void InitializeEvent();
//                                이전 시간으로부터 얼마나 지났는지
//시속 3km/h로 달리는 당신, 1.5시간 뒤에 당신은 얼마나 움직일 것인가?
//4.5km를 이동해야 한다!
public delegate void UpdateEvent(float deltaTime);
public delegate void DestroyEvent();

public class GameManager : MonoBehaviour
{
	//Static : 프로그램에서 단 하나, 어디서든지 접근 가능!
	static GameManager _instance;
	public static GameManager Instance => _instance;

	UIManager _ui;
	public UIManager		UI => _ui;

	DataManager _data;
	public DataManager		Data => _data;

	ObjectManager _objectM;
	public ObjectManager ObjectM => _objectM;

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

	IEnumerator initializing; //초기화 중 코루틴!

	public static event InitializeEvent	OnInitializeManager;
	public static event InitializeEvent	OnInitializeController;
	public static event InitializeEvent	OnInitializeCharacter;
	public static event InitializeEvent	OnInitializeObject;
	public static event UpdateEvent		OnUpdateManager;
	public static event UpdateEvent		OnUpdateController;
	public static event UpdateEvent		OnUpdateCharacter;
	public static event UpdateEvent		OnUpdateObject;
	public static event DestroyEvent	OnDestroyManager;
	public static event DestroyEvent	OnDestroyController;
	public static event DestroyEvent	OnDestroyCharacter;
	public static event DestroyEvent	OnDestroyObject;

	bool isLoading = true;
	bool isPlaying = true;

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
			return;
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
		//반환형식은 IEnumerator입니다. "반복자" => 반복해서 함수가 실행됨 => 프레임 단위로 기다렸다가 실행!
		//한 번 실행을 하고 Yield 양보했다가 다음 프레임에 또 나와서 실행하고! 반복!
		//                 (더 기다려야 되는 경우에는 더 기다리기도 가능!)
		//                    WaitForSeconds(10.0f), 일어났는데 아직 시간이 안됐음. 더 잘 수 있겠군 ㅇㅇ
		//그럼.. 이걸 IEnumerator로 "저장"했을 때 무엇을 할 수 있을까?
		initializing = InitializeManagers();

		//저장했기 때문에, 이 친구를 "시작"시키거나 "중단"시킬 수 있어요!
		//시작을 시키는 것은
		StartCoroutine(initializing);

	}

	void OnDestroy() //매니저가 없어지면
	{
		if(initializing != null) StopCoroutine(initializing); //로딩을 진행하는 중이었다면 끊어버릴 수 있도록!
		DeleteManagers(); //하위 매니저들도 없어지게!
	}

	//얘가 문제
	//로딩은... 얼마나 걸릴까
	//1프레임만에 끝낼 수 있을까?
	//1프레임 넘는 시간동안 "이 함수"가 실행되고 있으면 무슨 일이 일어날까?
	//게임이 멈춥니다. 이 함수 끝날 때까지
	//이 상태에서 게임을 클릭하면 어떻게 되는가 => 응답없음 => 유저는 꺼버림
	//"기다림 함수"
	//coroutine = co - routine
	//           함께   루틴
	//         화면출력 유저입력 /   로딩
	//					 요리   /   청소
	//운동을 해야 합니다. 상체루틴 하체루틴
	//                    1시간    1시간
	//오늘 남은 시간이 1시간
	//옆에 있는 친구를 데려와서 상체 1시간 시키고
	//저는 하체 1시간 하면 => 암튼 둘 다 했음
	//IEnumerator => Start
	//WaitForSeconds을 통해서 시간을 "기다린" 적이 있었죠!
	IEnumerator InitializeManagers()
	{
		//UI를 만들어서 로딩창이라던지, 다른 유저에게 보여줄 수 있는 공간
		//데이터 불러오기
		//유저 세이브 불러오기
		//설정값을 찾아서 세팅
		//언어도 세팅
		//사운드도 세팅
		//카메라 초기화
		//유저 입력 받기 시작
		//몇 개가 필요한지 집계를 받을 때 => 필요한 것! 적어둘 공간이 필요해요!
		int totalLoadCount = 0;
		totalLoadCount += CreateManager(ref _ui).LoadCount;
		totalLoadCount += CreateManager(ref _data).LoadCount;
		totalLoadCount += CreateManager(ref _objectM).LoadCount;
		totalLoadCount += CreateManager(ref _save).LoadCount;
		totalLoadCount += CreateManager(ref _setting).LoadCount;
		totalLoadCount += CreateManager(ref _language).LoadCount;
		totalLoadCount += CreateManager(ref _audio).LoadCount;
		totalLoadCount += CreateManager(ref _camera).LoadCount;
		totalLoadCount += CreateManager(ref _input).LoadCount;

		yield return UI.Initialize(this);
		UIBase loadingUI = UIManager.ClaimOpenUI(UIType.Loading); //UI System이 돌아가기 시작했으니까 기능을 실행해보기!
		IProgress<int> loadingProgress = loadingUI as IProgress<int>;

		loadingProgress?.Set(0, totalLoadCount);
		yield return Data.Connect(this);
		loadingProgress?.AddCurrent(1);
		yield return ObjectM.Connect(this);
		loadingProgress?.AddCurrent(1);
		yield return UI.Connect(this);
		loadingProgress?.AddCurrent(1);
		yield return Save.Connect(this);
		loadingProgress?.AddCurrent(1);
		yield return Setting.Connect(this);
		loadingProgress?.AddCurrent(1);
		yield return Language.Connect(this);
		loadingProgress?.AddCurrent(1);
		yield return Audio.Connect(this);
		loadingProgress?.AddCurrent(1);
		yield return Camera.Connect(this);
		loadingProgress?.AddCurrent(1);
		yield return Input.Connect(this);
		loadingProgress?.AddCurrent(1);
		yield return null;
		UIManager.ClaimCloseUI(UIType.Loading);
		isLoading = false;
	}

	void DeleteManagers()
	{
		//유저입력	InputManager
		Input?.Disconnect();
		//오브젝트	ObjectManager
		ObjectM?.Disconnect();
		//오디오		AudioManager
		Audio?.Disconnect();
		//언어		LanguageManager
		Language?.Disconnect();
		//세팅		SettingManager
		Setting?.Disconnect();
		//세이브		SaveManager
		Save?.Disconnect();
		//카메라		CameraManager
		Camera?.Disconnect();
		//UI		UIManager
		UI?.Disconnect();
		//데이터파일 DataManager
		Data?.Disconnect();
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
			targetVariable = this.TryAddComponent<ManagerType>();
		}

		return targetVariable;
	}

	public static void Pause()
	{
		Instance.isPlaying = false;
	}

	public static void Unpause()
	{
		Instance.isPlaying = true;
	}

	void InvokeInitializeEvent(ref InitializeEvent OriginEvent)
	{
		if (OriginEvent != null) //이벤트가 있어야 실행하지
		{
			InitializeEvent CurrentEvent = OriginEvent; //저장해놓고
			OriginEvent = null; //비우고
			CurrentEvent.Invoke(); //저장해둔거 실행하기
		}
	}
	void InvokeDestroyEvent(ref DestroyEvent OriginEvent)
	{
		if (OriginEvent != null) //이벤트가 있어야 실행하지
		{
			DestroyEvent CurrentEvent = OriginEvent; //저장해놓고
			OriginEvent = null; //비우고
			CurrentEvent.Invoke(); //저장해둔거 실행하기
		}
	}

	//게임매니저만 업데이트를 하는 이유!
	//모두가 업데이트를 하겠다고 아우성이라면
	//누가 먼저하는지 모르고
	//만약에, 마우스가 움직였는데 그게 갱신되지 않은 상태로
	//플레이어 캐릭터가 너무 신나서 먼저 쏘기로 결정했다!

	//1프레임 전에 제가 땅에 마우스를 올리고 있었는데
	//이번 프레임에 몬스터를 겨냥했습니다.

	//플레이어 캐릭터가 InputManager보다 한 발자국 먼저 쏜다면?
	//몬스터가 죽은 상태 << 모든 애들의 턴이 지난 다음에 체크하기!
	//하스스톤에서 여러번 반복해서 때리는 카드를 보면
	//이미 죽은 카드들을 또 때리는 것도 볼 수 있거든요?
	//영웅이 죽었을 때? 하수인이 죽었을 때? => 순서를 맞춘다!
	void Update()
    {
		//게임 진행을 할 수 있는지 여부를 조정할 수도 있다!
		//초기화 해야하는지, 하지 말아야 하는지~
		//Pause상태다! => 업데이트를 하지 않는다!
		if (isLoading) return;

		//초기화
		//매니저를 초기화한다
		InvokeInitializeEvent(ref OnInitializeManager);
		//캐릭터를 초기화한다
		InvokeInitializeEvent(ref OnInitializeCharacter);
		//컨트롤러를 초기화한다 => 캐릭터가 있는 상태에서 돌아가야 하니까!
		InvokeInitializeEvent(ref OnInitializeController);
		//오브젝트를 초기화한다
		InvokeInitializeEvent(ref OnInitializeObject);

		if (isPlaying)
		{
			//프레임 사이에 몇 초가 지났을까?
			float deltaTime = Time.deltaTime;
			//매니저가 업데이트 하는 경우
			OnUpdateManager?.Invoke(deltaTime);
			//컨트롤러를 업데이트한다 => 먼저 판단하고
			OnUpdateController?.Invoke(deltaTime);
			//캐릭터를 업데이트한다 => 캐릭터가 수행하고
			OnUpdateCharacter?.Invoke(deltaTime);
			//오브젝트를 업데이트한다 => 오브젝트 진행
			OnUpdateObject?.Invoke(deltaTime);
		}

		//오브젝트를 제거한다
		InvokeDestroyEvent(ref OnDestroyObject);
		//컨트롤러를 제거한다
		InvokeDestroyEvent(ref OnDestroyController);
		//캐릭터를 제거한다
		InvokeDestroyEvent(ref OnDestroyCharacter);
		//매니저를 제거한다
		InvokeDestroyEvent(ref OnDestroyManager);
	}
}
