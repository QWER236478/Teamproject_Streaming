using UnityEngine;

[DisallowMultipleComponent]
[RequireComponent(typeof(CharacterController))]
public class MannequinChaser : MonoBehaviour
{
    // ────────────── 타깃/애니메이터 ──────────────
    [Header("타깃(플레이어)")]
    [Tooltip("플레이어의 Transform (XR Origin 전체)")]
    public Transform target;
    [Tooltip("플레이어의 머리카메라(쳐다보면 정지 옵션용)")]
    public Transform head;
    [Tooltip("적 모델의 Animator")]
    public Animator animator;

    // ────────────── 기본 이동/시선/시야 ──────────────
    [Header("기본 이동/시선")]
    [Tooltip("기본 이동 속도(m/s)")]
    public float moveSpeed = 1.8f;
    [Tooltip("회전 부드러움(클수록 빠르게 회전)")]
    public float turnSpeed = 8f;

    [Header("쳐다보면 정지(옵션)")]
    public bool freezeWhenLookedAt = false;
    [Tooltip("플레이어 정면 ±각도 이내에서 나를 보면 정지")]
    public float lookStopAngle = 25f;
    [Tooltip("시야 차단(벽 등) 레이어마스크")]
    public LayerMask losMask = ~0;

    // ────────────── ‘쫓기는 느낌’ 튜닝 ──────────────
    [Header("추격 간격(느슨하게 유지)")]
    [Tooltip("유지하고 싶은 거리(여기쯤에서 멈춤/슬로우)")]
    public float followDistance = 3.0f;
    [Tooltip("이 거리보다 멀어지면 확실히 다시 추격")]
    public float resumeDistance = 4.5f;
    [Tooltip("목표 거리 근처 감속 구간")]
    public float softZone = 0.8f;
    [Tooltip("멀리 떨어졌을 때 가속 배수(캐치업)")]
    public float catchupMultiplier = 1.6f;
    [Tooltip("플레이어 움직임에 대한 반응 지연(초)")]
    public float reactionLag = 0.25f;

    [Header("뒤쪽 오프셋 추종(뒤에서 따라오기)")]
    public bool stayBehind = true;
    [Tooltip("플레이어 뒤로 얼마나 떨어질지")]
    public float behindOffset = 2.0f;
    [Tooltip("좌우 살짝 흔들림 양(0이면 끔)")]
    public float lateralJitter = 0.25f;
    [Tooltip("흔들림 속도")]
    public float jitterSpeed = 0.7f;

    // ────────────── 발소리(루프 + 크로스페이드) ──────────────
    [Header("발소리 루프(항상 재생 + 볼륨 페이드)")]
    [Tooltip("걷기 루프 클립")]
    public AudioClip walkLoop;
    [Tooltip("달리기/캐치업 루프 클립")]
    public AudioClip runLoop;
    [Range(0, 1)] public float walkMaxVolume = 0.8f;
    [Range(0, 1)] public float runMaxVolume = 1.0f;
    [Tooltip("볼륨 변화 속도(1/sec)")]
    public float volumeFade = 10f;
    [Tooltip("이 속도 이하에선 무음(미세 떨림 무시)")]
    public float minSpeedForAudio = 0.05f;
    [Tooltip("수평 속도가 이 값을 넘거나, 캐치업 상태면 달리기 루프로 전환")]
    public float runSpeedThreshold = 2.0f;
    [Header("오디오 3D 설정(처음엔 2D로 확인 권장)")]
    public bool use3D = false;
    public float minDistance = 6f;
    public float maxDistance = 24f;
    public float walkPitch = 1.00f;
    public float runPitch = 1.05f;

    // ────────────── 애니 파라미터 ──────────────
    [Header("애니메이션 파라미터")]
    [Tooltip("Animator.SetFloat(\"Speed\")에 적용할 댐핑(초)")]
    public float animSpeedDamp = 0.15f;

    // ────────────── 내부 상태 ──────────────
    CharacterController cc;
    Vector3 grav;                      // 중력
    Vector3 targetSmoothed;            // reactionLag 보간용
    AudioSource walkSrc, runSrc;
    const float G = -9.81f;

    void Reset()
    {
        cc = GetComponent<CharacterController>();
    }

    void Awake()
    {
        cc = GetComponent<CharacterController>();
        if (!animator) animator = GetComponentInChildren<Animator>();
        if (target) targetSmoothed = target.position;

        // 오디오 소스 2개 준비(걷기/달리기), 루프는 항상 재생하고 볼륨만 제어
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
        s.volume = 0f;
        s.pitch = pitch;
        s.dopplerLevel = 0f;
        s.spatialBlend = spatial ? 1f : 0f;   // 0=2D, 1=3D
        s.minDistance = minDistance;
        s.maxDistance = maxDistance;
        if (clip) s.Play();                    // 항상 재생
        return s;
    }

    void Update()
    {
        if (!target) return;

        // 타깃 위치 지연(사람같은 둔탁함)
        targetSmoothed = Vector3.Lerp(targetSmoothed, target.position,
                                      Time.deltaTime / Mathf.Max(0.01f, reactionLag));

        // 수평 방향/거리
        Vector3 toPlayer = target.position - transform.position; toPlayer.y = 0f;
        float distToPlayer = toPlayer.magnitude;
        Vector3 dirToPlayer = distToPlayer > 0.0001f ? toPlayer / distToPlayer : Vector3.zero;

        // 쳐다보는지(옵션)
        bool playerLooking = false;
        if (freezeWhenLookedAt && head)
        {
            float ang = Vector3.Angle(head.forward, (transform.position - head.position));
            if (ang <= lookStopAngle)
            {
                if (!Physics.Linecast(head.position, transform.position, out var hit, losMask) ||
                    hit.transform == transform) playerLooking = true;
            }
        }

        // 회전(항상 플레이어 쪽으로)
        if (dirToPlayer != Vector3.zero)
        {
            var look = Quaternion.LookRotation(dirToPlayer);
            transform.rotation = Quaternion.Slerp(transform.rotation, look, turnSpeed * Time.deltaTime);
        }

        // 목표 지점(플레이어 뒤쪽 + 좌우 살짝 흔들림)
        Vector3 fwd = Vector3.ProjectOnPlane(target.forward, Vector3.up).normalized;
        Vector3 right = Vector3.Cross(Vector3.up, fwd).normalized;
        Vector3 desiredPoint = targetSmoothed;
        if (stayBehind)
        {
            desiredPoint -= fwd * behindOffset;
            desiredPoint += right * Mathf.Sin(Time.time * jitterSpeed) * lateralJitter;
        }

        Vector3 toGoal = desiredPoint - transform.position; toGoal.y = 0f;
        float distToGoal = toGoal.magnitude;

        // 추격 여부(히스테리시스)
        bool shouldChase =
            (distToPlayer > followDistance + 0.1f) || // 살짝 여유
            (distToPlayer > resumeDistance);          // 확실히 멀어짐

        // 이동 속도 스케일(멀면 빠르게, 가까우면 감속)
        float t = Mathf.InverseLerp(followDistance + softZone, resumeDistance, distToPlayer);
        float speedScale = Mathf.Clamp01(t);
        if (distToPlayer > resumeDistance) speedScale *= catchupMultiplier;

        Vector3 move = Vector3.zero;
        if (!playerLooking && shouldChase && distToGoal > 0.05f)
        {
            float speedNow = moveSpeed * Mathf.Max(0.2f, speedScale); // 바닥값으로 너무 0에 가깝지 않게
            move = toGoal.normalized * speedNow;
        }

        // 중력
        if (cc.isGrounded && grav.y < 0f) grav.y = -2f;
        grav.y += G * Time.deltaTime;

        // 실제 이동
        cc.Move((move + grav) * Time.deltaTime);

        // 애니메이션 Speed(수평 속도 기준) + 댐핑
        float horizSpeed = new Vector2(cc.velocity.x, cc.velocity.z).magnitude;
        if (animator) animator.SetFloat("Speed", horizSpeed, animSpeedDamp, Time.deltaTime);

        // 발소리 볼륨(항상 재생 중인 루프의 볼륨만 제어)
        bool isMoving = horizSpeed > minSpeedForAudio && !playerLooking;
        bool catchingUp = distToPlayer > resumeDistance || horizSpeed >= runSpeedThreshold;

        float walkTarget = (isMoving && !catchingUp && walkLoop) ? walkMaxVolume : 0f;
        float runTarget = (isMoving && catchingUp && runLoop) ? runMaxVolume : 0f;

        if (walkSrc) walkSrc.volume = Mathf.MoveTowards(walkSrc.volume, walkTarget, volumeFade * Time.deltaTime);
        if (runSrc) runSrc.volume = Mathf.MoveTowards(runSrc.volume, runTarget, volumeFade * Time.deltaTime);
    }

    // 에디터에서 거리 링 보기
    void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(0f, 1f, 0f, 0.25f);
        Gizmos.DrawWireSphere(transform.position, followDistance);
        Gizmos.color = new Color(1f, 0.6f, 0f, 0.25f);
        Gizmos.DrawWireSphere(transform.position, resumeDistance);
    }

    void OnValidate()
    {
        resumeDistance = Mathf.Max(resumeDistance, followDistance + 0.2f);
        softZone = Mathf.Max(0f, softZone);
        catchupMultiplier = Mathf.Max(1f, catchupMultiplier);
        if (minDistance < 0.1f) minDistance = 0.1f;
        if (maxDistance <= minDistance) maxDistance = minDistance + 0.1f;
    }
}