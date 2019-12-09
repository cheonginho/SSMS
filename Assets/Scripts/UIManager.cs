using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class UIManager : MonoBehaviour
{
    
    public Text inputID, inputPW, warningMsg;
    public GameObject warningPage, calendarPage,joinPage;
    FirebaseMgr _dbMgr = FirebaseMgr.Instance;



    public Text joinmail, joinpw, joinconfirmpw, joinNickname;

    public void Join()
    {
       
        if(joinmail.text.Equals("") || joinpw.text.Equals("") || joinconfirmpw.text.Equals("")|| joinNickname.text.Equals(""))
        {
            OnWarningMsg("wirte all information");
        }
        else if (!joinmail.text.Contains("@"))
        {
            OnWarningMsg("write email");
        }
        else if (!joinpw.text.Equals(joinconfirmpw.text))
        {
            OnWarningMsg("enter correct password");
        }
        else
        {
            _dbMgr.GoogleJoin(joinmail.text, joinpw.text, joinNickname.text);
        }
       
    }

    public void CloseJoinPage()
    {
        joinmail.text = "";
        joinpw.text = "";
        joinconfirmpw.text = "";
        joinNickname.text = "";
        joinPage.SetActive(false);

    }
    public void Login()
    {
        _dbMgr.GoogleLogin(inputID.text, inputPW.text);
        //SceneManager.LoadScene("Calendar");
        //_dbMgr.GoogleLogin("jj@naver.com", "jointest");
    }
    public void loadscene()
    {
        SceneManager.LoadScene("Calendar");
    }
    public void OnWarningMsg(string msg)
    {
        warningMsg.text = msg;
        Debug.Log("ho!");
        warningPage.SetActive(true);
    }

    public void OffAllEvents()
    {

    }
}
