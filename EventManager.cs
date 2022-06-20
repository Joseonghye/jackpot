//------------------------------------------------------------------//
// 작성자 : 조성혜
// 미니게임, 확대조사등의 이벤트 매니저 
// 최종 수정일자 : 18.06.17
//------------------------------------------------------------------//
using System.Collections.Generic;
using UnityEngine;
using System.Xml;
using UnityEngine.SceneManagement;
using Cinemachine;
using UnityEngine.UI;
using System.Collections;

public class EventManager : MonoBehaviour {

    GameObject _mainCamera;
    GameObject[] _eventCamera;

    XmlDocument eventDoc;
    TextAsset eventText;
    XmlNode eventNode;

    XmlDocument objDoc;
 

    Image panel;

	void Awake ()
    {
        eventText = (TextAsset)Resources.Load("Xml/Event");
        _mainCamera = GameManager.Instance.Main;
    }
    void OnEnable()
    {
        SceneManager.sceneLoaded += OnLevelFinishedLoading;
    }

    void OnDisable()
    {
        SceneManager.sceneLoaded -= OnLevelFinishedLoading;
    }

    //         씬 변경시 유아이 설정
    private void OnLevelFinishedLoading(Scene scene, LoadSceneMode mode)
    {
        _mainCamera = GameManager.Instance.Main;

        panel = GameObject.Find("FadePanel").GetComponent<Image>();
        panel.enabled=false;

        _eventCamera = GameObject.FindGameObjectsWithTag("event");

        for (int i = 0; i < _eventCamera.Length; i++)
        {
            _eventCamera[i].SetActive(false);
        }
    }

    public void EndEvent()
    {
        if (panel != null)
        {
            StartCoroutine(FadeOut());
        }
 
    }

    public void ActiveEvent(string eventName)
    {
        if (eventName.Contains("Game"))
        {
            GameManager.Instance.SetNextScene(SceneType.Game_Gear);
            GameManager.Instance.NextScene();
        }
        else {
            if (panel != null)
            {
                panel.enabled = true;
                StartCoroutine(FadeIn());
            }
            eventDoc = new XmlDocument();
            eventDoc.LoadXml(eventText.text);

            // 같은 이름 찾기
            XmlNode node = eventDoc.SelectSingleNode("Event/" + eventName);

            string camName = node.SelectSingleNode("CameraName").InnerText;

            // 퀘스트 언락
            string str_QuestID = node.SelectSingleNode("QuestListUnlock").InnerText;
            int questID = int.Parse(str_QuestID);
            UIManager.Instance.Quest.AddQuest(questID);

            // 카메라 변경
            foreach (GameObject camera in _eventCamera)
            {
                if (camera.name == camName)
                {

                    camera.SetActive(true);
                    break;
                }
            }
        }

        _mainCamera.SetActive(false);

    }

    IEnumerator FadeIn()
    {
        Color color = panel.color;
        while (color.a > 0)
        {
            color.a -= Time.deltaTime;
            panel.color = color;

            yield return new WaitForSeconds(Time.deltaTime);
        }
        yield break;
    }

    IEnumerator FadeOut()
    {
        Color color = panel.color;
        while (color.a < 1)
        {
            color.a += Time.deltaTime;
            panel.color = color;
            yield return new WaitForSeconds(Time.deltaTime);
        }
        panel.enabled = false;
        _mainCamera.SetActive(true);
        _mainCamera.GetComponent<CameraInteraction>().enabled = true;
        yield break;
    }
}
