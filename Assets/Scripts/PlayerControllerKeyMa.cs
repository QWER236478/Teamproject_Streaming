using UnityEngine;

[RequireComponent(typeof(CharacterController))]
[RequireComponent(typeof(AudioSource))]
public class PlayerControllerKeyMa : MonoBehaviour
{
    [Header("이동 설정")]
    public float moveSpeed = 1f;      // 걷기 속도
    public float runSpeed = 3f;       // 달리기 속도
    public float mouseSensitivity = 2f;

    float pitch;
    CharacterController cc;

    [Header("플래시라이트")]
    public Light handlight;
    bool flashlightOn = false;

    [Header("발소리(루프)")]
    public AudioClip walkLoop;           // 걷기 루프
    public AudioClip runLoop;            // 달리기 루프
    public float moveThreshold = 0.1f;   // 이동 판정 임계값

    [Header("볼륨/피치 정밀 제어")]
    public float walkVolume = 0.35f;     // 걷기 기본 볼륨
    public float runVolume = 0.25f;     // 달리기 기본 볼륨(필요하면 더 낮게)
    public float volumeBySpeed = 0.15f;  // 속도에 따른 추가 볼륨 (걷기/달리기 공통)
    public float basePitch = 1.0f;       // 기본 피치
    public float pitchBySpeed = 0.10f;   // 속도에 따른 추가 피치 (걷기/달리기 공통)

    AudioSource audioSrc;

    void Start()
    {
        cc = GetComponent<CharacterController>();
        Cursor.lockState = CursorLockMode.Locked;

        if (handlight) handlight.enabled = false;

        // 발소리용 오디오소스 설정
        audioSrc = GetComponent<AudioSource>();
        audioSrc.loop = true;          // 이동 중에만 루프
        audioSrc.playOnAwake = false;  // 시작 시 자동 재생 금지
        audioSrc.volume = 0f;          // 시작 무음
    }

    void Update()
    {
        // 마우스 회전
        float yaw = Input.GetAxis("Mouse X") * mouseSensitivity;
        pitch -= Input.GetAxis("Mouse Y") * mouseSensitivity;
        pitch = Mathf.Clamp(pitch, -80, 80);
        transform.Rotate(0, yaw, 0);
        if (Camera.main)
            Camera.main.transform.localRotation = Quaternion.Euler(pitch, 0, 0);

        // 입력
        float h = Input.GetAxis("Horizontal");
        float v = Input.GetAxis("Vertical");
        bool wantsRun = Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift);
        bool hasMoveInput = Mathf.Abs(h) > 0.01f || Mathf.Abs(v) > 0.01f;
        bool isRunning = wantsRun && hasMoveInput;

        // 이동 (걷기/달리기 속도 적용)
        float speed = isRunning ? runSpeed : moveSpeed;
        Vector3 move = (transform.forward * v + transform.right * h) * speed;
        cc.SimpleMove(move);

        // 손전등 토글
        if (Input.GetKeyDown(KeyCode.N) && handlight)
        {
            flashlightOn = !flashlightOn;
            handlight.enabled = flashlightOn;
        }

        // 발소리: 지면 위 + 일정 속도 이상일 때만
        bool isMoving = cc.isGrounded && move.magnitude > moveThreshold;

        if (isMoving)
        {
            // 사용할 루프 선택
            AudioClip targetClip = isRunning ? runLoop : walkLoop;

            // 클립 변경 시 매끄럽게 교체
            if (audioSrc.clip != targetClip)
            {
                audioSrc.Stop();
                audioSrc.clip = targetClip;
            }

            // 루프 재생
            if (audioSrc.clip && !audioSrc.isPlaying)
                audioSrc.Play();

            // 볼륨/피치 계산 (속도 비율: 0~1)
            float speed01 = Mathf.Clamp01(move.magnitude / runSpeed);

            float baseVol = isRunning ? runVolume : walkVolume;                 // 상황별 기본 볼륨
            float vol = Mathf.Clamp01(baseVol + speed01 * volumeBySpeed);       // 속도에 따른 가산
            float pit = Mathf.Clamp(basePitch + speed01 * pitchBySpeed, 0.8f, 1.4f);

            audioSrc.volume = vol;
            audioSrc.pitch = pit;
        }
        else
        {
            // 멈추면 즉시 끔
            if (audioSrc.isPlaying) audioSrc.Stop();
        }
    }
}