using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class SettingMgr : MonoBehaviour
{
    MainController _controller;
    // Start is called before the first frame update
    void Start()
    {
        _controller = GameObject.Find("mainCtr").GetComponent<MainController>();
    }


    public Toggle alarmToggle;
    public void OpenSetting()
    {

        alarmToggle.isOn = _controller._user.isAlarmOn;
        
    }

    public void SwitchAlarm()
    {
        Debug.Log(alarmToggle.isOn);
        if (alarmToggle.isOn)
        {
            FirebaseMgr.Instance.SetAlarm("true");
        }
        else{
            FirebaseMgr.Instance.SetAlarm("false");
        }
        
        _controller._user.isAlarmOn = alarmToggle.isOn;
    }

    public void Logout()
    {
        Destroy(_controller.gameObject);
        SceneManager.LoadScene(0);
        FirebaseMgr.Instance.Logout();

    }
    
}
