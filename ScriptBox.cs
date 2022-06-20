using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ScriptBox : MonoBehaviour
{

    public float time = 0.5f;

    private int curPos = 0;         // 글자 순서 

    private Text txt_name;          // 캐릭터 이름 텍스트
    private Text txt_chat;          // 대화 내용 텍스트
    private string str_chat;        // 대화 내용 저장용

    string script;
    string chName;


    // Use this for initialization
    void Awake()
    {
        txt_name = transform.GetChild(1).GetComponent<Text>();
        txt_chat = transform.GetChild(2).GetComponent<Text>();
    }

    private void OnEnable()
    {
        enabled = true;
        script = txt_chat.text;
        chName = txt_name.text;


        StartCoroutine(TypingDialogue());
    }

    // 대화 내용 한 글자씩 출력
    IEnumerator TypingDialogue()
    {
        txt_name.text = chName;
        str_chat = script;
        // txt_chat.text = " ";

        int num = str_chat.Length;

        //글자를 다 출력하거나 클릭 하기 전까지 루프
        while (curPos < num)
        {
            txt_chat.text = str_chat.Substring(0, curPos);
            ++curPos;

            yield return new WaitForSeconds(time);
        }
        //대사 출력 완료
        curPos = 0;
        txt_chat.text = str_chat;


        //코루틴을 멈춤
        yield break;
    }

}