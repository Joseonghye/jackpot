using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class TalkListBtn : MonoBehaviour {
    Image _titleImg;
    Animator _anim;
  //  Image _animImg;

    EventSystem eventSys;

    public bool isClick = false;
    public void SetClick()
    {
        isClick = false;
        _titleImg.color = Color.white;
  //      _animImg.fillAmount = 0;
        _anim.SetBool("Click", false);
    }


    private void Awake()
    {
        eventSys = EventSystem.current.GetComponent<EventSystem>();

   //     _animImg = transform.GetChild(0).GetComponent<Image>();
        _titleImg = transform.GetChild(1).GetComponent<Image>();
        _anim = GetComponentInChildren<Animator>();
    }

    public void PointerEnter()
    {
        if (!isClick)
        {
            _titleImg.color = Color.black;
            _anim.SetBool("Click", true);
        }
    }

    public void PointerExit()
    {
        if (!isClick)
        {
   //         _animImg.fillAmount = 0;
            _anim.SetBool("Click", false);
            _titleImg.color = Color.white;
        }
    }

    public void OnClick()
    {
       eventSys.SetSelectedGameObject(this.gameObject);
        isClick = true;
        _titleImg.color = Color.black;
        _anim.SetBool("Click", true);
    }
}
