using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DetailsCamera : MonoBehaviour
{

    public GameObject obj;

    Camera _camera;

    int itemLayer = (1 << 29);
    private Vector3 startPos, endPos;   // 마우스 시작점, 끝 점
    Vector3 angle;

    private void Awake()
    {
        _camera = GetComponent<Camera>();
    }

    void DetailsObserve()
    {
        // 조사 오브젝트와 같은 이름 오브젝트 생성
        //        GameObject.Instantiate();

    }


    void Update()
    {
        if (Input.GetMouseButton(0))
            Click();
        // 마우스 오른쪽 버튼 + 드래그 = 오브젝트 회전
        if (Input.GetMouseButtonDown(1))
            MouseDown();
        if (Input.GetMouseButton(1))
            MouseMove();
    }



    // 마우스 오른쪽 버튼 다운
    void MouseDown()
    {
        angle = Vector3.zero;
        // 마우스 시작 위치 설정
        startPos = endPos = Input.mousePosition;
    }

    // 마우스 오른쪽 버튼 다운 + 드래그
    void MouseMove()
    {
        //현재 위치 설정
        endPos = Input.mousePosition;

        // 마우스 이동 x,y 값
        float angleX = endPos.x - startPos.x;
        float angleY = endPos.y - startPos.y;

        angle.x = angleY ;
        angle.y = -angleX;

        // 쿼터니언으로 변경
        //    rot = Quaternion.Euler(angle);

        // 회전 값 설정
        //transform.rotation = rot;
        obj.transform.Rotate(angle * Time.deltaTime,Space.World);

    }

    void Click()
    {
        Ray r = _camera.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;
        if (Physics.Raycast(r, out hit, 10000.0f, itemLayer))
        {
            //대사
            DialoguePanel.Instance.StartDialogue(DialogueType.DET, hit.collider.name);
        }
    }
}
