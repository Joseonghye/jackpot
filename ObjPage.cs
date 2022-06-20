using System.Collections;
using System.Collections.Generic;
using System.Xml;
using UnityEngine;

public class ObjPage : NotePage
{
    RectTransform rect;

    List<Slot> clueList;
    List<GameObject> uiList;

    XmlDocument doc;

    GameObject rightPage;

    private void Awake()
    {
        rightPage = transform.parent.parent.GetChild(2).GetChild(2).gameObject;
        rect = GetComponent<RectTransform>();
        Init();
    }

    private void OnEnable()
    {
        rightPage.SetActive(true);
    }

    private void OnDisable()
    {
        rightPage.SetActive(false);
    }


    public override void Init()
    {
        clueList = new List<Slot>();
        uiList = new List<GameObject>();

        doc = new XmlDocument();
        doc.Load(Application.dataPath + "/Play_note.xml");

        int cur = (int)GameManager.Instance.CurScene;

        //루프 노드 설정
        XmlNodeList nodelist = doc.SelectSingleNode("Note/Obj/Clue[@DefaultState='on']").ChildNodes;

        foreach (XmlNode node in nodelist)
        {

            string str_id = node.Attributes["id"].Value;
            int _id = int.Parse(str_id);

            string _KrName = node.SelectSingleNode("EvidenceNameKr").InnerText;

            string _prefabName = null;
            if (node.SelectSingleNode("EvidenceDefaultName") != null)
                _prefabName = node.SelectSingleNode("EvidenceDefaultName").InnerText;

            if (node.SelectSingleNode("Option") != null)
            {
                XmlNode option = node.SelectSingleNode("Option");

                    Note.CheckOption(option);

            }

            Slot newSlot;
            newSlot.id = _id;
            newSlot.name = _KrName;
            newSlot.prefabName = _prefabName;

            clueList.Add(newSlot);

        }

        SetPage();
    }

    public void Add(Slot newSlot)
    {
        clueList.Add(newSlot);
    }

    public override void SetPage()
    {
        int val = clueList.Count - uiList.Count;

        for (int i = 0; i < val; i++)
        {
            //Create Slot Object
            GameObject slot;
            //--------slot 지정
            slot = null;

            //Set Name
            slot.name = "clue" + rect.childCount;

            //Set Parent
            slot.transform.SetParent(rect);

            //Set Position
        }
    }
}
