using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(CharacterController))]
public class PlayerWalkingAudio : MonoBehaviour
{
    [Header("클립")]
    [Tooltip("걷기 루프 오디오클립")]
    public AudioClip walkLoop;
    [Tooltip("달리기 루프 오디오클립")]
    public AudioClip runLoop;

    [Header("볼륨/페이드")]
    [Range(0, 1)] public float walkMaxVolume = 0.8f;
    [Range(0, 1)] public float runMaxVolume = 1.0f;
    [Tooltip("볼륨 변화 속도(1/sec)")]
    public float fadeSpeed = 10f;

    [Header("재생 조건(속도 기준)")]
    [Tooltip("이 속도 이하에선 무시(서서히 흔들림 제거)")]
    public float minSpeed = 0.12f;
    [Tooltip("이 속도 이상이면 달리기로 판정")]
    public float runSpeedThreshold = 2.0f;

    [Header("재생 조건(입력 보조, 선택)")]
    [Tooltip("달리기 입력(예: XRI LeftHand/Select 또는 RightHand/PrimaryButton)")]
    public InputActionProperty runAction; // 버튼/트리거(0~1) 허용
    [Tooltip("runAction 값이 이 이상이면 달리기 판정에 가산")]
    [Range(0, 1)] public float runPressThreshold = 0.5f;

    [Header("3D 설정")]
    [Tooltip("처음엔 2D로 확인 후 OK면 3D로 전환 권장")]
    public bool use3D = false;
    public float minDistance = 6f;
    public float maxDistance = 24f;

    [Header("피치(선택)")]
    [Tooltip("달리기일 때 조금 더 높은 피치로")]
    public float runPitch = 1.05f;
    public float walkPitch = 1.0f;

    CharacterController cc;
    AudioSource walkSrc, runSrc;

    void Awake()
    {
        cc = GetComponent<CharacterController>();

        // 걷기 소스
        walkSrc = gameObject.AddComponent<AudioSource>();
        walkSrc.playOnAwake = false;
        walkSrc.loop = true;
        walkSrc.clip = walkLoop;
        walkSrc.volume = 0f;
        walkSrc.spatialBlend = use3D ? 1f : 0f;
        walkSrc.minDistance = minDistance;
        walkSrc.maxDistance = maxDistance;
        walkSrc.dopplerLevel = 0f;
        walkSrc.pitch = walkPitch;
        if (walkLoop) walkSrc.Play();

        // 달리기 소스
        runSrc = gameObject.AddComponent<AudioSource>();
        runSrc.playOnAwake = false;
        runSrc.loop = true;
        runSrc.clip = runLoop;
        runSrc.volume = 0f;
        runSrc.spatialBlend = use3D ? 1f : 0f;
        runSrc.minDistance = minDistance;
        runSrc.maxDistance = maxDistance;
        runSrc.dopplerLevel = 0f;
        runSrc.pitch = runPitch;
        if (runLoop) runSrc.Play();
    }

    void Update()
    {
        // 수평 속도 계산
        Vector3 v = cc ? cc.velocity : Vector3.zero;
        v.y = 0f;
        float speed = v.magnitude;

        // 입력에 의한 가산(선택)
        bool runPressed = false;
        if (runAction.reference != null)
        {
            float val = 0f;
            // 버튼/트리거/축 모두 커버
            try { val = runAction.action.ReadValue<float>(); } catch { }
            runPressed = val >= runPressThreshold;
        }

        // 상태 판정
        bool isMoving = speed > minSpeed;
        bool isRunning = (speed >= runSpeedThreshold) || runPressed;

        // 타겟 볼륨
        float walkTarget = (isMoving && !isRunning && walkLoop) ? walkMaxVolume : 0f;
        float runTarget = (isMoving && isRunning && runLoop) ? runMaxVolume : 0f;

        // 크로스페이드
        walkSrc.volume = Mathf.MoveTowards(walkSrc.volume, walkTarget, fadeSpeed * Time.deltaTime);
        runSrc.volume = Mathf.MoveTowards(runSrc.volume, runTarget, fadeSpeed * Time.deltaTime);

        // 안전장치: 끊겼으면 다시 재생
        if (walkLoop && !walkSrc.isPlaying && walkTarget > 0f) walkSrc.Play();
        if (runLoop && !runSrc.isPlaying && runTarget > 0f) runSrc.Play();

        // 필요 시 피치 보정(속도 기반 미세 조정 원하면 아래 주석 해제)
        // walkSrc.pitch = Mathf.Lerp(0.95f, 1.05f, Mathf.InverseLerp(minSpeed, runSpeedThreshold, speed));
        // runSrc.pitch  = Mathf.Lerp(1.0f,  1.15f, Mathf.InverseLerp(runSpeedThreshold, runSpeedThreshold*2f, speed));
    }
}