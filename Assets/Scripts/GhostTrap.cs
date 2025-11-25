using UnityEngine;
using System.Collections;

public class GhostTrap : MonoBehaviour
{
    [Header("연결할 것")]
    public BloodHands bloodHandsScript; // 아까 그 스크립트를 여기에 연결!
    public GameObject ghostFace;        // 튀어나올 귀신 (Quad)
    public AudioSource audioSource;     // 비명 소리
    public AudioClip screamClip;

    bool trapTriggered = false;

    private void OnTriggerEnter(Collider other)
    {
        // 플레이어가 밟았는데 + 아직 함정 발동 안 했고
        if (other.CompareTag("Player") && !trapTriggered)
        {
            // ★ 핵심: 저쪽 스크립트가 "위험하다(isDangerous)"고 하면 -> 귀신 소환
            if (bloodHandsScript.isDangerous == true)
            {
                StartCoroutine(Jumpscare());
            }
        }
    }

    IEnumerator Jumpscare()
    {
        trapTriggered = true;

        // 귀신 켜고 비명 지르기
        if (ghostFace) ghostFace.SetActive(true);
        if (audioSource) audioSource.PlayOneShot(screamClip);

        // 1.5초 뒤 귀신 사라짐
        yield return new WaitForSeconds(1.5f);

        if (ghostFace) ghostFace.SetActive(false);
        Destroy(gameObject); // 함정 끝
    }
}