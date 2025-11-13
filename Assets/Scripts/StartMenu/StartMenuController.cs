using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.XR.Interaction.Toolkit;

public class StartMenuController : MonoBehaviour
{
    [Header("타겟 오브젝트")]
    public GameObject buttonGroup;       // 버튼들이 들어있는 그룹
    public GameObject selectAnyButton;   // "Select Any Button" 오브젝트

    [Header("설정")]
    public bool buttonGroupStartsHidden = true; // 처음엔 버튼 숨김
    public bool fadeIn = true;                  // 버튼 페이드 인
    public float fadeDuration = 0.3f;

    bool shown = false;

    void Start()
    {
        if (buttonGroup != null) buttonGroup.SetActive(!buttonGroupStartsHidden);
        if (selectAnyButton != null) selectAnyButton.SetActive(true);
    }

    void Update()
    {
        if (shown) return;

        // 아무 입력(키/마우스/터치) 감지
        if (Input.anyKeyDown || Input.GetMouseButtonDown(0) || Input.GetMouseButtonDown(1) || Input.touchCount > 0)
        {
            ShowMenu();
        }
    }

    // 유니티 이벤트로도 호출 가능 (버튼 OnClick 등에 연결해도 됨)
    public void ShowMenu()
    {
        if (shown) return;
        shown = true;

        if (selectAnyButton != null)
            selectAnyButton.SetActive(false);

        if (buttonGroup != null)
        {
            buttonGroup.SetActive(true);

            if (fadeIn)
            {
                var cg = buttonGroup.GetComponent<CanvasGroup>();
                if (cg == null) cg = buttonGroup.AddComponent<CanvasGroup>();
                cg.alpha = 0f;
                StartCoroutine(FadeIn(cg));
            }
        }
    }

    System.Collections.IEnumerator FadeIn(CanvasGroup cg)
    {
        float t = 0f;
        while (t < fadeDuration)
        {
            t += Time.unscaledDeltaTime;
            cg.alpha = Mathf.Lerp(0f, 1f, t / fadeDuration);
            yield return null;
        }
        cg.alpha = 1f;
    }

    public void OnGameStartClick() // 게임 시작
    {
        //ClickButton.Play();
        Debug.Log("게임시작");
        SceneManager.LoadScene("Chapter2"); 
    }

    public void OnLoadGameClick() //이어하기 버튼
    {
        //ClickButton.Play();
        Debug.Log("이어하기");
    }

    public void OnOptionClick() //설정 버튼
    {
        //OptionPOPUP.SetActive(true);
        //ClickButton.Play();
        Debug.Log("설정 창");
    }

    public void OptionCancelClick()
    {
        //ClickButton.Play();
        //OptionPOPUP.SetActive(false);
    }

    public void ContinueLockClick()
    {
        //ClickButton.Play();
        //ContinueLock.SetActive(true);
    }

    //public void ContinueLockCancel()
    //{
        //ClickButton.Play();
        //ContinueLock.SetActive(false);
    //}

    public void OnQuitClick() //종료하기 버튼
    {
        Application.Quit(); //종료
        Debug.Log("종료됨");
    }
}