//------------------------------------------------------------------//
// 작성자 : 조성혜
// 대화, 심문, 확대 조사 의 선택지를 관리하는 코드
// 최근 수정 사항: 대화 선택지가 없는 경우 일반 스크립트가 나오도록
// 최종 수정일자 : 18.06.06
//------------------------------------------------------------------//
using System.Collections;
using System.Collections.Generic;
using System.Xml;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

struct Select
{
    public string id;
    public string scriptText;
    public int nextId;
}

public class SelectSystem : MonoBehaviour {

    CameraInteraction _cameraInteraction;
    public CameraInteraction CameraInteraction { get { return _cameraInteraction; } }

    List<Select> selectList = new List<Select>();

    GameObject[] btn = new GameObject[5];
    Text[] txt = new Text[5];

    string curScene;
    string gID;

    XmlDocument doc;

    //싱글톤
    private static SelectSystem _instance = null;
    public static SelectSystem Instance { get { return _instance; } }

    private void Awake()
    {
        _cameraInteraction = GameObject.FindGameObjectWithTag("MainCamera").GetComponent<CameraInteraction>();
        // 씬에 이미 게임 매니저가 있을 경우
        if (_instance)
        {
            //삭제
            Destroy(gameObject);
            return;
        }

        // 버튼 초기화 
        for (int i = 0; i < 5; i++)
        {
            btn[i] = transform.GetChild(i).gameObject;
            txt[i] = btn[i].transform.GetChild(0).GetComponent<Text>();
        }

        // 유일한 게임 매니저 
        _instance = this;
    }
    //============================================
    //         씬 변경시 유아이 설정
    //============================================
    private void OnLevelFinishedLoading(Scene scene, LoadSceneMode mode)
    {
        _cameraInteraction = GameObject.Find("CameraSpring").GetComponent<CameraInteraction>();
    }

    void OnEnable()
    {
        SceneManager.sceneLoaded += OnLevelFinishedLoading;
    }

    void OnDisable()
    {
        SceneManager.sceneLoaded -= OnLevelFinishedLoading;
    }
    //==============================================

    // 선택지 셋팅
    public void InitSelect(string groupID)
    {
        selectList.Clear();

        gID = groupID;
        curScene = GameManager.Instance.CurScene.ToString();
        if (curScene == "DoctorRoom_middle")
            curScene = "DoctorRoom";

        doc = new XmlDocument();
        doc.Load(Application.dataPath + "/Play_select.xml");

        XmlNodeList groupList = doc.SelectSingleNode("Select/" + curScene).ChildNodes;
        XmlNodeList nodeList = null;
        foreach (XmlNode group in groupList)
        {
            if (group.Attributes["GroupID"].Value == groupID)
            {
                nodeList = group.ChildNodes;
                break;
            }
        }

        foreach (XmlNode node in nodeList)
        {
            string state = node.Attributes["DefaultState"].Value;
            if (state == "on")
            {
                Select select = new Select();

                select.id = node.Attributes["ID"].Value;
                select.scriptText = node.Attributes.GetNamedItem("SelectKrText").Value;

                string str_next = node.Attributes.GetNamedItem("ScriptNextID").Value;
                select.nextId = int.Parse(str_next);

                selectList.Add(select);
            }
        }
        SetSelect();
    }


    // 갯수에 따른 위치 설정
    void SetSelect()
    {
        for (int i = 0; i < 5; i++)
        {
            if (i < selectList.Count)
            {
                btn[i].SetActive(true);
                txt[i].text = selectList[i].scriptText;
            }
            else btn[i].SetActive(false);
        }
    }

    public void ShowBtn()
    {
        for (int i = 0; i < selectList.Count; i++)
        {
            btn[i].SetActive(true);
        }
    }

    // 클릭 시 인덱스를 받아와 작동
    public void SelectClick(int index)
    {
        for (int i = 0; i < selectList.Count; i++)
        {
            btn[i].SetActive(false);
        }

        // 데이터 테이블 off

        int id = selectList[index].nextId;
        selectList.RemoveAt(index);

        UIManager.Instance.Go_dialoue.SetActive(true);
        DialoguePanel.Instance.StartDialogue(DialogueType.SELECT, id);
    }

    public void SetSelectState(int checkID, bool state, string _scene = null)
    {
        doc = new XmlDocument();
        doc.Load(Application.dataPath + "/Play_select.xml");

        string str_state;
        if (state) str_state = "on";
        else str_state = "off";

        if (_scene != null)
        {
            XmlNode node = doc.SelectSingleNode("Select" + _scene + "/SelGroup[@GroupId='" + gID + "']/SelInfo[@ID='" + checkID + "']");
            if (node != null)
                node.Attributes["DefaultState"].Value = str_state;
            else Debug.Log(name + "(177): 코드확인필요");

            /*
            XmlNodeList groupList = doc.SelectSingleNode("Select/" + _scene).ChildNodes;
            XmlNodeList nodeList = null;
            foreach (XmlNode group in groupList)
            {
                if (group.Attributes["GroupID"].Value == gID)
                {
                    nodeList = group.ChildNodes;
                    break;
                }
            }

            foreach (XmlNode node in nodeList)
            {
                string str_id = node.Attributes["ID"].Value;
                int id = int.Parse(str_id);
                if (id == checkID)
                {
                    node.Attributes["DefaultState"].Value = str_state;
                    break;
                }
            }
            */
        }

        else
        {
            bool isFind = false;
            XmlNodeList sceneList = doc.SelectSingleNode("Select").ChildNodes;

            foreach (XmlNode scene in sceneList)
            {
                XmlNodeList groupList = scene.ChildNodes;
                foreach(XmlNode group in groupList)
                {
                    XmlNode node = group.SelectSingleNode("SelInfo[@ID='" + checkID + "']");
                    if(node != null)
                    {
                        node.Attributes["DefaultState"].Value = str_state;
                        isFind = true;
                        break;
                    }
                    /*
                    XmlNodeList nodeList = group.ChildNodes;
                    foreach(XmlNode node in nodeList)
                    {
                        string str_id = node.Attributes["ID"].Value;
                        int id = int.Parse(str_id);
                        if(id == checkID)
                        {
                            node.Attributes["DefaultState"].Value = str_state;
                            isFind = true;
                            break;
                        }
                    }
                   */
                    if (isFind) break;
                }
                if (isFind) break;
            }

        }
        doc.Save(Application.dataPath + "/Play_select.xml");
    }
}
