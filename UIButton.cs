using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIButton : MonoBehaviour {

    Image _img;

    public Sprite sprite_default;
    public Sprite sprite_hover;
    public Sprite sprite_active;

    bool isClick = false;
    public void SetClick()
    {
        isClick = false;
        _img.sprite = sprite_default;
    }

    private void Awake()
    {
        _img = GetComponent<Image>();
    }

   public void PointerEnter()
    {
        _img.sprite = sprite_hover;
    }

    public void PointerExit()
    {
        if (!isClick)
            _img.sprite = sprite_default;
        else
            _img.sprite = sprite_active;
    }

    public void OnClick()
    {
        isClick = true;
        _img.sprite = sprite_active;
    }
}
