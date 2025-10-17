using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LoopManager : MonoBehaviour
{
    [Header("복도 프리팹")]
    public GameObject[] corridors;

    [Header("시작 복도 (A)")]
    public GameObject startCorridor;

    [Header("유지할 복도 수")]
    public int maxKeepCount = 2;

    private List<GameObject> activeCorridors = new List<GameObject>();
    private int nextIndex = 1; // A 다음은 B부터 시작

    //중복 생성 방지용 쿨다운
    private bool isSpawning = false;
    private float spawnCooldown = 0.4f; // 0.4초간 추가 생성 금지

    void Start()
    {
        if (startCorridor != null)
            activeCorridors.Add(startCorridor);
    }

    public void SpawnNext()
    {
        if (isSpawning) return; // 쿨다운 중이면 무시
        StartCoroutine(SpawnNextRoutine());
    }

    private IEnumerator SpawnNextRoutine()
    {
        isSpawning = true;

        // 1. 다음 복도 프리팹 결정
        GameObject prefab = corridors[nextIndex];

        // 2. 프리팹 원래 Transform 값 그대로 생성
        GameObject newCorridor = Instantiate(prefab);
        activeCorridors.Add(newCorridor);

        // 3. 오래된 복도 삭제
        if (activeCorridors.Count > maxKeepCount)
        {
            Destroy(activeCorridors[0]);
            activeCorridors.RemoveAt(0);
        }

        // 4. 인덱스 순환
        nextIndex = (nextIndex + 1) % corridors.Length;

        // 5. 잠깐 대기 후 다시 스폰 허용
        yield return new WaitForSeconds(spawnCooldown);
        isSpawning = false;
    }
}