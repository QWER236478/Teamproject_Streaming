using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class Mannequin_Chaser : MonoBehaviour
{
    [Header("Ÿ��(�÷��̾�)")]
    public Transform target;               // XR Origin(�÷��̾�)
    public Transform head;                 // (�ɼ�) �÷��̾� ī�޶�
    public Animator animator;

    [Header("������")]
    public float moveSpeed = 1.6f;
    public float turnSpeed = 8f;
    public float stopDistance = 1.8f;
    public bool freezeWhenLookedAt = false;
    public float lookStopAngle = 25f;
    public LayerMask losMask = ~0;

    [Header("Walk SFX (�׻� ��� + ���� ���̵�)")]
    public AudioSource walkSource;         // ����θ� �ڵ� ����
    public AudioClip walkClip;             // �ȱ� ���� ����
    [Range(0f, 1f)] public float walkVolume = 1f;
    public float fadeSpeed = 10f;          // ���� ��ȭ �ӵ� (1/sec)
    public float speedThreshold = 0.12f;   // �� �̻��̸� "�ȴ� ��"���� ����
    public bool use3D = false;             // ���� false(2D)�� Ȯ�� �� true(3D) ����
    public float minDistance = 6f;         // 3D�� ���� �ǹ� ����
    public float maxDistance = 24f;

    CharacterController cc;
    Vector3 vel;
    const float gravity = -9.81f;

    // Animator �Ķ���� ������(��� Ʀ ����)
    public float animSpeedDamp = 0.15f;

    void Awake()
    {
        cc = GetComponent<CharacterController>();
        if (!animator) animator = GetComponentInChildren<Animator>();

        // �ȱ� ����� �ҽ� �غ� �� ���� ����(���� 0)
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

        // --- Ÿ�� ����/�Ÿ�(����) ---
        Vector3 to = target.position - transform.position;
        to.y = 0f;
        float dist = to.magnitude;
        Vector3 dir = (dist > 0.0001f) ? to / dist : Vector3.zero;

        // --- �Ĵٺ��� ���̸� ����(�ɼ�) ---
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

        // --- ȸ�� ---
        if (dir != Vector3.zero)
        {
            Quaternion look = Quaternion.LookRotation(dir);
            transform.rotation = Quaternion.Slerp(transform.rotation, look, turnSpeed * Time.deltaTime);
        }

        // --- �̵� + �ּ� �Ÿ� ---
        Vector3 move = Vector3.zero;
        if (dist > stopDistance && !playerLooking)
            move = dir * moveSpeed;

        // --- �߷� ---
        if (cc.isGrounded && vel.y < 0f) vel.y = -2f;
        vel.y += gravity * Time.deltaTime;

        cc.Move((move + vel) * Time.deltaTime);

        // --- �ִϸ��̼� Speed �Ķ����(������ ����) ---
        float horizSpeed = new Vector2(cc.velocity.x, cc.velocity.z).magnitude;
        if (animator) animator.SetFloat("Speed", horizSpeed, animSpeedDamp, Time.deltaTime);

        // --- �ȱ� ����: �׻� ��� + ������ ���̵� ---
        if (walkClip)
        {
            if (!walkSource.isPlaying) walkSource.Play(); // ������ġ
            float targetVol = (horizSpeed > speedThreshold && !playerLooking) ? walkVolume : 0f;
            targetVol = Mathf.Clamp01(targetVol);
            walkSource.volume = Mathf.MoveTowards(walkSource.volume, targetVol, fadeSpeed * Time.deltaTime);
        }
    }
}