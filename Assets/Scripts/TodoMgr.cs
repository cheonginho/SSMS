using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using static MainController;

public class TodoMgr : MonoBehaviour
{
    public GameObject AddTodo, todoList,DeleteMessage, editBtn;
    public GameObject TodoPrefab;
    public Dictionary<int, string> objToList = new Dictionary<int, string>();
    MainController _controller;
    public bool isInit = true;
    int objidx = 0;
    // Start is called before the first frame update
    void Start()
    {
        _controller = GameObject.Find("mainCtr").GetComponent<MainController>();
        Initialize();
    }
    

    public void Initialize()
    {
        Debug.Log("투두리스트 오픈. 총 개수 :" + _controller._user.todo_list.Count);
        todoList.GetComponent<RectTransform>().sizeDelta = new Vector2(0, 100 * _controller._user.todo_list.Count);
        Dictionary<string, Todo> tmpList = _controller._user.todo_list;

        isInit = true;
        foreach (KeyValuePair<string,Todo> item in tmpList)
        {
            //GameObject tmpObj = Instantiate(TodoPrefab);
            //tmpObj.transform.parent = todoList.transform;
            GameObject tmpObj = todoList.transform.GetChild(objidx).gameObject;
            tmpObj.SetActive(true);
            tmpObj.transform.GetChild(1).GetComponent<Text>().text = item.Value.description;
            objToList.Add(objidx, item.Key);
            MarkTodo(item.Value.isComplete, objidx);
            objidx++;
        }
        isInit = false;
    }

    public void MarkTodo(bool iscomplete, int objindex)
    {
        todoList.transform.GetChild(objindex).transform.GetChild(0).GetComponent<Toggle>().isOn = iscomplete;
        string text = _controller._user.todo_list[objToList[objindex]].description;  
        if (iscomplete)
        {
            todoList.transform.GetChild(objindex).transform.GetChild(1).GetComponent<Text>().text = StrikeThrough(text);
        }
        else
        {
            todoList.transform.GetChild(objindex).transform.GetChild(1).GetComponent<Text>().text = text;
        }
    }

    private string StrikeThrough(string s)
    {
        string strikethrough = "";
        foreach (char c in s)
        {
            strikethrough = strikethrough + c + '\u0336';
        }
        return strikethrough;
    }


    public InputField todoDescription;
    public Text EditPageTitle;
    public Button addTodoBtn;

    public void OpenAddTodoPage()
    {
        Debug.Log("click");
        todoDescription.text = "";
        AddTodo.SetActive(true);
        addTodoBtn.gameObject.SetActive(true);
        editBtn.SetActive(false);
        EditPageTitle.text = "할일 추가";
    }


    int _currentSelectedTodoObj = 0;
    public void OpenEditTodoPage()
    {
        AddTodo.SetActive(true);
        addTodoBtn.gameObject.SetActive(false);
        editBtn.gameObject.SetActive(true);
        EditPageTitle.text = "할일 수정";
        GameObject tmp = EventSystem.current.currentSelectedGameObject;
        GameObject parent = tmp.transform.parent.transform.parent.gameObject;

        Todo currentTodo = null; 
        for(int i= 0; i<parent.transform.childCount; i++)
        {
            if(parent.transform.GetChild(i).gameObject == tmp.transform.parent.gameObject)
            {
                currentTodo = _controller._user.todo_list[objToList[i]];
                _currentSelectedTodoObj = i;
                Debug.Log(i + "번째 일정 선택");
                break;
            }
        }
        if(currentTodo != null)
        {
            Debug.Log(currentTodo.description);
            todoDescription.text = currentTodo.description;
        }
        

    }
   
    public void EditTodo()
    {
        if (todoDescription.text == "") return;
        Todo todo = _controller._user.todo_list[objToList[_currentSelectedTodoObj]];
        todo.description = todoDescription.text;

        AddTodo.SetActive(false);
        FirebaseMgr.Instance.EditTodo(objToList[_currentSelectedTodoObj], todo);
        if (todoList.transform.GetChild(_currentSelectedTodoObj).GetChild(0).GetComponent<Toggle>().isOn)
        {
            todoList.transform.GetChild(_currentSelectedTodoObj).GetChild(1).GetComponent<Text>().text = StrikeThrough(todo.description);
        }
        else
        {
            todoList.transform.GetChild(_currentSelectedTodoObj).GetChild(1).GetComponent<Text>().text = todo.description;
        }
        

    }
    public void AddNewTodo()
    {
        if (todoDescription.text == "") return;
        Todo todo = new Todo();
        todo.description = todoDescription.text;
        todo.isComplete = false;

        string key = FirebaseMgr.Instance.AddNewTodo(this,todo);
        GameObject tmpObj = todoList.transform.GetChild(objidx).gameObject;
        tmpObj.SetActive(true);
        tmpObj.transform.GetChild(1).GetComponent<Text>().text = todo.description;
        objToList.Add(objidx, key);
        objidx++;
        //_controller._user.todo_list.Add(key, todo);
        AddTodo.SetActive(false);
        todoList.GetComponent<RectTransform>().sizeDelta = new Vector2(0, 100 * _controller._user.todo_list.Count);
    }


    public void DeleteTodo()
    {
        FirebaseMgr.Instance.DeleteTodo(objToList[_currentSelectedTodoObj]);
        _controller._user.todo_list.Remove(objToList[_currentSelectedTodoObj]);
        todoList.transform.GetChild(_currentSelectedTodoObj).gameObject.SetActive(false);
        objToList.Remove(_currentSelectedTodoObj);

        AddTodo.SetActive(false);
        todoList.GetComponent<RectTransform>().sizeDelta = new Vector2(0, 100 * _controller._user.todo_list.Count);
    }

    public void CheckTodo()
    {
        if (isInit) return;
        GameObject tmp = EventSystem.current.currentSelectedGameObject;
        GameObject parent = tmp.transform.parent.gameObject;
        if (tmp == null) return;
        int todoIndex = 0;
        Debug.Log(parent.name);
        for(int idx = 0; idx<todoList.transform.childCount; idx++)
        {
            if(todoList.transform.GetChild(idx).gameObject == parent)
            {
                todoIndex = idx;
                Debug.Log("todoindx ; " + idx);
                break;
            }
        }
        if (tmp.GetComponent<Toggle>().isOn)
        {
            Text tmpText = parent.transform.GetChild(1).GetComponent<Text>();
            tmpText.text = StrikeThrough(tmpText.text);
            //todo : firebase
        }
        else
        {
            
            parent.transform.GetChild(1).GetComponent<Text>().text = _controller._user.todo_list[objToList[todoIndex]].description;

        }
        _controller._user.todo_list[objToList[todoIndex]].isComplete = tmp.GetComponent<Toggle>().isOn;
        FirebaseMgr.Instance.EditTodo(objToList[todoIndex], _controller._user.todo_list[objToList[todoIndex]]);
    }


}
