using UnityEngine;

[DisallowMultipleComponent]
[RequireComponent(typeof(CharacterController))]
public class MannequinChaser : MonoBehaviour
{
    // ���������������������������� Ÿ��/�ִϸ����� ����������������������������
    [Header("Ÿ��(�÷��̾�)")]
    [Tooltip("�÷��̾��� Transform (XR Origin ��ü)")]
    public Transform target;
    [Tooltip("�÷��̾��� �Ӹ�ī�޶�(�Ĵٺ��� ���� �ɼǿ�)")]
    public Transform head;
    [Tooltip("�� ���� Animator")]
    public Animator animator;

    // ���������������������������� �⺻ �̵�/�ü�/�þ� ����������������������������
    [Header("�⺻ �̵�/�ü�")]
    [Tooltip("�⺻ �̵� �ӵ�(m/s)")]
    public float moveSpeed = 1.8f;
    [Tooltip("ȸ�� �ε巯��(Ŭ���� ������ ȸ��)")]
    public float turnSpeed = 8f;

    [Header("�Ĵٺ��� ����(�ɼ�)")]
    public bool freezeWhenLookedAt = false;
    [Tooltip("�÷��̾� ���� ������ �̳����� ���� ���� ����")]
    public float lookStopAngle = 25f;
    [Tooltip("�þ� ����(�� ��) ���̾��ũ")]
    public LayerMask losMask = ~0;

    // ���������������������������� ���ѱ�� ������ Ʃ�� ����������������������������
    [Header("�߰� ����(�����ϰ� ����)")]
    [Tooltip("�����ϰ� ���� �Ÿ�(�����뿡�� ����/���ο�)")]
    public float followDistance = 3.0f;
    [Tooltip("�� �Ÿ����� �־����� Ȯ���� �ٽ� �߰�")]
    public float resumeDistance = 4.5f;
    [Tooltip("��ǥ �Ÿ� ��ó ���� ����")]
    public float softZone = 0.8f;
    [Tooltip("�ָ� �������� �� ���� ���(ĳġ��)")]
    public float catchupMultiplier = 1.6f;
    [Tooltip("�÷��̾� �����ӿ� ���� ���� ����(��)")]
    public float reactionLag = 0.25f;

    [Header("���� ������ ����(�ڿ��� �������)")]
    public bool stayBehind = true;
    [Tooltip("�÷��̾� �ڷ� �󸶳� ��������")]
    public float behindOffset = 2.0f;
    [Tooltip("�¿� ��¦ ��鸲 ��(0�̸� ��)")]
    public float lateralJitter = 0.25f;
    [Tooltip("��鸲 �ӵ�")]
    public float jitterSpeed = 0.7f;

    // ���������������������������� �߼Ҹ�(���� + ũ�ν����̵�) ����������������������������
    [Header("�߼Ҹ� ����(�׻� ��� + ���� ���̵�)")]
    [Tooltip("�ȱ� ���� Ŭ��")]
    public AudioClip walkLoop;
    [Tooltip("�޸���/ĳġ�� ���� Ŭ��")]
    public AudioClip runLoop;
    [Range(0, 1)] public float walkMaxVolume = 0.8f;
    [Range(0, 1)] public float runMaxVolume = 1.0f;
    [Tooltip("���� ��ȭ �ӵ�(1/sec)")]
    public float volumeFade = 10f;
    [Tooltip("�� �ӵ� ���Ͽ��� ����(�̼� ���� ����)")]
    public float minSpeedForAudio = 0.05f;
    [Tooltip("���� �ӵ��� �� ���� �Ѱų�, ĳġ�� ���¸� �޸��� ������ ��ȯ")]
    public float runSpeedThreshold = 2.0f;
    [Header("����� 3D ����(ó���� 2D�� Ȯ�� ����)")]
    public bool use3D = false;
    public float minDistance = 6f;
    public float maxDistance = 24f;
    public float walkPitch = 1.00f;
    public float runPitch = 1.05f;

    // ���������������������������� �ִ� �Ķ���� ����������������������������
    [Header("�ִϸ��̼� �Ķ����")]
    [Tooltip("Animator.SetFloat(\"Speed\")�� ������ ����(��)")]
    public float animSpeedDamp = 0.15f;

    // ���������������������������� ���� ���� ����������������������������
    CharacterController cc;
    Vector3 grav;                      // �߷�
    Vector3 targetSmoothed;            // reactionLag ������
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

        // ����� �ҽ� 2�� �غ�(�ȱ�/�޸���), ������ �׻� ����ϰ� ������ ����
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
        if (clip) s.Play();                    // �׻� ���
        return s;
    }

    void Update()
    {
        if (!target) return;

        // Ÿ�� ��ġ ����(������� ��Ź��)
        targetSmoothed = Vector3.Lerp(targetSmoothed, target.position,
                                      Time.deltaTime / Mathf.Max(0.01f, reactionLag));

        // ���� ����/�Ÿ�
        Vector3 toPlayer = target.position - transform.position; toPlayer.y = 0f;
        float distToPlayer = toPlayer.magnitude;
        Vector3 dirToPlayer = distToPlayer > 0.0001f ? toPlayer / distToPlayer : Vector3.zero;

        // �Ĵٺ�����(�ɼ�)
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

        // ȸ��(�׻� �÷��̾� ������)
        if (dirToPlayer != Vector3.zero)
        {
            var look = Quaternion.LookRotation(dirToPlayer);
            transform.rotation = Quaternion.Slerp(transform.rotation, look, turnSpeed * Time.deltaTime);
        }

        // ��ǥ ����(�÷��̾� ���� + �¿� ��¦ ��鸲)
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

        // �߰� ����(�����׸��ý�)
        bool shouldChase =
            (distToPlayer > followDistance + 0.1f) || // ��¦ ����
            (distToPlayer > resumeDistance);          // Ȯ���� �־���

        // �̵� �ӵ� ������(�ָ� ������, ������ ����)
        float t = Mathf.InverseLerp(followDistance + softZone, resumeDistance, distToPlayer);
        float speedScale = Mathf.Clamp01(t);
        if (distToPlayer > resumeDistance) speedScale *= catchupMultiplier;

        Vector3 move = Vector3.zero;
        if (!playerLooking && shouldChase && distToGoal > 0.05f)
        {
            float speedNow = moveSpeed * Mathf.Max(0.2f, speedScale); // �ٴڰ����� �ʹ� 0�� ������ �ʰ�
            move = toGoal.normalized * speedNow;
        }

        // �߷�
        if (cc.isGrounded && grav.y < 0f) grav.y = -2f;
        grav.y += G * Time.deltaTime;

        // ���� �̵�
        cc.Move((move + grav) * Time.deltaTime);

        // �ִϸ��̼� Speed(���� �ӵ� ����) + ����
        float horizSpeed = new Vector2(cc.velocity.x, cc.velocity.z).magnitude;
        if (animator) animator.SetFloat("Speed", horizSpeed, animSpeedDamp, Time.deltaTime);

        // �߼Ҹ� ����(�׻� ��� ���� ������ ������ ����)
        bool isMoving = horizSpeed > minSpeedForAudio && !playerLooking;
        bool catchingUp = distToPlayer > resumeDistance || horizSpeed >= runSpeedThreshold;

        float walkTarget = (isMoving && !catchingUp && walkLoop) ? walkMaxVolume : 0f;
        float runTarget = (isMoving && catchingUp && runLoop) ? runMaxVolume : 0f;

        if (walkSrc) walkSrc.volume = Mathf.MoveTowards(walkSrc.volume, walkTarget, volumeFade * Time.deltaTime);
        if (runSrc) runSrc.volume = Mathf.MoveTowards(runSrc.volume, runTarget, volumeFade * Time.deltaTime);
    }

    // �����Ϳ��� �Ÿ� �� ����
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