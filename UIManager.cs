using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class UIManager : MonoBehaviour {

    // 싱글 톤
    private static UIManager _instance = null;
    public static UIManager Instance { get { return _instance; } }

    GameObject ui_tile;
    GameObject ui_game;

    //----------------- 수첩  ---------------------
    private GameObject panel_Note;

    // ----------------- 대화 ---------------------
    private GameObject panel_Dialogue;
    public GameObject Go_dialoue { get { return panel_Dialogue; } }

    //------------------- 퀘스트 ------------------------
    private QuestSystem _quest;
    public QuestSystem Quest { get { return _quest; } }
    private GameObject ui_Quest;

    //-----------------일시정지--------------------------
    private GameObject panel_esc;

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
        DontDestroyOnLoad(gameObject);

        _quest = transform.GetChild(0).GetComponent<QuestSystem>();

        // -------------- 게임 오브젝트 초기화 ----------------
        ui_game = transform.GetChild(0).gameObject;
        ui_game.SetActive(false);
        ui_tile = transform.GetChild(1).gameObject;

        // 인게임
        panel_Note = transform.GetChild(0).GetChild(4).gameObject;
        panel_Dialogue = transform.GetChild(0).GetChild(5).gameObject;
        ui_Quest = transform.GetChild(0).GetChild(3).gameObject;

        panel_esc = transform.GetChild(0).GetChild(8).gameObject;
    }

    void Update()
    {
        if(Input.GetKeyDown(KeyCode.Escape))
        {
            if (panel_esc.activeSelf)
            {
                panel_esc.SetActive(false);
            }
            else
                panel_esc.SetActive(true);
        }
        // I키 
        if (Input.GetKeyDown(KeyCode.I))
        {
            // 노트가 열려 있을 때
            if (panel_Note.activeSelf)
            {
                //노트 닫기
                panel_Note.SetActive(false);
            }
            // 노트가 닫혀있을 때 노트 열기 
            else panel_Note.SetActive(true);
        }
     
    }
    //============================================
    //         씬 변경시 유아이 설정
    //============================================
    void OnEnable()
    {
        SceneManager.sceneLoaded += OnLevelFinishedLoading;
    }
    void OnDisable()
    {
        SceneManager.sceneLoaded -= OnLevelFinishedLoading;
    }

    private void OnLevelFinishedLoading(Scene scene, LoadSceneMode mode)
    {
       SceneType cur = GameManager.Instance.CurScene;

        if (cur == SceneType.Title)
        {
            ui_tile.SetActive(true);
            ui_game.SetActive(false);
        }
      //  else if (GameManager.Instance.CurScene == SceneType.DoctorRoom_event || GameManager.Instance.CurScene == SceneType.DoctorRoom_out)
      //  {
      //      ui_tile.SetActive(false);
      //      ui_game.SetActive(false);
      //  }
        else
        {
            
            ui_tile.SetActive(false);

            if (cur == SceneType.Library || cur == SceneType.DoctorRoom || cur == SceneType.Game_Gear)
            {
                ui_game.SetActive(true);

                if (cur == SceneType.DoctorRoom)
                {
                    GameManager.Instance.DrRoomCtrl.Init();
                    GameManager.Instance.DrRoomCtrl.CheckDR();
                }
                _quest.InitQuest();
                _quest.CheckedQuset();
            }
            else
            {
                ui_game.SetActive(false);
            }
        }
    }

}
