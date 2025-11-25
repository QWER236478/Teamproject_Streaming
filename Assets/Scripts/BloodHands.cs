using System.Collections;
using UnityEngine;

public class BloodHands : MonoBehaviour
{
    [Header("트리거가 밟히면 실행할 피 연출 스크립트")]
    public BloodStampAreaSpawnerWithSound spawner;
    [Header("유인하는 목소리 ")]
    public AudioSource myAudioSource;  // 이 오브젝트에 달린 스피커
    public AudioClip cryingSound;       // 처음에 나올 울음 소리 (3초간)
    public AudioClip helpVoice;        // "도와주세요" 파일
    [Header("추가 설정: 사라지는 시간")]
    public float duration = 10f;
    [HideInInspector] public bool isDangerous = false;
    public bool playOnce = true;   // 한 번만 실행할지
    bool hasPlayed = false;

    private void OnTriggerEnter(Collider other)
    {
        // 플레이어만 반응
        if (!other.CompareTag("Player"))
            return;

        if (playOnce && hasPlayed)
            return;

        if (spawner != null)
        {
            spawner.Begin();// 여기서 손자국 + 사운드 연출 시작
        
            hasPlayed = true;

        }// 2. 오디오 시퀀스 시작 (울음 -> 대사)
        if (myAudioSource != null)
        {
            StartCoroutine(PlayAudioSequence());
        }
        StartCoroutine(TimerRoutine());
    }
    // 소리 시간차 재생 함수
    IEnumerator PlayAudioSequence()
    {
        // (1) 먼저 울음 소리 재생
        if (cryingSound != null)
        {
            myAudioSource.PlayOneShot(cryingSound);

            // (2) 3초 대기 (울음 소리 듣는 시간)
            yield return new WaitForSeconds(cryingSound.length);
        }
        else
        {
            // 만약 울음소리 파일이 없으면 그냥 1초만 쉼
            yield return new WaitForSeconds(1.0f);
        }
        // (3) "도와주세요" 목소리 재생
        if (helpVoice != null)
        {
            myAudioSource.PlayOneShot(helpVoice);
        }
    }
    IEnumerator TimerRoutine()
    {
        isDangerous = true; // "지금 지나가면 죽는다" 상태 ON

        yield return new WaitForSeconds(duration); // 10초 대기

        isDangerous = false; // "이제 안전하다" 상태 OFF
    }
}
