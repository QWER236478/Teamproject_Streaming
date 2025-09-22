using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.XR.Interaction.Toolkit;

[DisallowMultipleComponent]
[RequireComponent(typeof(CharacterController))]
public class PlayerDash : MonoBehaviour
{
    // ───────── 참조 ─────────
    [Header("참조")]
    [Tooltip("Action-based Continuous Move Provider (왼쪽 스틱 이동)")]
    public ActionBasedContinuousMoveProvider moveProvider;
    [Tooltip("같은 오브젝트에 있는 CharacterController")]
    public CharacterController cc;

    // ───────── 입력 ─────────
    [Header("입력 (왼손 트리거)")]
    [Tooltip("XRI LeftHand / Activate (float)")]
    public InputActionProperty leftTrigger;
    [Range(0, 1f)] public float triggerThreshold = 0.75f;

    // ───────── 스프린트 ─────────
    [Header("스프린트 설정")]
    [Tooltip("기본 걷기 속도 (Move Provider의 Move Speed와 동일하게 설정)")]
    public float baseMoveSpeed = 1.4f;
    [Tooltip("트리거 누를 때 곱해줄 배수(=달리기 배속)")]
    public float sprintMultiplier = 2.0f;
    [Tooltip("속도 전환 부드러움(클수록 즉각적)")]
    public float speedLerp = 12f;

    // ───────── 오디오 ─────────
    [Header("발소리 루프(자동 크로스페이드)")]
    public AudioClip walkLoop;                           // 걷기 루프
    public AudioClip runLoop;                            // 달리기 루프
    [Range(0, 1)] public float walkMaxVolume = 0.8f;
    [Range(0, 1)] public float runMaxVolume = 1.0f;
    [Tooltip("볼륨 페이드 속도(1/sec)")]
    public float volumeFade = 10f;
    [Tooltip("이 속도보다 느리면 무음 처리")]
    public float minMoveSpeed = 0.12f;

    [Header("3D 설정 (처음엔 2D 권장)")]
    public bool use3D = false;
    public float minDistance = 6f;
    public float maxDistance = 24f;
    public float walkPitch = 1.0f;
    public float runPitch = 1.05f;

    // 내부
    AudioSource walkSrc, runSrc;

    void Reset()
    {
        cc = GetComponent<CharacterController>();
        moveProvider = GetComponent<ActionBasedContinuousMoveProvider>();
    }

    void Awake()
    {
        if (!cc) cc = GetComponent<CharacterController>();
        if (!moveProvider) moveProvider = GetComponent<ActionBasedContinuousMoveProvider>();
        if (moveProvider && baseMoveSpeed <= 0f) baseMoveSpeed = moveProvider.moveSpeed;

        walkSrc = CreateLoopSource("__WalkLoop", walkLoop, use3D, walkPitch);
        runSrc = CreateLoopSource("__RunLoop", runLoop, use3D, runPitch);
    }

    AudioSource CreateLoopSource(string name, AudioClip clip, bool spatial, float pitch)
    {
        var s = gameObject.AddComponent<AudioSource>();
        s.name = name;
        s.playOnAwake = false;
        s.loop = true;
        s.clip = clip;
        s.volume = 0f;                    // 항상 0에서 시작, 페이드로 조절
        s.pitch = pitch;
        s.dopplerLevel = 0f;
        s.spatialBlend = spatial ? 1f : 0f;   // 0=2D, 1=3D
        s.minDistance = minDistance;
        s.maxDistance = maxDistance;
        if (clip) s.Play();               // 루프는 항상 재생, 볼륨만 바꿈
        return s;
    }

    void Update()
    {
        // 1) 스프린트 입력
        bool sprint = leftTrigger.reference &&
                      leftTrigger.action.ReadValue<float>() >= triggerThreshold;

        // 2) Move Provider 속도 보간 (스프린트 동안만 배속)
        if (moveProvider)
        {
            float target = baseMoveSpeed * (sprint ? sprintMultiplier : 1f);
            moveProvider.moveSpeed = Mathf.Lerp(moveProvider.moveSpeed, target,
                                                speedLerp * Time.deltaTime);
        }

        // 3) 현재 수평 속도(오디오 판정용)
        float speedXY = 0f;
        if (cc) { var v = cc.velocity; v.y = 0f; speedXY = v.magnitude; }
        bool isMoving = speedXY > minMoveSpeed;

        // 4) 오디오 크로스페이드
        float walkTarget = (isMoving && !sprint && walkLoop) ? walkMaxVolume : 0f;
        float runTarget = (isMoving && sprint && runLoop) ? runMaxVolume : 0f;

        if (walkSrc) walkSrc.volume = Mathf.MoveTowards(walkSrc.volume, walkTarget, volumeFade * Time.deltaTime);
        if (runSrc) runSrc.volume = Mathf.MoveTowards(runSrc.volume, runTarget, volumeFade * Time.deltaTime);

        // 안전장치: 페이드 타겟이 생기면 재생 보장
        if (walkLoop && walkSrc && !walkSrc.isPlaying && walkTarget > 0f) walkSrc.Play();
        if (runLoop && runSrc && !runSrc.isPlaying && runTarget > 0f) runSrc.Play();
    }
}