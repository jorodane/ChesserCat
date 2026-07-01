using Firebase;
using Firebase.Auth;
using Firebase.Database;
using Firebase.Extensions;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

public class DBManager : ManagerBase
{
    FirebaseAuth authentication;
    FirebaseUser user;
    DatabaseReference rootDB;

    protected override IEnumerator OnConnected(GameManager newManager)
    {
        //                 의존성 검사     비동기
        //FirebaseApp.CheckAndFixDependenciesAsync().ContinueWithOnMainThread(InitializeFireBase);
        yield return null;
    }

    protected override void OnDisconnected()
    {

    }

    void InitializeFireBase(Task<DependencyStatus> task)
    {
        if(task.Result == DependencyStatus.Available)
        {
            //인증용 인스턴스 가져오기!
            authentication = FirebaseAuth.DefaultInstance;
            //인증을 하기 위해서는 "유저"가 있어야 한다
            user = authentication.CurrentUser;
            //데이터 베이스에 가려면 데이터 베이스가 어디에 있는지 찾아갈 수 있어야 한다!
            //데이터베이스 참조(Reference)
            rootDB = FirebaseDatabase.DefaultInstance.RootReference;

            //로그인을 합시다!
            GuestLogin();

            Debug.Log("Firebase Initialized");
        }
        else
        {
            Debug.LogError($"Fail to Initialize Firebase : {task.Exception}");
        }
    }

    public TMPro.TMP_InputField nickNameInput;

    public void MakeUserData()
    {
        WriteData(MakeNewUserData(nickNameInput.text), "users", "userData", user.UserId);
    }

    public async void GuestLogin()
    {
        //인증기가 존재하지 않으면    ??
        if (authentication is null) return;
        //이미 로그인 되었는지 확인하기
        if(user is not null)
        {
            Debug.LogError($"Login Failed : Already Has Login Data ({user.IsValid()}, {user.UserId})");
            UserData resultData = await ReadDataAsync<UserData>("users", "userData", user.UserId);
            if(resultData is not null)
            {
                Debug.Log(resultData.nickname);
            }
            else
            {
                WriteData( MakeNewUserData("NoNamed"), "users", "userData", user.UserId );
            }
            return;
        }
        //익명으로 로그인하기!
        await authentication.SignInAnonymouslyAsync().ContinueWithOnMainThread(OnLoginResult);
    }

    void OnLoginResult(Task<AuthResult> task)
    {
        if (task.IsCanceled || task.IsFaulted)
        {
            Debug.LogError($"Fail to Sign in : {task.Exception}");
            return;
        }

        user = task.Result.User;
        WriteData( MakeNewUserData("NoNamed"), "users", "userData", user.UserId );
        Debug.Log($"Sign in Succeed : {user.UserId}");
    }

    void OnTaskResult(Task task)
    {
        if (task.IsCanceled || task.IsFaulted)
        {
            Debug.LogError(task.Exception);
        }
    }

    [Serializable]
    public class UserData
    {
        public string   nickname;
        public DateTime assignDate;
        public int      userLevel;
        public int      money;
        public int      attendtime;
    }

    public UserData MakeNewUserData(string wantNickname) => new()
    {
        nickname    = wantNickname,
        assignDate  = DateTime.Now,
        userLevel   = 1,
        money       = 3000,
        attendtime  = 0
    };

    public DatabaseReference GetFinalDirectory(DatabaseReference root, params string[] directory)
    {
        //폴더를 따라 내려가는 것
        //제일 처음에 만든 rootDB가 바로 root폴더 => C드라이브다
        if (directory is null || directory.Length == 0) return root;
        //일단 뿌리에서 시작해보죠!
        DatabaseReference currentReference = root;
        foreach (string currentChild in directory)
        {
            currentReference = currentReference.Child(currentChild);
        }
        return currentReference;
    }

    public async void WriteDataAsync(object wantData, params string[] directory)
    {
        if (rootDB is null || wantData is null) return;
        string jsonData = JsonUtility.ToJson(wantData);
        await GetFinalDirectory(rootDB, directory).SetRawJsonValueAsync(jsonData).ContinueWithOnMainThread(OnTaskResult);
    }

    public void WriteData(object wantData, params string[] directory)
    {
        if (rootDB is null || wantData is null) return;
        //NoSQL은 무엇으로 저장하는지 기억하시나요?
        //JSON으로 저장합니다!
        //{    키       값   => 딕셔너리에서 들어본 거다!
        //   "이름" : "내용"
        //}
        string jsonData = JsonUtility.ToJson(wantData);
        GetFinalDirectory(rootDB, directory).SetRawJsonValueAsync(jsonData).ContinueWithOnMainThread(OnTaskResult);
    }

    public async void WriteDataAsync(Dictionary<string, object> changes, params string[] directory)
    {
        if (rootDB is null || changes is null) return;
        await GetFinalDirectory(rootDB, directory).UpdateChildrenAsync(changes).ContinueWithOnMainThread(OnTaskResult);
    }

    public void WriteData(Dictionary<string, object> changes, params string[] directory)
    {
        if (rootDB is null || changes is null) return;
        //Update : 최신화하다 => 내용을 기입하다
        GetFinalDirectory(rootDB, directory).UpdateChildrenAsync(changes).ContinueWithOnMainThread(OnTaskResult);
    }
    public void ReadData(Action<Task<DataSnapshot>> OnReadData, params string[] directory)
    {
        GetFinalDirectory(rootDB, directory).GetValueAsync().ContinueWithOnMainThread(OnReadData);
    }

    //값을 가져오라고 보내놓고 기다려야 하는 상황!
    //X 로그인 시도 => 일단 들어가 있다가 => 로그인 결과를 받기!
    //O 로그인 시도 => 결과를 받고 => 괜찮으면 => 들어가기
    //X 상점에서 아이템 삼 => 일단 아이템을 가져오고 => 괜찮은지 받기?
    //O 상점에서 아이템 삼 => 내 골드가 괜찮은지 봄 => 아이템 살 수 있으면 교환
    //X 프로필을 열고 => 아직 데이터 갱신 안됐지만 아무튼 엶 => 열고 있는 상태에서 데이터가 오면 갱신
    //이런식으로 값을 가져오는 경우는 확인하는 것이니만큼 기다려야하는 경우가 대다수!
    public IEnumerator ReadDataCoroutine(Action<Task<DataSnapshot>> OnReadData, params string[] directory)
    {
        Task<DataSnapshot> readTask = GetFinalDirectory(rootDB, directory).GetValueAsync();
        yield return readTask.WaitForTask();
        OnReadData?.Invoke(readTask);
    }

    //기다릴 수 있는 형태의 함수 => 코루틴, 비동기
    //                       IEnumerator  async
    public async Task<T> ReadDataAsync<T>(params string[] directory)
    {
        //다른 비동기 함수가 진행되는 동안 기다린다라고 알려주는 구문
        DataSnapshot currentTask = await GetFinalDirectory(rootDB, directory).GetValueAsync();
        //결과가 나온 값이 널이면 걍 널
        if(currentTask is null) return default;
        //결과가 널이 아니지만 결과값이 존재하지 않는다.
        if (!currentTask.Exists) return default;

        //1. 복합타입
        //구조화된 존재를 어떻게 저장하고 있었을까?
        //JSON의 형태로 저장했었다!
        try 
        { 
            if(currentTask.HasChildren)
            {
                return JsonUtility.FromJson<T>(currentTask.GetRawJsonValue());
            }
            //2. 단일타입
            //double 8byte 소수점이 있는 숫자
            //float  4byte 소수점이 있는 숫자
            //float로 저장해놓았으면 double못빼는 게 맞나요?
            //long, int는 서로 호환이 안되는가?
            return (T)System.Convert.ChangeType(currentTask.Value, typeof(T)); 
        }
        catch (Exception e)
        {
            Debug.LogError(e);
            return default;
        }
    }
}
