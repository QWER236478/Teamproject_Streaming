using System.Collections;
using UnityEngine;

public class AnomalyController : MonoBehaviour
{
    [Header("이상현상 소스")]
    public GameObject anomalyPrefab;      // 인스턴스 생성용(선택)
    public Transform spawnPoint;          // 생성 위치(없으면 자기 Transform)
    public bool oneShot = true;           // 한 번만 발동
    public bool useExistingChild = false; // 프리팹 안의 비활성 자식 사용 여부
    public GameObject existingChild;      // 비활성 자식 레퍼런스(선택)

    private bool _triggered;

    /// <summary>
    /// delay초 후 이상현상 발동
    /// </summary>
    public void ActivateAfter(float delay)
    {
        if (oneShot && _triggered) return;
        if (!gameObject.activeInHierarchy) return; // 파괴 직전 등 안전장치
        StartCoroutine(ActivateRoutine(delay));
    }

    private IEnumerator ActivateRoutine(float delay)
    {
        _triggered = true;
        yield return new WaitForSeconds(delay);

        if (this == null || gameObject == null) yield break; // 이미 파괴된 경우

        if (useExistingChild && existingChild != null)
        {
            existingChild.SetActive(true);
            // 자식에 IAnomalyEffect 등 붙었다면 여기서 초기화/재생 호출 가능
            // existingChild.GetComponent<IAnomalyEffect>()?.Play();
            yield break;
        }

        // 프리팹 인스턴스 생성 루트
        if (anomalyPrefab != null)
        {
            Transform parent = spawnPoint != null ? spawnPoint : transform;
            GameObject inst = Instantiate(anomalyPrefab, parent.position, parent.rotation, parent);
            // 필요 시 스케일/초기화
            // var effect = inst.GetComponent<IAnomalyEffect>(); effect?.Play();
        }
        else
        {
            Debug.LogWarning($"[AnomalyController] anomalyPrefab이 비어있습니다. ({name})");
        }
    }
}