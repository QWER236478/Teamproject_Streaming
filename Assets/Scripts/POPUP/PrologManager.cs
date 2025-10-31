using System.Collections;
using UnityEngine;
using TMPro;

[System.Serializable]
public class TypeEntry
{
    public TMP_Text targetText; // 출력 대상 TMP
    [TextArea(3, 10)] public string content; // 출력할 문장
}

public class PrologManager : MonoBehaviour
{
    [Header("타이핑 항목(등록 한 순서대로 실행)")]
    public TypeEntry[] entries; //타이핑 할 문장 종류 

    [Header("설정")]
    [Tooltip("초당 글자 수(CPS) 기준. 0이면 즉시 표시")]
    public float charsPerSecond = 25f;          // = 1/typeSpeed
    public float pauseBetweenEntries = 0.5f;    // 항목 간 대기 (초)
    public bool useUnscaledTime = false;        // 로딩/멈춤 중에도 타이핑 유지할지

    [Header("루프형 타자 사운드(선택)")]
    public AudioSource typeSource;              // 긴 루프 클립 권장(키보드 타자 소리 등)
    public bool useLoopSound = true;

    private bool isTyping;
    public bool IsTyping() => isTyping;

    private Coroutine runCo;

    // 캐시용 대기객체(할당 줄이기 사유 - 씬 이동할 때 렉이걸리면 타자기 처럼 입력되는 듯한 연출이 렉이걸려 제대로 작동하지 않기때문)
    WaitForSeconds waitBetweenEntries;
    float secPerChar => (charsPerSecond <= 0f ? 0f : 1f / charsPerSecond);

    void OnEnable()
    {
        StartTyping();
    }

    public void StartTyping()
    {
        if (runCo != null) StopCoroutine(runCo);

        // pauseBetweenEntries용 Wait 캐시
        waitBetweenEntries = pauseBetweenEntries > 0f ? new WaitForSeconds(pauseBetweenEntries) : null;

        runCo = StartCoroutine(TypeAll());
    }

    IEnumerator TypeAll() 
    {
        isTyping = true;

        // 0) 초기화 + 한 번에 세팅(문자열 재할당 방지)
        for (int i = 0; i < entries.Length; i++)
        {
            var e = entries[i];
            if (e != null && e.targetText)
            {
                e.targetText.maxVisibleCharacters = 0;
                e.targetText.SetText(e.content);     // 한 번만 세팅
                e.targetText.ForceMeshUpdate();      // textInfo 계산
            }
        }

        // 1) 루프 사운드 시작
        if (useLoopSound && typeSource && !typeSource.isPlaying)
        {
            typeSource.loop = true;
            typeSource.Play();
        }

        // 2) 각 항목 타이핑
        for (int i = 0; i < entries.Length; i++)
        {
            var e = entries[i];
            if (e == null || e.targetText == null) continue;

            var tmp = e.targetText;
            // 총 글자수 (공백/리치텍스트 포함한 실제 렌더링 캐릭터 수)
            int total = tmp.textInfo.characterCount;
            int visible = 0;

            if (charsPerSecond <= 0f) // 즉시 표시 모드
            {
                tmp.maxVisibleCharacters = total;
            }
            else
            {
                // per-char 대기 방식을 할당 없이 처리
                float perChar = secPerChar;
                float acc = 0f;

                while (visible < total)
                {
                    // 시간 누적
                    acc += useUnscaledTime ? Time.unscaledDeltaTime : Time.deltaTime;

                    // 누적 시간이 글자 간격을 넘을 때마다 여러 글자 한꺼번에 드러내기(저사양 보호)
                    while (acc >= perChar && visible < total)
                    {
                        acc -= perChar;
                        visible++;
                        tmp.maxVisibleCharacters = visible;
                    }

                    yield return null; // 매 프레임 한 번만 yield
                }
            }

            // 항목 사이 잠깐 쉬기
            if (waitBetweenEntries != null)
                yield return waitBetweenEntries;
        }

        // 3) 사운드 종료
        if (useLoopSound && typeSource)
        {
            typeSource.loop = false;
            typeSource.Stop();
        }

        isTyping = false;
        runCo = null;
    }
}