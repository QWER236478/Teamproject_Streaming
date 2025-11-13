using UnityEngine;
using UnityEngine.AI;
using UnityEngine.SceneManagement;
using System.Collections;

public class Twins : MonoBehaviour
{
    [Header("Refs")]
    public Transform target;                  // Player Transform
    public Animator animator;                 // Twins Animator
    NavMeshAgent agent;

    [Header("Chase")]
    public float runSpeed = 5f;
    public float repathInterval = 0.1f;

    [Header("Detect")]
    public float detectRadius = 8f;           // 이 안으로 들어오면 추격 시작
    public float idleSoundRadius = 15f;       // 이 안으로 들어오면 숨소리 들림 (추격 전)

    bool canChase = false;                    // 감지 후부터 추적 시작
    bool chaseSoundPlayed = false;            // 추격음 1번만 재생용

    [Header("Audio")]
    public AudioSource idleAudio;             // Idle 상태 사운드(숨소리)
    public AudioSource chaseAudio;            // 추격 시작 때만 한번 재생할 사운드

    [Header("Kill")]
    public float facePlayerSpeed = 12f;       // 회전 속도
    public float restartDelay = 2.0f;         // 암전 후 재시작까지
    public CanvasGroup fade;                  // 검은 화면 CanvasGroup
    public float fadeTime = 0.6f;
    public MonoBehaviour[] playerScriptsToDisable;

    [Header("BGM")]
    public AudioSource bgmSource;      // 씬에서 BGM 틀고 있는 AudioSource
    public AudioClip normalBGM;        // 평소 BGM (선택)
    public AudioClip chaseBGM;         // 추격 BGM
    bool bgmSwitched = false;          // 한 번만 바꾸려고 체크

    bool killing;
    float tRepath;

    void OnValidate()
    {
        // idleSoundRadius는 최소 detectRadius 이상이 되게 보정
        if (idleSoundRadius < detectRadius)
            idleSoundRadius = detectRadius;
    }

    void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        if (!target) target = GameObject.FindGameObjectWithTag("Player")?.transform;
        if (!animator) animator = GetComponent<Animator>();

        agent.autoRepath = true;
        agent.stoppingDistance = 0f;
        agent.speed = runSpeed;

        // Idle 상태로 시작
        agent.isStopped = true;
        if (animator) animator.SetBool("Chase", false);

        // 오디오 세팅
        if (idleAudio)
        {
            idleAudio.loop = true;
            idleAudio.playOnAwake = false;
        }

        if (chaseAudio)
        {
            // 추격음은 한 번만 재생할 거라 loop 끄기
            chaseAudio.loop = false;
            chaseAudio.playOnAwake = false;
        }
    }

    void Update()
    {
        if (killing || !target) return;

        float dist = Vector3.Distance(transform.position, target.position);

        // ============================
        // 1) 아직 추격 전 상태
        // ============================
        if (!canChase)
        {
            // 1-1) idleSoundRadius 밖 → 완전 정적, 아무 소리 X
            if (dist > idleSoundRadius)
            {
                agent.isStopped = true;
                if (animator) animator.SetBool("Chase", false);

                if (idleAudio && idleAudio.isPlaying) idleAudio.Stop();
                if (chaseAudio && chaseAudio.isPlaying) chaseAudio.Stop();
                return;
            }

            // 1-2) idleSoundRadius 안, detectRadius 밖 → Idle + 숨소리만
            if (dist > detectRadius)
            {
                agent.isStopped = true;
                if (animator) animator.SetBool("Chase", false);

                if (idleAudio && !idleAudio.isPlaying) idleAudio.Play();
                if (chaseAudio && chaseAudio.isPlaying) chaseAudio.Stop();
                return;
            }

            // 1-3) detectRadius 안 → 추격 시작
            canChase = true;
            agent.isStopped = false;

            // Idle 숨소리 끄고,
            if (idleAudio && idleAudio.isPlaying) idleAudio.Stop();

            // 추격음은 딱 한 번만 재생
            if (chaseAudio && !chaseSoundPlayed)
            {
                chaseAudio.Play();
                chaseSoundPlayed = true;
            }

            if (!bgmSwitched && bgmSource && chaseBGM)
            {
                bgmSwitched = true;
                StartCoroutine(SwitchToChaseBGM());
            }
        }

        // ============================
        // 2) 추격 중 상태
        // ============================
        if (animator) animator.SetBool("Chase", true);

        tRepath += Time.deltaTime;
        if (tRepath >= repathInterval)
        {
            tRepath = 0f;
            agent.SetDestination(target.position);
        }
    }

    //void OnTriggerEnter(Collider other)
    //{
    //    if (killing) return;
    //    if (other.CompareTag("Player"))
    //    {
    //        StartCoroutine(KillSequence(other.gameObject));
    //    }
    //}

    //IEnumerator KillSequence(GameObject player)
    //{
    //    killing = true;

    //    // 움직임 정지
    //    agent.isStopped = true;
    //    agent.ResetPath();

    //    // 플레이어 조작 off
    //    foreach (var mb in playerScriptsToDisable)
    //        if (mb) mb.Enabled = false;

    //    // 모든 사운드 정지
    //    if (idleAudio && idleAudio.isPlaying) idleAudio.Stop();
    //    if (chaseAudio && chaseAudio.isPlaying) chaseAudio.Stop();

    //    // 플레이어 바라보기
    //    float t = 0f;
    //    while (t < 0.2f)
    //    {
    //        t += Time.deltaTime;
    //        Vector3 look = player.transform.position;
    //        look.y = transform.position.y;

    //        transform.rotation = Quaternion.Slerp(
    //            transform.rotation,
    //            Quaternion.LookRotation(look - transform.position),
    //            Time.deltaTime * facePlayerSpeed
    //        );

    //        yield return null;
    //    }

    //    // 암전
    //    yield return StartCoroutine(Fade(1f, fadeTime));

    //    // 씬 리로드
    //    yield return new WaitForSeconds(restartDelay);
    //    Scene scene = SceneManager.GetActiveScene();
    //    SceneManager.LoadScene(scene.buildIndex);
    //}

    //IEnumerator Fade(float targetAlpha, float duration)
    //{
    //    if (!fade) yield break;
    //    float start = fade.alpha;
    //    float t = 0f;

    //    while (t < duration)
    //    {
    //        t += Time.deltaTime;
    //        fade.alpha = Mathf.Lerp(start, targetAlpha, t / duration);
    //        yield return null;
    //    }

    //    fade.alpha = targetAlpha;
    //}

    //// 기즈모: 노란색 = 숨소리 범위, 빨간색 = 추격 시작 범위
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, idleSoundRadius);

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, detectRadius);
    }

    IEnumerator SwitchToChaseBGM()
    {
        // 간단한 페이드 아웃 / 인 (원하면 숫자만 바꿔줘)
        float fadeTime = 1.0f;
        float t = 0f;

        // 1) 기존 BGM 페이드 아웃
        float startVol = bgmSource.volume;
        while (t < fadeTime)
        {
            t += Time.deltaTime;
            bgmSource.volume = Mathf.Lerp(startVol, 0f, t / fadeTime);
            yield return null;
        }

        // 2) 클립 교체
        bgmSource.Stop();
        bgmSource.clip = chaseBGM;
        bgmSource.loop = true;
        bgmSource.Play();

        // 3) 페이드 인
        t = 0f;
        while (t < fadeTime)
        {
            t += Time.deltaTime;
            bgmSource.volume = Mathf.Lerp(0f, startVol, t / fadeTime);
            yield return null;
        }

        bgmSource.volume = startVol;
    }
}