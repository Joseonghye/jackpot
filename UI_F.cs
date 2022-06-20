//------------------------------------------------------------------//
// 작성자 : 조성혜
// [UI] F키 ui를 눌렀을 시 반응, 이미지 설정 
// 최종 수정일자 : 18.09.02
//------------------------------------------------------------------//
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UI_F : MonoBehaviour {

    string objtag;
    private Text name;

    private void Awake()
    {
        name = GetComponentInChildren<Text>();
    }

    // Update is called once per frame
    void Update ()
    {
        if (Input.GetKeyDown(KeyCode.F))
            Response();
	}

    public void SetInfo(string _tag)
    {
        objtag = _tag;
    }

    // 반응 = 대화창
    public void Response()
    {
        //  DialoguePanel.Instance.gameObject.SetActive(true);
        UIManager.Instance.Go_dialoue.SetActive(true);
        switch (objtag)
        {
            case "OBJ":
                DialoguePanel.Instance.StartDialogue(DialogueType.OBJ, name.text);
                break;

            case "NPC":
                DialoguePanel.Instance.StartDialogue(DialogueType.NPC, name.text);
                break;

            case "EXP": // 확대조사
                GameManager.Instance.Event.ActiveEvent("EXP_" + name.text);
                break;

            case "DET": // 세부조사
                break;
        }
    }
}
