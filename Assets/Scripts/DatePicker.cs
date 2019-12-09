using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DatePicker : MonoBehaviour
{
   
    public enum PickerMode
    {
        WEEK,
        HOUR,
        MINIUTE
    }

    int _curIndex;
    public PickerMode mode;
    int list_size = 0;
    public Text dataText;
    public GameObject content;
    string[] weekList = { "Monday","Tuesday","Wednsday","Thursday","Friday"};

       
    void Awake()
    {
        switch (mode) {
            case PickerMode.WEEK:
                list_size = 5;
                _curIndex = 0;
                break;
            case PickerMode.HOUR:
                list_size = 24;
                _curIndex = 9;
                break;
            case PickerMode.MINIUTE:
                list_size = 60;
                _curIndex = 0;
                break;
        }

        for(int idx = 0; idx < content.transform.childCount; idx++)
        {
            content.transform.GetChild(idx).transform.GetComponent<RectTransform>().anchoredPosition = Vector2.down * (25 + 50 * idx);
        }
    }
    public void Before()
    {
        _curIndex = (_curIndex == 0) ? list_size - 1 : _curIndex - 1;
        if(mode != PickerMode.WEEK)
        {
            dataText.text = _curIndex.ToString();

        }
        else
        {
            if (_curIndex > 4) return;
            dataText.text = weekList[_curIndex];
        }
    }
    public void Next()
    {
        _curIndex = (_curIndex == list_size - 1) ? 0 : _curIndex + 1;
        if (mode != PickerMode.WEEK)
        {
            dataText.text = _curIndex.ToString();

        }
        else
        {
            if (_curIndex > 4) return;
            dataText.text = weekList[_curIndex];
        }
    }

    public string GetData()
    {
        
        if(mode == PickerMode.WEEK)
        {
            return weekList[_curIndex];
        }
        else
        {
            return _curIndex.ToString();
        }
    }

    public void SelectItem(int i)
    {
        _curIndex = i;
        dataText.text = _curIndex.ToString("00");
        
    }
    public void SelectItem(string name)
    {
        for(int idx =0; idx < list_size; idx++)
        {
            if (weekList[idx].Equals(name))
            {
                _curIndex = idx;
                dataText.text = weekList[_curIndex];
                break;
            }
        }
    }
}
