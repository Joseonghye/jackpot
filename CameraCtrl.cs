//------------------------------------------------------------------//
// 작성자 : 조성혜
// 고정 카메라, 상호작용 
// 최종 수정일자 : 18.09.01
//------------------------------------------------------------------//
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public enum DialougeType { OBJ,NPC,EXP,DET,SEL,ALO};

public class CameraCtrl : MonoBehaviour {

    Camera _camera;
    [SerializeField]
    private Transform _playerCenter;

    public GameObject ui_f;
    Text txt_name;

    float fildDistance = 1.5f;

    int interactionLayer = (1 << 30);

    private void Awake()
    {
        _camera = GetComponentInChildren<Camera>();
        txt_name = ui_f.GetComponentInChildren<Text>();
    }

    private void Update()
    {
        Interaction();
    }
    // 상호작용 인식
    void Interaction()
    {
        Collider[] colls = Physics.OverlapSphere(_playerCenter.position, fildDistance, interactionLayer);

        Collider obj = null;
        float minDist = 5.0f;

        if (colls.Length == 0)
            ui_f.SetActive(false);

        // 가장 가까운 오브젝트 찾기
        foreach (Collider coll in colls)
        {
            float dist = Vector3.Distance(coll.transform.position, _playerCenter.transform.position);
            if (dist > 1.3f)
            {
                Renderer[] render = coll.GetComponentsInChildren<Renderer>();
                foreach (Renderer ren in render)
                    ren.material.SetFloat("_Ifem", 0.0f);
            }
            else
            {
                // 이펙트 
                Renderer[] render = coll.GetComponentsInChildren<Renderer>();
                foreach (Renderer ren in render)
                    ren.material.SetFloat("_Ifem", 1.0f);


                if (minDist > dist)
                {
                    obj = coll;
                    minDist = dist;
                }
            }
        }

        if (obj != null)
        {
            // UI Setting
            SetFUI(obj.transform.position);
            string name = obj.name;
            txt_name.text = name;
            ui_f.GetComponent<UI_F>().SetInfo(obj.tag);
            ui_f.SetActive(true);
        }
    }

    void SetFUI(Vector3 objPos)
    {
        Vector3 screenPos = _camera.WorldToScreenPoint(objPos);
        screenPos.z = 0;

        Vector3 viewPos = _camera.ScreenToViewportPoint(screenPos);
        if (viewPos.x > 0.9f)
            viewPos.x = 0.9f;
        if (viewPos.x < 0.1f)
            viewPos.x = 0.1f;

        if (viewPos.y > 0.9f)
            viewPos.y = 0.9f;
        if (viewPos.y < 0.1f)
            viewPos.y = 0.1f;

        screenPos = _camera.ViewportToScreenPoint(viewPos);

        ui_f.transform.position = screenPos;
    }

}
