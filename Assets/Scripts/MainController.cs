using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using static Enums;
using static FlatCalendar;

public class MainController :  MonoBehaviour
{
    public class Dday
    {
        public int year;
        public int month;
        public int day;
        public int hour;
        public int min;
        public string description;

        public Dday(int _year, int _month, int _day, int _hour, int _min, string _description)
        {
            year = _year;
            month = _month;
            day = _day;
            hour = _hour;
            min = _min;
            description = _description;
            
        }

    }

    public class Schedule
    {
        public string day1;
        public string day2;
        public string start1;
        public string start2;
        public string finish1;
        public string finish2;
        public string description;


        public Schedule() { }

    }

    public class Member
    {
        public string uid;
        public string nickname;
        public Dictionary<string,Schedule> schedule_list = new Dictionary<string, Schedule>();

    }
    public class Events
    {
        public int year;
        public int month;
        public int day;
        public int hour;
        public int min;
        public string place;
        public string title;
        public string description;

        public Events(int _year, int _month, int _day, int _hour, int _min, string _title, string _description, string _place)
        {
            year = _year;
            month = _month;
            day = _day;
            hour = _hour;
            min = _min;
            place = _place;
            description = _description;
            title = _title;
        }

    }
    public class Group {

        public string code;
        public string name;

        public List<Member> member = new List<Member>();
        public Dictionary<string, Events> events_list = new Dictionary<string, Events>();
        public Dictionary<string, Feed> feed_list = new Dictionary<string, Feed>();

        public Group() { }

        public Group(string key, string gname, string uid)
        {
            code = key;
            name = gname;
        }
        
    }
    public class Todo
    {
        public string description;
        public bool isComplete;

        public Todo() { }

    }
    public class Feed
    {
        public string groupcode;
        public string title;
        public string body;
    }

    public class User
    {
        public Dictionary<string, Events> events_list;
        public string email;
        //public List<Events> events_list;
        public List<Dday> dDay_list;
        public string default_calendar;
        public  string deviceID;
        public bool isAlarmOn;
        public Dictionary<string,Schedule> schedule_list;
        public Dictionary<string, Todo> todo_list;
        public string nickname;
        public Dictionary<string,Feed> feed_list;
        
        public User() {

            default_calendar = "default";
            deviceID = "TEST";
            isAlarmOn = false;
            dDay_list = new List<Dday>();
            schedule_list = new Dictionary<string, Schedule>();
            events_list = new Dictionary<string, Events>();
            todo_list = new Dictionary<string, Todo>();
            feed_list = new Dictionary<string, Feed>();
        }
    
    }

    

    FirebaseMgr _dbMgr;
    public UIManager _uiMgr;
    public FlatCalendar _calendar;
    public User _user;
    public List<Group> group_list = new List<Group>();
    public int CurrentGroup = -1;

    void Awake()
    {
        _dbMgr = FirebaseMgr.Instance;
        _dbMgr.Initialize(this);

        //_uiMgr = UIManager.Instance;
        //_uiMgr.Initialize();

        DontDestroyOnLoad(gameObject);
        //SceneManager.LoadScene("Calendar");
        _user = new User();
        // _calendar = GameObject.Find("FlatCalendar").GetComponent<FlatCalendar>();

        _dbMgr.AutoLogin();
        
    }

    public GameObject joinPage;
    public void OffJoinPage()
    {
        joinPage.SetActive(false);
    }

    public void OpenCalendar()
    {
        Debug.Log("open calendar!!!");
        //_uiMgr.loadscene();
        SceneManager.LoadScene(1);
        OffProgressBar();
    }

    public string FindEventKey(EventObj obj,int year, int month, int day)
    {
        foreach(KeyValuePair<string,Events> items in _user.events_list)
        {
            Events item = items.Value;
            if(item.year == year && item.month == month && item.day == day){

                if(obj.name.Equals(item.title) && obj.description.Equals(item.description)
                    && obj.hour == item.hour && obj.min == item.min && obj.place.Equals(item.place))
                {
                    return items.Key;
                } 
            }
        }

        return null;
    }

    public Text WarningText,ProgressText;
    public GameObject warningPage;
    public void OnWarningMsg(string msg)
    {
        OffProgressBar();
        gameObject.transform.GetChild(0).gameObject.SetActive(true);
        WarningText.text = msg;
    }

    public void OnProgressBar(string msg)
    {
        gameObject.transform.GetChild(1).gameObject.SetActive(true);
        ProgressText.text = msg;
    }
    public void OffProgressBar()
    {
        gameObject.transform.GetChild(1).gameObject.SetActive(false);
    }
    public void setGroup()
    {
        GameObject.Find("Calendar").GetComponent<CalendarMgr>().SetGroup();
    }
}
