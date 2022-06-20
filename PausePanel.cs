using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PausePanel : UIPanel {

    Slider soundSlider;

	void Start ()
    {
        soundSlider = transform.GetChild(2).GetComponent<Slider>();	
	}

    public void SoundSize()
    {
        AudioListener.volume = soundSlider.value;
    }

    public void MoveToTitle()
    {
        this.gameObject.SetActive(false);
        GameManager.Instance.SetNextScene(SceneType.Title);
        GameManager.Instance.NextScene();
    }
}
