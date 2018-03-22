using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Xml;


public class DialogueManager : MonoBehaviour {

    // ------------- xml -------------------
    TextAsset story_txt;

    XmlDocument clueDoc;
    XmlNode clue;                   // 현재 단서 노드


    //--------------- 단서 -------------------
    private string str_name;        // 현재 아이템 이름

    string specificItem = null;     // 필요한 특정 아이템
    string checkState = null;       // 아이템 체크 상태
    string GetPossibility = null;   // 챙김 가능 여부 

    string curScene = null;         // 현재 위치한 씬 

    //--------------- 대화 --------------------
    List<string> speaker;           // 말하는 대상 리스트
    List<string> contents;          // 대화 내용 


    string curScene = null;         // 현재 위치한 씬 

    private void Awake()
    {
        //리스트 초기화
        speaker = new List<string>();
        contents = new List<string>();

        //스토리 텍스트 에셋 초기화 
        story_txt = (TextAsset)Resources.Load("Xml/Dialogue");
    }

    // 변수 초기화
    void Clean()
    {
        str_name = null;
        specificItem = null;
        checkState = null;
        GetPossibility = null;

        curScene = null;
    }


    // 변수 초기화
    void Clean()
    {
        str_name = null;
        specificItem = null;
        checkState = null;
        GetPossibility = null;

        curScene = null;
    }

    //---------------------------------------
    // xml 파일 열기
    //---------------------------------------
    public void OpenFile(string name)
    {
        // 변수 초기화
        Clean();

        //현재 씬
        curScene = GameManager.Instance.CurScene.ToString();

        //현재 찾은 아이템 이름
        str_name = name;

        clueDoc = new XmlDocument();
        clueDoc.Load(Application.dataPath + "/Play_clue.xml");

        // 인식할 아이템 노드 선택
        clue = clueDoc.SelectSingleNode("Clue/" + curScene + "/" + str_name);

        if (clue.ChildNodes.Count == 3)
        {
            specificItem = clue.SelectSingleNode("SpecificItem").InnerText;
        }
        checkState = clue.SelectSingleNode("CheckState").InnerText;
        GetPossibility = clue.SelectSingleNode("GetPossibility").InnerText;

        //대사 가져오기
        GetStory();
    }


    //---------------------------------------
    // 대화 내용 가져 오기 
    // 1. 특정 아이템 체크
    // 2. 현재 체크 상태 확인
    // 3. 대화 내용 불러오기
    //---------------------------------------
    void GetStory()
    {
        bool specific = false;

        // 따로 특정 아이템을 먼저 확인 해야 하는 경우 
        if (specificItem != null)
        {
            //특정 아이템을 확인했는지 체크
            specific = CheckSpecificItem();
        }
        // 따로 특정아이템이 필요 없거나 본적이 없을 경우  
        if (!specific)
        {
            XmlDocument storyDoc = new XmlDocument();
            storyDoc.LoadXml(story_txt.text);

            XmlNode node = storyDoc.SelectSingleNode("Dialogue/Before/" + curScene + "/" + str_name);

            // 하이라이트 대사 조건
            // 1. 두 번이상 체크한 경우 (하이라이트 대사)
            // 2. 하이라이트 대사가 있는 경우

            // 두번 이상 확인 한 경우 
            if (checkState == "Before_h")
            { //하이라이트 번호 가져오기
                string highlight = node.SelectSingleNode("Highlight").InnerText;
                int num = int.Parse(highlight);

                // 하이라이트가 있을때
                if (num != 0)
                {
                    speaker.Add(node.ChildNodes[num].Attributes.GetNamedItem("name").Value);
                    contents.Add(node.ChildNodes[num].Attributes.GetNamedItem("contents").Value);
                }
                //하이라이트 없을때
                else
                {
                    for (int i = 1; i < node.ChildNodes.Count; i++)
                    {
                        string n = node.ChildNodes[i].Attributes.GetNamedItem("name").Value;
                        string content = node.ChildNodes[i].Attributes.GetNamedItem("contents").Value;
                        speaker.Add(n);
                        contents.Add(content);
                    }
                }

            }
            // 처음 조사하는 경우
            else
            {
                for (int i = 1; i < node.ChildNodes.Count; i++)
                {
                    string n = node.ChildNodes[i].Attributes.GetNamedItem("name").Value;
                    string content = node.ChildNodes[i].Attributes.GetNamedItem("contents").Value;
                    speaker.Add(n);
                    contents.Add(content);
                }

                // 수첩에 획득 가능할때 
                if (GetPossibility == "Y")
                {
                    //수첩 정보 파일 로드
                    XmlDocument noteDoc = new XmlDocument();
                    noteDoc.Load(Application.dataPath + "/Play_note.xml");

                    //노트 노드 로드
                    XmlNode note = noteDoc.SelectSingleNode("Note/" + str_name);
                    XmlNode get = note.SelectSingleNode("Get");
                    get.InnerText = "Y";
                    clue.SelectSingleNode("GetPossibility").InnerText = "N";

                    noteDoc.Save(Application.dataPath + "/Play_note.xml");
                }

                // 이미 한번 확인 했음 으로 변경
                clue.SelectSingleNode("CheckState").InnerText = "Before_h";

                //저장
                clueDoc.Save(Application.dataPath + "/Play_clue.xml");
            }

        }

        // 대화 대상, 내용 전달 
        UIManager.Instance.SetDialogue(speaker, contents);

    }


    // 필요 아이템 체크
    bool CheckSpecificItem()
    {
        //필요아이템 노드
        XmlNode itemNode = clueDoc.SelectSingleNode("Clue/" + curScene + "/" + specificItem);

        // 필요 아이템 체크 상태
        string check = itemNode.SelectSingleNode("CheckState").InnerText;

        //필요 아이템을 확인 한적이 없는 경우
        if (check == "Before") return false;

        // 필요 아이템을 한번이상 확인한 경우 
        else
        {
            //스토리 텍스트
            XmlDocument storyDoc = new XmlDocument();
            storyDoc.LoadXml(story_txt.text);

            // 조사 중인 아이템 스토리 노드 
            XmlNode node = storyDoc.SelectSingleNode("Dialogue/After/" + curScene + "/" + str_name);

            // 바뀐 대사를 이미 본 경우 > 하이라이트 
            if (checkState == "After")
            {
                //하이라이트 위치 구하기
                string highlight = node.SelectSingleNode("Highlight").InnerText;
                int num = int.Parse(highlight);


                //하이라이트가 0이 아닌 경우 (하이라이트 대사 출력)
                if (num != 0)
                {
                    speaker.Add(node.ChildNodes[num].Attributes.GetNamedItem("name").Value);
                    contents.Add(node.ChildNodes[num].Attributes.GetNamedItem("contents").Value);
                }

                // 하이라이트가 0인 경우 (전체 대사 출력)
                else
                {
                    for (int i = 1; i < node.ChildNodes.Count; i++)
                    {
                        speaker.Add(node.ChildNodes[i].Attributes.GetNamedItem("name").Value);
                        contents.Add(node.ChildNodes[i].Attributes.GetNamedItem("contents").Value);
                    }
                }
            }

            // 바뀐 대사를 처음 보는 경우 (전체 대사 출력)
            else
            {
                for (int i = 1; i < node.ChildNodes.Count; i++)
                {
                    speaker.Add(node.ChildNodes[i].Attributes.GetNamedItem("name").Value);
                    contents.Add(node.ChildNodes[i].Attributes.GetNamedItem("contents").Value);
                }
                //바뀐 대사를 본적 있음 으로 변경
                clue.SelectSingleNode("CheckState").InnerText = "After";
            }
            //저장
            clueDoc.Save(Application.dataPath + "/Play_clue.xml");
            return true;
        }
    }


}
