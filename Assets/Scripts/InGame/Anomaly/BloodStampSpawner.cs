using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BloodStampAreaSpawnerWithSound : MonoBehaviour
{
    [Header("스프라이트 프리팹 (SpriteRenderer 필수)")]
    public GameObject bloodSpritePrefab;

    [Header("스폰 범위 (이 오브젝트에 붙은 BoxCollider 사용)")]
    public BoxCollider area;
    public LayerMask surfaceMask;

    [Header("찍히는 개수 / 속도")]
    public int totalCount = 40;
    public float spawnInterval = 0.3f;

    [Header("표면 옵션")]
    public bool hitLeft = true;
    public bool hitRight = true;
    public bool hitCeiling = true;
    public bool hitFloor = false;
    public float surfaceOffset = 0.01f;
    public float projectDistanceScale = 1.5f;
    public Vector2 scaleRange = new Vector2(0.8f, 1.2f);
    public Vector2 rollRange = new Vector2(-35f, 35f);

    [Header("자동 시작 여부")]
    public bool autoStart = true;

    [Header("관리 옵션")]
    public int maxActiveStamps = 50;
    public float fadeOutDuration = 0.8f;

    [Header("조명 연출")]
    public Light[] pointLights;

    [Header("사운드 설정")]
    public AudioSource heartbeatAudio;     // 두두두두두 (루프)
    public AudioSource flickerAudio;       // 깜빡이는 동안
    //public AudioSource powerOnAudio;       // 불 완전히 켜질 때

    private readonly Queue<GameObject> activeStamps = new();

    void Reset()
    {
        if (!area) area = GetComponent<BoxCollider>();
    }

    void Start()
    {
        if (!area) area = GetComponent<BoxCollider>();
        if (surfaceMask.value == 0) surfaceMask = Physics.DefaultRaycastLayers;

        if (autoStart)
            Begin();
    }

    public void Begin() => StartCoroutine(SpawnRoutine());

    IEnumerator SpawnRoutine()
    {
        //손자국 생성
        if (heartbeatAudio) heartbeatAudio.Play();

        for (int i = 0; i < totalCount; i++)
        {
            SpawnOne();
            yield return new WaitForSeconds(spawnInterval);
        }

        //손자국 사라짐
        yield return StartCoroutine(ClearAllRemainingStamps());

        //사운드 전환: 두두두 종료, 불 깜빡임 시작
        if (heartbeatAudio) heartbeatAudio.Stop();
        if (flickerAudio) flickerAudio.Play();

        //불 깜빡이며 켜짐 (색 변화 포함)
        yield return StartCoroutine(FlickerColorAndBrightenLights());

        //전원 켜짐 사운드
        if (flickerAudio) flickerAudio.Stop();
        //if (powerOnAudio) powerOnAudio.Play();
    }

    void SpawnOne()
    {
        if (!bloodSpritePrefab || !area) return;

        Vector3 half = area.size * 0.5f;
        Vector3 local = new Vector3(
            Random.Range(-half.x, half.x),
            Random.Range(-half.y, half.y),
            Random.Range(-half.z, half.z)
        ) + area.center;

        Vector3 origin = area.transform.TransformPoint(local);

        List<Vector3> dirs = new();
        if (hitLeft) dirs.Add(-area.transform.right);
        if (hitRight) dirs.Add(area.transform.right);
        if (hitCeiling) dirs.Add(area.transform.up);
        if (hitFloor) dirs.Add(-area.transform.up);
        if (dirs.Count == 0) return;

        Vector3 dir = dirs[Random.Range(0, dirs.Count)];
        float diag = area.size.magnitude;
        float maxDist = diag * projectDistanceScale;

        if (!Physics.Raycast(origin, dir, out RaycastHit hit, maxDist, surfaceMask, QueryTriggerInteraction.Ignore))
            return;

        Vector3 pos = hit.point + hit.normal * surfaceOffset;
        Quaternion face = Quaternion.LookRotation(-hit.normal, area.transform.up);
        Quaternion roll = Quaternion.AngleAxis(Random.Range(rollRange.x, rollRange.y), hit.normal);
        Quaternion rot = roll * face;

        GameObject go = Instantiate(bloodSpritePrefab, pos, rot);
        go.transform.localScale *= Random.Range(scaleRange.x, scaleRange.y);

        var sr = go.GetComponent<SpriteRenderer>();
        if (sr)
        {
            Color c = sr.color;
            c.a = Random.Range(0.65f, 1f);
            sr.color = c;
        }


        activeStamps.Enqueue(go);
        if (activeStamps.Count > maxActiveStamps)
        {
            GameObject oldest = activeStamps.Dequeue();
            if (oldest)
            {
                if (fadeOutDuration > 0) StartCoroutine(FadeAndDestroy(oldest));
                else Destroy(oldest);
            }
        }
    }

    IEnumerator ClearAllRemainingStamps()
    {
        while (activeStamps.Count > 0)
        {
            GameObject obj = activeStamps.Dequeue();
            if (obj)
            {
                if (fadeOutDuration > 0)
                    yield return StartCoroutine(FadeAndDestroy(obj));
                else
                    Destroy(obj);
            }
        }
    }

    IEnumerator FadeAndDestroy(GameObject obj)
    {
        if (!obj) yield break;

        SpriteRenderer sr = obj.GetComponent<SpriteRenderer>();
        if (!sr)
        {
            Destroy(obj);
            yield break;
        }

        float t = 0f;
        Color original = sr.color;
        while (t < fadeOutDuration)
        {
            t += Time.deltaTime;
            float a = Mathf.Lerp(original.a, 0f, t / fadeOutDuration);
            sr.color = new Color(original.r, original.g, original.b, a);
            yield return null;
        }

        Destroy(obj);
    }

    IEnumerator FlickerColorAndBrightenLights()
    {
        if (pointLights == null || pointLights.Length == 0)
            yield break;

        // 깜빡이며 붉은빛 점멸
        float flickerDuration = 2.5f;
        float flickerInterval = 0.12f;
        float elapsed = 0f;

        while (elapsed < flickerDuration)
        {
            elapsed += flickerInterval;
            foreach (var l in pointLights)
            {
                if (!l) continue;

                l.enabled = !l.enabled;

                if (l.enabled)
                {
                    l.color = Color.Lerp(
                        new Color(0.4f, 0f, 0f),
                        new Color(1f, 0.3f, 0.1f),
                        Random.value
                    );
                    l.intensity = Random.Range(0.2f, 0.8f);
                }
            }
            yield return new WaitForSeconds(flickerInterval);
        }

        // 흰색으로 서서히 전환하며 밝아짐
        foreach (var l in pointLights)
        {
            if (!l) continue;
            l.enabled = true;
            l.intensity = 0f;
        }

        float brightenTime = 2.5f;
        float t = 0f;
        while (t < brightenTime)
        {
            t += Time.deltaTime;
            float blend = t / brightenTime;
            foreach (var l in pointLights)
            {
                if (!l) continue;
                l.color = Color.Lerp(new Color(1f, 0.2f, 0.2f), Color.white, blend);
                l.intensity = Mathf.Lerp(0f, 1.5f, blend);
            }
            yield return null;
        }

        foreach (var l in pointLights)
        {
            if (!l) continue;
            l.color = Color.white;
            l.intensity = 1.5f;
        }
    }

    void OnDrawGizmosSelected()
    {
        if (!area) area = GetComponent<BoxCollider>();
        if (!area) return;
        Gizmos.color = new Color(1, 0, 0, 0.15f);
        Matrix4x4 m = Matrix4x4.TRS(area.transform.TransformPoint(area.center), area.transform.rotation, area.transform.lossyScale);
        Gizmos.matrix = m;
        Gizmos.DrawCube(Vector3.zero, area.size);
        Gizmos.matrix = Matrix4x4.identity;
    }
}