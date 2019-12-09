using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using static MainController;

public class FeedMgr : MonoBehaviour
{
    public GameObject inviteFeed, groupFeed, inviteList, groupfeedList;
    public Dictionary<int, string> objtoinviteList = new Dictionary<int, string>();
    public Dictionary<int, string> objtoGroupfeedList = new Dictionary<int, string>();
    public Dictionary<int, int> objToGroupcode = new Dictionary<int, int>();

    MainController _controller;
    // Start is called before the first frame update
    void Start()
    {
        _controller = GameObject.Find("mainCtr").GetComponent<MainController>();

        int inviteIdx = 0;
        foreach (KeyValuePair<string, MainController.Feed> items in _controller._user.feed_list)
        {
            inviteList.transform.GetChild(inviteIdx).gameObject.SetActive(true);
            inviteList.transform.GetChild(inviteIdx).GetChild(0).GetComponent<Text>().text = items.Value.body;
            objtoinviteList.Add(inviteIdx, items.Key);

            inviteIdx++;
        }

        int groupfeedListindex = 0;
        for(int i = 0; i< _controller.group_list.Count; i++)
        {
            Group groups = _controller.group_list[i];
            string curGroupcode = groups.code;
            foreach (KeyValuePair<string, MainController.Feed> items in groups.feed_list)
            {
                Debug.Log("feedlist : " + groups.feed_list.Count);
                groupfeedList.transform.GetChild(groupfeedListindex).gameObject.SetActive(true);
                groupfeedList.transform.GetChild(groupfeedListindex).GetChild(0).GetComponent<Text>().text = items.Value.body;
                objtoGroupfeedList.Add(groupfeedListindex, items.Key);
                objToGroupcode.Add(groupfeedListindex, i);
                groupfeedListindex++;
            }
        }
    }


    public void DenyInvite()
    {
        GameObject tmp = EventSystem.current.currentSelectedGameObject;
        GameObject parent = tmp.transform.parent.gameObject;

        int selectItemindex =-1 ;
        for(int i = 0; i<inviteList.transform.childCount; i++)
        {
            if(inviteList.transform.GetChild(i).gameObject == parent.gameObject)
            {
                selectItemindex = i;
                inviteList.transform.GetChild(i).gameObject.SetActive(false);
                break;
            }
        }
        FirebaseMgr.Instance.RemoveUserFeed(objtoinviteList[selectItemindex]);
        _controller._user.feed_list.Remove(objtoinviteList[selectItemindex]);
        objtoinviteList.Remove(selectItemindex);

    }

    public void AcceptInvite()
    {
        GameObject tmp = EventSystem.current.currentSelectedGameObject;
        GameObject parent = tmp.transform.parent.gameObject;

        int selectItemindex = -1;
        for (int i = 0; i < inviteList.transform.childCount; i++)
        {
            if (inviteList.transform.GetChild(i).gameObject == parent.gameObject)
            {
                selectItemindex = i;
                inviteList.transform.GetChild(i).gameObject.SetActive(false);
                break;
            }
        }

        FirebaseMgr.Instance.AcceptInvite(_controller._user.feed_list[objtoinviteList[selectItemindex]]);
        FirebaseMgr.Instance.RemoveUserFeed(objtoinviteList[selectItemindex]);
        _controller._user.feed_list.Remove(objtoinviteList[selectItemindex]);
        objtoinviteList.Remove(selectItemindex);


    }
    public void CheckGroupAlarm()
    {
        
    }
    public void RemoveGroupAlarm()
    {
        GameObject tmp = EventSystem.current.currentSelectedGameObject;
        GameObject parent = tmp.transform.parent.gameObject;

        int selectItemindex = -1;
        for (int i = 0; i < inviteList.transform.childCount; i++)
        {
            if (inviteList.transform.GetChild(i).gameObject == parent.gameObject)
            {
                selectItemindex = i;
                inviteList.transform.GetChild(i).gameObject.SetActive(false);
                break;
            }
        }

        string gcode = _controller.group_list[objToGroupcode[selectItemindex]].code;
        FirebaseMgr.Instance.RemoveGroupAlarm(gcode, objtoGroupfeedList[selectItemindex]);
        objtoGroupfeedList.Remove(selectItemindex);
        objToGroupcode.Remove(selectItemindex);
    }

    
}
