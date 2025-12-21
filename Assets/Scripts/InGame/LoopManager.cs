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

    [Header("보스 스테이지 및 엔딩 설정")]
    public int loopsToDisableBoss = 1; // 이 횟수 이상이면 보스 문 닫힘 & 엔딩 손잡이 활성화

    // 숨길 오브젝트 리스트 (예: 보스 스테이지로 가는 문)
    public List<GameObject> objectsToHide = new List<GameObject>();

    // [추가됨] 엔딩 손잡이 오브젝트 (처음엔 콜라이더가 꺼져있다가 나중에 켜짐)
    public GameObject endingHandleObject;

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

            // 루프 사이클 1회 완료 시점
            if (currentIndex >= corridorOrder.Length)
            {
                currentIndex = 0;
                loopCount++;

                Debug.Log($"[LoopManager] 루프 완료. 총 루프 수: {loopCount}");

                // 조건: 일정 루프 횟수(loopsToDisableBoss) 이상 돌았을 때
                if (loopCount >= loopsToDisableBoss)
                {
                    // 1. 보스 트리거 활성화
                    if (bossStartTrigger)
                        bossStartTrigger.SetActive(true);

                    // 2. 등록된 오브젝트들 숨기기 (Objects To Hide)
                    foreach (GameObject obj in objectsToHide)
                    {
                        if (obj != null)
                        {
                            obj.SetActive(false);
                        }
                    }
                    Debug.Log($"[LoopManager] 등록된 {objectsToHide.Count}개의 오브젝트 비활성화 완료");

                    // 3. [핵심] 엔딩 손잡이의 박스 콜라이더 켜기
                    if (endingHandleObject != null)
                    {
                        Collider col = endingHandleObject.GetComponent<Collider>();
                        if (col != null)
                        {
                            col.enabled = true; // 이제 플레이어가 상호작용 가능해짐
                            Debug.Log("[LoopManager] 엔딩 손잡이 상호작용 활성화됨!");
                        }
                        else
                        {
                            Debug.LogError("[LoopManager] 엔딩 손잡이 오브젝트에 Collider가 없습니다!");
                        }
                    }
                }

                // 4. 이상현상(공포 이벤트) 트리거 처리
                TriggerAnomalies();
            }
        }
        else
        {
            // 잘못 된 문을 열면 무시함
            Debug.Log($"[LoopManager] 잘못된 순서: {corridorID}, 기대: {corridorOrder[currentIndex]}");
        }
    }

    // 이상현상 실행 로직
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

        // 이미 있는 자식 오브젝트 켜기
        if (a.existingChild)
        {
            a.existingChild.SetActive(true);
            yield break;
        }

        // 프리팹 생성하기
        if (a.prefab)
        {
            var pos = a.spawnPoint ? a.spawnPoint.position : transform.position;
            var rot = a.spawnPoint ? a.spawnPoint.rotation : transform.rotation;
            Instantiate(a.prefab, pos, rot);
        }
    }
}