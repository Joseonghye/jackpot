//------------------------------------------------------------------//
// 작성자 : 조성혜
// 수첩 추론의 주제부
// 최종 수정일자 : 18.08.24
//------------------------------------------------------------------//
using System.Collections;
using System.Collections.Generic;
using System.Xml;
using UnityEngine;
using UnityEngine.UI;

public class InfPage : NotePage
{
    List<string> id;
    XmlDocument doc;

    public GameObject r_inf;
    public InfSub _sub;

    private void Awake()
    {
        id = new List<string>();
        Init();
    }

    private void OnEnable()  { r_inf.SetActive(true); }
    private void OnDisable() { r_inf.SetActive(false); }

    public override void Init()
    {
        doc = new XmlDocument();
        doc.Load(Application.dataPath + "/Play_infM.xml");

        //루프 노드 설정
        XmlNodeList nodelist = doc.SelectSingleNode("Main").ChildNodes;

        int num = 0;
        foreach (XmlNode node in nodelist)
        {
            string str_id = node.Attributes["SubjectListID"].Value;

            //프리팹추가
            GameObject prefab = (GameObject)Resources.Load("Prefabs/Inf/" + node.Attributes["Subject_Prefab"].Value);
            //Instantiate()
            GameObject newSlot = Instantiate(prefab);

            // 클릭이벤트 
            Button btn = newSlot.GetComponent<Button>();
            btn.onClick.AddListener(() => Click(str_id));

            // 위치 
            if (id.Count < 3)
            {
                newSlot.transform.SetParent(transform);
                newSlot.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, num * -160);
            }
            else
            {
                newSlot.transform.SetParent(r_inf.transform);
                newSlot.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, (num % 3) * -160);
            }



            if (node.Attributes["DefaultState"].Value == "on")
            {
                newSlot.SetActive(true);
            }
            else
                newSlot.SetActive(false);

            id.Add(str_id);
            num++;
        }
    }

    public void Add(string str_id)
    {
        for (int i = 0; i < id.Count; i++)
        {
            if (id[i] == str_id)
            {
                if (i < 3)
                    transform.GetChild(i).gameObject.SetActive(true);
                else
                    r_inf.transform.GetChild(i).gameObject.SetActive(true);

                break;
            }
        }

        doc = new XmlDocument();
        doc.Load(Application.dataPath + "/Play_infM.xml");

        //루프 노드 설정
        XmlNodeList nodelist = doc.SelectSingleNode("Main/").ChildNodes;

        foreach (XmlNode node in nodelist)
        {
            if (str_id == node.Attributes["SubjectListID"].Value)
            {
                node.Attributes["DefaultState"].Value = "on";
                break;
            }
        }
    }

    void Click(string _id)
    {
        //세부 추론으로 이동
        _sub.SetQuestID(_id);
        r_inf.gameObject.SetActive(false);
        _sub.gameObject.SetActive(true);
        transform.gameObject.SetActive(false);
    }

    public override void SetPage(){}
}
