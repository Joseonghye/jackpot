//------------------------------------------------------------------//
// 작성자 : 조성혜
// 수첩 추론의 세부 항목 
// 최종 수정일자 : 18.09.02
//------------------------------------------------------------------//

using System.Collections;
using System.Collections.Generic;
using System.Xml;
using UnityEngine;
using UnityEngine.UI;

struct SubSlot
{
    public int id;
    public bool state;
    public bool isComplete;
    public string question;
    public string questDetails;
    public string img;
    public string subText;
    public List<int> answerList;
    public List<int> printList;
    public List<string> Onid;
}

public class InfSub : MonoBehaviour
{

    string questID;
    int selectNum;
    RectTransform rect;

    public GameObject prefab_sub; // 추론 세부 목록 프리팹
    public GameObject r_sub;    // 추론 세부 정보 

    //왼쪽
    List<SubSlot> slotList;

    // 오른쪽
    List<int> clickList;

    Image InfImg;

    GameObject[] answers;
    Image[] answerImg;

    GameObject[] prints;
    Image[] printImg;

    XmlDocument doc;

    // 기본 이미지 
    public Sprite defaultImg;
    public Sprite defaultAnwser;

    private void Awake()
    {
        slotList = new List<SubSlot>();
        clickList = new List<int>();
        rect = GetComponent<RectTransform>();

        InfImg = r_sub.transform.GetChild(1).GetComponent<Image>();

        answers = new GameObject[3];
        answerImg = new Image[3];
        for (int i = 0; i < 3; i++)
        {
            answers[i] = r_sub.transform.GetChild(2).GetChild(i).gameObject;
            answerImg[i] = answers[i].GetComponent<Image>();
        }
        prints = new GameObject[5];
        printImg = new Image[5];
        for (int j = 0; j < 5; j++)
        {
            prints[j] = r_sub.transform.GetChild(4).GetChild(j).gameObject;
            printImg[j] = prints[j].GetComponent<Image>();
        }
    }

    private void OnEnable()
    {
        Init();
    }

    private void OnDisable()
    {
        r_sub.SetActive(false);
    }

    public void SetQuestID(string _id)
    {
        questID = _id;
    }
    
    //세부목록 셋팅
    void Init()
    {
        slotList.Clear();

        doc = new XmlDocument();
        doc.Load(Application.dataPath + "/Play_infS.xml");

        XmlNodeList subList = doc.SelectSingleNode("Sub/SubGroup[@SubjectQuestionID='"+questID+"']").ChildNodes;

        foreach (XmlNode node in subList)
        {
            SubSlot slot = new SubSlot();

            slot.state = false;
            if (node.Attributes["DefaultState"].Value == "on")
                slot.state = true;

            slot.question = node.Attributes["QuestionMain_KR"].Value;
            slot.img = node.Attributes["CombPic_Name"].Value;
            slot.questDetails = node.Attributes["QuestionDetails_KR"].Value;
            slot.subText = node.Attributes["CombNameKR"].Value;

            slot.answerList = new List<int>();
            string answer = node.Attributes["AnswerProviso"].Value;
            string[] answers = answer.Split('/');
            foreach (string str in answers)
            {
                slot.answerList.Add(int.Parse(str));
            }

            slot.printList = new List<int>();
            if (node.Attributes["PrintProviso"] != null)
            {
                string clue = node.Attributes["PrintProviso"].Value;
                string[] clues = clue.Split('/');

                for (int i = 0; i < clues.Length; i++)
                {
                    slot.printList.Add(int.Parse(clues[i]));
                }
            }
            if (slot.printList.Count == 0) slot.isComplete = true;
            else slot.isComplete = false;

            if(node.Attributes["GetOnStateID"] != null)
            {
                string str_on = node.Attributes["GetOnStateID"].Value;
                string[] onID = str_on.Split('/');

                for (int i = 0; i < onID.Length; i++)
                {
                    slot.Onid.Add(onID[i]);
                }
            }

            slotList.Add(slot);

        }

        // UI 생성
        for (int num = 0; num < slotList.Count; num++)
        {
            //프리팹 생성
            GameObject newSlot = Instantiate(prefab_sub);
            // 부모지정
            newSlot.transform.SetParent(rect);
            // 포지션 지정
            newSlot.GetComponent<RectTransform>().anchoredPosition = new Vector2(50, num * -110);

            // 이름 및 내용 
            Text name = newSlot.transform.GetChild(0).GetComponent<Text>();
            Text detail = newSlot.transform.GetChild(1).GetComponent<Text>();
            if (slotList[num].state)
            {
                name.text = slotList[num].question;
                if (!slotList[num].isComplete) detail.text = "아직 밝혀지지 않았다.";
                else
                {
                    detail.text = slotList[num].questDetails;
                    detail.color = Color.white;
                }
                //클릭이벤트
                int n = num;
                Button btn = newSlot.GetComponent<Button>();
                btn.onClick.AddListener(() => Click(n));
            }
            else
            {
                name.text = "???";
                detail.text = "";
            }

        }
    }

    //뒤로가기
    void BackPage()
    {
    }
    
    void Add()
    {
        // 테이블 on
        // UI 변경
        // 클릭이벤트
    }

    //목록클릭
    void Click(int num)
    {
        //클릭한 슬롯
        selectNum = num;

        if (!r_sub.activeSelf) r_sub.SetActive(true);

        //오른쪽 페이지 셋팅
        r_sub.transform.GetChild(0).GetComponent<Text>().text = slotList[num].subText;

        // 완성 단서 갯수
        for(int i=0; i<3; i++)
        {
            if (i < slotList[num].answerList.Count)
            {
                answers[i].SetActive(true);
            }
            else answers[i].SetActive(false);
        }
        //위치 지정


        //조합이 완료된 경우
        if (slotList[num].isComplete)
        {
            // 조합 이미지
            InfImg.sprite = (Sprite)Resources.Load("Note/" + slotList[selectNum].img);

            // 완성 단서 
            for (int i = 0; i < slotList[num].answerList.Count; i++)
            {
                answerImg[i].sprite = (Sprite)Resources.Load("Note/" + slotList[num].printList[i]);
                answers[i].GetComponent<Button>().enabled = false;
            }
            // 프린트 목록은 끄기
            for (int j = 0; j < 5; j++)
            {
                prints[j].SetActive(false);
            }
        }
        else
        {
            Debug.Log("미완성 오픈");

            //조합이미지 = 기본 ??? 이미지
            InfImg.sprite = defaultImg;
      
            for (int i = 0; i < slotList[num].answerList.Count; i++)
            {
                //완성 단서 이미지 = 기본
                answerImg[num].sprite = defaultAnwser;
                //클릭 이벤트
                answers[i].GetComponent<Button>().enabled = true;
            }

            // 등록 단서 갯수 
            for (int j = 0; j < 5; j++)
            {
                if (j < slotList[num].printList.Count)
                {
                    printImg[j].sprite = (Sprite)Resources.Load("Note/" + slotList[num].printList[j]);
                    prints[j].SetActive(true);
                }
                else prints[j].SetActive(false);
            }
            //위치지정

         
        }
    }

    // 등록 취소 
    public void SelectCancle(int num)
    {
        if (num < clickList.Count)
        {
            Debug.Log("추론 완성 이미지 등록 취소");
            // 완성 이미지 = 기본
            answerImg[num].sprite = defaultAnwser;
            // clickList 에서 제외 
            clickList.RemoveAt(num);
        }
    }
    //-------- 등록 가능 단서-----------//
    public void PrintEnter()
    {
        // 단서 설명
    }

    public void PrintClick(int i)
    {
        Debug.Log("Click");
        // 완성 슬롯에 추가 
        clickList.Add(slotList[selectNum].printList[i]);

        // 완성 목록 이미지 변경
        answerImg[clickList.Count - 1].sprite = printImg[i].sprite;

        if (clickList.Count == slotList[selectNum].answerList.Count)
        {
            // 맞는 단서를 선택했는지 확인
            bool check = false;

            foreach (int id in clickList)
            {
                if (slotList[selectNum].answerList.Contains(id))
                {
                    check = true;
                }
                else
                {
                    check = false;
                    break;
                }
            }

            //맞는 단서들
            if(check)
            {
                Debug.Log("조합 완료");
                SubSlot newSlot = slotList[selectNum];
                newSlot.isComplete = true;
                newSlot.printList.Clear();
                slotList[selectNum] = newSlot;

                // 이미지 변경
                InfImg.sprite = (Sprite)Resources.Load("Note/" + slotList[selectNum].img);

                // 테이블에서 printList 삭제
                XmlNode node = doc.SelectSingleNode("Sub/SubGroup[@SubjectQuestionID='" + questID + "']/field[@QuestionID='" + slotList[selectNum].id + "']");
                node.Attributes["PrintProviso"].Value = null;

                doc.Save(Application.dataPath + "/Play_infS.xml");

                //onStateID
               for(int j =0; j< slotList[selectNum].Onid.Count;j++)
                {

                }
            }
        }
    }
}
