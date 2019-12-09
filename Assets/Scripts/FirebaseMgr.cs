using Firebase;
using Firebase.Auth;
using Firebase.Database;
using Firebase.InstanceId;
using Firebase.Unity.Editor;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static FlatCalendar;
using static MainController;
using UnityEngine.SceneManagement;


public class FirebaseMgr : Singleton<FirebaseMgr>
{

    FirebaseAuth auth;
    MainController _controller;
    DatabaseReference reference;
    public string idToken ="not";
    Firebase.Auth.FirebaseUser newUser = null;

    bool retreiveFlg = false;
    public void Initialize(MainController ctr)
    {
        
        _controller = ctr;

        Firebase.Messaging.FirebaseMessaging.TokenReceived += OnTokenReceived;
        Firebase.Messaging.FirebaseMessaging.MessageReceived += OnMessageReceived;
        FirebaseApp.DefaultInstance.SetEditorDatabaseUrl("https://familia-39879.firebaseio.com/");
       
        auth = FirebaseAuth.DefaultInstance;




        ///google Service version update
        Firebase.FirebaseApp.CheckAndFixDependenciesAsync().ContinueWith(task =>
        {
            var dependencyStatus = task.Result;
            if (dependencyStatus == Firebase.DependencyStatus.Available)
            {
                reference = FirebaseDatabase.DefaultInstance.RootReference;
                FirebaseInstanceId.GetInstanceId(FirebaseApp.DefaultInstance).GetTokenAsync().ContinueWith(tokentask =>
                {
                    if (tokentask.IsCompleted)
                    {
                        string result = tokentask.Result;
                        idToken = result;   
                    }

                });


            } 
            else
            {
                Debug.Log("error : could not resolve all firebase dependencies");
                return;
            }
        });
        Firebase.Messaging.FirebaseMessaging.MessageReceived += OnMessageReceived;
    }


   
    public void OnTokenReceived(object sender, Firebase.Messaging.TokenReceivedEventArgs token)
    {
        UnityEngine.Debug.Log("Received Registration Token: " + token.Token);
        idToken = token.Token.ToString();

    }

    public void OnMessageReceived(object sender, Firebase.Messaging.MessageReceivedEventArgs e)
    {
        UnityEngine.Debug.Log("Received a new message from: " + e.Message.From);
        //NotificationManager.Send(5f, e.Message.Notification.Title.ToString(), e.Message.Notification.Body.ToString(), Color.grey);
    }

    
    public void AutoLogin()
    {
        if(PlayerPrefs.HasKey("ID") && PlayerPrefs.HasKey("PW"))
        {
            GoogleLogin(PlayerPrefs.GetString("ID"), PlayerPrefs.GetString("PW"));
            Debug.Log("자동로그인 시도");
        }
  

        Debug.Log("자동로그인 실패");
    }
    public void GoogleJoin(string email, string password,string nickname)
    {
        _controller.OnProgressBar("회원가입 진행중..");
        
        Debug.Log("google join start");
        auth.CreateUserWithEmailAndPasswordAsync(email, password).ContinueWith(task => {
            if (task.IsCanceled)
            {
                Debug.LogError("CreateUserWithEmailAndPasswordAsync was canceled.");
                return;
            }
            if (task.IsFaulted)
            {

                //UIManager.Instance.OnWarningMsg(task.Exception.Message);
                Debug.LogError("CreateUserWithEmailAndPasswordAsync encountered an error: " + task.Exception);
                _controller.OffProgressBar();
                _controller.OnWarningMsg(task.Exception.Message);
                _controller.OffJoinPage();
                return;
            }

            // Firebase user has been created.
            newUser = task.Result;
            User user = new User();
            user.nickname = nickname;
            user.email = email;
            user.isAlarmOn = true;

            SaveUserData(newUser.UserId, user);

            _controller.OffProgressBar();
            _controller.OnWarningMsg("회원가입을 완료하였습니다.");
            _controller.OffJoinPage();

        });


    }

    private void SaveUserData(string id, User user)
    {
        string json = JsonUtility.ToJson(user);
        Debug.Log(json + "," + reference.ToString() + "," + id);
        reference.Child("USER").Child(id).SetRawJsonValueAsync(json);
    }

    public string userID;

    public void GoogleLogin(string email,string password)
    {
        _controller.OnProgressBar("로그인중..");
        FirebaseApp.Create();
        auth.SignInWithEmailAndPasswordAsync(email, password).ContinueWith(task => {
            if (task.IsCanceled)
            {
                Debug.LogError("SignInWithEmailAndPasswordAsync was canceled.");
                _controller.OnWarningMsg("로그인 실패");
                return;
            }
            if (task.IsFaulted)
            {
                Debug.LogError("SignInWithEmailAndPasswordAsync encountered an error: " + task.Exception);
                _controller._uiMgr.OnWarningMsg(task.Exception.Message);
                return;
            }
            Debug.Log("로긘 함수 들엉ㅁ");
            
            Debug.Log("GoogleLoginSuccess");
            FirebaseUser user = task.Result;

            
            Debug.Log("GoogleLoginSuccess2");
            RetrieveUserData(user.UserId);
            userID = user.UserId;

            PlayerPrefs.SetString("ID", email);
            PlayerPrefs.SetString("PW", password);

            SaveTokenToDB(user.UserId);
            
        });

              
    }

    private void SaveTokenToDB(string uid)
    {

        FirebaseDatabase.DefaultInstance.GetReference("USER/" + uid).Child("deviceID").SetValueAsync(idToken);
       // FirebaseInstanceId.getInstance().getToken();
       
       //FirebaseInstanceId


    }
    private void RetrieveUserData(string uid)
    {
        Debug.Log("retrieveUserData");

        User user = new User();
        FirebaseDatabase.DefaultInstance.GetReference("USER/" + uid).GetValueAsync().ContinueWith(userbasicdata =>
        {
            if (userbasicdata.IsFaulted)
            {

            }
            else if (userbasicdata.IsCanceled)
            {

            }
            else if(userbasicdata.IsCompleted)
            { 
                DataSnapshot snap = userbasicdata.Result;

                if (snap.ChildrenCount == 0)
                {
                    Debug.Log("empty");
                    return;
                }

                IDictionary result = (IDictionary)snap.Value;

                user.deviceID = result["deviceID"].ToString();
                user.isAlarmOn = Convert.ToBoolean(result["isAlarmOn"].ToString());
                user.default_calendar = result["default_calendar"].ToString();
                user.nickname = result["nickname"].ToString();
                user.email = result["email"].ToString();

                if (result.Contains("dDay_list"))
                {
                    List<object> dday_list = (List<object>)result["dDay_list"];

                    foreach(var ddayitem in dday_list)
                    {
                        IDictionary tmp = (IDictionary)ddayitem;
                        int year = Convert.ToInt32(tmp["year"].ToString());
                        int month = Convert.ToInt32(tmp["month"].ToString());
                        int day = Convert.ToInt32(tmp["day"].ToString());
                        int hour = Convert.ToInt32(tmp["hour"].ToString());
                        int min = Convert.ToInt32(tmp["min"].ToString());
                        string description = tmp["description"].ToString();

                        user.dDay_list.Add(new Dday(year, month, day, hour, min, description));
                    }
                }
                
                if (result.Contains("events_list"))
                {
                    Dictionary<string, object> evnet_list = (Dictionary<string, object>)result["events_list"];

                    //List<object> evnet_list = (List<object>)result["events_list"];
                    foreach (var eitem in evnet_list)
                    {
                        
                        string key = eitem.Key;
                        IDictionary tmp = (IDictionary)eitem.Value;
                        
                        int year = Convert.ToInt32(tmp["year"].ToString());
                        int month = Convert.ToInt32(tmp["month"].ToString());
                        int day = Convert.ToInt32(tmp["day"].ToString());
                        int hour = Convert.ToInt32(tmp["hour"].ToString());
                        int min = Convert.ToInt32(tmp["min"].ToString());
                        string title = tmp["title"].ToString();
                        string description = tmp["description"].ToString();
                        string place = tmp["place"].ToString();
                        user.events_list.Add(key, new Events(year, month, day, hour, min, title, description, place));
                    }
                }

                if (result.Contains("feed"))
                {
                    Dictionary<string, object> feed_list = (Dictionary<string, object>)result["feed"];

                    //List<object> evnet_list = (List<object>)result["events_list"];
                    foreach (var eitem in feed_list)
                    {

                        string key = eitem.Key;
                        IDictionary tmp = (IDictionary)eitem.Value;

                        string title = tmp["title"].ToString();
                        string body = tmp["body"].ToString();
                        string gcode = tmp["groupcode"].ToString();

                        MainController.Feed tmpFeed = new MainController.Feed();
                        tmpFeed.title = title;
                        tmpFeed.body = body;
                        tmpFeed.groupcode = gcode;

                        user.feed_list.Add(key,tmpFeed);
                  
                    }
                }
                if (result.Contains("schedule_list"))
                {

                    Debug.Log("zaa");
                    Dictionary<string, object> schedule_list = (Dictionary<string, object>)result["schedule_list"];
                    //List<object> schedule_list = (List<object>)result["schedule_list"];
                    
                    foreach (var sitem in schedule_list)
                    {
                        Debug.Log("zaa2");
                        Schedule tmpSch = new Schedule();
                        IDictionary tmp = (IDictionary)sitem.Value;
                        tmpSch.day1 = tmp["day1"].ToString();
                        tmpSch.start1 = tmp["start1"].ToString();
                        tmpSch.finish1 = tmp["finish1"].ToString();
                        tmpSch.description = tmp["description"].ToString();

                        Debug.Log(tmp.Count);
                        if (tmp.Count>5)
                        {
                            tmpSch.day2 = tmp["day2"].ToString();
                            tmpSch.start2 = tmp["start2"].ToString();
                            tmpSch.finish2 = tmp["finish2"].ToString();
                        }
                        Debug.Log("a!");
                        user.schedule_list.Add(sitem.Key,tmpSch);
                        
                    }

                }
                Debug.Log("zaa3");
                if (result.Contains("todo_list"))
                {
                    Dictionary<string, object> todo_list = (Dictionary<string, object>)result["todo_list"];
                    Debug.Log("zaa3");
                    foreach (var items in todo_list)
                    {

                        string key = items.Key;
                        string description = items.Value.ToString();

                        IDictionary tmp = (IDictionary)items.Value;
                        string text = tmp["description"].ToString();
                        bool iscomplete = Convert.ToBoolean(tmp["isComplete"].ToString());
                        Todo todo = new Todo();
                        todo.description = text;
                        todo.isComplete = iscomplete;
                        user.todo_list.Add(key, todo);
                        Debug.Log("add :"+key+"."+text);
                    }
                }
                Debug.Log("zaa4");
                if (result.Contains("group_list"))
                {
                    List<object> group_list = (List<object>)result["group_list"];
                    Debug.Log("zaa5");
                    foreach (var gitem in group_list)
                    {
                        IDictionary tmp = (IDictionary)gitem;
                        string code = tmp["code"].ToString();
                        RetrieveGroupData(code);

                        //FirebaseDatabase.DefaultInstance.GetReference("GROUP/" + code + "/events_list/").ValueChanged += HandleValueChanged;
                    }
                    Debug.Log("finish getting data from firebase");
                    // Debug.Log(_controller);

                    _controller._user = user;
                    Debug.Log(_controller._user.todo_list.Count);
                    Debug.Log("add333");
                    SceneManager.LoadScene(1);
                    _controller.OffProgressBar();

                }
                else
                {
                    Debug.Log("finish getting data from firebase");
                    // Debug.Log(_controller);

                    _controller._user = user;
                    Debug.Log(_controller._user.todo_list.Count);
                    Debug.Log("add333");
                    SceneManager.LoadScene(1);
                    _controller.OffProgressBar();
                }

            }

        });

       
        
    }

   
    private void RetrieveGroupData(string code)
    {
        FirebaseDatabase.DefaultInstance.GetReference("GROUP/" + code).GetValueAsync().ContinueWith(task =>
        {
            if (task.IsFaulted)
            {

            }
            else if (task.IsCanceled)
            {

            }
            else if (task.IsCompleted)
            {
                DataSnapshot snap = task.Result;
                Group group = new Group();

                if (snap.ChildrenCount == 0)
                {
                    Debug.Log("empty");
                    return;
                }

                IDictionary result = (IDictionary)snap.Value;
                Debug.Log("zaa6");
                group.name = result["name"].ToString();
                group.code = result["code"].ToString();
                //그룹의 시간표
                if (result.Contains("events_list"))
                {
                    Dictionary<string, object> evnet_list = (Dictionary<string, object>)result["events_list"];

                    //List<object> evnet_list = (List<object>)result["events_list"];
                    foreach (var eitem in evnet_list)
                    {

                        string key = eitem.Key;
                        IDictionary tmp = (IDictionary)eitem.Value;

                        int year = Convert.ToInt32(tmp["year"].ToString());
                        int month = Convert.ToInt32(tmp["month"].ToString());
                        int day = Convert.ToInt32(tmp["day"].ToString());
                        int hour = Convert.ToInt32(tmp["hour"].ToString());
                        int min = Convert.ToInt32(tmp["min"].ToString());
                        string title = tmp["title"].ToString();
                        string description = tmp["description"].ToString();
                        string place = tmp["place"].ToString();
                        group.events_list.Add(key, new Events(year, month, day, hour, min, title, description, place));
                    }
                }

                if (result.Contains("feed"))
                {
                    Dictionary<string, object> feed_list = (Dictionary<string, object>)result["feed"];

                    //List<object> evnet_list = (List<object>)result["events_list"];
                    foreach (var eitem in feed_list)
                    {

                        string key = eitem.Key;
                        IDictionary tmp = (IDictionary)eitem.Value;

                        if(tmp["author"].ToString() == userID)
                        {
                            continue;
                        }
                        string title = tmp["title"].ToString();
                        string body = tmp["body"].ToString();

                        
                        string gcode = group.code;

                        MainController.Feed tmpFeed = new MainController.Feed();
                        tmpFeed.title = title;
                        tmpFeed.body = body;
                        tmpFeed.groupcode = gcode;

                        
                        group.feed_list.Add(key, tmpFeed);
                    }
                }

                if (result.Contains("member"))
                {
                    List<object> memberlist = (List<object>)result["member"];
                    foreach (var sitem in memberlist)
                    {
                        if (!sitem.ToString().Equals(userID))
                        {
                            RetrieveMemberData(group, sitem.ToString());
                        }
                    }

                }

                //todo add groupinfo
                _controller.group_list.Add(group);

            }

        });
    }

    public void DeleteEventDB(string key)
    {
        FirebaseDatabase.DefaultInstance.GetReference("USER/" + userID + "/events_list/" + key).RemoveValueAsync().ContinueWith(task =>
        {
            if (!task.IsCompleted)
            {
                Debug.LogError("cannot remove data. error");                
            }
            else
            {
                Debug.Log("remove data !"+idToken+","+key);
                TimeObj time = _controller._calendar.currentTime;
                _controller._calendar.updateUiLabelEvents(time.year, time.month, time.day);
            }
        });


    }

    public void DeleteGroupEventDB(string gcode, string key)
    {
        FirebaseDatabase.DefaultInstance.GetReference("GROUP/" + gcode + "/events_list/" + key).RemoveValueAsync().ContinueWith(task =>
        {
            if (!task.IsCompleted)
            {
                Debug.LogError("cannot remove data. error");
            }
            else
            {
                Debug.Log("remove data !" + idToken + "," + key);
                TimeObj time = _controller._calendar.currentTime;
                _controller._calendar.updateUiLabelEvents(time.year, time.month, time.day);
            }
        });


    }

    public string AddEventDB(Events eve)
    {
        //userID = "GE13hkDuB9UqA7NPdbTDBBUUQ382";
        string json = JsonUtility.ToJson(eve);
        string key = FirebaseDatabase.DefaultInstance.GetReference("USER/" + userID + "/events_list/").Push().Key;
        Debug.Log("key : " + key);
        
        FirebaseDatabase.DefaultInstance.GetReference("USER/" + userID + "/events_list/" + key).SetRawJsonValueAsync(json);
        return key;
       
    }
    public string AddGroupEventDB(string gcode, Events eve)
    {
        string json = JsonUtility.ToJson(eve);
        string key = FirebaseDatabase.DefaultInstance.GetReference("GROUP/" + gcode + "/events_list/").Push().Key;
        Debug.Log("key : " + key);


        
        FirebaseDatabase.DefaultInstance.GetReference("GROUP/" + gcode + "/events_list/" + key).SetRawJsonValueAsync(json);
        return key;
    }
    public string AddGroupFeed(string gcode,string gname, string title)
    {
        string key = FirebaseDatabase.DefaultInstance.GetReference("GROUP/" + gcode + "/feed/").Push().Key;
        GroupFeed tmp = new GroupFeed();
        tmp.title = "일정추가";
        tmp.body = "<" + gname + ">에서 일정이 추가되었습니다 :" + title;
        tmp.author = userID;
        string json = JsonUtility.ToJson(tmp);
        FirebaseDatabase.DefaultInstance.GetReference("GROUP/" + gcode + "/feed/" + key).SetRawJsonValueAsync(json);
        return key;
    }
    public void ReviseEvent(string key, Events eve)
    {
        string json = JsonUtility.ToJson(eve);
        FirebaseDatabase.DefaultInstance.GetReference("USER/" + userID + "/events_list/" + key).SetRawJsonValueAsync(json);
        TimeObj time = _controller._calendar.currentTime;
        _controller._calendar.updateUiLabelEvents(time.year, time.month, time.day);

    }
    public void ReviseGroupEvent(string gcode,string key, Events eve)
    {
        string json = JsonUtility.ToJson(eve);
        FirebaseDatabase.DefaultInstance.GetReference("GROUP/" + gcode + "/events_list/" + key).SetRawJsonValueAsync(json);

    }



    //// 그룹과 관련된 함수들////
    ///
    public void RetrieveMemberData(Group group, string memberid)
    {
        FirebaseDatabase.DefaultInstance.GetReference("USER/" + memberid).GetValueAsync().ContinueWith(task =>
        {
            if (task.IsFaulted)
            {

            }
            else if (task.IsCanceled)
            {

            }
            else if (task.IsCompleted)
            {
                DataSnapshot snap = task.Result;

                if (snap.ChildrenCount == 0)
                {
                    Debug.Log("empty");
                    return;
                }

                IDictionary result = (IDictionary)snap.Value;
                Member member = new Member();
                member.nickname = result["nickname"].ToString();
                member.uid = memberid;

                //그룹의 시간표
                if (result.Contains("schedule_list"))
                {
                    Dictionary<string,object> schedule_list = (Dictionary<string, object>)result["schedule_list"];

                    foreach (var sitem in schedule_list)
                    {
                        Schedule tmpSch = new Schedule();
                        IDictionary tmp = (IDictionary)sitem.Value;
                        tmpSch.day1 = tmp["day1"].ToString();
                        tmpSch.start1 = tmp["start1"].ToString();
                        tmpSch.finish1 = tmp["finish1"].ToString();
                        tmpSch.description = tmp["description"].ToString();

                        if (tmp.Count > 5)
                        {
                            tmpSch.day2 = tmp["day2"].ToString();
                            tmpSch.start2 = tmp["start2"].ToString();
                            tmpSch.finish2 = tmp["finish2"].ToString();
                        }
                        member.schedule_list.Add(sitem.Key,tmpSch);
                        
                    }

                }

                group.member.Add(member);
            }

        });
    }
    public void MakeNewGroup(string groupname)
    {
        string newKey = FirebaseDatabase.DefaultInstance.GetReference("GROUP/").Push().Key; //새로운 키를 생성함 -> 그룹 고유의 아이디임
        Group newbe = new Group(newKey, groupname,userID);


        DB_GROUPCODE db = new DB_GROUPCODE();
        db.code = newKey;


        
        
        db.member.Add(userID);
        db.name = groupname;
        string json = JsonUtility.ToJson(db);
        FirebaseDatabase.DefaultInstance.GetReference("GROUP/" + newKey).SetRawJsonValueAsync(json);
        _controller.group_list.Add(newbe);

        int index = _controller.group_list.Count - 1;
        GROUPDB tmpdb = new GROUPDB();
        tmpdb.code = newKey;
        FirebaseDatabase.DefaultInstance.GetReference("USER/" + userID + "/group_list/" + index ).SetRawJsonValueAsync(JsonUtility.ToJson(tmpdb));


    }
    public class GROUPDB
    {
        public string code;
    }
   
    public class DB_GROUPCODE
    {
        public string code;
        public List<string> member = new List<string>();
        public string name;

    }

    public class GroupFeed
    {
        public string title;
        public string body;
        public string author;
    }



/// <summary>
/// 시간표에 관련된것들. 
/// 시간표 추가 / 시간표 수정 / 시간표삭제
/// </summary>
/// <param name="saveindex"></param>
/// <param name="savedata"></param>
    public string AddSchedule(Schedule savedata)
    {
        string newKey = FirebaseDatabase.DefaultInstance.GetReference("USER/" + userID + "/schedule_list/").Push().Key;
        
        if(savedata.day2 == null|| savedata.day2 == "")
        {
            OnlyOneSchedule tmp = new OnlyOneSchedule();
            tmp.start1 = savedata.start1;
            tmp.finish1 = savedata.finish1;
            tmp.description = savedata.description;
            tmp.day1 = savedata.day1;

            string json = JsonUtility.ToJson(tmp);
            FirebaseDatabase.DefaultInstance.GetReference("USER/" + userID + "/schedule_list/" + newKey + "/").SetRawJsonValueAsync(json);
        }
        else
        {
            string json = JsonUtility.ToJson(savedata);
            FirebaseDatabase.DefaultInstance.GetReference("USER/" + userID + "/schedule_list/" + newKey + "/").SetRawJsonValueAsync(json);
        }
        return newKey;
    }

    public bool RemoveSchedule(string key)
    {
        FirebaseDatabase.DefaultInstance.GetReference("USER/" + userID + "/schedule_list/" + key).RemoveValueAsync().ContinueWith(task =>
        {
            if (!task.IsCompleted)
            {
                Debug.LogError("cannot remove data. error");
                return false;
            }
            else
            {
                Debug.Log("success");
                return true;
            }
        });

        return false;
    }



    ////todo////
    ///
    public string AddNewTodo(TodoMgr mgr , Todo todo)
    {
        string json = JsonUtility.ToJson(todo);
        string key = FirebaseDatabase.DefaultInstance.GetReference("USER/" + userID + "/todo_list/").Push().Key;
        FirebaseDatabase.DefaultInstance.GetReference("USER/" + userID + "/todo_list/" + key + "/").SetRawJsonValueAsync(json);

        _controller._user.todo_list.Add(key, todo);
        return key;

    }

    public void EditTodo(string key, Todo todo)
    {
        string json = JsonUtility.ToJson(todo);
        FirebaseDatabase.DefaultInstance.GetReference("USER/" + userID + "/todo_list/" + key + "/").SetRawJsonValueAsync(json);
    }
    public void DeleteTodo(string key)
    {
        FirebaseDatabase.DefaultInstance.GetReference("USER/" + userID + "/todo_list/" + key).RemoveValueAsync().ContinueWith(task =>
        {
            if (!task.IsCompleted)
            {
                Debug.LogError("cannot remove data. error");
            }
            else
            {
                Debug.Log("remove data !" + idToken + "," + key);
                _controller._user.todo_list.Remove(key);
            }
        });
    }

    public void SendInviteMessage(string email, string gcode, string gname)
    {
        FirebaseDatabase.DefaultInstance.RootReference.GetValueAsync().ContinueWith(task => 
        {
            if (task.IsFaulted)
            {

            }
            else if (task.IsCanceled)
            {

            }
            else if (task.IsCompleted)
            {
                DataSnapshot snap = task.Result;

                if (snap.ChildrenCount == 0)
                {
                    Debug.Log("empty");
                    return;
                }

                IDictionary result = (IDictionary)snap.Value;
                
                Dictionary<string, object> user_list = (Dictionary<string, object>)result["USER"];

                //List<object> evnet_list = (List<object>)result["events_list"];
                foreach (var eitem in user_list)
                {
                    IDictionary tmp = (IDictionary)eitem.Value;
                    if (tmp["email"].Equals(email))
                    {

                        List<Member> memberList = _controller.group_list[_controller.CurrentGroup].member;
                        for (int i = 0; i <memberList.Count; i++)
                        {
                            if (memberList[i].uid.Equals(eitem.Key))
                            {
                                _controller.OnWarningMsg("이미 그룹에 존재하는 사용자입니다.");
                                return;
                            }
                        }
                        Feed feed = new Feed();
                        feed.title = "그룹 초대";
                        feed.body = _controller._user.nickname + "님께서 <" + gname + ">그룹으로 초대하였습니다";
                        feed.groupcode = gcode;

                        string json = JsonUtility.ToJson(feed);
                        string key = FirebaseDatabase.DefaultInstance.GetReference("USER/" + eitem.Key + "/feed/").Push().Key;
                        FirebaseDatabase.DefaultInstance.GetReference("USER/" + eitem.Key + "/feed/"+key).SetRawJsonValueAsync(json);

                        //todo:초대완료 알림
                        Debug.Log("a");
                        _controller.OnWarningMsg("초대메시지를 발송하였습니다.");
                        return;
                    }
                }
                //todo: 실패알림.
                _controller.OnWarningMsg("찾을 수 없는 이메일입니다.");

            }
        });
    }



    public void SetAlarm(string ison)
    {
        FirebaseDatabase.DefaultInstance.GetReference("USER/" + userID + "/isAlarmOn/").SetRawJsonValueAsync(ison);
    }
    public void Logout()
    {
        auth.SignOut();
        PlayerPrefs.DeleteAll();
    }

    public void RemoveUserFeed(string code)
    {
        FirebaseDatabase.DefaultInstance.GetReference("USER/" + userID + "/feed/" + code).RemoveValueAsync().ContinueWith(task =>
        {
            if (!task.IsCompleted)
            {
                Debug.LogError("cannot remove data. error");
            }
            else
            {
                
                
            }
        });
    }

    public void AcceptInvite(Feed feed)
    {
        string code = feed.groupcode;
        
        FirebaseDatabase.DefaultInstance.GetReference("GROUP/" + code).GetValueAsync().ContinueWith(task =>
        {
            if (task.IsFaulted)
            {

            }
            else if (task.IsCanceled)
            {

            }
            else if (task.IsCompleted)
            {
                DataSnapshot snap = task.Result;
                Group group = new Group();

                if (snap.ChildrenCount == 0)
                {
                    Debug.Log("empty");
                    return;
                }

                IDictionary result = (IDictionary)snap.Value;

                group.name = result["name"].ToString();
                group.code = result["code"].ToString();
                //그룹의 시간표
                if (result.Contains("events_list"))
                {
                    Dictionary<string, object> evnet_list = (Dictionary<string, object>)result["events_list"];

                    //List<object> evnet_list = (List<object>)result["events_list"];
                    foreach (var eitem in evnet_list)
                    {

                        string key = eitem.Key;
                        IDictionary tmp = (IDictionary)eitem.Value;

                        int year = Convert.ToInt32(tmp["year"].ToString());
                        int month = Convert.ToInt32(tmp["month"].ToString());
                        int day = Convert.ToInt32(tmp["day"].ToString());
                        int hour = Convert.ToInt32(tmp["hour"].ToString());
                        int min = Convert.ToInt32(tmp["min"].ToString());
                        string title = tmp["title"].ToString();
                        string description = tmp["description"].ToString();
                        string place = tmp["place"].ToString();
                        group.events_list.Add(key, new Events(year, month, day, hour, min, title, description, place));
                    }
                }


                if (result.Contains("feed"))
                {
                    Dictionary<string, object> feed_list = (Dictionary<string, object>)result["feed"];

                    //List<object> evnet_list = (List<object>)result["events_list"];
                    foreach (var eitem in feed_list)
                    {

                        string key = eitem.Key;
                        IDictionary tmp = (IDictionary)eitem.Value;

                        string title = tmp["title"].ToString();
                        string body = tmp["body"].ToString();
                        string gcode = group.code;

                        MainController.Feed tmpFeed = new MainController.Feed();
                        tmpFeed.title = title;
                        tmpFeed.body = body;
                        tmpFeed.groupcode = gcode;

                        group.feed_list.Add(key, tmpFeed);
                    }
                }
                DB_GROUPCODE tmpdb = new DB_GROUPCODE();
                if (result.Contains("member"))
                {
                    List<object> memberlist = (List<object>)result["member"];
                    foreach (var sitem in memberlist)
                    {
                        tmpdb.member.Add(sitem.ToString());

                        if (!sitem.ToString().Equals(userID))
                        {
                            RetrieveMemberData(group, sitem.ToString());
                        }
                    }

                    

                }
                tmpdb.code = code;
                tmpdb.name = group.name;
                tmpdb.member.Add(userID);
                //todo add groupinfo
                _controller.group_list.Add(group);
               
                Debug.Log("add");

                int memberidx = _controller.group_list[_controller.group_list.Count - 1].member.Count;
                
                FirebaseDatabase.DefaultInstance.GetReference("GROUP/" + code).SetRawJsonValueAsync(JsonUtility.ToJson(tmpdb));
                int gidx = _controller.group_list.Count - 1;
                GROUPDB DB = new GROUPDB();
                DB.code = code;

                FirebaseDatabase.DefaultInstance.GetReference("USER/" + userID + "/group_list/" + gidx).SetRawJsonValueAsync(JsonUtility.ToJson(DB));
                _controller.setGroup();
            }

        });
       
        
    }

    public void RemoveGroupAlarm(string gcode, string key)
    {
        FirebaseDatabase.DefaultInstance.GetReference("GROUP/" + gcode + "/feed/" + key).RemoveValueAsync().ContinueWith(task =>
        {
            if (!task.IsCompleted)
            {
                Debug.LogError("cannot remove data. error");
            }
            else
            {


            }
        });
    }
}

public class OnlyOneSchedule
{
    public string start1;
    public string finish1;
    public string description;
    public string day1;
    
}
