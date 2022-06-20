using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIPanel : MonoBehaviour {
	
	void Update () {

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            PopDown();
        }
	}

    public virtual void PopUp()
    {
        this.gameObject.SetActive(true);
    }

    public virtual void PopDown()
    {
        this.gameObject.SetActive(false);
    }

}
