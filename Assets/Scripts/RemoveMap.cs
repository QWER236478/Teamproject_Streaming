using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RemoveMap : MonoBehaviour
{
  

    public GameObject targetMap; // 없애버릴 맵 (지하 창고)

    private void OnTriggerEnter(Collider other)
    {
        // 플레이어가 이 선을 넘으면
        if (other.CompareTag("Player"))
        {
            CharacterController cc = other.GetComponent<CharacterController>();
            if (cc != null)
            {
                cc.skinWidth = 0.08f; // 피부를 두껍게 (벽에 안 붙게)
                cc.radius = 0.15f;    // 몸통을 0.2 -> 0.15로 홀쭉하게 (문 통과용)
                cc.stepOffset = 0.3f;

                Debug.Log("플레이어 변신 완료! (Skin: 0.08, Radius: 0.15)");
            }
            // 맵을 비활성화(꺼버림)
            if (targetMap != null)
            {
                targetMap.SetActive(false);
                Debug.Log("지하 창고 삭제 완료!");
            }

            // 이 스위치(트리거)도 할 일 다 했으니 삭제
            this.gameObject.SetActive(false);
        }
    }
}