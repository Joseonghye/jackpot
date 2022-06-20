using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Xml;
using UnityEngine.SceneManagement;
using System.IO;
using System;

// 저장 시스템 타입 
public enum SystemType
{
    New = 0,
    Save,
    Load,
    Delete
}

public enum SceneType
{
    Title=0,
    LibraryIntro,
    Library,
    ModoRoom,
    DoctorRoom,
    Game_Gear,
    DoctorRoom_event,
    DoctorRoom_out,
    SecretRoom,
    TestScene
}

public class GameManager : MonoBehaviour
{

    //싱글톤
    private static GameManager _instance = null;
    public static GameManager Instance { get { return _instance; } }

    [SerializeField]
    private GameObject _main;
    public GameObject Main { get { return _main; } set { _main = value; } }

    EventManager _event;
    public EventManager Event { get { return _event; } }

    DoctorRoomCtrl _drRoomCtrl;
    public DoctorRoomCtrl DrRoomCtrl { get { return _drRoomCtrl; } }

    //현재 씬
    static SceneType _curScene = SceneType.Title;
    public SceneType CurScene { get { return _curScene; } }

    //다음 씬
    bool _isNext = false;
    public bool IsNext { get { return _isNext; } }

    SceneType _nextScene;
    public void SetNextScene(SceneType type) { _nextScene = type; _isNext = true; }
    public void NextScene()
    {
        _curScene = _nextScene;
        _isNext = false;

        /*
        if (_curScene == SceneType.DoctorRoom_event)
        {
            Cursor.visible = false;
            Cursor.lockState = CursorLockMode.Locked;
        }
        else
        {
            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.None;
        }
        */
        if (_curScene == SceneType.Library)
        {
            _audio_bgm.enabled = true;
        }
        else if (_curScene == SceneType.DoctorRoom_event)
        {
            _audio_bgm.Pause();
            _drRoomCtrl.setCheck(true);
        }
        else
        {
            if (!_audio_bgm.isPlaying)
                _audio_bgm.UnPause();
        }
        
        SceneManager.LoadScene(_curScene.ToString());
    }

    DateTime _startTime;
    public DateTime StartTime { get { return _startTime; } }
    public void SetStratTime() { _startTime = DateTime.Now; }

    [SerializeField]
   // NoteManager _note;
   // public NoteManager Note { get { return _note; } }

    AudioSource _audio_bgm;
    public AudioSource Audio;

    private void Awake()
    {
        // 씬에 이미 게임 매니저가 있을 경우
        if (_instance)
        {
            //삭제
            Destroy(gameObject);
            return;
        }
        // 유일한 게임 매니저 
        _instance = this;

        _main = GameObject.Find("Main Camera");

        _event = GetComponent<EventManager>();
        _drRoomCtrl = GetComponent<DoctorRoomCtrl>();
        _audio_bgm = GetComponent<AudioSource>();
 
        DontDestroyOnLoad(gameObject);

    }

    //========================================================================
    //                               New Game
    //========================================================================
    public void NewGame()
    {
        
        TextAsset txt = (TextAsset)Resources.Load("Xml/Observe");
        XmlDocument objDoc = new XmlDocument();
        objDoc.LoadXml(txt.text);
        objDoc.Save(Application.dataPath + "/Play_ob.xml");

        txt = (TextAsset)Resources.Load("Xml/Note");
        XmlDocument noteDoc = new XmlDocument();
        noteDoc.LoadXml(txt.text);
        noteDoc.Save(Application.dataPath + "/Play_note.xml");

        txt = (TextAsset)Resources.Load("Xml/QL");
        XmlDocument QLDoc = new XmlDocument();
        QLDoc.LoadXml(txt.text);
        QLDoc.Save(Application.dataPath + "/Play_QL.xml");

        txt = (TextAsset)Resources.Load("Xml/Select");
        XmlDocument sDoc = new XmlDocument();
        sDoc.LoadXml(txt.text);
          sDoc.Save(Application.dataPath + "/Play_select.xml");

        //단서 초기화 
       // _note.Init(false);

        //게임 시작 
     //   _curScene = SceneType.LibraryIntro;

        SetStratTime();
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;

        _curScene = SceneType.TestScene;
        // SceneManager.LoadScene(_curScene.ToString());
        SceneManager.LoadScene("TestScene");

    }


    // 파일 복사 ( 복사 할 곳 , 원본 파일이름 )
    private void CopyFile(XmlDocument doc,string nodeName)
    {
        // 원본 파일 불러오기
        TextAsset txt = (TextAsset)Resources.Load("Xml/" + nodeName);
        XmlDocument txtDoc = new XmlDocument();
        txtDoc.LoadXml(txt.text);

        // 복사 할 노드 선택
        XmlNode node = txtDoc.SelectSingleNode(nodeName);
        XmlNodeList list = node.ChildNodes;

        // 복사 
        foreach (XmlNode content in list)
        {
            doc.DocumentElement.AppendChild(doc.ImportNode(content, true));
        }

    }
    // 파일 복사 ( 복사 할 곳 , 파일 경로, 노드 이름 )
    private void CopyFile(XmlDocument doc, string path,string nodeName)
    {
        // 원본 파일 불러오기
        XmlDocument txtDoc = new XmlDocument();
        txtDoc.Load(path);

        // 복사 할 노드 선택
        XmlNode node = txtDoc.SelectSingleNode(nodeName);
        XmlNodeList list = node.ChildNodes;

        // 복사 
        foreach (XmlNode content in list)
        {
            doc.DocumentElement.AppendChild(doc.ImportNode(content, true));
        }

    }

    //========================================================================
    //                                  Save
    //========================================================================
    public void SaveGame(int num)
    {
        // 1. 현재 사용 중인 파일 가져오기
        XmlDocument objDoc = new XmlDocument();
        objDoc.Load(Application.dataPath + "/Play_obj.xml");
        XmlDocument noteDoc = new XmlDocument();
        noteDoc.Load(Application.dataPath + "/Play_note.xml");

        // 2. 슬롯 번호에 맞게 저장
        objDoc.Save(Application.dataPath + "/Play_obj" + num + ".xml");
        noteDoc.Save(Application.dataPath + "/Play_note" + num + ".xml");

        // 3. UI 재 설정

    }


    //========================================================================
    //                                 Load
    //========================================================================
    public void LoadGame(int num)
    {
        // 비어 있는 슬롯이 아니라면
        if (num != 0)
        {
            FileInfo fi = new FileInfo(Application.dataPath + "/Play_obj" + num + ".xml");
            if (fi.Exists)
            {
                // 단서 파일 불러오기 (복사)
                XmlDocument clueDoc = new XmlDocument();
                XmlDeclaration dec = clueDoc.CreateXmlDeclaration("1.0", "utf-8", "no");
                XmlElement start = clueDoc.CreateElement("Obj");
                clueDoc.AppendChild(dec);
                clueDoc.AppendChild(start);
                string path = Application.dataPath + "/Play_obj" + num + ".xml";
                CopyFile(clueDoc, path, "Obj");
                clueDoc.Save(Application.dataPath + "/Play_obj.xml");

                // 수첩 파일 불러오기 (복사)
                XmlDocument noteDoc = new XmlDocument();
                XmlDeclaration root = noteDoc.CreateXmlDeclaration("1.0", "utf-8", "no");
                XmlElement node = noteDoc.CreateElement("Note");
                noteDoc.AppendChild(root);
                noteDoc.AppendChild(node);
                path = Application.dataPath + "/Play_note" + num + ".xml";
                CopyFile(noteDoc, path, "Note");
                noteDoc.Save(Application.dataPath + "/Play_note.xml");

                //게임 시작 
                _curScene = SceneType.Library;
                SceneManager.LoadScene("Library");
            }
        }

    }

}
