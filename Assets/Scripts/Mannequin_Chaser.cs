using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class Mannequin_Chaser : MonoBehaviour
{
    [Header("타겟(플레이어)")]
    public Transform target;               // XR Origin(플레이어)
    public Transform head;                 // (옵션) 플레이어 카메라
    public Animator animator;

    [Header("추적자")]
    public float moveSpeed = 1.6f;
    public float turnSpeed = 8f;
    public float stopDistance = 1.8f;
    public bool freezeWhenLookedAt = false;
    public float lookStopAngle = 25f;
    public LayerMask losMask = ~0;

    [Header("Walk SFX (항상 재생 + 볼륨 페이드)")]
    public AudioSource walkSource;         // 비워두면 자동 생성
    public AudioClip walkClip;             // 걷기 루프 사운드
    [Range(0f, 1f)] public float walkVolume = 1f;
    public float fadeSpeed = 10f;          // 볼륨 변화 속도 (1/sec)
    public float speedThreshold = 0.12f;   // 이 이상이면 "걷는 중"으로 간주
    public bool use3D = false;             // 먼저 false(2D)로 확인 후 true(3D) 권장
    public float minDistance = 6f;         // 3D일 때만 의미 있음
    public float maxDistance = 24f;

    CharacterController cc;
    Vector3 vel;
    const float gravity = -9.81f;

    // Animator 파라미터 스무딩(토글 튐 방지)
    public float animSpeedDamp = 0.15f;

    void Awake()
    {
        cc = GetComponent<CharacterController>();
        if (!animator) animator = GetComponentInChildren<Animator>();

        // 걷기 오디오 소스 준비 및 루프 시작(볼륨 0)
        if (!walkSource) walkSource = GetComponent<AudioSource>();
        if (!walkSource) walkSource = gameObject.AddComponent<AudioSource>();

        walkSource.playOnAwake = false;
        walkSource.loop = true;
        walkSource.spatialBlend = use3D ? 1f : 0f;
        walkSource.minDistance = minDistance;
        walkSource.maxDistance = maxDistance;
        walkSource.dopplerLevel = 0f;
        walkSource.volume = 0f;

        if (walkClip) walkSource.clip = walkClip;
        if (walkClip && !walkSource.isPlaying) walkSource.Play();
    }

    void Update()
    {
        if (!target) return;

        // --- 타깃 방향/거리(수평) ---
        Vector3 to = target.position - transform.position;
        to.y = 0f;
        float dist = to.magnitude;
        Vector3 dir = (dist > 0.0001f) ? to / dist : Vector3.zero;

        // --- 쳐다보는 중이면 멈춤(옵션) ---
        bool playerLooking = false;
        if (freezeWhenLookedAt && head)
        {
            float ang = Vector3.Angle(head.forward, (transform.position - head.position));
            if (ang <= lookStopAngle)
            {
                if (!Physics.Linecast(head.position, transform.position, out var hit, losMask) ||
                    hit.transform == transform)
                    playerLooking = true;
            }
        }

        // --- 회전 ---
        if (dir != Vector3.zero)
        {
            Quaternion look = Quaternion.LookRotation(dir);
            transform.rotation = Quaternion.Slerp(transform.rotation, look, turnSpeed * Time.deltaTime);
        }

        // --- 이동 + 최소 거리 ---
        Vector3 move = Vector3.zero;
        if (dist > stopDistance && !playerLooking)
            move = dir * moveSpeed;

        // --- 중력 ---
        if (cc.isGrounded && vel.y < 0f) vel.y = -2f;
        vel.y += gravity * Time.deltaTime;

        cc.Move((move + vel) * Time.deltaTime);

        // --- 애니메이션 Speed 파라미터(스무딩 적용) ---
        float horizSpeed = new Vector2(cc.velocity.x, cc.velocity.z).magnitude;
        if (animator) animator.SetFloat("Speed", horizSpeed, animSpeedDamp, Time.deltaTime);

        // --- 걷기 사운드: 항상 재생 + 볼륨만 페이드 ---
        if (walkClip)
        {
            if (!walkSource.isPlaying) walkSource.Play(); // 안전장치
            float targetVol = (horizSpeed > speedThreshold && !playerLooking) ? walkVolume : 0f;
            targetVol = Mathf.Clamp01(targetVol);
            walkSource.volume = Mathf.MoveTowards(walkSource.volume, targetVol, fadeSpeed * Time.deltaTime);
        }
    }
}