//------------------------------------------------------------------//
// 작성자 : 조성혜
// 퀘스트를 관리 하는 코드
// 최근 수정 사항: 저장데이터를 불러올때 제대로 퀘스트 체크가 되지 않던걸 해결
// 최종 수정일자 : 18.06.06
//------------------------------------------------------------------//
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Xml;
using UnityEngine.UI;

struct QuestSub
{
    public int id;
    public string QuestScript;
    public List<int> provisoID;

    public int checkCount;//퀘스트 체크갯수
    public int uiIndex; // ui 순서

}

struct QuestTitle
{
    public int id;
    public bool DefaultSate;
    public string questUIText;
    public int MonologueNextID;
    public List<int> GetOffStateID;
    public List<int> GetOnStateID;
    public string NextSceneName;

    public int uiIndex;

    public List<QuestSub> quest;

}

public class QuestSystem : MonoBehaviour {

    XmlDocument QLDoc;
    XmlDocument QTDoc;

    TextAsset QTText;

    List<QuestTitle> questList;
    List<GameObject> uiList;

    RectTransform parent;
    public GameObject prefab_title;
    public GameObject prefab_script;

    bool _isEndQuest = false;
    public bool IsEndQuest {
        get { return _isEndQuest; }
        set { _isEndQuest = false;  }
    }

    string curScene;

	void Start () {
        questList = new List<QuestTitle>();
        uiList = new List<GameObject>();

        QTText = (TextAsset)Resources.Load("Xml/QT");

        parent = transform.GetChild(4).GetComponent<RectTransform>();
    }

    public void AddQuest(int Questid)
    {
       int index = -1;
        for (int i=0; i< questList.Count; i++)
        {
            if (questList[i].id == Questid)
            {               
                QuestTitle node = questList[i];
                node.DefaultSate = true;
                node.uiIndex = uiList.Count;
                for(int j =0; j<node.quest.Count; j++)
                {
                    QuestSub qt = node.quest[j];
                    qt.uiIndex = j;
                    node.quest[j] = qt;
                }
                questList[i] = node;
                index = i;
                break;
            }
        }

        if(index != -1) CreateUI(index);
    }

    public void InitQuest()
    {
        questList.Clear();
        foreach (GameObject ui in uiList)
        {
            ui.transform.SetParent(null);
            Destroy(ui);
        }
        uiList.Clear();

        QLDoc = new XmlDocument();
        QLDoc.Load(Application.dataPath + "/Play_QL.xml");

        curScene = GameManager.Instance.CurScene.ToString();
        XmlNodeList sceneList = QLDoc.SelectSingleNode("QL/" + curScene).ChildNodes;

        //현재 씬에 on 되어있는 퀘스트타이틀 들을 리스트에 추가.
        foreach (XmlNode QLNode in sceneList)
        {
            string state = QLNode.Attributes["DefaultState"].Value;
            //완료된 퀘스트가 아닌경우 
            if (state != "complete")
            {
                QuestTitle title = new QuestTitle();

                string str_id = QLNode.Attributes.GetNamedItem("id").Value;
                int id = int.Parse(str_id);
                title.id = id;

                title.DefaultSate = false;
                if ("on" == state)
                {
                    title.DefaultSate = true;
                    title.uiIndex = uiList.Count;
                }

                string uiText = QLNode.SelectSingleNode("QuestUIText").InnerText;
                title.questUIText = uiText;

                XmlNode option = QLNode.SelectSingleNode("Option");

                int monologueID = 0;
                if (option.Attributes["MonologueNextID"] != null)
                {
                    string str_mono = option.Attributes["MonologueNextID"].Value;
                    monologueID = int.Parse(str_mono);
                }
                title.MonologueNextID = monologueID;

                title.GetOffStateID = new List<int>();
                if (option.Attributes["GetOffStateID"] != null)
                {
                    string str_off = option.Attributes["GetOffStateID"].Value;
                    string[] IDs = str_off.Split('/');

                    foreach (string offID in IDs)
                        title.GetOffStateID.Add(int.Parse(offID));
                }

                title.GetOnStateID = new List<int>();
                if (option.Attributes["GetOnStateID"] != null)
                {
                    string str_on = option.Attributes["GetOnStateID"].Value;
                    string[] IDs = str_on.Split('/');

                    foreach (string onID in IDs)
                        title.GetOnStateID.Add(int.Parse(onID));
                }

                string nextScene = null;
                if (option.Attributes["NextSceneName"] != null)
                {
                    nextScene = option.Attributes["NextSceneName"].Value;
                }
                title.NextSceneName = nextScene;

                // 퀘스트 타이틀 그룹의 퀘스트테이블 리스트 제작
                List<QuestSub> subList = new List<QuestSub>();

                QTDoc = new XmlDocument();
                QTDoc.LoadXml(QTText.text);
                XmlNodeList nodelist = QTDoc.SelectSingleNode("QT/" + curScene +"/Title[@GroupID='"+id+"']").ChildNodes;

                int uiIndex = 0;
                foreach (XmlNode node in nodelist)
                {
                    QuestSub sub = new QuestSub();

                    string str_qID = node.Attributes["id"].Value;
                    sub.id = int.Parse(str_qID);

                    string script = node.Attributes["QuestScript"].Value;
                    sub.QuestScript = script;

                    sub.provisoID = new List<int>();

                    if(node.Attributes["GetProvisoID"]!=null)
                    {
                        string str_proviso = node.Attributes["GetProvisoID"].Value;
                        string[] IDs = str_proviso.Split('/');

                        foreach (string inID in IDs)
                            sub.provisoID.Add(int.Parse(inID));
                    }

                    sub.checkCount = sub.provisoID.Count;

                    if (title.DefaultSate)
                    {
                        sub.uiIndex = uiIndex;
                        uiIndex++;
                    }

                    subList.Add(sub);
                }
                title.quest = subList;
                questList.Add(title);

                CreateUI(questList.Count - 1);
            }
        }
    }

    // 퀘스트 체크
    public void CheckQuest(int checkID)
    {
        bool breakRoof = false;
        //_isEndQuest = false;
        //qusetList 돌리기
        for (int i = 0; i < questList.Count; i++)
        {
            // qusetTable list 돌리기..
            for (int j = 0; j < questList[i].quest.Count; j++)
            {
                QuestTable table = questList[i].quest[j];
                
                //id 체크
                if (table.provisoID.Contains(checkID))
                {
                    // 찾은 경우 qusetTable 목록에서 지우기
                    questList[i].quest[j].provisoID.Remove(checkID);
                    breakRoof = true;

                    int count = questList[i].quest[j].num - questList[i].quest[j].provisoID.Count;

                    int titleIndex = questList[i].uiIndex;
                    int index = questList[i].quest[j].uiIndex;
                   
                    // 퀘스트 숫자 갱신
                    Text numTxt = uiList[titleIndex].transform.GetChild(index).GetChild(0).GetComponent<Text>();
                    numTxt.text = count.ToString();

                    Text uiTxt = uiList[titleIndex].transform.GetChild(index).GetComponent<Text>();
                    if (questList[i].id >= 9500 && uiTxt.text == "???")
                        uiTxt.text = table.QuestScript;

                    if (table.provisoID.Count <= 0)
                    {
                       questList[i].quest.Remove(table);
                            // 퀘스트 유아이 글씨 회색처리 하기
                            numTxt.color = Color.gray;
                        uiList[titleIndex].transform.GetChild(index).GetChild(1).GetComponent<Text>().color = Color.gray;
                        uiList[titleIndex].transform.GetChild(index).GetComponent<Text>().color = Color.gray;
                        
                    }

                    break;
                }
            }
            if (questList[i].quest.Count <= 0)
            {
                int offStateID = questList[i].GetOffStateID;
                // 퀘스트로 완료로 인해 Off 시킬것  
                if (offStateID > 0)
                {
                    // 오브젝트
                    if (offStateID >= 1000 && offStateID < 2001)
                    {
                 //       UIManager.Instance.Dialouge.OffObj(offStateID);
                    }
                    // 선택지
                    if (offStateID >= 5000 && offStateID < 5001)
                    {
              //          UIManager.Instance.Select.OffSelect(offStateID);
                    }
                    // 수첩
                    if (offStateID >= 7000 && offStateID < 8001)
                    {
                        //   GameManager.Instance.Note.AddEvidence(offStateID, curScene);
                    }

                }
                int onStateID = questList[i].GetOnStateID;

                // xml 설정
                QLDoc = new XmlDocument();
                QLDoc.Load(Application.dataPath + "/Play_QL.xml");

                XmlNodeList sceneList = QLDoc.SelectSingleNode("QL/" + curScene).ChildNodes;
                foreach (XmlNode QLNode in sceneList)
                {
                    string str_id = QLNode.Attributes.GetNamedItem("id").InnerText;
                    int id = int.Parse(str_id);
                    if (questList[i].id == id)
                    {
                        QLNode.SelectSingleNode("DefalutState").InnerText = "complete";
                        QLDoc.Save(Application.dataPath + "/Play_QL.xml");
                        break;
                    }
                }
                _isEndQuest = true;

                int titleIndex = questList[i].uiIndex;
                questList.Remove(questList[i]);

                
                //// 다른 유아이 위치 재설정
                //if (titleIndex < uiList.Count - 1)
                //{
                //    Debug.Log(this + ":: titleIndex = " + titleIndex);
                //    //현재 지워질 유아이 보다 위에 있는 경우 
                //    for (int j = titleIndex + 1; j < uiList.Count; j++)
                //    {
                //        Debug.Log(this + ":: j = " + j);
                //        float height = uiList[j - 1].GetComponent<RectTransform>().anchoredPosition.y;
                //        //  height += 25;
                //        height += 90 + (40 * uiList[j].transform.childCount);
                //    }
                //}  

                //유아이 삭제
                GameObject ui = uiList[titleIndex];
                uiList.RemoveAt(titleIndex);
                Destroy(ui);

                // 퀘스트로 완료로 인해 On 시킬것  
                if (onStateID > 0)
                {
                    // 오브젝트
                    if (onStateID >= 1000 && onStateID < 2001)
                    {
                   //     UIManager.Instance.Dialouge.OnObj(onStateID);
                        CheckQuest(onStateID);
                    }
                    // 선택지
                    if (onStateID >= 5000 && onStateID < 6001)
                    {
                    //    UIManager.Instance.Select.OnSelect(onStateID);
                    }
                    // 수첩
                    if (onStateID >= 7000 && onStateID < 8001)
                    {
     //                   GameManager.Instance.Note.AddEvidence(onStateID);
                        CheckQuest(onStateID);

                    }
                }
            }

            if (breakRoof)
            {
                breakRoof = false;
                break;
            }
        }

        //qusetList count가 0인경우 리스트 초기화, 다음씬
        if (questList.Count <= 0)
        {
            questList.Clear();
            if (GameManager.Instance.CurScene == SceneType.Library)
            {            // 다음씬
                GameManager.Instance.SetNextScene(SceneType.DoctorRoom);
            }
            else if(GameManager.Instance.CurScene == SceneType.DoctorRoom)
            {
                if (GameManager.Instance.DrRoomCtrl.bCheckDr)
                    GameManager.Instance.SetNextScene(SceneType.DoctorRoom_out);
            }
        }
    }
    
    // 로드시 체크됐던 퀘스트
    public void CheckedQuset()
    {
        //qusetList 돌리기
        for (int i = 0; i < questList.Count; i++)
        {
            if (questList[i].DefaultSate)
            {
                // qusetTable list 돌리기..
                for (int j = 0; j < questList[i].quest.Count; j++)
                {
                    QuestTable table = questList[i].quest[j];

                    // 퀘스트 체크 id 목록 돌리기 
                    for (int index = 0; index < table.provisoID.Count; index++)
                    {
                        int id = table.provisoID[index];
                        bool b_check;

                        if (id >= 7000 && id < 8001) { b_check = false; }
          //                  b_check = GameManager.Instance.Note.CheckNote(id);
                        else
                            b_check = false;
                           // b_check = UIManager.Instance.Dialouge.CheckDialogue(id);

                        if (b_check)
                        {
                            // 찾은 경우 qusetTable 목록에서 지우기
                            questList[i].quest[j].provisoID.Remove(id);

                            int count = questList[i].quest[j].num - table.provisoID.Count;

                            int titleIndex = questList[i].uiIndex;
                            int uiIndex = questList[i].quest[j].uiIndex;

                            // 퀘스트 숫자 갱신
                            Text uitxt = uiList[titleIndex].transform.GetChild(uiIndex).GetChild(0).GetComponent<Text>();
                            uitxt.text = count.ToString();

                            if (table.provisoID.Count <= 0)
                            {
                                questList[i].quest.Remove(table);
                                // 퀘스트 유아이 글씨 회색처리 하기
                                uitxt.color = Color.gray;
                                uiList[titleIndex].transform.GetChild(uiIndex).GetChild(1).GetComponent<Text>().color = Color.gray;
                                uiList[titleIndex].transform.GetChild(uiIndex).GetComponent<Text>().color = Color.gray;

                            }

                        }
                    }
                }
            }

        }
    }

    void CreateUI(int index)
    {
        //현재 on 상태인 퀘스트라면
        if (questList[index].DefaultSate && questList.Count > uiList.Count)
        {
            // 게임 오브젝트 생성
            GameObject newQuest = Instantiate(prefab_title);

            // text 지정 및 부모 지정
            newQuest.GetComponent<Text>().text = questList[index].questUIText;
            newQuest.transform.SetParent(parent);

            int questCount = questList[index].quest.Count;
            int uiIndex = parent.childCount;

            float height = 0.0f;
            if (uiIndex > 1)
            {
               height = parent.GetChild(uiIndex - 2).GetComponent<RectTransform>().anchoredPosition.y;
                height += 15;
            }
            height += 30 + (35 * questCount);

            RectTransform rect = newQuest.GetComponent<RectTransform>();
            rect.localScale = Vector3.one;
            rect.anchoredPosition = new Vector3(-400, height, 0.0f);

            if (questList[index].id >= 9500)
                newQuest.transform.GetChild(0).GetComponent<Text>().text = "???";
            else
                newQuest.transform.GetChild(0).GetComponent<Text>().text = questList[index].quest[0].QuestScript;

            newQuest.transform.GetChild(0).GetChild(1).GetComponent<Text>().text = "/" + questList[index].quest[0].provisoID.Count.ToString();

            if (questCount >= 2)
            {
                for (int i = 1; i < questCount; i++)
                {
                    GameObject newScript = Instantiate(prefab_script);

                    newScript.transform.SetParent(newQuest.transform);

                    float y = -35 * (i + 1);
                    RectTransform scriptRect = newScript.GetComponent<RectTransform>();
                    scriptRect.anchoredPosition = new Vector3(20, y, 0);

                    if (questList[index].id >= 9500)
                        newScript.GetComponent<Text>().text = "???";
                    else
                       newScript.GetComponent<Text>().text = questList[index].quest[i].QuestScript;
                    newScript.transform.GetChild(1).GetComponent<Text>().text = "/" + questList[index].quest[i].provisoID.Count.ToString();
                }
            }
            //  if(uiList.Count <= index) 
            uiList.Add(newQuest);
           // else
            //    uiList.Insert(index, newQuest);
           
        }
    }

}

