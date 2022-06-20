using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public struct Slot
{
    public int id;   //테이블 아이디
    public string name;      // 이름
    public string prefabName; // 설명 프리팹 이름 


    public Slot(int _id, string _name, string _prefabName)
    {
        id = _id;
        name = _name;
        prefabName = _prefabName;
    }
}

public abstract class NotePage : MonoBehaviour {

    protected NotePanel _note;
    protected NotePanel Note { get { return _note; } }
    private void Awake()
    {
        _note = GameObject.Find("NotePanel").GetComponent<NotePanel>();
    }

    //초기화
    public abstract void Init();

    // 클릭

    // 새로 추가된거 재 설정
    public abstract void SetPage();
}
