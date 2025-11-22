using System.Collections;
using UnityEngine;

public class GhostDoor : MonoBehaviour
{
    // === 에디터에서 설정할 수 있는 변수 ===
    public Transform doorPivot;     // 문이 회전할 축 (경첩)
    public float openAngle = 90f;   // 열릴 각도

    // **문이 열리는 데 걸리는 시간 (초)**
    // 이 시간(1초~3초) 안에 문이 완전히 열립니다.
    public float doorOpenDuration = 2.0f;

    public AudioClip openingSound;  // 으스스한 문 열리는 소리 클립

    // **오디오 시작 시간과 길이**
    // 이 오디오 클립의 0.0초부터 2.0초까지만 재생합니다. (Inspector에서 조정)
    public float soundStartTime = 0.0f;
    public float soundDuration = 2.0f;

    // === 내부 변수 ===
    private bool isOpening = false; // 문이 열리는 중인지 확인
    private Quaternion startRotation;
    private Quaternion targetRotation;
    private AudioSource audioSource;
    private float startTime; // 문이 열리기 시작한 시간

    void Start()
    {
        if (doorPivot == null) doorPivot = this.transform;

        // AudioSource 컴포넌트 설정
        audioSource = gameObject.GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }
        audioSource.playOnAwake = false;
        audioSource.clip = openingSound;
        audioSource.loop = false;

        startRotation = doorPivot.localRotation;
    }

    // doorsensor(Collider)에 물체가 닿으면 문 열기
    private void OnTriggerEnter(Collider other)
    {
        // 문이 이미 열리고 있지 않다면 작동
        if (!isOpening)
        {
            isOpening = true;
            startTime = Time.time; // 문 열기 시작 시간 기록

            // 목표 각도 설정
            targetRotation = startRotation * Quaternion.Euler(0, openAngle, 0);

            // **오디오 재생:** 지정된 구간만 재생하고, 지정된 시간 후에 멈춥니다.
            if (openingSound != null)
            {
                audioSource.clip = openingSound;
                audioSource.time = soundStartTime; // 클립의 시작 위치 설정
                audioSource.Play();

                // 지정된 재생 시간 후에 오디오를 멈추는 코루틴 시작
                StartCoroutine(StopSoundAfterDuration(soundDuration));
            }

            // 문을 열기 위한 코루틴 시작
            StartCoroutine(OpenDoorOverTime());
        }
    }

    // **문이 일정 시간 동안 열리도록 제어하는 코루틴**
    IEnumerator OpenDoorOverTime()
    {
        float elapsedTime = 0f;

        while (elapsedTime < doorOpenDuration)
        {
            // 경과 시간 계산
            elapsedTime += Time.deltaTime;

            // 진행률 (0.0에서 1.0 사이)
            float t = Mathf.Clamp01(elapsedTime / doorOpenDuration);

            // Slerp을 사용하여 부드럽게 목표 각도로 이동
            doorPivot.localRotation = Quaternion.Slerp(startRotation, targetRotation, t);

            yield return null; // 다음 프레임까지 대기
        }

        // 정확히 목표 각도에 고정
        doorPivot.localRotation = targetRotation;
        isOpening = false; // 문 열기 완료
    }

    // **지정된 시간(soundDuration) 후 오디오를 멈추는 코루틴**
    IEnumerator StopSoundAfterDuration(float duration)
    {
        // 지정된 시간만큼 기다림
        yield return new WaitForSeconds(duration);

        // 오디오가 여전히 재생 중이면 멈춤
        if (audioSource.isPlaying)
        {
            audioSource.Stop();
        }
    }
}