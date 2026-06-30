using Firebase;
using Firebase.Auth;
using Firebase.Database;
using Firebase.Extensions;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public class DBManager : ManagerBase
{
    FirebaseAuth authentication;
    FirebaseUser user;
    DatabaseReference rootDB;

    protected override IEnumerator OnConnected(GameManager newManager)
    {
        //                 의존성 검사     비동기
        FirebaseApp.CheckAndFixDependenciesAsync().ContinueWithOnMainThread(InitializeFireBase);
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

    public void GuestLogin()
    {
        //인증기가 존재하지 않으면    ??
        if (authentication is null) return;
        //이미 로그인 되었는지 확인하기
        if(user is not null)
        {
            Debug.LogError($"Login Failed : Already Has Login Data ({user.IsValid()}, {user.UserId})");
            WriteData( MakeNewUserData("밤말팅"), "users", "userData", user.UserId );
            return;
        }
        //익명으로 로그인하기!
        authentication.SignInAnonymouslyAsync().ContinueWithOnMainThread(OnLoginResult);
    }
    void OnLoginResult(Task<AuthResult> task)
    {
        if (task.IsCanceled || task.IsFaulted)
        {
            Debug.LogError($"Fail to Sign in : {task.Exception}");
            return;
        }

        user = task.Result.User;
        WriteData( MakeNewUserData("밤말팅"), "users", "userData" );
        Debug.Log($"Sign in Succeed : {user.UserId}");
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

    public void WriteData(object wantData, params string[] directory)
    {
        if(rootDB is null || wantData is null) return;
        //NoSQL은 무엇으로 저장하는지 기억하시나요?
        //JSON으로 저장합니다!
        //{    키       값   => 딕셔너리에서 들어본 거다!
        //   "이름" : "내용"
        //}
        string jsonData = JsonUtility.ToJson(wantData);
        //일단 뿌리에서 시작해보죠!
        DatabaseReference currentReference = rootDB;
        foreach (string currentChild in directory)
        {
            currentReference = currentReference.Child(currentChild);
        }
        currentReference.SetRawJsonValueAsync(jsonData).ContinueWithOnMainThread(OnTaskResult);

        Dictionary<string, object> item = new()
        {
            { "name"    , "돌" },
            { "weight"  , .3 },
            { "price"   , 1 },
        };

        //폴더를 따라 내려가는 것
        //제일 처음에 만든 rootDB가 바로 root폴더 => C드라이브다
        //Update : 최신화하다 => 내용을 기입하다
        rootDB.Child("Items").Child("Misc").Child("Nature").Child("Stone")
            .UpdateChildrenAsync(item).ContinueWithOnMainThread(OnTaskResult);
    }

    void OnTaskResult(Task task)
    {
        if(task.IsCanceled || task.IsFaulted)
        {
            Debug.LogError(task.Exception);
        }
    }
}
