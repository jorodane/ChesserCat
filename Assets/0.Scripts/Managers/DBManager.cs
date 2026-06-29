using Firebase;
using Firebase.Auth;
using Firebase.Database;
using System;
using System.Collections;
using System.Threading.Tasks;
using UnityEngine;

public class DBManager : ManagerBase
{
    FirebaseAuth authentication;
    FirebaseUser user;
    DatabaseReference DBReference;

    protected override IEnumerator OnConnected(GameManager newManager)
    {
        //                 의존성 검사     비동기
        FirebaseApp.CheckAndFixDependenciesAsync().ContinueWith(InitializeFireBase);
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
            DBReference = FirebaseDatabase.DefaultInstance.RootReference;

            //로그인을 합시다!
            GuestLogin();

            Debug.Log("Firebase Initialized");
        }
        else
        {
            Debug.LogError($"Fail to Initialize Firebase : {task.Exception}");
        }
    }

    public void GuestLogin()
    {
        //인증기가 존재하지 않으면    ??
        if (authentication is null) return;
        //이미 로그인 되었는지 확인하기
        if(user is not null)
        {
            Debug.LogError("Login Failed : Already Has Login Data");
        }
        //익명으로 로그인하기!
        authentication.SignInAnonymouslyAsync().ContinueWith(OnLoginResult);
    }
    void OnLoginResult(Task<AuthResult> task)
    {
        if (task.IsCanceled || task.IsFaulted)
        {
            Debug.LogError($"Fail to Sign in : {task.Exception}");
            return;
        }

        user = task.Result.User;
        Debug.Log($"Sign in Succeed : {user.UserId}");
    }
}
