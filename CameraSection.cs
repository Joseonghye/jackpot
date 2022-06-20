using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraSection : MonoBehaviour {

    [SerializeField]
    private GameObject _camera;
    public GameObject Camera { get { return _camera; } } 

    public virtual void OnTriggerEnter(Collider col)
    {
        if (col.tag == "Player")
        {
            GameObject cam = GameManager.Instance.Main;
            if (cam != gameObject)
            {
                _camera.gameObject.SetActive(true);
                cam.SetActive(false);

                GameManager.Instance.Main = _camera;
            }
        }
    }
}
