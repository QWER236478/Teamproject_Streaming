using UnityEngine;
using UnityEngine.Video;
using UnityEngine.SceneManagement;
using System.Collections;

public class VideoEndController : MonoBehaviour
{
    public VideoPlayer videoPlayer;
    public string startMenuSceneName = "StartMenu";

    [Header("엔딩 텍스트 설정")]
    public GameObject endingTextUI; // 텍스트가 포함된 UI 오브젝트 (Canvas 또는 Panel)
    public float fadeDuration = 2.0f; // 텍스트가 서서히 나타나는 데 걸리는 시간
    public float textDuration = 4.0f; // 텍스트가 다 나온 뒤 유지되는 시간

    void Start()
    {
        if (videoPlayer == null)
            videoPlayer = FindObjectOfType<VideoPlayer>();

        // 시작할 때 UI는 꺼둠
        if (endingTextUI != null) endingTextUI.SetActive(false);

        videoPlayer.loopPointReached += OnVideoEnd;
    }

    void OnVideoEnd(VideoPlayer vp)
    {
        Debug.Log("영상 종료. 페이드 연출 시작.");
        StartCoroutine(ShowTextAndLoadScene());
    }

    IEnumerator ShowTextAndLoadScene()
    {
        if (endingTextUI != null)
        {
            // 1. 투명도 조절을 위한 CanvasGroup 가져오기 (없으면 자동 추가)
            CanvasGroup cg = endingTextUI.GetComponent<CanvasGroup>();
            if (cg == null) cg = endingTextUI.AddComponent<CanvasGroup>();

            // 2. 초기 상태 설정 (완전 투명)
            cg.alpha = 0f;
            endingTextUI.SetActive(true);

            // (선택) 영상 화면 끄기 (검은 배경에서 글씨만 나오게 하려면 주석 해제)
            // if (videoPlayer.targetTexture != null) videoPlayer.targetTexture.Release();
            // videoPlayer.gameObject.SetActive(false);

            // 3. 페이드 인 (서서히 나타나기)
            float timer = 0f;
            while (timer < fadeDuration)
            {
                timer += Time.deltaTime;
                cg.alpha = Mathf.Lerp(0f, 1f, timer / fadeDuration);
                yield return null;
            }
            cg.alpha = 1f; // 확실하게 1로 고정
        }

        // 4. 글씨가 다 나온 상태로 대기
        yield return new WaitForSeconds(textDuration);

        // 5. 씬 이동
        Debug.Log("메뉴로 이동.");
        SceneManager.LoadScene(startMenuSceneName);
    }
}