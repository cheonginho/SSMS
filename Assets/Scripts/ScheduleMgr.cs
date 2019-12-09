using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using static MainController;

public class ScheduleMgr : MonoBehaviour
{
    public GameObject[] _scheduleObj;
    public Text ScheduleName;
    const int _scheduleObjLength = 50;
    public Color[] color;

    // Start is called before the first frame update
    MainController _controller;
    public GameObject schBottom;
    bool isSecondSchOn = false;
    public GameObject member_not, member_exist, member_scrollview;
    void Start()
    {
        _controller = GameObject.Find("mainCtr").GetComponent<MainController>();
        Initialize();
        
    }
    public void Initialize()
    {
        ShowSchedule(_controller._user.schedule_list);

        if (_controller.CurrentGroup == -1 || _controller.group_list[_controller.CurrentGroup].member.Count == 0)
        {
            schBottom.SetActive(false);
            member_not.SetActive(true);
            member_exist.SetActive(false);
        }
        else
        {
            schBottom.SetActive(true);
            member_not.SetActive(false);
            member_exist.SetActive(true);
            int scrollIndex = 1;

            List<Member> memberList = _controller.group_list[_controller.CurrentGroup].member;
            int memberCount = memberList.Count;
            Debug.Log("memberCoutn :" + memberList.Count);
            int idx = 0;
            for (; idx < memberCount; idx++)
            {
                member_scrollview.transform.GetChild(idx + 1).gameObject.SetActive(true);
                member_scrollview.transform.GetChild(idx + 1).GetChild(0).GetComponent<Text>().text = memberList[idx].nickname + "의 시간표";

            }
            member_scrollview.transform.GetChild(idx + 1).gameObject.SetActive(true);
            member_scrollview.transform.GetChild(idx + 1).GetChild(0).GetComponent<Text>().text = "모든 시간표 합쳐보기";
            idx++;
            for (; idx < member_scrollview.transform.childCount - 1; idx++)
            {
                member_scrollview.transform.GetChild(idx + 1).gameObject.SetActive(false);

            }


            //todo: 리스트생성하기.
        }

    }

    int objIndex = 0;
    int colorIndex = 0;
    public GameObject editScheduleBtn;
    Dictionary<int, string> objToScheduleList = new Dictionary<int, string>();
    public void ShowSchedule(Dictionary<string, Schedule> list)
    {
        Debug.Log("start to load my schedule");
        _isMySchedule = true;
        editScheduleBtn.SetActive(true);
        ScheduleName.text = "내 시간표";
        //기존의 스케쥴 지우기
        objToScheduleList = new Dictionary<int, string>();
        for (int idx = 0; idx < _scheduleObjLength; idx++)
        {
            _scheduleObj[idx].SetActive(false);
        }

        objIndex = 0;
        colorIndex = 0;
        foreach (KeyValuePair<string, Schedule> result in list)
        {
            Schedule item = result.Value;

            AddSingleScheduleItem(item.start1, item.finish1, objIndex, item.description, item.day1, colorIndex);
            objToScheduleList.Add(objIndex, result.Key);
            objIndex++;

            //두번째수업 존재할때
            if (item.day2 != null && item.day2 !="")
            {
                AddSingleScheduleItem(item.start2, item.finish2, objIndex, item.description, item.day2, colorIndex);
                objToScheduleList.Add(objIndex, result.Key);
                objIndex++;
            }

            colorIndex = (colorIndex + 1) % 5;
        }

    }

    public void ShowSchedule(string nickname, Dictionary<string,Schedule> list)
    {
        _isMySchedule = false;
        editScheduleBtn.SetActive(false);
        ScheduleName.text = nickname + " 의 시간표";
        //기존의 스케쥴 지우기
        objToScheduleList.Clear();
        for (int idx = 0; idx < _scheduleObjLength; idx++)
        {
            _scheduleObj[idx].SetActive(false);
        }

        objIndex = 0;
        colorIndex = 0;
        foreach (KeyValuePair<string, Schedule> result in list)
        {
            Schedule item = result.Value;

            AddSingleScheduleItem(item.start1, item.finish1, objIndex, item.description, item.day1, colorIndex);
            objToScheduleList.Add(objIndex, result.Key);
            objIndex++;

            //두번째수업 존재할때
            if (item.day2 != null && item.day2 != "")
            {
                AddSingleScheduleItem(item.start2, item.finish2, objIndex, item.description, item.day2, colorIndex);
                objToScheduleList.Add(objIndex, result.Key);
                objIndex++;
            }

            colorIndex = (colorIndex + 1) % 5;
        }
        foreach (KeyValuePair<string, Schedule> result in list)
        {
            Schedule item = result.Value;

            AddSingleScheduleItem(item.start1, item.finish1, objIndex, item.description, item.day1, colorIndex);
            objToScheduleList.Add(objIndex, result.Key);
            objIndex++;

            //두번째수업 존재할때
            if (item.day2 != null && item.day2 !="")
            {
                AddSingleScheduleItem(item.start2, item.finish2, objIndex, item.description, item.day2, colorIndex);
                objToScheduleList.Add(objIndex, result.Key);
                objIndex++;
            }

            colorIndex = (colorIndex + 1) % 5;
        }

    }
    private void ShowAllMembersSchedule()
    {
        _isMySchedule = false;
        editScheduleBtn.SetActive(false);
        ScheduleName.text = "모두의 시간표";
        //기존의 스케쥴 지우기
        objToScheduleList.Clear();
        for (int idx = 0; idx < _scheduleObjLength; idx++)
        {
            _scheduleObj[idx].SetActive(false);
        }


        objIndex = 0;

        foreach (KeyValuePair<string, Schedule> result in _controller._user.schedule_list)
        {
            Schedule item = result.Value;

            AddSingleScheduleItem(item.start1, item.finish1, objIndex, "", item.day1, 0);
            objToScheduleList.Add(objIndex, result.Key);
            objIndex++;

            //두번째수업 존재할때
            if (item.day2 != null && item.day2 != "")
            {
                AddSingleScheduleItem(item.start2, item.finish2, objIndex,"" , item.day2, 0);
                objToScheduleList.Add(objIndex, result.Key);
                objIndex++;
            }

            
        }


        foreach (Group group in _controller.group_list)
        {
            foreach(Member member in group.member)
            {
                foreach (KeyValuePair<string, Schedule> result in member.schedule_list)
                {
                    Schedule item = result.Value;

                    AddSingleScheduleItem(item.start1, item.finish1, objIndex, "", item.day1, 0);
                    objToScheduleList.Add(objIndex, result.Key);
                    objIndex++;

                    //두번째수업 존재할때
                    if (item.day2 != null && item.day2 != "")
                    {
                        AddSingleScheduleItem(item.start2, item.finish2, objIndex, "", item.day2, 0);
                        objToScheduleList.Add(objIndex, result.Key);
                        objIndex++;
                    }

                    
                }
            }
        }

    }
    private void AddSingleScheduleItem(string start, string finish, int objIndex, string description, string day, int colorIndex)
    {

        TimeSpan span = Convert.ToDateTime(finish) - Convert.ToDateTime(start);
        int hourspan = span.Hours;
        int minspan = span.Minutes;

        _scheduleObj[objIndex].SetActive(true);
        string[] hourtmp = start.Split(':');
        int hour = Convert.ToInt32(hourtmp[0]);
        int min = Convert.ToInt32(hourtmp[1]);

        //_scheduleObj[objIndex].GetComponent<RectTransform>().anchoredPosition = Vector2.down * (50 + 100 * (hour - 9) + a);
        _scheduleObj[objIndex].GetComponent<RectTransform>().sizeDelta = Vector2.right * 125 + Vector2.up * (hourspan * 100 + ((float)minspan / 60 * 100));
        _scheduleObj[objIndex].transform.GetChild(0).GetComponent<Text>().text = description;
        float a = (float)min / 60 * 100;
        Vector2 tmp = Vector2.zero;

        switch (day)
        {
            case "Monday":
                break;
            case "Tuesday":
                tmp = Vector2.right * 127;
                break;
            case "Wednsday":
                tmp = Vector2.right * 254;
                break;
            case "Thursday":
                tmp = Vector2.right * 381;
                break;
            case "Friday":
                tmp = Vector2.right * 508;
                break;
        }

        _scheduleObj[objIndex].GetComponent<RectTransform>().anchoredPosition = tmp + Vector2.down * (_scheduleObj[objIndex].GetComponent<RectTransform>().sizeDelta.y / 2 + 100 * (hour - 9) + a);
        _scheduleObj[objIndex].GetComponent<Image>().color = color[colorIndex];

    }


    public Text _addSchDescription;
    public DatePicker day1, day2, s1hour, s2hour, s1min, s2min, f1hour, f1min, f2hour, f2min;



    /// <summary>
    /// 추가/삭제/수정함수
    /// 
    /// </summary>


    /// <summary>
    /// 시간표가 새로 추가될수있는지 파악함.
    /// 1.시작시간이 끝나는 시간보다 작을것
    /// 2.두번째 스케쥴이 들어가는건지,아닌지 파악할것 ( 시간이 같으면 추가되지 않는거임)
    /// 3. 이미 기존에 있는 시간표와 조금이라도 겹치는지
    /// 
    /// </summary>
    /// <param name="sc"></param>
    /// <param name="idx">만약 수정하는거라면 몇 번째 값이 수정되는건지. 추가면 -1의값을가진다.</param>
    public Schedule CheckIfCanbeAdded(Schedule sc, int idx)
    {
        //todo: check if this schedule canbe added.
        return sc;
    }

    public InputField add_shDescription;
    public GameObject add_page;
    public void AddSchedule()
    {
        if(add_shDescription.text == "")
        {
            _controller.OnWarningMsg("수업 이름을 기입하세요.");
            return;
        }

        Schedule tmp = new Schedule();
        tmp.start1 = s1hour.GetData() + ":" + s1min.GetData();
        tmp.finish1 = f1hour.GetData() + ":" + f1min.GetData();
        tmp.day1 = day1.GetData();
        tmp.description = add_shDescription.text;
        
        if (isSecondSchOn)
        {
            tmp.start2 = s2hour.GetData() + ":" + s2min.GetData();
            tmp.finish2 = f2hour.GetData() + ":" + f2min.GetData();
            tmp.day2 = day2.GetData();
        }
        else
        {
            tmp.start2 = null;
            tmp.finish2 = null;
            tmp.day2 = null;
        }
        if (CheckIfCanbeAdded(tmp,-1) == null)
        {
            return;
        }

       

        string key = FirebaseMgr.Instance.AddSchedule(tmp);
        _controller._user.schedule_list.Add(key, tmp);
        AddSingleScheduleItem(tmp.start1, tmp.finish1, ++objIndex, tmp.description, tmp.day1, colorIndex);
        objToScheduleList.Add(objIndex, key);
        if (isSecondSchOn)
        {
            AddSingleScheduleItem(tmp.start2, tmp.finish2, ++objIndex, tmp.description, tmp.day2, colorIndex);
            objToScheduleList.Add(objIndex, key);
            AddSingleScheduleItem(tmp.start2, tmp.finish2, ++objIndex, tmp.description, tmp.day2, colorIndex);
            objToScheduleList.Add(objIndex, key);
        }

        isSecondSchOn = false;
        add_page.SetActive(false);
    }

    public void EditSchedule()
    {
        //todo : GUI
        Schedule tmp = new Schedule();
        tmp.start1 = s1hour.GetData() + ":" + s1min.GetData();
        tmp.finish1 = f1hour.GetData() + ":" + f1min.GetData();
        tmp.day1 = day1.GetData();
        if (isSecondSchOn)
        {
            tmp.start2 = s2hour.GetData() + ":" + s2min.GetData();
            tmp.finish2 = f2hour.GetData() + ":" + f2min.GetData();
            tmp.day2 = day2.GetData();
        }
        

        

        if (CheckIfCanbeAdded(tmp, -1) == null)
        {
            return;
        }

        

        string key = FirebaseMgr.Instance.AddSchedule(tmp);
        _controller._user.schedule_list[editingSchedule] = tmp;
        ShowSchedule(_controller._user.schedule_list);
    }

    string editingSchedule = "";//고유번호
    public GameObject editingPage;
    public Text class_descriptoin, class_time, class_time2;
    public void OpenEditPage(int index)
    {
        if (!_isMySchedule) return;
        Debug.Log(index);
        string key = objToScheduleList[index];
        
        Schedule sch = _controller._user.schedule_list[key];
        class_descriptoin.text = sch.description;
        class_time.text = sch.day1 + "/" + sch.start1 + " ~ " + sch.finish1;
        if(sch.day2 !=null &&sch.day2 != "")
        {
            class_time2.text = sch.day2 + "/" + sch.start2 + " ~ " + sch.finish2;
        }
        else
        {
            class_time.text = "";
        }
        editingSchedule = key ;
        editingPage.SetActive(true);
        
    }


    
    public void DeleteSchedule()
    {
        Debug.Log("start to delete . index : " + editingSchedule);
        _controller._user.schedule_list.Remove(editingSchedule);
        ShowSchedule(_controller._user.schedule_list);

        FirebaseMgr.Instance.RemoveSchedule(editingSchedule);
        editingPage.SetActive(false);

    }
    bool _isMySchedule = true; //수정가능한 스케쥴인지?

    public void ShowMemberSchedule(int index)
    {
        if(index == -1)
        {
            ShowSchedule(_controller._user.schedule_list);
        }
        else if(index >= _controller.group_list[_controller.CurrentGroup].member.Count)
        {
            ShowAllMembersSchedule();
            Debug.Log("aa");
        }
        else
        {
            Member tmp = _controller.group_list[_controller.CurrentGroup].member[index];
            ShowSchedule(tmp.nickname, tmp.schedule_list);
        }
    }
    
    public void SecondSchon()
    {
        this.isSecondSchOn = true;
    }

  
    
  
}
