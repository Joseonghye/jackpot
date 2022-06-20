using System.Collections;
using System.Collections.Generic;
using System.Xml;
using UnityEngine;

public class NotePanel : UIPanel {

    ObjPage obj;
    TalkPage talk;
    InfPage inf;

    GameObject[] person;

    int open = -1;

	void Awake ()
    {
        talk = transform.GetChild(1).GetChild(2).GetComponent<TalkPage>();
        obj = transform.GetChild(1).GetChild(3).GetComponent<ObjPage>();
        inf = transform.GetChild(1).GetChild(4).GetComponent<InfPage>();

        person = new GameObject[3];
        person[0] = transform.GetChild(0).GetChild(6).gameObject;
        person[1] = transform.GetChild(1).GetChild(1).gameObject;
        person[2] = transform.GetChild(2).GetChild(0).gameObject;
    }

    public void Click(int num)
    {
        if (open != num)
        {
         
            open = num;
            switch (num)
            {
                case -1:
                    SetChar(true);
                    talk.gameObject.SetActive(false);
                    obj.gameObject.SetActive(false);
                    inf.gameObject.SetActive(false);
                    break;
                case 0:
                    talk.gameObject.SetActive(true);
                    obj.gameObject.SetActive(false);
                    inf.gameObject.SetActive(false);
                    SetChar(false);
                    break;

                case 1:
                    talk.gameObject.SetActive(false);
                    obj.gameObject.SetActive(true);
                    inf.gameObject.SetActive(false);
                    SetChar(false);
                    break;

                case 2:
                    talk.gameObject.SetActive(false);
                    obj.gameObject.SetActive(false);
                    inf.gameObject.SetActive(true);
                    SetChar(false);
                    break;

                default:
                    break;
            }
        }
    }

    void SetChar(bool set)
    {
        for(int i=0; i<3; i++)
        {
            person[i].SetActive(set);
        }
    }

    //초기화

    // 단서 추가 
    public void Add(string checkID)
    {
        XmlDocument doc = new XmlDocument();
        //추론 메인
        if (checkID.Contains("L"))
        {
            doc.Load(Application.dataPath + "Play_InfM.xml");
            XmlNode node = doc.SelectSingleNode("Main/field[@SubjectListID='" + checkID + "']");
            node.Attributes["DefaultState"].Value = "on";
            doc.Save(Application.dataPath + "Play_InfM.xml");

            inf.Add(checkID);
        }
    }
    public void Add(int checkID)
    {
        XmlDocument doc = new XmlDocument();

        // 세부 추론
        if (checkID > 20000)
        {
            doc.Load(Application.dataPath + "Play_InfS.xml");
            XmlNodeList groupList = doc.SelectSingleNode("Sub").ChildNodes;
            foreach (XmlNode group in groupList)
            {
                XmlNode node = group.SelectSingleNode("SubInfo[@QuestionID='" + checkID + "']");
                if (node != null)
                {
                    node.Attributes["DefaultState"].Value = "on";
                    doc.Save(Application.dataPath + "Play_InfS.xml");
                    break;
                }
            }
        }
        // 증거 증언
        else
        {
            doc.Load(Application.dataPath + "Play_note.xml");
            XmlNodeList typeList = doc.SelectSingleNode("Note").ChildNodes;
            foreach(XmlNode type in typeList)
            {
                XmlNode node = type.SelectSingleNode("Clue[@ID='" + checkID + "']");
                if(node != null)
                {
                    node.Attributes["DefaultState"].Value = "on";
                    doc.Save(Application.dataPath + "Play_note.xml");

                    string name = node.SelectSingleNode("EvidenceNameKr").InnerText;
                    string _prefabName = null;
                    if (node.SelectSingleNode("EvidenceDefaultName") != null)
                        _prefabName = node.SelectSingleNode("EvidenceDefaultName").InnerText;
                    Slot _slot = new Slot(checkID,name,_prefabName);
                    if (type.Name == "OBJ")
                        obj.Add(_slot);
                    else
                        talk.Add(_slot);

                    XmlNode option = node.SelectSingleNode("Option");
                    if(option != null)
                    {
                        CheckOption(option);
                    }

                    break;
                }
            }

        }
    }

	public void CheckOption(XmlNode option)
    {
        if (option.Attributes["QuestSuccess"] != null)
        {

        }
        if (option.Attributes["GetOffStateID"] != null)
        {
            string onStateID = option.Attributes["GetOffStateID"].Value;

            string[] IDs = onStateID.Split('/');

            for (int i = 0; i < IDs.Length; i++)
            { 
                int _id = int.Parse(IDs[i]);

                //탐색
                if (_id >= 1000 && _id < 2000)
                {
                    DialoguePanel.Instance.SetObserveState(_id, false);
                }
                //선택지
                else if (_id >= 7000 && _id < 8000)
                {

                }
                //단서
                else
                {

                }
            }
        }

        if (option.Attributes["GetOnStateID"] != null)
        {
            string onStateID = option.Attributes["GetOnStateID"].Value;

            string[] IDs = onStateID.Split('/');

            for (int i = 0; i < IDs.Length; i++)
            {
                int _id;
                bool val = int.TryParse(IDs[i], out _id);

                if (val)
                {
                    //탐색
                    if (_id >= 1000 && _id < 2000)
                    {
                        DialoguePanel.Instance.SetObserveState(_id, true);
                    }
                    //선택지
                    else if (_id >= 7000 && _id < 8000)
                    {

                    }
                    //퀘스트 
                    else if (_id >= 9000 && _id < 10000)
                    {
                        UIManager.Instance.Quest.AddQuest(_id);
                    }
                    //단서 
                    else
                    {
                        Add(_id);
                    }
                }
                else
                {
                    // 추론 메인
                    Add(IDs[i]);
                }
            }
        }
        
    }
}