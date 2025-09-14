using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TeleportGate : MonoBehaviour
{
    [Header("Pair")]
    public TeleportGate destination;

    [Header("Options")]
    public string playerTag = "Player";
    public bool preserveYaw = true;            // �÷��̾� ���� �ٶ󺸴� ���� ����
    public bool alignToDestinationForward = false; // �������� forward�� ������
    public float cooldown = 0.6f;              // ���� ����Ʈ ��Ʈ���� ����
    public float upOffset = 0.05f;             // ���� �Ĺ��� ���� ��¦ ����
    public bool zeroVelocityOnTeleport = true; // �����̵� �� �ӵ� �ʱ�ȭ

    [Header("Optional Fade (CanvasGroup)")]
    public CanvasGroup fade;
    public float fadeTime = 0.12f;

    Collider _col;

    // �÷��̾ ��ٿ� �ð� ����
    static readonly System.Collections.Generic.Dictionary<Transform, float> _cool = new();

    void Reset()
    {
        _col = GetComponent<Collider>();
        _col.isTrigger = true;
    }
    void Awake()
    {
        _col = GetComponent<Collider>();
        _col.isTrigger = true;
    }

    void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag(playerTag)) return;

        var root = other.attachedRigidbody ? other.attachedRigidbody.transform : other.transform;

        // ��Ʈ����(�պ� ����) ����
        if (_cool.TryGetValue(root, out var until) && Time.time < until) return;

        if (destination == null) { Debug.LogWarning($"[{name}] destination ������"); return; }

        StartCoroutine(TeleportRoutine(root));
    }

    System.Collections.IEnumerator TeleportRoutine(Transform player)
    {
        if (fade) yield return Fade(1f);

        // Rigidbody ���� ó��
        var rb = player.GetComponent<Rigidbody>();
        if (rb) rb.isKinematic = true;

        // ��ǥ ��ġ/ȸ�� ���
        Vector3 pos = destination.GetSpawnPosition();
        Quaternion rot = player.rotation;

        if (alignToDestinationForward) rot = Quaternion.LookRotation(destination.transform.forward, Vector3.up);
        else if (!preserveYaw) rot = player.rotation; // �ʿ� �� �ٸ� ��å

        // ����
        player.SetPositionAndRotation(pos, rot);

        if (rb)
        {
            if (zeroVelocityOnTeleport) rb.velocity = Vector3.zero;
            rb.isKinematic = false;
        }

        // ��ٿ�(����)
        _cool[player] = Time.time + cooldown;
        if (destination != null) _cool[player] = Time.time + cooldown; // ���� ��� ��� ���

        if (fade) yield return Fade(0f);
    }

    public Vector3 GetSpawnPosition()
    {
        // ������ ����Ʈ�� ���� �ణ ������ ��ġ + �ణ ����
        Vector3 basePos = transform.position + transform.forward * 0.25f + Vector3.up * upOffset;

        // ���� ����(������)
        if (Physics.Raycast(basePos + Vector3.up * 0.5f, Vector3.down, out var hit, 2f,
            ~0, QueryTriggerInteraction.Ignore))
        {
            basePos = hit.point + Vector3.up * upOffset;
        }
        return basePos;
    }

    System.Collections.IEnumerator Fade(float target)
    {
        float start = fade.alpha;
        float t = 0f;
        while (t < fadeTime)
        {
            t += Time.unscaledDeltaTime;
            fade.alpha = Mathf.Lerp(start, target, t / fadeTime);
            yield return null;
        }
        fade.alpha = target;
    }

#if UNITY_EDITOR
    void OnDrawGizmos()
    {
        Gizmos.color = new Color(0.2f, 0.8f, 1f, 0.6f);
        Vector3 p = GetSpawnPosition();
        Gizmos.DrawSphere(p, 0.1f);
        if (destination)
        {
            Gizmos.DrawLine(transform.position, destination.transform.position);
            Gizmos.DrawWireSphere(destination.transform.position, 0.12f);
        }
    }
#endif
}