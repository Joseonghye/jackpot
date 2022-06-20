using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour {

    // 카메라
    [Header("시작 시 메인카메라")]
    [SerializeField]
    private GameObject _camera;
    private CameraCtrl _camControl;
    public void SetCamera(GameObject cam) { _camera = cam; }

    // 설정 값
    public float moveSpeed = 1.7f;
    public float rotSpeed = 5.0f;

    //애니메이션 
    private Animator _anim;
    private int _aniTurn;
    private int _aniForward;

    public float aniSpeed = 2.0f;

    private float _turn;
    private float _forward;

    float lerpTime;
    float countTime = 0.0f;
    bool btnDown = false;

    bool _bTalking = false;               //Check Talking

    void Start()
    {
        //카메라 설정
      //  _camera = GameObject.FindGameObjectWithTag("MainCamera");
      _camControl = _camera.GetComponent<CameraCtrl>();

        //애니메이션 셋팅
        _anim = transform.GetComponent<Animator>();
        _aniTurn = Animator.StringToHash("Turn");
        _aniForward = Animator.StringToHash("Forward");

        lerpTime = Time.deltaTime / 0.2f;
    }

    // 상태 설정 
    void SetState(float turn, float forward)
    {
        _anim.SetFloat(_aniTurn, turn);
        _anim.SetFloat(_aniForward, forward);
    }

    public void SetTalking(bool _b)
    {
        _bTalking = _b;
    }

    void LateUpdate()
    {
        if (!_bTalking)
            Move();
        else
            DontMove();
    }

    void DontMove()
    {
        _turn = Mathf.Lerp(_turn, 0.0f, lerpTime);
        countTime = 0.0f;
        _forward = Mathf.Lerp(_forward, 0.0f, lerpTime);

        SetState(_turn, _forward);
    }

    private void Move()
    {
        btnDown = false;
        float h = Input.GetAxis("Horizontal");
        if (0 != h)
        {
            countTime += Time.deltaTime;
            _turn += h * aniSpeed;
            btnDown = true;
        }
        else
        {
            _turn = Mathf.Lerp(_turn, 0.0f, lerpTime);
            countTime = 0.0f;
        }
        float v = Input.GetAxis("Vertical");
        if (v > 0)
        {
            if (h == 0)
                ViewForward();
            _forward += v * aniSpeed;
            btnDown = true;
        }
        else
            _forward = Mathf.Lerp(_forward, 0.0f, lerpTime);

        _turn = Mathf.Clamp(_turn, -1.0f, 1.0f);
        _forward = Mathf.Clamp(_forward, 0.0f, 1.0f);

        SetState(_turn, _forward);

        if (btnDown)
        {
            transform.Translate(Vector3.forward * moveSpeed * Time.deltaTime);
        }
    }

    // 카메라 forward 구하는 함수
    private Vector3 VecForward()
    {
        Vector3 right = _camera.transform.right;

        Vector3 up = Vector3.up;

        //외적
        Vector3 cross = Vector3.Cross(right, up).normalized;

        return cross;
    }

    private bool ViewForward()
    {
        Vector3 charVec = transform.forward;
        Vector3 camVec = VecForward();

        float dot = Vector3.Dot(charVec, camVec);

        if (dot > 0.9f)
        {
            return true;
        }

        Quaternion quat = _camera.transform.rotation;
        quat.x = 0; quat.z = 0;
        transform.rotation = Quaternion.Slerp(transform.rotation, quat, 0.1f);

        return false;
    }
}
