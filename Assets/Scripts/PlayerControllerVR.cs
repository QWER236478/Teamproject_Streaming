using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;

[RequireComponent(typeof(CharacterController))]
[RequireComponent(typeof(AudioSource))]
public class PlayerControllerVR : MonoBehaviour
{
    [Header("이동 설정")]
    public float moveSpeed = 1f;      // 걷기 속도
    public float runSpeed = 3f;       // 달리기 속도

    CharacterController cc;

    [Header("플래시라이트")]
    public Light handlight;
    bool flashlightOn = false;

    [Header("발소리(루프)")]
    public AudioClip walkLoop;           // 걷기 루프
    public AudioClip runLoop;            // 달리기 루프
    public float moveThreshold = 0.1f;   // 이동 판정 임계값(속도)

    [Header("볼륨/피치 정밀 제어")]
    public float walkVolume = 0.35f;
    public float runVolume = 0.25f;
    public float volumeBySpeed = 0.15f;
    public float basePitch = 1.0f;
    public float pitchBySpeed = 0.10f;

    AudioSource audioSrc;

    // XR 입력 (왼손 컨트롤러)
    InputDevice leftHand;
    Vector2 moveAxis;    // L스틱 값

    [Header("참조")]
    public Transform headTransformOverride; // XR Origin 안의 Main Camera 넣어주면 더 안전

    // 내부 상태
    Vector3 velocity;    // 중력 포함 속도

    void Start()
    {
        cc = GetComponent<CharacterController>();

        if (handlight) handlight.enabled = false;

        audioSrc = GetComponent<AudioSource>();
        audioSrc.loop = true;
        audioSrc.playOnAwake = false;
        audioSrc.volume = 0f;

        TryInitLeftHand();
    }

    void TryInitLeftHand()
    {
        var devices = new List<InputDevice>();
        InputDevices.GetDevicesAtXRNode(XRNode.LeftHand, devices);

        if (devices.Count > 0)
        {
            leftHand = devices[0];
            // Debug.Log("[PlayerVR] Left hand device : " + leftHand.name);
        }
    }

    void Update()
    {
        // ---------------- XR 입력 읽기 ----------------
        if (!leftHand.isValid)
            TryInitLeftHand();

        float h = 0f;
        float v = 0f;
        bool wantsRun = false;

        if (leftHand.isValid)
        {
            // L스틱
            if (leftHand.TryGetFeatureValue(CommonUsages.primary2DAxis, out moveAxis))
            {
                h = moveAxis.x;
                v = moveAxis.y;   // ★ 여기! 이제 부호 안 뒤집음 (위로 밀면 +, 아래로 -)
            }

            // 그립 버튼 = 달리기
            leftHand.TryGetFeatureValue(CommonUsages.gripButton, out wantsRun);
        }

        // 입력 벡터 (x,z)
        Vector3 input = new Vector3(h, 0f, v);
        input = Vector3.ClampMagnitude(input, 1f); // 대각선에서도 일정한 크기

        bool hasMoveInput = input.sqrMagnitude > 0.0001f;
        bool isRunning = wantsRun && hasMoveInput;
        float targetSpeed = isRunning ? runSpeed : moveSpeed;

        // -------------- HMD(카메라) 기준 방향 --------------
        Transform headT = headTransformOverride;
        if (headT == null && Camera.main != null)
            headT = Camera.main.transform;

        Vector3 moveDir = Vector3.zero;

        if (headT != null)
        {
            Vector3 headForward = headT.forward;
            headForward.y = 0f;
            headForward.Normalize();

            Vector3 headRight = headT.right;
            headRight.y = 0f;
            headRight.Normalize();

            moveDir = headForward * input.z + headRight * input.x;
        }
        else
        {
            // 카메라를 못 찾으면 본체 기준
            moveDir = transform.forward * input.z + transform.right * input.x;
        }

        if (moveDir.sqrMagnitude > 0.0001f)
            moveDir.Normalize();

        // -------------- 실제 속도 계산 + 이동 --------------
        float horizontalSpeed = targetSpeed * input.magnitude; // 현재 입력 기반 실제 속도
        Vector3 horizVelocity = moveDir * horizontalSpeed;

        // 중력 추가 (원하면 수정/제거)
        velocity = horizVelocity;
        velocity += Physics.gravity;

        cc.Move(velocity * Time.deltaTime);

        // -------------- 플래시라이트 (임시: N키) --------------
        if (Input.GetKeyDown(KeyCode.N) && handlight)
        {
            flashlightOn = !flashlightOn;
            handlight.enabled = flashlightOn;
        }

        // -------------- 발소리 처리 --------------
        bool isMoving = cc.isGrounded && horizontalSpeed > moveThreshold;

        if (isMoving)
        {
            AudioClip targetClip = isRunning ? runLoop : walkLoop;

            if (audioSrc.clip != targetClip)
            {
                audioSrc.Stop();
                audioSrc.clip = targetClip;
            }

            if (audioSrc.clip && !audioSrc.isPlaying)
                audioSrc.Play();

            float speed01 = Mathf.Clamp01(horizontalSpeed / runSpeed);

            float baseVol = isRunning ? runVolume : walkVolume;
            float vol = Mathf.Clamp01(baseVol + speed01 * volumeBySpeed);
            float pit = Mathf.Clamp(basePitch + speed01 * pitchBySpeed, 0.8f, 1.4f);

            audioSrc.volume = vol;
            audioSrc.pitch = pit;
        }
        else
        {
            if (audioSrc.isPlaying)
                audioSrc.Stop();
        }
    }
}