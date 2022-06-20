//------------------------------------------------------------------//
// 작성자 : 조성혜
// 카메라를 제어하는 스크립트
// 최근 수정 사항 : CamMove() -물체 충돌, 캐릭터 컬링/ 상호작용
// 수정해야 할 사항:CamMove를 캐릭터말고 다른곳에서 제어할 방법, 좌우 회전시 조금 느리게 따라가도록
// 최종 수정일자 : 18.05.06
//------------------------------------------------------------------//
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using Cinemachine;
public class CameraControl : MonoBehaviour
{
    Camera main;

    //플레이어
    [SerializeField]
    Transform _player;                   // 플레이어 트랜스 폼 
    public Transform Player { get { return _player; } }
    Player _playerCtrl;

    //설정 값
    public float _moveSpeed = 3.0f;       // 이동 속도
    public float _rotSpeed = 0.25f;        // 회전 속도
    public float _width = 1.6f;           // 캐릭터 - 카메라 사이 폭
    public float _height = 2.0f;          // 카메라 높이
    public float _minDist = 1.0f;         // 캐릭터 - 카메라 사이 최소 거리

    private Vector3 startPos, endPos;   // 마우스 시작점, 끝 점
    private Vector3 angle;              // 카메라 회전 값
    private Quaternion rot;
    Vector3 targetPos;

    // 레이어 설정
    int playerLayer = (1 << 31) + (1 << 2);
    bool _bTalking = false;               //Check Talking
    public void SetTalking(bool b)  { _bTalking = b; _playerCtrl.SetTalking(b); _interaction.enabled = !b; }
    public bool bTalking { get { return _bTalking; } }

    CameraInteraction _interaction;
    public void OnInteraction()
    { _interaction.enabled = true; }

    void Awake()
    {
        main = transform.GetComponentInChildren<Camera>();
        _interaction = GetComponent<CameraInteraction>();

        _playerCtrl = _player.gameObject.GetComponent<Player>();
        // 카메라 위치 설정
        transform.position = _player.position - (transform.forward * _width) + (Vector3.up * _height);
        angle = transform.rotation.eulerAngles;
    }

    void Update()
    {
        //대화 중이 아닌 경우 
        if (!_bTalking)
        {
            // 마우스 오른쪽 버튼 + 드래그 = 카메라 회전
            if (Input.GetMouseButtonDown(1))
                MouseDown();
            if (Input.GetMouseButton(1))
                MouseMove();
        }

    }

    public void SetCamera()
    {
        // 카메라 위치 설정
        transform.position = _player.position - (transform.forward * _width) + (Vector3.up * _height);
        angle = transform.rotation.eulerAngles;
    }

    public void CamRotation(float h)
    {
        float degree;
        //오른쪽 회전 중일때
        if(0 < h)
        {
         degree = _player.transform.eulerAngles.y - 90;
           

        }
        // 왼쪽 회전 중일때
        else
        {
           degree = _player.transform.eulerAngles.y + 90;

        }

        angle.y = degree;
        // 쿼터니언으로 변경
         rot = Quaternion.Euler(angle);

        Quaternion nRot = transform.rotation;

        transform.rotation = Quaternion.Slerp(nRot, rot, Time.deltaTime / _rotSpeed);

    }

    public Vector3 GetForword()
    {
        return Vector3.forward;
    }

    //카메라 이동 (캐릭터 이동, 카메라 회전시 호출)
    public void CamMove()
    {
        RaycastHit hitInfo;

        Vector3 startVec = _player.position + (transform.up * _height);

        // 캐릭터 기준 카메라 방향
        Vector3 direction = (transform.position - startVec).normalized;

        //물체와 닿을 때
        if (Physics.SphereCast(startVec,0.4f, direction, out hitInfo, _width,~playerLayer))
        {
            // 캐릭터 위치 ~ 닿은 물체 위치 
            float fixVal = Vector3.Distance(hitInfo.point, startVec) - 0.5f;
            // fixVal 의 값 만큼만 폭을 떨어트림 
            targetPos = _player.position - (transform.forward * fixVal) + (Vector3.up * _height);
            transform.position = Vector3.Lerp(transform.position, targetPos, _moveSpeed);
;
        }
        else
        {
            // 안 닿을 경우 기존 방식으로 위치 값 설정
            targetPos = _player.position - (transform.forward * _width) + (Vector3.up * _height);
            transform.position = Vector3.Lerp(transform.position, targetPos, _moveSpeed);
        }

        float dist = Vector3.Distance(startVec, transform.position);
        // 카메라와 플레이어 사이 거리가 너무 가까울때 
        if (dist < _minDist)
        {
            // 플레이어 레이어를 끔
            main.cullingMask = ~playerLayer;
        }
        else
        {
            // 원상 복귀 
            main.cullingMask = -1;
        }
    }

    // 마우스 오른쪽 버튼 다운
    void MouseDown()
    {
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

        startPos = endPos = Input.mousePosition;

        // 회전 값 축적
        //아래로 내릴 수록 위로
        angle.x += -angleY * _rotSpeed;
        angle.y += angleX * _rotSpeed;

        // x 값 제한
        angle.x = Mathf.Clamp(angle.x, 0.0f, 40.0f);

        // 쿼터니언으로 변경
        rot = Quaternion.Euler(angle);

        // 회전 값 설정
        transform.rotation = rot;

        CamMove();
    }

    

}
