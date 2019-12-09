using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static MainController;
using UnityEngine.UI;
using static FlatCalendar;

public class CalendarMgr : MonoBehaviour
{
    public FlatCalendar cal;
    MainController _controller;
    public GameObject group_view, show_eventView;
    public Dictionary<string, EventObj> eventToObj = new Dictionary<string, EventObj>();
    public Text show_evDate ,show_evTitle, show_evDescription,show_evTime, show_evPlace,user_nickname, user_email;
    
    void Awake()
    {
        
        _controller = GameObject.Find("mainCtr").GetComponent<MainController>();
        _controller._calendar = cal;
        cal.initFlatCalendar();
        cal.setUIStyle(4);
        user_nickname.text = _controller._user.nickname;
        user_email.text = _controller._user.email;
        Dictionary<string, Events> event_list = _controller._user.events_list;
        List<Group> group_list = _controller.group_list;
        //Debug.Log("count= " + event_list.Count);
        foreach(KeyValuePair<string, Events> items in event_list)
        {
            Events item = items.Value;
            EventObj newobj = new FlatCalendar.EventObj(items.Key, item.title, item.description, item.hour, item.min, item.place);
            cal.addEvent(item.year, item.month, item.day, newobj);
            eventToObj.Add(items.Key, newobj);
            Debug.Log("newobj :" + newobj.name);
        }
     

        int gidx = 1;
        foreach(Group g in group_list)
        {
            GameObject tmp = group_view.transform.GetChild(gidx).gameObject;
            tmp.SetActive(true);
            tmp.transform.GetChild(0).GetComponent<Text>().text = g.name;
            gidx++;
        }
        // cal.addEvent(2019, 11, 4, new FlatCalendar.EventObj("a", "b"));
        cal.updateUiLabelEvents(cal.currentTime.year, cal.currentTime.month, cal.currentTime.day);
    }
   
    public void SetGroup()
    {
        int gidx = 1;
        foreach (Group g in _controller.group_list)
        {
            GameObject tmp = group_view.transform.GetChild(gidx).gameObject;
            tmp.SetActive(true);
            tmp.transform.GetChild(0).GetComponent<Text>().text = g.name;
            gidx++;
        }
        group_view.GetComponent<RectTransform>().sizeDelta = new Vector2(0, 100 * gidx);
    }
    public EventObj currentEvent;
   // public int currentGroup = -1;
    public void ShowEvent(int i )
    {
        EventObj obj = cal.eventList[i];
        currentEvent = cal.eventList[i];
        show_evDescription.text = obj.description;
        show_evTitle.text = obj.name;
        show_evDate.text = cal.currentTime.year + "-" + cal.currentTime.month + "-" + cal.currentTime.day;
        show_evTime.text = obj.hour.ToString("00") + " : " + obj.min.ToString("00");
        show_evPlace.text = obj.place;
        show_eventView.SetActive(true);
    }
   
    public void DeleteEvent()
    {

        if(_controller.CurrentGroup == -1)
        {
            string key = currentEvent.key;
            //string key = _controller.FindEventKey(currentEvent, cal.currentTime.year, cal.currentTime.month, cal.currentTime.day);
            if (key != null)
            {
                FirebaseMgr.Instance.DeleteEventDB(key);
                _controller._user.events_list.Remove(key);
                cal.eventList.Remove(currentEvent);
                cal.updateUiLabelEvents(cal.currentTime.year, cal.currentTime.month, cal.currentTime.day);
            }
          
            
        }
        else
        {
            string key = currentEvent.key;
            FirebaseMgr.Instance.DeleteGroupEventDB(_controller.group_list[_controller.CurrentGroup].code, key);
            _controller.group_list[_controller.CurrentGroup].events_list.Remove(key);
            cal.eventList.Remove(currentEvent);
            cal.updateUiLabelEvents(cal.currentTime.year, cal.currentTime.month, cal.currentTime.day);
        }
        cal.eventList.Remove(currentEvent);
        show_eventView.SetActive(false);
    }

    public GameObject revisePage;
    public InputField re_title, re_desription, re_hour, re_min, re_place;
    public Text add_date;
    public Text re_date;
   
    public void OpenRevisePage()
    {
        re_title.text = currentEvent.name;
        re_desription.text = currentEvent.description;
        re_place.text = currentEvent.place;
        re_hour.text = currentEvent.hour.ToString();
        re_min.text = currentEvent.min.ToString();

        re_date.text = cal.currentTime.year + "-" + cal.currentTime.month + "-" + cal.currentTime.day;

        revisePage.SetActive(true);


    }
    public void ReviseEvent()
    {
        currentEvent.ReviseEvent(re_title.text, re_desription.text, System.Convert.ToInt32(re_hour.text), System.Convert.ToInt32(re_min.text), re_place.text);
        //currentEvent.name = re_title.text;
        //currentEvent.description = re_desription.text;
        //currentEvent.hour = System.Convert.ToInt32(re_hour.text);
        //currentEvent.min = System.Convert.ToInt32(re_min.text);
        //cal.eventList[idx].name = re_title.text;
        //currentEvent.place = re_place.text;
        Debug.Log(re_title.text);

        

        cal.updateUiLabelEvents(cal.currentTime.year, cal.currentTime.month, cal.currentTime.day);
        if (_controller.CurrentGroup == -1)
        {
            Events newevent = new Events(cal.currentTime.year, cal.currentTime.month,
                cal.currentTime.day, currentEvent.hour, currentEvent.min, currentEvent.name, currentEvent.description, currentEvent.place);
            FirebaseMgr.Instance.ReviseEvent(currentEvent.key, newevent);

            show_evDescription.text = currentEvent.description;
            show_evTitle.text = currentEvent.name;
            show_evTime.text = currentEvent.hour.ToString("00") + " : " + currentEvent.min.ToString("00");
            show_evPlace.text = currentEvent.place;
        }
        else
        {
            Events newevent = new Events(cal.currentTime.year, cal.currentTime.month,
                   cal.currentTime.day, currentEvent.hour, currentEvent.min, currentEvent.name, currentEvent.description, currentEvent.place);
            FirebaseMgr.Instance.ReviseGroupEvent(_controller.group_list[_controller.CurrentGroup].code, currentEvent.key, newevent);

        }

        revisePage.SetActive(false);
        cal.updateUiLabelEvents(cal.currentTime.year, cal.currentTime.month, cal.currentTime.day);

    }
   

    public InputField Add_title, Add_description, add_hour, add_min, add_place;
    public void OpenAddPage()
    {
        add_date.text = cal.currentTime.year + "-" + cal.currentTime.month + "-" + cal.currentTime.day;
        Add_title.text = "";
        Add_description.text = "";
        add_hour.text = "";
        add_min.text = "";
        add_place.text = "";
    }

    public void AddEvent()
    {
        string title = Add_title.text;
        string description = Add_description.text;
        int hour = System.Convert.ToInt32(add_hour.text);
        int min = System.Convert.ToInt32(add_min.text);
        string place = add_place.text;

        if(_controller.CurrentGroup == -1)
        {
            Events newevent = new Events(cal.currentTime.year, cal.currentTime.month,
                cal.currentTime.day, hour, min, title, description, place);
            string key = FirebaseMgr.Instance.AddEventDB(newevent);
            _controller._user.events_list.Add(key, newevent);
            cal.addEvent(newevent.year, newevent.month, newevent.day, new FlatCalendar.EventObj(key,title, description, hour, min, place));
            _controller._calendar.updateUiLabelEvents(cal.currentTime.year, cal.currentTime.month, cal.currentTime.day);
            

        }
        else
        {
            //todo
            Debug.Log("aa");
            Events newevent = new Events(cal.currentTime.year, cal.currentTime.month,
                cal.currentTime.day, hour, min, title, description, place);
            string key = FirebaseMgr.Instance.AddGroupEventDB(_controller.group_list[_controller.CurrentGroup].code, newevent);
            _controller.group_list[_controller.CurrentGroup].events_list.Add(key, newevent);
            cal.addEvent(newevent.year, newevent.month, newevent.day, new FlatCalendar.EventObj(key, title, description, hour, min, place));
            _controller._calendar.updateUiLabelEvents(cal.currentTime.year, cal.currentTime.month, cal.currentTime.day);

            string feedkey = FirebaseMgr.Instance.AddGroupFeed(_controller.group_list[_controller.CurrentGroup].code, _controller.group_list[_controller.CurrentGroup].name,title);


        }

    }


    public InputField newGroup;
    public void MakeNewGroup()
    {
        string gname = newGroup.text;
        FirebaseMgr.Instance.MakeNewGroup(gname);
        //메뉴초기화ㅣ
        GameObject tmp = group_view.transform.GetChild(_controller.group_list.Count).gameObject;
        tmp.transform.GetChild(0).GetComponent<Text>().text = gname;
        tmp.SetActive(true);
           
           
    }


    public Text calendarName;
    public void ChangeCalendar(int i)
    {
        Dictionary<string, Events> event_list = null;
        _controller.CurrentGroup = i;
        
        //제목설정
        if (i == -1)
        {
            calendarName.text = "내 캘린더";
            event_list = _controller._user.events_list;
        }
        else
        {
            calendarName.text = _controller.group_list[i].name;
            event_list = _controller.group_list[i].events_list;
        }

        cal.removeAllCalendarEvents(); //이전꺼 지우기
        
       
        //Debug.Log("count= " + event_list.Count);
        foreach (KeyValuePair<string, Events> items in event_list)
        {
            Events item = items.Value;
            cal.addEvent(item.year, item.month, item.day, new FlatCalendar.EventObj(items.Key, item.title, item.description, item.hour, item.min, item.place));
        }
        cal.setCurrentTime();
        cal.evtListener_GoToNowday();
        //cal.updateUiLabelEvents(cal.currentTime.year, cal.currentTime.month, cal.currentTime.day);
    }

    ////멤버관련함수///
    public GameObject memberpage, memberListContent;
    public Text memgerpage_groupname;
    public void OpenMemberPage()
    {
        
        int groupcode = _controller.CurrentGroup;
        if (groupcode >= 0 && groupcode < _controller.group_list.Count)
        {
            for (int i = 0; i < 20; i++)
            {
                memberListContent.transform.GetChild(i).gameObject.SetActive(false);
            }

            memberpage.SetActive(true);
            Group curGroup = _controller.group_list[groupcode];

            for (int i = 0; i < curGroup.member.Count; i++)
            {
                memberListContent.transform.GetChild(i).gameObject.SetActive(true);
                memberListContent.transform.GetChild(i).Find("Text").GetComponent<Text>().text = curGroup.member[i].nickname;
            }
            memberListContent.GetComponent<RectTransform>().sizeDelta = new Vector2(0, 200 * curGroup.member.Count);
            memgerpage_groupname.text = "<" + curGroup.name + ">의 멤버";

        }
        else
        {
            Debug.Log("멤버창열기 오류발생. 그룹코드 : " + groupcode);
        }
    }


    public Text InviteText;
    public GameObject invitePage;
    public void InviteNewMember()
    {
        if(_controller.group_list[_controller.CurrentGroup].member.Count > 20)
        {
            _controller.OnWarningMsg("그룹 정원을 초과하였습니다.");
            return;
        }
        
        string email = InviteText.text;
        if (email.Equals(_controller._user.email))
        {
            _controller.OnWarningMsg("본인을 초대할 수는 없습니다.");
            return;
        }
        string gcode = _controller.group_list[_controller.CurrentGroup].code;
        string gname = _controller.group_list[_controller.CurrentGroup].name;
        FirebaseMgr.Instance.SendInviteMessage(email, gcode, gname);
        invitePage.SetActive(false);
        SetGroup();
    }

    public GameObject sharegroupList,groupPage;
    public void OpenShareGroup()
    {
        if (_controller.group_list.Count == 0)
        {
            return;
        }
        groupPage.SetActive(true);
        RectTransform rt = sharegroupList.GetComponent(typeof(RectTransform)) as RectTransform;
        rt.sizeDelta = new Vector2(0, 100*_controller.group_list.Count);
        
        int idx = 0;
        for(; idx < _controller.group_list.Count; idx++)
        {
            sharegroupList.transform.GetChild(idx).gameObject.SetActive(true);
            sharegroupList.transform.GetChild(idx).GetChild(0).GetComponent<Text>().text = _controller.group_list[idx].name;
        }
        for(; idx < sharegroupList.transform.childCount; idx++)
        {
            sharegroupList.transform.GetChild(idx).gameObject.SetActive(false);
        }
    }

    
    public void ShareEventToGroup(int i)
    {
        Events newevent = new Events(cal.currentTime.year, cal.currentTime.month,
                cal.currentTime.day, currentEvent.hour, currentEvent.min, currentEvent.name, currentEvent.description, currentEvent.place); ;
        string key = FirebaseMgr.Instance.AddGroupEventDB(_controller.group_list[i].code, newevent);
        _controller.group_list[i].events_list.Add(key, newevent);

        string feedkey = FirebaseMgr.Instance.AddGroupFeed(_controller.group_list[i].code, _controller.group_list[i].name, newevent.title);
        _controller.OnWarningMsg("공유가 완료되었습니다.");
        groupPage.SetActive(false);
    }
}
