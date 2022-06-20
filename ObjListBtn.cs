 using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ObjListBtn : MonoBehaviour {

    Animator _anim;
    Text title;
    EventSystem eventSys;
    public bool isClick = false;
    public void SetClick()
    {
        isClick = false;

        title.color = Color.white;
        _anim.SetBool("Click", false);
    }

    private void Awake()
    {
        eventSys = EventSystem.current.GetComponent<EventSystem>();
        title = GetComponentInChildren<Text>();
        _anim = GetComponentInChildren<Animator>();
    }

    public void PointerEnter()
    {
        if (!isClick)
        {
            title.color = Color.black;
            _anim.SetBool("Click", true);
        }
    }

    public void PointerExit()
    {
        if (!isClick)
        {
            _anim.SetBool("Click", false);
            title.color = Color.white;
        }
    }

    public void OnClick()
    {
        eventSys.SetSelectedGameObject(this.gameObject);
        isClick = true;
        title.color = Color.black;
        _anim.SetBool("Click", true);
    }
}
