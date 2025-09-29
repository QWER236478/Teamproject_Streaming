using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TeleportGate : MonoBehaviour
{
    [Header("Pair")]
    public TeleportGate destination;

    [Header("Options")]
    public string playerTag = "Player";
    public bool preserveYaw = true;            // 플레이어 현재 바라보는 각도 유지
    public bool alignToDestinationForward = false; // 목적지의 forward로 맞출지
    public float cooldown = 0.6f;              // 양쪽 게이트 재트리거 방지
    public float upOffset = 0.05f;             // 땅에 파묻힘 방지 살짝 띄우기
    public bool zeroVelocityOnTeleport = true; // 순간이동 시 속도 초기화

    [Header("Optional Fade (CanvasGroup)")]
    public CanvasGroup fade;
    public float fadeTime = 0.12f;

    Collider _col;

    // 플레이어별 쿨다운 시간 저장
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

        // 재트리거(왕복 루프) 방지
        if (_cool.TryGetValue(root, out var until) && Time.time < until) return;

        if (destination == null) { Debug.LogWarning($"[{name}] destination 미지정"); return; }

        StartCoroutine(TeleportRoutine(root));
    }

    System.Collections.IEnumerator TeleportRoutine(Transform player)
    {
        if (fade) yield return Fade(1f);

        // Rigidbody 안전 처리
        var rb = player.GetComponent<Rigidbody>();
        if (rb) rb.isKinematic = true;

        // 목표 위치/회전 계산
        Vector3 pos = destination.GetSpawnPosition();
        Quaternion rot = player.rotation;

        if (alignToDestinationForward) rot = Quaternion.LookRotation(destination.transform.forward, Vector3.up);
        else if (!preserveYaw) rot = player.rotation; // 필요 시 다른 정책

        // 적용
        player.SetPositionAndRotation(pos, rot);

        if (rb)
        {
            if (zeroVelocityOnTeleport) rb.velocity = Vector3.zero;
            rb.isKinematic = false;
        }

        // 쿨다운(양쪽)
        _cool[player] = Time.time + cooldown;
        if (destination != null) _cool[player] = Time.time + cooldown; // 양쪽 모두 잠깐 잠금

        if (fade) yield return Fade(0f);
    }

    public Vector3 GetSpawnPosition()
    {
        // 목적지 게이트의 앞쪽 약간 떨어진 위치 + 약간 위로
        Vector3 basePos = transform.position + transform.forward * 0.25f + Vector3.up * upOffset;

        // 지면 스냅(있으면)
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