using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using UnityEngine;
using UnityEngine.UI;

public class SavePanel : UIPanel {

    RectTransform rect;

    bool b_move = false; //움직이는 중인가
    bool b_pop = false; // 나오는지 들어가는지

    public GameObject prefab_save;
    bool[] isSave = new bool[5]; //저장되어있는 슬롯인지

    TimeSpan time_played;

    private void Awake()
    {
        rect = GetComponent<RectTransform>();
        InitSlot();
    }

    void InitSlot()
    {
        for (int i = 1; i < 6; i++)
        {
            FileInfo fi = new FileInfo(Application.dataPath + "/SaveData_" + i + ".xml");

            if (fi.Exists)
            {
                int num = i;
                GameObject orign = transform.GetChild(i).gameObject;
                RectTransform orignRect = orign.GetComponent<RectTransform>();

                GameObject slot = Instantiate(prefab_save);
                slot.transform.SetParent(transform);

                //자식 순서
                slot.transform.SetSiblingIndex(num);

                slot.GetComponent<RectTransform>().anchoredPosition = orignRect.anchoredPosition;

                isSave[num - 1] = true;

                // 버튼 이벤트 지정
                Button save_btn = slot.transform.GetChild(4).GetComponent<Button>();
                save_btn.onClick.AddListener(() => Save(num));

                Button load_btn = slot.transform.GetChild(5).GetComponent<Button>();
                load_btn.onClick.AddListener(() => Load(num));

                //기존것 삭제 
                orign.transform.SetParent(null);
                Destroy(orign);

                //플레이 타임, 장소, 저장시간 가져오기
                XmlDocument saveDoc = new XmlDocument();
                saveDoc.Load(Application.dataPath + "/SaveData_" + i + ".xml");

                string str_time = saveDoc.SelectSingleNode("Save/PlayTime").InnerText;
                slot.transform.GetChild(2).GetComponent<Text>().text = str_time;

                string str_location = saveDoc.SelectSingleNode("Save/Location").InnerText;
                slot.transform.GetChild(3).GetComponent<Text>().text = str_location;

                string str_date = saveDoc.SelectSingleNode("Save/Date").InnerText;
                slot.transform.GetChild(1).GetComponent<Text>().text = str_date;

            }
            else
            {
                isSave[i - 1] = false;
            }
        }
    }

    //----------------------------------
    //            UI Pop
    //----------------------------------
    public override void PopUp()
    {
        base.PopUp();
        b_move = true;
        b_pop = true;
        StartCoroutine(MoveUI());
    }

    public override void PopDown()
    {
        b_move = true;
        b_pop = false;
        StartCoroutine(MoveUI());
    }

    //저장 패널 나오고 들어감 
    IEnumerator MoveUI()
    {
        float x = 0;
        if (b_pop) x = 971;

        while (b_move)
        {
            if (b_pop)
            {
                x -= 20;
                if (x <= 0)
                {
                    x = 0;
                    b_move = false;
                }
            }
            else
            {
                x += 20;
                if (x >= 971)
                {
                    x = 971;

                    b_move = false;
                }
            }

            rect.anchoredPosition = new Vector3(x, 0.0f, 0.0f);
            yield return new WaitForSeconds(0.01f);
        }

        if (!b_pop) this.gameObject.SetActive(false);

        yield break;
    }

    //----------------------------------
    //            Save
    //----------------------------------
    public void Save(int num)
    {
        GameObject slot;
        //새로 저장일 경우
        if (!isSave[num - 1])
        {
            //기존 슬롯 오브젝트 
            GameObject orign = transform.GetChild(num).gameObject;
            RectTransform orignRect = orign.GetComponent<RectTransform>();

            // Make new Save slot & Set parent
            slot = Instantiate(prefab_save);
            slot.transform.parent = transform;

            // Set children num
            slot.transform.SetSiblingIndex(num);
            // Set Position
            slot.GetComponent<RectTransform>().anchoredPosition = orignRect.anchoredPosition;

            isSave[num - 1] = true;

            // Set Button Event
            Button save_btn = slot.transform.GetChild(4).GetComponent<Button>();
            save_btn.onClick.AddListener(() => Save(num));

            Button load_btn = slot.transform.GetChild(5).GetComponent<Button>();
            load_btn.onClick.AddListener(() => Load(num));

            // Delete Orignal slot 
            Destroy(orign);

        }
        // 덮어쓰기 
        else
        {
            slot = transform.GetChild(num).gameObject;
        }
        // 저장 시간
        string str_date = DateTime.Now.ToString("yy/MM/dd, HH:mm");
        slot.transform.GetChild(1).GetComponent<Text>().text = str_date;

        // 플레이 타임 
        DateTime start = GameManager.Instance.StartTime;
        DateTime end = DateTime.Now;
        TimeSpan time = end - start;

        TimeSpan now = time_played.Add(time);
        string str_time = string.Format("{0:00}:{1:00}:{2:00}", now.Hours, now.Minutes, now.Seconds);
        slot.transform.GetChild(2).GetComponent<Text>().text = str_time;

        // 장소 
        string str_location = GameManager.Instance.CurScene.ToString();
        slot.transform.GetChild(3).GetComponent<Text>().text = str_location;

        //xml
        // 1. 현재 사용 중인 파일 가져오기
        XmlDocument objDoc = new XmlDocument();
        objDoc.Load(Application.dataPath + "/Play_ob.xml");
        XmlDocument noteDoc = new XmlDocument();
        noteDoc.Load(Application.dataPath + "/Play_note.xml");
        XmlDocument QLDoc = new XmlDocument();
        QLDoc.Load(Application.dataPath + "/Play_QL.xml");
        XmlDocument sDoc = new XmlDocument();
        sDoc.Load(Application.dataPath + "/Play_select.xml");

        // 2. 슬롯 번호에 맞게 저장
        objDoc.Save(Application.dataPath + "/Play_ob" + num + ".xml");
        noteDoc.Save(Application.dataPath + "/Play_note" + num + ".xml");
        QLDoc.Save(Application.dataPath + "/Play_QL" + num + ".xml");
        sDoc.Save(Application.dataPath + "/Play_select" + num + ".xml");

        // save xml 만들기
        XmlDocument saveDoc = new XmlDocument();
        XmlElement root = saveDoc.CreateElement("Save");

        saveDoc.AppendChild(root);

        XmlElement date = saveDoc.CreateElement("Date");
        date.InnerText = str_date;
        root.AppendChild(date);

        XmlElement playTime = saveDoc.CreateElement("PlayTime");
        playTime.InnerText = str_time;
        root.AppendChild(playTime);

        XmlElement location = saveDoc.CreateElement("Location");
        location.InnerText = str_location;
        root.AppendChild(location);

        //비밀방 열림 여부
        XmlElement drRoom = saveDoc.CreateElement("DrRoom");
        drRoom.InnerText = GameManager.Instance.DrRoomCtrl.bCheckDr.ToString();
        root.AppendChild(drRoom);

        saveDoc.Save(Application.dataPath + "/SaveData_" + num + ".xml");
    }

    public void Load(int num)
    {
        //플레이 타임, 장소 가져오기
        XmlDocument saveDoc = new XmlDocument();
        saveDoc.Load(Application.dataPath + "/SaveData_" + num + ".xml");

        string str_time = saveDoc.SelectSingleNode("Save/PlayTime").InnerText;
        time_played = TimeSpan.Parse(str_time);

        string str_location = saveDoc.SelectSingleNode("Save/Location").InnerText;
        SceneType type = (SceneType)Enum.Parse(typeof(SceneType), str_location);
        GameManager.Instance.SetNextScene(type);

        string str_check = saveDoc.SelectSingleNode("Save/DrRoom").InnerText;
        bool b = bool.Parse(str_check);
        GameManager.Instance.DrRoomCtrl.setCheck(b);

        //xml 
        // 단서
        XmlDocument doc = new XmlDocument();
        doc.Load(Application.dataPath + "/Play_ob" + num + ".xml");
        doc.Save(Application.dataPath + "/Play_ob.xml");
        // 수첩
        doc = new XmlDocument();
        doc.Load(Application.dataPath + "/Play_note" + num + ".xml");
        doc.Save(Application.dataPath + "/Play_note.xml");
        // 퀘스트 리스트
        doc = new XmlDocument();
        doc.Load(Application.dataPath + "/Play_QL" + num + ".xml");
        doc.Save(Application.dataPath + "/Play_QL.xml");
        // 선택지
        doc = new XmlDocument();
        doc.Load(Application.dataPath + "/Play_select" + num + ".xml");
        doc.Save(Application.dataPath + "/Play_select.xml");

        //노트 갱신
  //      GameManager.Instance.Note.Init(true);
  //      GameManager.Instance.Note.InitNote();

        //시작 시간 재 설정
        GameManager.Instance.SetStratTime();
        // 이동
        GameManager.Instance.NextScene();
    }
}
