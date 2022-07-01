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
    DetailsCamera _details;
    public DetailsCamera Details { get { return _details; } }

    [SerializeField]
    private Transform playerTrans;

    GameObject ui_f;
    Text txt_name;

    int interactionLayer = (1 << 30);
    bool objChk = false;
    string objTag;

    float interactionDist = 8.f;

    private void Awake()
    {
        _camera = GetComponent<Camera>();
        _details = GetComponent<DetailsCamera>();
        ui_f = UIManager.Instance.UI_ob;
        txt_name = ui_f.GetComponentInChildren<Text>();

    }


    private void Update()
    {
        if (!GameManager.Instance.IsObserve && !GameManager.Instance.IsNext &&
            !UIManager.Instance.OpenNote && !UIManager.Instance.StartCriminal && !UIManager.Instance.StartGame)
        {
           Interaction();
        }
        else
        {
            ui_f.SetActive(false);
        }
    }

    Material[] _mat;
    Color orign = Color.black;
    [SerializeField]
    Color over;
    GameObject obj;
    // 상호작용 인식
    void Interaction()
    {
        RaycastHit hit = new RaycastHit();
        GameObject target = null;

        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray.origin, ray.direction, out hit, interactionDist, interactionLayer))
        {
            target = hit.collider.gameObject;

            if (obj != target)
            {
                objChk = true;
                if (obj != null)
                {
                    foreach (Material mat in _mat)
                        mat.SetColor("_EmissionColor", orign);
                }

                objTag = target.tag;
                obj = target;

                UIManager.Instance.SetObUI(objTag);

                MeshRenderer[] render = target.GetComponentsInChildren<MeshRenderer>();
                _mat = new Material[render.Length];
                for (int i = 0; i < render.Length; i++)
                {
                    _mat[i] = render[i].material;
                    _mat[i].SetColor("_EmissionColor", over);
                }
                
                if (_mat == null)
                    _mat[0] = target.transform.GetChild(0).GetComponent<MeshRenderer>().material;

                foreach (Material mat in _mat)
                    mat.SetColor("_EmissionColor", over);

                ui_f.SetActive(true);
                txt_name.text = target.name;
            }
            SetFUI(Input.mousePosition);

            if (Input.GetMouseButton(0))
            {
              
                Click();
            }
        }
        else
        {
            if (obj != null)
            {
                foreach (Material mat in _mat)
                    mat.SetColor("_EmissionColor",orign);
                obj = null;
                ui_f.SetActive(false);
            }
        }
    }

    void Click()
    {
        GameManager.Instance.IsObserve = true;
        ui_f.SetActive(false);
        string n = txt_name.text.Replace(" ", "");

        foreach (Material mat in _mat)
            mat.SetColor("_EmissionColor", orign);

        switch (objTag)
        {
            case "OBJ":
                UIManager.Instance.Dialoue.SetActive(true);
                DialoguePanel.Instance.StartDialogue(DialogueType.OBJ, n);
                break;

            case "NPC":
                UIManager.Instance.Select.SetCamera(obj);
                UIManager.Instance.Select.Init(txt_name.text);
                break;

            case "EXP": // 확대조사
                UIManager.Instance.Dialoue.SetActive(true);
                DialoguePanel.Instance.StartDialogue(DialogueType.EXP, n);
                break;

            case "DET": // 세부조사
                UIManager.Instance.Dialoue.SetActive(true);
                DialoguePanel.Instance.StartDialogue(DialogueType.DET, n);
                break;
        }
    }

    void SetFUI(Vector2 objPos)
    {
        Vector2 screenPos = objPos + new Vector2(80,-10); // _camera.WorldToScreenPoint(objPos);

        Vector2 viewPos = _camera.ScreenToViewportPoint(screenPos);
        if (viewPos.x > 0.8f)
            viewPos.x = 0.8f;
        if (viewPos.x < 0.2f)
            viewPos.x = 0.2f;

        if (viewPos.y > 0.8f)
            viewPos.y = 0.8f;
        if (viewPos.y < 0.2f)
            viewPos.y = 0.2f;

        screenPos = _camera.ViewportToScreenPoint(viewPos);

        ui_f.transform.position = screenPos;
    }

}
