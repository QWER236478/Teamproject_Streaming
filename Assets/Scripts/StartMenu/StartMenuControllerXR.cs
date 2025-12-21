using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.XR; // XR 입력 감지용 네임스페이스 추가

public class StartMenuControllerXR : MonoBehaviour
{
    [Header("타겟 오브젝트")]
    public GameObject buttonGroup;       // 버튼들이 들어있는 그룹
    public GameObject selectAnyButton;   // "Press A Button" 텍스트 오브젝트

    [Header("설정")]
    public bool buttonGroupStartsHidden = true; // 처음엔 버튼 숨김
    public bool fadeIn = true;                  // 버튼 페이드 인
    public float fadeDuration = 0.3f;

    private bool shown = false;
    private bool wasPressed = false; // 버튼 중복 눌림 방지용

    void Start()
    {
        if (buttonGroup != null) buttonGroup.SetActive(!buttonGroupStartsHidden);
        if (selectAnyButton != null) selectAnyButton.SetActive(true);
    }

    void Update()
    {
        // 이미 메뉴가 보이면 입력 체크 안 함
        if (shown) return;

        // --- XR 입력 감지 (오른손 A버튼) ---
        InputDevice device = InputDevices.GetDeviceAtXRNode(XRNode.RightHand);
        bool isPressed = false;

        // primaryButton은 오큘러스/메타 퀘스트 컨트롤러 기준 'A' 버튼입니다.
        if (device.TryGetFeatureValue(CommonUsages.primaryButton, out bool pressedValue))
        {
            isPressed = pressedValue;
        }

        // 버튼을 '딱' 눌렀을 때 실행 (누르고 있는 동안 계속 실행 방지)
        if (isPressed && !wasPressed)
        {
            ShowMenu();
        }

        wasPressed = isPressed; // 현재 상태 저장
    }

    // 메뉴 보이기 (기존 로직 동일)
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

    // --- UI 버튼 연결용 함수들 (기존 동일) ---

    public void OnGameStartClick()
    {
        Debug.Log("게임시작");
        SceneManager.LoadScene("Chapter1");
    }

    public void OnLoadGameClick()
    {
        Debug.Log("이어하기");
    }

    public void OnOptionClick()
    {
        Debug.Log("설정 창");
        // OptionPOPUP.SetActive(true); 등 필요한 로직 주석 해제하여 사용
    }

    public void OptionCancelClick()
    {
        Debug.Log("설정 취소");
    }

    public void OnQuitClick()
    {
        Application.Quit();
        Debug.Log("종료됨");
    }
}