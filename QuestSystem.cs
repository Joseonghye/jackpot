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
    public List<string> provisoID;

    public int checkCount;//퀘스트 체크갯수
    public int uiIndex; // ui 순서
}

struct QuestTitle
{
    public int id;
    public bool DefaultSate;
    public string questUIText;
    public int MonologueNextID;
    public List<string> GetOffStateID;
    public List<string> GetOnStateID;
    public string NextSceneName;

    public int uiIndex;

    public List<QuestSub> quest;
}

public class QuestSystem : MonoBehaviour {

    XmlDocument QLDoc;
    XmlDocument QTDoc;

    TextAsset QTText;

    List<QuestTitle> defaultList;
    List<GameObject> default_uiList;

    List<QuestTitle> observeList;
    List<GameObject> observe_uiList;

    RectTransform parent_default;
    RectTransform parent_observe;

    public GameObject prefab_title;
    public GameObject prefab_script;

    bool _isEndQuest = false;
    public bool IsEndQuest {
        get { return _isEndQuest; }
        set { _isEndQuest = false; }
    }

    string curScene;

    [SerializeField]
    AudioClip clip_end;
    AudioSource _audio;

    void Awake() {
        defaultList = new List<QuestTitle>();
        default_uiList = new List<GameObject>();

        observeList = new List<QuestTitle>();
        observe_uiList = new List<GameObject>();

        QTText = (TextAsset)Resources.Load("Xml/QT");

        _audio = GameManager.Instance.Audio;

        parent_default = transform.GetChild(1).GetChild(0). GetComponent<RectTransform>();
        parent_observe = transform.GetChild(1).GetChild(1).GetComponent<RectTransform>();
    }

    public void NewGame()
    {
        if (defaultList != null)
        {
            defaultList.Clear();
            foreach (GameObject ui in default_uiList)
            {
                ui.transform.SetParent(null);
                Destroy(ui);
            }
            default_uiList.Clear();

            observeList.Clear();
            foreach (GameObject ui in observe_uiList)
            {
                ui.transform.SetParent(null);
                Destroy(ui);
            }
            observe_uiList.Clear();
        }
    }

    public void SetObserve(bool chk)
    {
        if(chk)
        {
            parent_default.gameObject.SetActive(false);
            parent_observe.gameObject.SetActive(true);
        }
        else
        {
            parent_default.gameObject.SetActive(true);
            parent_observe.gameObject.SetActive(false);
        }
    }

    public void AddDefaultQuest(string Questid)
    {
#if UNITY_EDITOR
        if (Questid == null)
            Debug.Log(name + ": Quest Error");
#endif

        string str = Questid.Replace("QL_", "");
        int qID = int.Parse(str);

        bool _have = false;
        foreach (QuestTitle t in defaultList)
        {
            if (t.id == qID)
            {
                _have = true;
                break;
            }
        }


        if (!_have)
        {
            QLDoc.Load(Application.dataPath + "/Play_QL.xml");

            curScene = GameManager.Instance.CurScene.ToString();
            XmlNode QLNode = QLDoc.SelectSingleNode("QL/" + curScene + "/Quest[@ID='" + Questid + "']");

            Debug.Log(curScene);
            //퀘스트가 현재 씬의 것이 아닌 경우 init에서 가능하도록 on
            if (QLNode == null)
            {
                FixXml(qID, "on");
            }
            else
            {
                string state = QLNode.Attributes["DefaultState"].Value;
                if (state == "off")
                {
                    QuestTitle title = new QuestTitle();

                    title.id = qID;
                    title.DefaultSate = true;
                    title.uiIndex = default_uiList.Count;

                    string uiText = QLNode.SelectSingleNode("QuestUIText").InnerText;
                    title.questUIText = uiText;

                    XmlNode option = QLNode.SelectSingleNode("Option");

                    int monologueID = 0;
                    if (QLNode.SelectSingleNode("MonologueID") != null)
                    {
                        string str_mono = QLNode.SelectSingleNode("MonologueID").InnerText;
                        monologueID = int.Parse(str_mono);
                    }
                    title.MonologueNextID = monologueID;

                    title.GetOffStateID = new List<string>();
                    if (option.Attributes["GetOffStateID"] != null)
                    {
                        string str_off = option.Attributes["GetOffStateID"].Value;
                        string[] IDs = str_off.Split('/');

                        foreach (string offID in IDs)
                            title.GetOffStateID.Add(offID);
                    }

                    title.GetOnStateID = new List<string>();
                    if (option.Attributes["GetOnStateID"] != null)
                    {
                        string str_on = option.Attributes["GetOnStateID"].Value;
                        string[] IDs = str_on.Split('/');

                        foreach (string onID in IDs)
                            title.GetOnStateID.Add(onID);
                    }

                    string nextScene = null;
                    if (QLNode.SelectSingleNode("NextSceneName") != null)
                    {
                        nextScene = QLNode.SelectSingleNode("NextSceneName").InnerText;
                    }
                    title.NextSceneName = nextScene;

                    // 퀘스트 타이틀 그룹의 퀘스트테이블 리스트 제작
                    List<QuestSub> subList = new List<QuestSub>();

                    QTDoc = new XmlDocument();
                    QTDoc.LoadXml(QTText.text);
                    XmlNodeList nodelist = QTDoc.SelectNodes("QT/" + curScene + "/Title[@QuestGroupID='" + Questid + "']");

                    int uiIndex = 1;
                    foreach (XmlNode node in nodelist)
                    {
                        QuestSub sub = new QuestSub();

                        string str_qID = node.SelectSingleNode("Sub").Attributes["ID"].Value;
                        string reID = str_qID.Replace("Q_", "");
                        sub.id = int.Parse(reID);

                        string script = node.SelectSingleNode("Sub").Attributes["QuestScript"].Value;
                        sub.QuestScript = script;

                        sub.provisoID = new List<string>();

                        if (node.SelectSingleNode("Sub").Attributes["GetProvisoID"] != null)
                        {
                            string str_proviso = node.SelectSingleNode("Sub").Attributes["GetProvisoID"].Value;
                            string[] IDs = str_proviso.Split('/');

                            foreach (string inID in IDs)
                                sub.provisoID.Add(inID);
                        }

                        sub.checkCount = sub.provisoID.Count;

                        sub.uiIndex = uiIndex;
                        uiIndex++;

                        subList.Add(sub);
                    }
                    title.quest = subList;
                    defaultList.Add(title);

                    FixXml(qID, "ing");
                    CreateDefaultUI(defaultList.Count - 1);
                }
            }

        }
    }
    public void AddObserveQuest(string Questid)
    {
#if UNITY_EDITOR
        if (Questid == null)
            Debug.Log(name + ": Quest Error");
#endif

        string str = Questid.Replace("QL_", "");
        int qID = int.Parse(str);

        bool _have = false;
        foreach (QuestTitle t in observeList)
        {
            if (t.id == qID)
            {
                _have = true;
                break;
            }
        }


        if (!_have)
        {
            QLDoc.Load(Application.dataPath + "/Play_QL.xml");

            curScene = GameManager.Instance.CurScene.ToString();
            XmlNode QLNode = QLDoc.SelectSingleNode("QL/" + curScene + "/Quest[@ID='" + Questid + "']");

            Debug.Log(curScene);
            //퀘스트가 현재 씬의 것이 아닌 경우 init에서 가능하도록 on
            if (QLNode == null)
            {
                FixXml(qID, "on");
            }
            else
            {
                string state = QLNode.Attributes["DefaultState"].Value;
                if (state == "off")
                {
                    QuestTitle title = new QuestTitle();

                    title.id = qID;
                    title.DefaultSate = true;
                    title.uiIndex = observe_uiList.Count;

                    string uiText = QLNode.SelectSingleNode("QuestUIText").InnerText;
                    title.questUIText = uiText;

                    XmlNode option = QLNode.SelectSingleNode("Option");

                    int monologueID = 0;
                    if (QLNode.SelectSingleNode("MonologueID") != null)
                    {
                        string str_mono = QLNode.SelectSingleNode("MonologueID").InnerText;
                        monologueID = int.Parse(str_mono);
                    }
                    title.MonologueNextID = monologueID;

                    title.GetOffStateID = new List<string>();
                    if (option.Attributes["GetOffStateID"] != null)
                    {
                        string str_off = option.Attributes["GetOffStateID"].Value;
                        string[] IDs = str_off.Split('/');

                        foreach (string offID in IDs)
                            title.GetOffStateID.Add(offID);
                    }

                    title.GetOnStateID = new List<string>();
                    if (option.Attributes["GetOnStateID"] != null)
                    {
                        string str_on = option.Attributes["GetOnStateID"].Value;
                        string[] IDs = str_on.Split('/');

                        foreach (string onID in IDs)
                            title.GetOnStateID.Add(onID);
                    }

                    string nextScene = null;
                    if (QLNode.SelectSingleNode("NextSceneName") != null)
                    {
                        nextScene = QLNode.SelectSingleNode("NextSceneName").InnerText;
                    }
                    title.NextSceneName = nextScene;

                    // 퀘스트 타이틀 그룹의 퀘스트테이블 리스트 제작
                    List<QuestSub> subList = new List<QuestSub>();

                    QTDoc = new XmlDocument();
                    QTDoc.LoadXml(QTText.text);
                    XmlNodeList nodelist = QTDoc.SelectNodes("QT/" + curScene + "/Title[@QuestGroupID='" + Questid + "']");

                    int uiIndex = 1;
                    foreach (XmlNode node in nodelist)
                    {
                        QuestSub sub = new QuestSub();

                        string str_qID = node.SelectSingleNode("Sub").Attributes["ID"].Value;
                        string reID = str_qID.Replace("Q_", "");
                        sub.id = int.Parse(reID);

                        string script = node.SelectSingleNode("Sub").Attributes["QuestScript"].Value;
                        sub.QuestScript = script;

                        sub.provisoID = new List<string>();

                        if (node.SelectSingleNode("Sub").Attributes["GetProvisoID"] != null)
                        {
                            string str_proviso = node.SelectSingleNode("Sub").Attributes["GetProvisoID"].Value;
                            string[] IDs = str_proviso.Split('/');

                            foreach (string inID in IDs)
                                sub.provisoID.Add(inID);
                        }

                        sub.checkCount = sub.provisoID.Count;

                        sub.uiIndex = uiIndex;
                        uiIndex++;

                        subList.Add(sub);
                    }
                    title.quest = subList;
                    observeList.Add(title);

                    FixXml(qID, "ing");
                    CreateObserveUI(observeList.Count - 1);
                }
            }

        }
    }

    public void InitQuest()
    {
        QLDoc = new XmlDocument();
        QLDoc.Load(Application.dataPath + "/Play_QL.xml");

        curScene = GameManager.Instance.CurScene.ToString();
        XmlNodeList sceneList = QLDoc.SelectNodes("QL/" + curScene+ "/Quest[@DefaultState='on']");
        //현재 씬에 on 되어있는 퀘스트타이틀 들을 리스트에 추가.
        foreach (XmlNode QLNode in sceneList)
        {
            //완료된 퀘스트가 아닌경우 

            QuestTitle title = new QuestTitle();

            string str_id = QLNode.Attributes.GetNamedItem("ID").Value;
            string reId = str_id.Replace("QL_", "");    // 아이디의 영문 제거 
            int id = int.Parse(reId);
            title.id = id;

            title.DefaultSate = true;
            title.uiIndex = default_uiList.Count;

            string uiText = QLNode.SelectSingleNode("QuestUIText").InnerText;
            title.questUIText = uiText;

            XmlNode option = QLNode.SelectSingleNode("Option");

            int monologueID = 0;
            if (QLNode.SelectSingleNode("MonologueID") != null)
            {
                string str_mono = QLNode.SelectSingleNode("MonologueID").InnerText;
                monologueID = int.Parse(str_mono);
            }
            title.MonologueNextID = monologueID;

            title.GetOffStateID = new List<string>();
            if (option.Attributes["GetOffStateID"] != null)
            {
                string str_off = option.Attributes["GetOffStateID"].Value;
                string[] IDs = str_off.Split('/');

                foreach (string offID in IDs)
                    title.GetOffStateID.Add(offID);
            }

            title.GetOnStateID = new List<string>();
            if (option.Attributes["GetOnStateID"] != null)
            {
                string str_on = option.Attributes["GetOnStateID"].Value;
                string[] IDs = str_on.Split('/');

                foreach (string onID in IDs)
                    title.GetOnStateID.Add(onID);
            }

            string nextScene = null;
            if (QLNode.SelectSingleNode("NextSceneName") != null)
            {
                nextScene = QLNode.SelectSingleNode("NextSceneName").InnerText;
            }
            title.NextSceneName = nextScene;

            // 퀘스트 타이틀 그룹의 퀘스트테이블 리스트 제작
            List<QuestSub> subList = new List<QuestSub>();

            QTDoc = new XmlDocument();
            QTDoc.LoadXml(QTText.text);
            XmlNodeList nodelist = QTDoc.SelectNodes("QT/" + curScene + "/Title[@QuestGroupID='" + str_id + "']");

            int uiIndex = 1;
            foreach (XmlNode node in nodelist)
            {
                QuestSub sub = new QuestSub();

                string str_qID = node.SelectSingleNode("Sub").Attributes["ID"].Value;
                string reID = str_qID.Replace("Q_", "");
                sub.id = int.Parse(reID);

                string script = node.SelectSingleNode("Sub").Attributes["QuestScript"].Value;
                sub.QuestScript = script;

                sub.provisoID = new List<string>();

                if (node.SelectSingleNode("Sub").Attributes["GetProvisoID"] != null)
                {
                    string str_proviso = node.SelectSingleNode("Sub").Attributes["GetProvisoID"].Value;
                    string[] IDs = str_proviso.Split('/');

                    foreach (string inID in IDs)
                        sub.provisoID.Add(inID);
                }

                sub.checkCount = sub.provisoID.Count;

                sub.uiIndex = uiIndex;
                uiIndex++;

                subList.Add(sub);
            }
            title.quest = subList;
            defaultList.Add(title);

            FixXml(title.id, "ing");
            CreateDefaultUI(defaultList.Count - 1);

        }
    }

    void CheckDefaultTitle()
    {
        for (int i = defaultList.Count - 1; i >= 0; i--)
        {
            //모든 세부 퀘스트가 끝났을 때 = 메인 퀘스트 완료 
            if (defaultList[i].quest.Count == 0 && defaultList[i].DefaultSate)
            {
                if (defaultList[i].GetOnStateID != null)
                {
                    // on
                    foreach (string onID in defaultList[i].GetOnStateID)
                    {
                         if (onID.Contains("QL_"))
                            AddDefaultQuest(onID);
                        else
                        {
                            int _id = int.Parse(onID);

                            // 오브젝트 조사 
                            if (_id >= 10000 && _id < 20000)
                            {
                                DialoguePanel.Instance.SetObserveState(_id, true);
                            }
                            //단서(증거 증언)
                            if ((_id >= 30000 && _id < 40000) || _id >= 50000)
                                UIManager.Instance.Note.Add(_id);

                            // 선택지
                            UIManager.Instance.Select.SetSelectState(_id, true);
                        }
                    }
                }

                // 독백
                if (defaultList[i].MonologueNextID > 0)
                {
                    UIManager.Instance.Dialoue.SetActive(true);
                    DialoguePanel.Instance.Monologue(defaultList[i].MonologueNextID);
                }

                if(defaultList[i].NextSceneName != null)
                {
                    if(defaultList[i].NextSceneName.Contains("G"))
                    {
                        UIManager.Instance.StartGame = true;
                        if (!DialoguePanel.Instance.IsTalk && !GameManager.Instance.bPlayEvent)
                            UIManager.Instance.PlayMiniGame();
                    }
                    if (defaultList[i].NextSceneName.Contains("C"))
                    {
                        UIManager.Instance.StartCriminal = true;
                        if (!DialoguePanel.Instance.IsTalk && !GameManager.Instance.bPlayEvent)
                            UIManager.Instance.PlayCriminal();
                    }
                }

                // xml 상태 변경
                FixXml(defaultList[i].id, "complete");

                int index = defaultList[i].uiIndex;

                // 타 메인 퀘스트 ui  위치 수정 
                if (index + 1 < default_uiList.Count)
                {
                    for (int num = index + 1; num < default_uiList.Count; num++)
                    {
                        GameObject ui = default_uiList[num];
                        RectTransform rect = ui.GetComponent<RectTransform>();
                        rect.anchoredPosition = default_uiList[num - 1].GetComponent<RectTransform>().anchoredPosition;
                    }
                }

                GameObject del = default_uiList[index];
                default_uiList.RemoveAt(index);
                // ui 삭제
                Destroy(del);

                QuestTitle node = defaultList[i];
                node.DefaultSate = false;
                node.uiIndex = -1;
                defaultList[i] = node;

                ChangeDefaultIndex(i,index);
            }
        }

    }
    void ChangeDefaultIndex(int index, int uiIndex)
    {
        for(int i = defaultList.Count -1; i>= index; i--)
        {
                QuestTitle node = defaultList[i];
            if (node.uiIndex > uiIndex)
            {
                node.uiIndex = defaultList[i].uiIndex - 1; ;
                defaultList[i] = node;
            }
        }
    }

    void CheckObserveTitle()
    {
        for (int i = observeList.Count - 1; i >= 0; i--)
        {
            //모든 세부 퀘스트가 끝났을 때 = 메인 퀘스트 완료 
            if (observeList[i].quest.Count == 0 && observeList[i].DefaultSate)
            {

                _isEndQuest = true;
                if (observeList[i].GetOnStateID != null)
                {
                    // on
                    foreach (string onID in observeList[i].GetOnStateID)
                    {
                        if (onID.Contains("QL_"))
                            AddDefaultQuest(onID);
                        else
                        {
                            int _id = int.Parse(onID);

                            // 오브젝트 조사 
                            if (_id >= 10000 && _id < 20000)
                            {
                                DialoguePanel.Instance.SetObserveState(_id, true);
                            }
                            //단서(증거 증언)
                            if ((_id >= 30000 && _id < 40000) || _id >= 50000)
                                UIManager.Instance.Note.Add(_id);

                            // 선택지
                            UIManager.Instance.Select.SetSelectState(_id, true);
                        }
                    }
                }

                // 독백
                if (observeList[i].MonologueNextID > 0)
                {
                    UIManager.Instance.Dialoue.SetActive(true);
                    DialoguePanel.Instance.Monologue(observeList[i].MonologueNextID);
                }

                if (observeList[i].NextSceneName != null)
                {
                    if (observeList[i].NextSceneName.Contains("G"))
                    {
                        UIManager.Instance.StartGame = true;
                        if (!DialoguePanel.Instance.IsTalk && !GameManager.Instance.bPlayEvent)
                            UIManager.Instance.PlayMiniGame();
                    }
                    if (observeList[i].NextSceneName.Contains("C"))
                    {
                        UIManager.Instance.StartCriminal = true;
                        if (!DialoguePanel.Instance.IsTalk && !GameManager.Instance.bPlayEvent)
                            UIManager.Instance.PlayCriminal();
                    }
                }

                // xml 상태 변경
                FixXml(observeList[i].id, "complete");

                int index = observeList[i].uiIndex;

                // 타 메인 퀘스트 ui  위치 수정 
                if (index + 1 < observe_uiList.Count)
                {
                    for (int num = index + 1; num < observe_uiList.Count; num++)
                    {
                        GameObject ui = observe_uiList[num];
                        RectTransform rect = ui.GetComponent<RectTransform>();
                        rect.anchoredPosition = observe_uiList[num - 1].GetComponent<RectTransform>().anchoredPosition;
                    }
                }

                GameObject del = observe_uiList[index];
                observe_uiList.RemoveAt(index);
                // ui 삭제
                Destroy(del);

                QuestTitle node = observeList[i];
                node.DefaultSate = false;
                node.uiIndex = -1;
                observeList[i] = node;

                ChangeObserveIndex(i, index);
            }
        }
    }
    void ChangeObserveIndex(int index, int uiIndex)
    {
        for (int i = observeList.Count - 1; i >= index; i--)
        {
            QuestTitle node = observeList[i];
            if (node.uiIndex > uiIndex)
            {
                node.uiIndex = observeList[i].uiIndex - 1; ;
                observeList[i] = node;
            }
        }
    }
    /*
    // 단서 수집 시 완료되는 퀘스트가 있는 지 확인
    public void QuestDefaultCheck(string checkID)
    {
       
        bool isEnd = false;
        foreach (QuestTitle title in defaultList)
        {
            if (title.DefaultSate)
            {
                foreach (QuestSub sub in title.quest)
                {
                    if (sub.provisoID.Contains(checkID))
                    {
                        //세부 퀘스트 단서 목록에서 지우기
                        sub.provisoID.Remove(checkID);

                        // 세부 퀘스트 UI
                        GameObject subUI = default_uiList[title.uiIndex].transform.GetChild(sub.uiIndex).gameObject;

                        // 세부퀘스트 수 갱신
                        int count = sub.checkCount - sub.provisoID.Count;
                        Text countTxt = subUI.transform.GetChild(0).GetComponent<Text>();
                        countTxt.text = count.ToString();

                        // 확대 조사일 경우 세부 퀘스트 내용 수정
                        Text scriptTxt = subUI.GetComponent<Text>();
                        if (title.id > 9500 & scriptTxt.text == "???")
                            scriptTxt.text = sub.QuestScript;

                        // 세부 퀘스트 완료시 ui 변경
                        if (sub.provisoID.Count <= 0)
                        {
                            isEnd = true;
                            // 세부 퀘스트 삭제
                            title.quest.Remove(sub);
                            //회색 처리
                            scriptTxt.color = Color.gray;
                            countTxt.color = Color.gray;
                            subUI.transform.GetChild(1).GetComponent<Text>().color = Color.gray;
                            subUI.transform.GetChild(2).gameObject.SetActive(true);
                        }
                        break;
                    }
                }
            }
            if (isEnd) break;
        }
        if (isEnd)
        {
            isEnd = false;
            CheckDefaultTitle();
        }
    }
  */
    public void QuestCheck(string checkID)
    {
        bool isEnd = false;
        foreach (QuestTitle title in defaultList)
        {
            if (title.DefaultSate)
            {
                foreach (QuestSub sub in title.quest)
                {
                    if (sub.provisoID.Contains(checkID))
                    {
                        //세부 퀘스트 단서 목록에서 지우기
                        sub.provisoID.Remove(checkID);

                        // 세부 퀘스트 UI
                        GameObject subUI = default_uiList[title.uiIndex].transform.GetChild(sub.uiIndex).gameObject;

                        // 세부퀘스트 수 갱신
                        int count = sub.checkCount - sub.provisoID.Count;
                        Text countTxt = subUI.transform.GetChild(0).GetComponent<Text>();
                        countTxt.text = count.ToString();

                        // 확대 조사일 경우 세부 퀘스트 내용 수정
                        Text scriptTxt = subUI.GetComponent<Text>();
                        if (title.id > 9500 & scriptTxt.text == "???")
                            scriptTxt.text = sub.QuestScript;

                        // 세부 퀘스트 완료시 ui 변경
                        if (sub.provisoID.Count <= 0)
                        {
                            isEnd = true;
                            // 세부 퀘스트 삭제
                            title.quest.Remove(sub);
                            //회색 처리
                            scriptTxt.color = Color.gray;
                            countTxt.color = Color.gray;
                            subUI.transform.GetChild(1).GetComponent<Text>().color = Color.gray;
                            subUI.transform.GetChild(2).gameObject.SetActive(true);
                        }
                        break;
                    }
                }
            }
            if (isEnd) break;
        }
        if (isEnd)
        {
            _audio.PlayOneShot(clip_end);
            isEnd = false;
            CheckDefaultTitle();
        }
        else
        {
            foreach (QuestTitle title in observeList)
            {
                if (title.DefaultSate)
                {
                    foreach (QuestSub sub in title.quest)
                    {
                        if (sub.provisoID.Contains(checkID))
                        {
                            //세부 퀘스트 단서 목록에서 지우기
                            sub.provisoID.Remove(checkID);

                            // 세부 퀘스트 UI
                            GameObject subUI = observe_uiList[title.uiIndex].transform.GetChild(sub.uiIndex).gameObject;

                            // 세부퀘스트 수 갱신
                            int count = sub.checkCount - sub.provisoID.Count;
                            Text countTxt = subUI.transform.GetChild(0).GetComponent<Text>();
                            countTxt.text = count.ToString();

                            // 확대 조사일 경우 세부 퀘스트 내용 수정
                            Text scriptTxt = subUI.GetComponent<Text>();
                            if (title.id > 9500 & scriptTxt.text == "???")
                                scriptTxt.text = sub.QuestScript;

                            // 세부 퀘스트 완료시 ui 변경
                            if (sub.provisoID.Count <= 0)
                            {
                                isEnd = true;
                                // 세부 퀘스트 삭제
                                title.quest.Remove(sub);
                                //회색 처리
                                scriptTxt.color = Color.gray;
                                countTxt.color = Color.gray;
                                subUI.transform.GetChild(1).GetComponent<Text>().color = Color.gray;
                                subUI.transform.GetChild(2).gameObject.SetActive(true);
                            }
                            break;
                        }
                    }
                }
                if (isEnd) break;
            }
            if (isEnd)
            {
                _audio.PlayOneShot(clip_end);
                isEnd = false;
                CheckObserveTitle();
            }
        }
    }

    // 데이터 로드 시 진행 중이던 세부 퀘스트확인
    /*public void CheckedQuest()
    {
        foreach(QuestTitle title in questList)
        {
            // on 상태인 메인 퀘스트의 세부 퀘스트들
            if(title.DefaultSate)
            {
                foreach(QuestSub sub in title.quest)
                {
                    foreach (string proviso in sub.provisoID)
                    {
                        bool bcheck = false;

                        int num;
                        if (int.TryParse(proviso, out num))
                        {
                            //오브젝트 조사 
                            //인물,증거
                            Debug.Log(name + "CheckedQuset : 오브젝트 조사/ 인물,증거");
                        }
                        else
                        {
                            // 추론
                            Debug.Log(name + "CheckedQuset :추론");
                        }

                        // 완료된 퀘스트인 경우
                        if (bcheck)
                        {
                            // 단서 목록에서 지우기
                            sub.provisoID.Remove(proviso);

                            // ui 갱신 
                            GameObject subUI = uiList[title.uiIndex].transform.GetChild(sub.uiIndex).gameObject;

                            int count = sub.checkCount - sub.provisoID.Count;
                            Text countTxt = subUI.transform.GetChild(0).GetComponent<Text>();
                            countTxt.text = count.ToString();

                            // 확대 조사퀘스트일 경우 세부 퀘스트 내용 수정
                            Text scriptTxt = subUI.GetComponent<Text>();
                            if (title.id > 9500 & scriptTxt.text == "???")
                                scriptTxt.text = sub.QuestScript;

                            // 세부 퀘스트 완료시 ui 변경
                            if (sub.provisoID.Count <= 0)
                            {
                                // 세부 퀘스트 삭제
                                title.quest.Remove(sub);
                                //회색 처리
                                scriptTxt.color = Color.gray;
                                countTxt.color = Color.gray;
                                subUI.transform.GetChild(1).GetComponent<Text>().color = Color.gray;
                                subUI.transform.GetChild(2).gameObject.SetActive(true);
                            }

                        }
                    }
                }
            }
        }
    }
    */

    public bool CheckEndEvent(string id)
    {
        if (observeList == null)
        {
            return true;
        }
        else
        {
            foreach (QuestTitle title in observeList)
            {
                if ("QL_" + title.id == id)
                {
                    if (title.quest.Count == 0)
                    {
                        return true;
                    }
                }
            }

            return false;
        }
    }

    void FixXml(int titleID,string state)
    {
        QLDoc = new XmlDocument();
        QLDoc.Load(Application.dataPath + "/Play_QL.xml");

        curScene = GameManager.Instance.CurScene.ToString();
        XmlNode node = QLDoc.SelectSingleNode("QL/" + curScene + "/Quest[@ID='QL_"+titleID+"']");

        if(node == null)
        {
            XmlNodeList list = QLDoc.SelectSingleNode("QL").ChildNodes;
            foreach(XmlNode n in list)
            {
               node= n.SelectSingleNode("Quest[@ID='QL_" + titleID + "']");
                if (node != null) break;

            }
        }
        node.Attributes["DefaultState"].Value = state;

        QLDoc.Save(Application.dataPath + "/Play_QL.xml");
    }

    void CreateDefaultUI(int index)
    {
        //현재 on 상태인 퀘스트라면
        if ( defaultList.Count > default_uiList.Count)
        {
            // 게임 오브젝트 생성
            GameObject newQuest = Instantiate(prefab_title);

            // 서브 퀘스트 갯수 , 현재 생성된 퀘스트 ui 갯수 
            int questCount = defaultList[index].quest.Count;
            int uiCount = parent_default.childCount;


            // text 지정 및 부모 지정
            newQuest.transform.GetChild(0).GetComponent<Text>().text = defaultList[index].questUIText;
            newQuest.transform.SetParent(parent_default);

            float height = 0.0f;

            if (uiCount > 0)
            {
                RectTransform before = parent_default.GetChild(uiCount - 1).GetComponent<RectTransform>();
                height = (before.childCount-2) * -35;
                height -= 125 - before.anchoredPosition.y; 
            }

            RectTransform rect = newQuest.GetComponent<RectTransform>();
            rect.localScale = Vector3.one;
            rect.anchoredPosition = new Vector3(0, height, 0.0f);

            GameObject sub = newQuest.transform.GetChild(1).gameObject;
            Text scriptText = sub.GetComponent<Text>();

            if (defaultList[index].id >= 9500)
                scriptText.text = "???";
            else
                scriptText.text = defaultList[index].quest[0].QuestScript;

            sub.transform.GetChild(1).GetComponent<Text>().text = "/" + defaultList[index].quest[0].provisoID.Count.ToString();

            if (questCount >= 2)
            {
                for (int i = 1; i < questCount; i++)
                {
                    GameObject newScript = Instantiate(prefab_script);

                    newScript.transform.SetParent(newQuest.transform);

                    float y =-30 * i;
                    RectTransform scriptRect = newScript.GetComponent<RectTransform>();
                    scriptRect.anchoredPosition = new Vector3(43, y, 0);

                    if (defaultList[index].id >= 9500)
                        newScript.GetComponent<Text>().text = "???";
                    else
                       newScript.GetComponent<Text>().text = defaultList[index].quest[i].QuestScript;

                    newScript.transform.GetChild(1).GetComponent<Text>().text = "/" + defaultList[index].quest[i].provisoID.Count.ToString();
                }
            }
            default_uiList.Add(newQuest);
           
        }
    }

    void CreateObserveUI(int index)
    {
        //현재 on 상태인 퀘스트라면
        if (observeList.Count > observe_uiList.Count)
        {
            // 게임 오브젝트 생성
            GameObject newQuest = Instantiate(prefab_title);

            // 서브 퀘스트 갯수 , 현재 생성된 퀘스트 ui 갯수 
            int questCount = observeList[index].quest.Count;
            int uiCount = parent_observe.childCount;


            // text 지정 및 부모 지정
            newQuest.transform.GetChild(0).GetComponent<Text>().text = observeList[index].questUIText;
            newQuest.transform.SetParent(parent_observe);

            float height = 0.0f;

            if (uiCount > 0)
            {
                RectTransform before = parent_observe.GetChild(uiCount - 1).GetComponent<RectTransform>();
                height = (before.childCount - 2) * -35;
                height -= 125 - before.anchoredPosition.y;
            }

            RectTransform rect = newQuest.GetComponent<RectTransform>();
            rect.localScale = Vector3.one;
            rect.anchoredPosition = new Vector3(0, height, 0.0f);

            GameObject sub = newQuest.transform.GetChild(1).gameObject;
            Text scriptText = sub.GetComponent<Text>();

            if (observeList[index].id >= 9500)
                scriptText.text = "???";
            else
                scriptText.text = observeList[index].quest[0].QuestScript;

            sub.transform.GetChild(1).GetComponent<Text>().text = "/" + observeList[index].quest[0].provisoID.Count.ToString();

            if (questCount >= 2)
            {
                for (int i = 1; i < questCount; i++)
                {
                    GameObject newScript = Instantiate(prefab_script);

                    newScript.transform.SetParent(newQuest.transform);

                    float y = -30 * i;
                    RectTransform scriptRect = newScript.GetComponent<RectTransform>();
                    scriptRect.anchoredPosition = new Vector3(43, y, 0);

                    if (observeList[index].id >= 9500)
                        newScript.GetComponent<Text>().text = "???";
                    else
                        newScript.GetComponent<Text>().text = observeList[index].quest[i].QuestScript;

                    newScript.transform.GetChild(1).GetComponent<Text>().text = "/" + observeList[index].quest[i].provisoID.Count.ToString();
                }
            }
            observe_uiList.Add(newQuest);

        }
    }

}

