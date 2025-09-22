using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(CharacterController))]
public class PlayerWalkingAudio : MonoBehaviour
{
    [Header("Ŭ��")]
    [Tooltip("�ȱ� ���� �����Ŭ��")]
    public AudioClip walkLoop;
    [Tooltip("�޸��� ���� �����Ŭ��")]
    public AudioClip runLoop;

    [Header("����/���̵�")]
    [Range(0, 1)] public float walkMaxVolume = 0.8f;
    [Range(0, 1)] public float runMaxVolume = 1.0f;
    [Tooltip("���� ��ȭ �ӵ�(1/sec)")]
    public float fadeSpeed = 10f;

    [Header("��� ����(�ӵ� ����)")]
    [Tooltip("�� �ӵ� ���Ͽ��� ����(������ ��鸲 ����)")]
    public float minSpeed = 0.12f;
    [Tooltip("�� �ӵ� �̻��̸� �޸���� ����")]
    public float runSpeedThreshold = 2.0f;

    [Header("��� ����(�Է� ����, ����)")]
    [Tooltip("�޸��� �Է�(��: XRI LeftHand/Select �Ǵ� RightHand/PrimaryButton)")]
    public InputActionProperty runAction; // ��ư/Ʈ����(0~1) ���
    [Tooltip("runAction ���� �� �̻��̸� �޸��� ������ ����")]
    [Range(0, 1)] public float runPressThreshold = 0.5f;

    [Header("3D ����")]
    [Tooltip("ó���� 2D�� Ȯ�� �� OK�� 3D�� ��ȯ ����")]
    public bool use3D = false;
    public float minDistance = 6f;
    public float maxDistance = 24f;

    [Header("��ġ(����)")]
    [Tooltip("�޸����� �� ���� �� ���� ��ġ��")]
    public float runPitch = 1.05f;
    public float walkPitch = 1.0f;

    CharacterController cc;
    AudioSource walkSrc, runSrc;

    void Awake()
    {
        cc = GetComponent<CharacterController>();

        // �ȱ� �ҽ�
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

        // �޸��� �ҽ�
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
        // ���� �ӵ� ���
        Vector3 v = cc ? cc.velocity : Vector3.zero;
        v.y = 0f;
        float speed = v.magnitude;

        // �Է¿� ���� ����(����)
        bool runPressed = false;
        if (runAction.reference != null)
        {
            float val = 0f;
            // ��ư/Ʈ����/�� ��� Ŀ��
            try { val = runAction.action.ReadValue<float>(); } catch { }
            runPressed = val >= runPressThreshold;
        }

        // ���� ����
        bool isMoving = speed > minSpeed;
        bool isRunning = (speed >= runSpeedThreshold) || runPressed;

        // Ÿ�� ����
        float walkTarget = (isMoving && !isRunning && walkLoop) ? walkMaxVolume : 0f;
        float runTarget = (isMoving && isRunning && runLoop) ? runMaxVolume : 0f;

        // ũ�ν����̵�
        walkSrc.volume = Mathf.MoveTowards(walkSrc.volume, walkTarget, fadeSpeed * Time.deltaTime);
        runSrc.volume = Mathf.MoveTowards(runSrc.volume, runTarget, fadeSpeed * Time.deltaTime);

        // ������ġ: �������� �ٽ� ���
        if (walkLoop && !walkSrc.isPlaying && walkTarget > 0f) walkSrc.Play();
        if (runLoop && !runSrc.isPlaying && runTarget > 0f) runSrc.Play();

        // �ʿ� �� ��ġ ����(�ӵ� ��� �̼� ���� ���ϸ� �Ʒ� �ּ� ����)
        // walkSrc.pitch = Mathf.Lerp(0.95f, 1.05f, Mathf.InverseLerp(minSpeed, runSpeedThreshold, speed));
        // runSrc.pitch  = Mathf.Lerp(1.0f,  1.15f, Mathf.InverseLerp(runSpeedThreshold, runSpeedThreshold*2f, speed));
    }
}