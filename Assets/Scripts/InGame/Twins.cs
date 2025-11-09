using UnityEngine;
using UnityEngine.AI;
using UnityEngine.SceneManagement;
using System.Collections;

public class TwinsChaseKill : MonoBehaviour
{
    [Header("Refs")]
    public Transform target;                  // Player Transform
    public Animator animator;                 // Twins Animator(달리기만)
    NavMeshAgent agent;

    [Header("Chase")]
    public float runSpeed = 5f;
    public float repathInterval = 0.1f;

    [Header("Kill")]
    public float facePlayerSpeed = 12f;       // 회전 속도
    public float restartDelay = 2.0f;         // 암전 후 재시작까지
    public CanvasGroup fade;                  // 검은 화면 CanvasGroup
    public float fadeTime = 0.6f;
    public MonoBehaviour[] playerScriptsToDisable;

    bool killing;
    float tRepath;

    void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        if (!target) target = GameObject.FindGameObjectWithTag("Player")?.transform;
        if (!animator) animator = GetComponent<Animator>();
        agent.autoRepath = true;
        agent.stoppingDistance = 0f;
        agent.speed = runSpeed;
    }

    void Update()
    {
        if (killing || !target) return;

        tRepath += Time.deltaTime;
        if (tRepath >= repathInterval)
        {
            tRepath = 0f;
            agent.SetDestination(target.position);
        }

        // 달리기 애니 on
        if (animator) animator.SetBool("Run", true);
    }

    void OnTriggerEnter(Collider other)
    {
        if (killing) return;
        if (other.CompareTag("Player"))
        {
            StartCoroutine(KillSequence(other.gameObject));
        }
    }

    IEnumerator KillSequence(GameObject player)
    {
        killing = true;

        // 1) 적 정지 + 플레이어 조작 off
        agent.isStopped = true; agent.ResetPath();
        foreach (var mb in playerScriptsToDisable) if (mb) mb.enabled = false;

        // 2) 적이 플레이어를 바라보게(짧은 보정)
        float t = 0f;
        while (t < 0.2f)
        {
            t += Time.deltaTime;
            Vector3 look = player.transform.position; look.y = transform.position.y;
            transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(look - transform.position), Time.deltaTime * facePlayerSpeed);
            yield return null;
        }

        // (옵션) 플레이어 카메라에 살짝 흔들림/사운드 추가 가능

        // 3) 암전
        yield return StartCoroutine(Fade(1f, fadeTime));

        // 4) 씬 리스타트
        yield return new WaitForSeconds(restartDelay);
        Scene scene = SceneManager.GetActiveScene();
        SceneManager.LoadScene(scene.buildIndex);
    }

    IEnumerator Fade(float targetAlpha, float duration)
    {
        if (!fade) yield break;
        float start = fade.alpha;
        float t = 0f;
        while (t < duration)
        {
            t += Time.deltaTime;
            fade.alpha = Mathf.Lerp(start, targetAlpha, t / duration);
            yield return null;
        }
        fade.alpha = targetAlpha;
    }
}