using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.XR.Interaction.Toolkit;

[DisallowMultipleComponent]
[RequireComponent(typeof(CharacterController))]
public class PlayerDash : MonoBehaviour
{
    // ������������������ ���� ������������������
    [Header("����")]
    [Tooltip("Action-based Continuous Move Provider (���� ��ƽ �̵�)")]
    public ActionBasedContinuousMoveProvider moveProvider;
    [Tooltip("���� ������Ʈ�� �ִ� CharacterController")]
    public CharacterController cc;

    // ������������������ �Է� ������������������
    [Header("�Է� (�޼� Ʈ����)")]
    [Tooltip("XRI LeftHand / Activate (float)")]
    public InputActionProperty leftTrigger;
    [Range(0, 1f)] public float triggerThreshold = 0.75f;

    // ������������������ ������Ʈ ������������������
    [Header("������Ʈ ����")]
    [Tooltip("�⺻ �ȱ� �ӵ� (Move Provider�� Move Speed�� �����ϰ� ����)")]
    public float baseMoveSpeed = 1.4f;
    [Tooltip("Ʈ���� ���� �� ������ ���(=�޸��� ���)")]
    public float sprintMultiplier = 2.0f;
    [Tooltip("�ӵ� ��ȯ �ε巯��(Ŭ���� �ﰢ��)")]
    public float speedLerp = 12f;

    // ������������������ ����� ������������������
    [Header("�߼Ҹ� ����(�ڵ� ũ�ν����̵�)")]
    public AudioClip walkLoop;                           // �ȱ� ����
    public AudioClip runLoop;                            // �޸��� ����
    [Range(0, 1)] public float walkMaxVolume = 0.8f;
    [Range(0, 1)] public float runMaxVolume = 1.0f;
    [Tooltip("���� ���̵� �ӵ�(1/sec)")]
    public float volumeFade = 10f;
    [Tooltip("�� �ӵ����� ������ ���� ó��")]
    public float minMoveSpeed = 0.12f;

    [Header("3D ���� (ó���� 2D ����)")]
    public bool use3D = false;
    public float minDistance = 6f;
    public float maxDistance = 24f;
    public float walkPitch = 1.0f;
    public float runPitch = 1.05f;

    // ����
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
        s.volume = 0f;                    // �׻� 0���� ����, ���̵�� ����
        s.pitch = pitch;
        s.dopplerLevel = 0f;
        s.spatialBlend = spatial ? 1f : 0f;   // 0=2D, 1=3D
        s.minDistance = minDistance;
        s.maxDistance = maxDistance;
        if (clip) s.Play();               // ������ �׻� ���, ������ �ٲ�
        return s;
    }

    void Update()
    {
        // 1) ������Ʈ �Է�
        bool sprint = leftTrigger.reference &&
                      leftTrigger.action.ReadValue<float>() >= triggerThreshold;

        // 2) Move Provider �ӵ� ���� (������Ʈ ���ȸ� ���)
        if (moveProvider)
        {
            float target = baseMoveSpeed * (sprint ? sprintMultiplier : 1f);
            moveProvider.moveSpeed = Mathf.Lerp(moveProvider.moveSpeed, target,
                                                speedLerp * Time.deltaTime);
        }

        // 3) ���� ���� �ӵ�(����� ������)
        float speedXY = 0f;
        if (cc) { var v = cc.velocity; v.y = 0f; speedXY = v.magnitude; }
        bool isMoving = speedXY > minMoveSpeed;

        // 4) ����� ũ�ν����̵�
        float walkTarget = (isMoving && !sprint && walkLoop) ? walkMaxVolume : 0f;
        float runTarget = (isMoving && sprint && runLoop) ? runMaxVolume : 0f;

        if (walkSrc) walkSrc.volume = Mathf.MoveTowards(walkSrc.volume, walkTarget, volumeFade * Time.deltaTime);
        if (runSrc) runSrc.volume = Mathf.MoveTowards(runSrc.volume, runTarget, volumeFade * Time.deltaTime);

        // ������ġ: ���̵� Ÿ���� ����� ��� ����
        if (walkLoop && walkSrc && !walkSrc.isPlaying && walkTarget > 0f) walkSrc.Play();
        if (runLoop && runSrc && !runSrc.isPlaying && runTarget > 0f) runSrc.Play();
    }
}