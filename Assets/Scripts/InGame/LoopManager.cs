using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LoopManager : MonoBehaviour
{
    [Header("루프 진행")]
    public string[] corridorOrder = { "C", "D", "A", "B" };  // 진행 순서
    private int currentIndex = 0;                            // 현재 기대하는 복도 인덱스
    public int loopCount = 0;                                // 완성된 루프 수

    [Header("보스 스테이지")]
    public int loopsToDisableBoss = 2;
    public GameObject bossStage;

    [Header("이상현상 스케줄")]
    public List<AnomalyEntry> anomalies = new List<AnomalyEntry>();

    [Header("보스 시작 트리거")]
    public GameObject bossStartTrigger;   // BossStartTrigger 오브젝트 (초기 비활성)


    [System.Serializable]
    public class AnomalyEntry
    {
        public string label = "event";
        public Transform spawnPoint;
        public GameObject prefab;
        public GameObject existingChild;
        public float delay = 2f;
        public int triggerOnLoop = 1;
        public bool onlyOnce = true;
        [HideInInspector] public bool done;
    }

    // 호출: DoorHandle이 해당 복도를 통과했을 때
    public void OnCorridorPassed(string corridorID)
    {
        if (string.IsNullOrEmpty(corridorID)) return;

        // 현재 기대하는 복도인가?
        if (corridorID == corridorOrder[currentIndex])
        {
            currentIndex++;

            // 루프 완료 시점
            if (currentIndex >= corridorOrder.Length)
            {
                currentIndex = 0;
                loopCount++;

                if (bossStartTrigger && loopCount >= loopsToDisableBoss)
                    bossStartTrigger.SetActive(true);

                Debug.Log($"[LoopManager] 루프 완료. 총 루프 수: {loopCount}");

                if (bossStage != null && loopCount >= loopsToDisableBoss)
                {
                    bossStage.SetActive(false);
                    Debug.Log("[LoopManager] Boss Stage 비활성화");
                }

                // 이상현상 트리거 처리
                TriggerAnomalies();
            }
        }
        else
        {
            // 잘못 된 문을 열면 무시함
            Debug.Log($"[LoopManager] 잘못된 순서: {corridorID}, 기대: {corridorOrder[currentIndex]}");
        }
    }

    void TriggerAnomalies()
    {
        foreach (var a in anomalies)
        {
            if (a == null) continue;
            if (a.onlyOnce && a.done) continue;
            if (a.triggerOnLoop != loopCount) continue;

            StartCoroutine(TriggerAnomaly(a));
        }
    }

    IEnumerator TriggerAnomaly(AnomalyEntry a)
    {
        a.done = true;
        yield return new WaitForSeconds(a.delay);

        if (a.existingChild)
        {
            a.existingChild.SetActive(true);
            yield break;
        }

        if (a.prefab)
        {
            var pos = a.spawnPoint ? a.spawnPoint.position : transform.position;
            var rot = a.spawnPoint ? a.spawnPoint.rotation : transform.rotation;
            Instantiate(a.prefab, pos, rot);
        }
    }
}