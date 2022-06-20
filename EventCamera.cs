using System.Collections;
using System.Collections.Generic;
using System.Xml;
using UnityEngine;

public class EventCamera : MonoBehaviour
{

    int itemLayer = (1 << 29);
    Camera _camera;

    private void Awake()
    {
        _camera = GetComponent<Camera>();
    }
    private void Update()
    {
        if (Input.GetMouseButtonDown(0))
            Interaction();

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            GameManager.Instance.Event.EndEvent();
            transform.gameObject.SetActive(false);
        }
    }
    //클릭 시 
    void Interaction()
    {
        Ray r = _camera.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;
        if (Physics.Raycast(r, out hit, 10000.0f, itemLayer))
        {
            string name = hit.collider.name;
            string curScene = GameManager.Instance.CurScene.ToString();

            UIManager.Instance.Quest.IsEndQuest = false;

            XmlDocument clueDoc = new XmlDocument();
            clueDoc.Load(Application.dataPath + "/Play_obj.xml");

            // 단서 추가
            XmlNode node;
            node = clueDoc.SelectSingleNode("Obj/" + curScene + "/" + name);

            // 수첩에 단서 추가
            if (node.SelectSingleNode("GetProvisoID") != null)
            {

                string avidence = node.SelectSingleNode("GetProvisoID").InnerText;
                int id = int.Parse(avidence);

                // 퀘스트 
                UIManager.Instance.Quest.CheckQuest(id);

      //          GameManager.Instance.Note.AddEvidence(id);

            }

            Destroy(hit.collider.gameObject);

            if (UIManager.Instance.Quest.IsEndQuest)
            {
                GameManager.Instance.Event.EndEvent();
                transform.gameObject.SetActive(false);
            }
            if (GameManager.Instance.IsNext) GameManager.Instance.NextScene();
        }
    }

}