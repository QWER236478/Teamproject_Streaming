using UnityEngine;
using UnityEngine.SceneManagement; 

public class NextChapterTrigger : MonoBehaviour
{
    [Header("이동할 씬 이름")]
    public string nextSceneName = "Chapter2"; 

    private void OnTriggerEnter(Collider other)
    {
        // 부딪힌 물체가 "Player" 태그를 가지고 있는지 확인
        if (other.CompareTag("Player"))
        {
            Debug.Log("플레이어 감지 챕터 2로 이동 중...");
            SceneManager.LoadScene(nextSceneName);
        }
    }
}