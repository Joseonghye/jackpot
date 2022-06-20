//------------------------------------------------------------------//
// 작성자 : 조성혜
// 대화 내용, 일러스트 등 조정 
// 최종 수정일자 : 18.09.02dlf
//------------------------------------------------------------------//
using System.Collections;
using System.Collections.Generic;
using System.Xml;
using UnityEngine;
using UnityEngine.UI;

public enum DialogueType { OBJ, NPC, SELECT,HEARING,EXP,DET}
public class DialoguePanel : UIPanel
{

    private static DialoguePanel _instance = null;
    public static DialoguePanel Instance { get { return _instance; } }

    DialogueType type;
    string curScene;

    // ------------- xml -------------------
    XmlDocument clueDoc;
    XmlNode curNode;

    //--------------- 대화 --------------------
    string content;          // 대화 내용 

    private bool isTurn = false;    //다음 대화
    int nextNum = -1;               //다음 대화 id
    private int curPos = 0;         // 글자 순서 

    //텍스트
    private Text txt_name;          // 캐릭터 이름 
    private Text txt_chat;          // 대화 내용 

    // 이미지
    private RectTransform bg_rect;
    private Image bg_img;

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

        bg_rect = transform.GetChild(0).GetComponent<RectTransform>();
        bg_img = transform.GetChild(0).GetComponent<Image>();
        txt_name = transform.GetChild(2).GetComponent<Text>();
        txt_chat = transform.GetChild(3).GetComponent<Text>();
    }

    private void OnEnable()
    {
        clueDoc = new XmlDocument();
        clueDoc.Load(Application.dataPath + "/Play_ob.xml");

        if (bg_rect.gameObject.activeSelf) bg_rect.gameObject.SetActive(false);
    }

    // 오브젝트 조사, 캐릭터 대화 
    public void StartDialogue(DialogueType _type, string name)
    {
        type = _type;

        curScene = GameManager.Instance.CurScene.ToString();
        if (curScene == "DoctorRoom_middle")
            curScene = "DoctorRoom";

        //      clueDoc = new XmlDocument();
        //      clueDoc.Load(Application.dataPath + "/Play_ob.xml");

        XmlNodeList nameList = null;
        if (type == DialogueType.OBJ)
            nameList = clueDoc.SelectNodes("Observe/" + curScene + "/OBJ/Info[@ObjNameEng='" + name+"']");
        else if (type == DialogueType.NPC)
            nameList = clueDoc.SelectNodes("Observe/" + curScene + "/NPC/Info[@ObjNameEng='" + name + "']");
        else if(type == DialogueType.DET)
            nameList = clueDoc.SelectNodes("Observe/" + curScene + "/DET/Info[@ObjNameEng='" + name + "']");
        else if(type == DialogueType.EXP)
            nameList = clueDoc.SelectNodes("Observe/" + curScene + "/EXP/Info[@ObjNameEng='" + name + "']");

        // 켜져있는 항목 번호
        List<int> onList = new List<int>();

        // Check 'Default State'
        for (int i = 0; i < nameList.Count; i++)
        {
            string state = nameList[i].SelectSingleNode("State").Attributes["DefaultState"].Value;
            if (state != null)
            {
                if (state == "on")
                {
                    onList.Add(i);
                }
            }
        }

        int index = onList[0];
        int priority = -1;
        for (int i = 0; i < onList.Count; i++)
        {
            string str_pri = nameList[onList[i]].SelectSingleNode("State").Attributes["ObjectPriority"].Value;
            int p = int.Parse(str_pri);

            if (p > priority)
            {
                index = i;
                priority = p;
            }
        }

        curNode = nameList[index];

        string chName = curNode.SelectSingleNode("ScriptChName").InnerText;

        //확대조사 , 세부조사
        if (chName == "n")
        {
            XmlNode op = curNode.SelectSingleNode("Option");
            if (op.Attributes["NextEventName"] != null)
            {
                GameManager.Instance.Event.ActiveEvent(op.Attributes["NextEventName"].Value);
                gameObject.SetActive(false);
            }
        }
        else
        {
            txt_name.text = curNode.SelectSingleNode("ScriptChName").InnerText;
            content = curNode.SelectSingleNode("ScriptChText").InnerText;

            string str_next = curNode.SelectSingleNode("ScriptNextID").InnerText;

            // 다음 스크립트가 있을때
            if (str_next != "n")
            {
                nextNum = int.Parse(str_next);
            }
            else nextNum = -1;

            StartCoroutine(ShowingDialogue());
            CheckOption();

        }
    }

    //선택지 
    public void StartDialogue(DialogueType _type, int id)
    {
        type = _type;
        curScene = GameManager.Instance.CurScene.ToString();
        if (curScene == "DoctorRoom_middle")
            curScene = "DoctorRoom";

      //  clueDoc = new XmlDocument();

      //  clueDoc.Load(Application.dataPath + "/Play_obj.xml");

        XmlNodeList nodeList = clueDoc.SelectSingleNode("Observe/" + curScene + "/SEL").ChildNodes;

        int index = -1;
        for (int i = 0; i < nodeList.Count; i++)
        {
            string str_num = nodeList[i].Attributes["ID"].Value;
            int num = int.Parse(str_num);

            if (num == id)
            {
                index = i;
                txt_name.text = nodeList[i].SelectSingleNode("ScriptChName").InnerText;
                content = nodeList[i].SelectSingleNode("ScriptChText").InnerText;
                break;
            }
        }

        curNode = nodeList[index];

        if (index > -1)
        {
            string str_nextId = curNode.SelectSingleNode("ScriptNextID").InnerText;

            //next id 를 판단해 다음 대사 출력
            if (str_nextId != "n")
            {
                nextNum = int.Parse(str_nextId);
            }
            else nextNum = -1;
        }

        StartCoroutine(ShowingDialogue());
        CheckOption();
    }

    void CheckOption()
    {
        XmlNode option = curNode.SelectSingleNode("Option");
        if (option != null)
        {
            // Xml Off
            if (option.Attributes["GetOffStateID"] != null)
            {
                string offStateID = option.Attributes["GetOffStateID"].Value;

                string[] IDs = offStateID.Split('/');

                for (int i = 0; i < IDs.Length; i++)
                {
                    //단서 퀘스트 선택지
                    int _id = int.Parse(IDs[i]);

                    //탐색
                    if(_id >= 1000 && _id < 2000)
                    {
                        SetObserveState(_id, false);
                    }

                    //퀘스트
                    if (_id >= 9000 && _id < 10000)
                    {
                       
                    }
                }
            }

            // Xml On
            if (option.Attributes["GetOnStateID"] != null)
            {
                string onStateID = option.Attributes["GetOnStateID"].Value;

                string[] IDs = onStateID.Split('/');

                for (int i = 0; i < IDs.Length; i++)
                {
                    //단서 퀘스트 선택지
                    int _id = int.Parse(IDs[i]);

                    //탐색
                    if (_id >= 1000 && _id < 2000)
                    {
                        SetObserveState(_id, true);
                    }
                }
            }

            //일러스트
            if (option.Attributes["BGNameTrans"] != null)
            {
                string bgName = option.Attributes["BGNameTrans"].Value;

                // n일경우 종료
                if (bgName == "n")
                    bg_rect.gameObject.SetActive(false);
                //아닌 경우 새로 띄우거나 그림 변경
                else
                {
                    if (!bg_rect.gameObject.activeSelf) bg_rect.gameObject.SetActive(true);

                    bg_img.sprite = Resources.Load<Sprite>("Img/Dial/" + bgName);
                    // 이미지 설정
                 //   bg_rect.sizeDelta = new Vector2(sprite.texture.width, sprite.texture.height);
                    bg_img.SetNativeSize();
                }
                
            }
        }
    }

    void NextScript(int nextID)
    {
        XmlNodeList nodeList = clueDoc.SelectSingleNode("Observe/" + curScene + "/ETC").ChildNodes;

        int index = -1;
        for (int i = 0; i < nodeList.Count; i++)
        {
            string str_num = nodeList[i].Attributes["ID"].Value;
            int num = int.Parse(str_num);

            if (num == nextID)
            {
                index = i;
                txt_name.text = nodeList[i].SelectSingleNode("ScriptChName").InnerText;
                content = nodeList[i].SelectSingleNode("ScriptChText").InnerText;
                break;
            }
        }

        curNode = nodeList[index];

        if (index > -1)
        {
            string str_nextId = curNode.SelectSingleNode("ScriptNextID").InnerText;

            //next id 를 판단해 다음 대사 출력
            if (str_nextId != "n")
            {
                nextNum = int.Parse(str_nextId);
            }
            else nextNum = -1;
        }

        StartCoroutine(ShowingDialogue());
        CheckOption();

    }

    public void ClickDialogue()
    {
        if(isTurn)
        {
            if (nextNum != -1)
            {
                NextScript(nextNum);
            }
            else
            {
                XmlNode option = curNode.SelectSingleNode("Option");
    
                //이벤트 
                if (option.Attributes["NextEventName"] != null)
                {
                    GameManager.Instance.Event.ActiveEvent(option.Attributes["NextEventName"].Value);
                }
                //선택지 
                if (option.Attributes["HearingGroupID"] != null)
                {
                    SelectSystem.Instance.InitSelect(option.Attributes["HearingGroupID"].Value);
                }

                gameObject.SetActive(false);
            }
        }
        else
        {
            isTurn = true;
        }

    }

    IEnumerator ShowingDialogue()
    {
        int num = content.Length;

        while(curPos < num && !isTurn)
        {
            txt_chat.text = content.Substring(0, curPos);
            ++curPos;

            yield return new WaitForSeconds(0.05f);
        }

        curPos = 0;
        txt_chat.text = content;
        isTurn = true;
        yield break;
    }

    public void SetObserveState(int checkId, bool _state)
    {
        string str_state;
        if (_state) str_state = "on";
        else str_state = "off";

        bool isFind = false;

        //  [@ObjNameEng='" + name+"']
        XmlNodeList sceneList = clueDoc.SelectSingleNode("Observe").ChildNodes;
        foreach (XmlNode scene in sceneList)
        {
            XmlNodeList typeList = scene.ChildNodes;
            foreach (XmlNode type in typeList)
            {
                XmlNode node = type.SelectSingleNode("Info[@ID='" + checkId + "']");
                if(node != null)
                {
                    node.SelectSingleNode("State").Attributes["DefaultState"].Value = str_state;
                    isFind = true;
                    break;
                }
            }
            if (isFind) break;
        }
        clueDoc.Save(Application.dataPath + "/Play_ob.xml");
    }
}
