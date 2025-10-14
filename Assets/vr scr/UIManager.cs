using UnityEngine;
using UnityEngine.UI;
using TMPro; // TextMeshPro를 사용하려면 필요
using UnityEngine.SceneManagement; // 씬 관리를 위해 필요 (재시작 기능)
using System.Collections.Generic; // List 사용을 위해 필요

public class UIManager : MonoBehaviour
{
    // === UI 요소 연결 변수 ===
    // 타이머
    public TextMeshProUGUI timerText;
    // REC 깜빡임
    public Image recIndicator; // REC 표시 이미지 오브젝트
    public float blinkInterval = 0.5f; // 깜빡이는 간격 (0.5초마다)

    // 배터리
    public Image batterySlider; // 슬라이더에서 Image fill 방식으로 변경된 오브젝트
    public TextMeshProUGUI batteryPercentText;
    public float initialBatteryLevel = 100f; // 0 ~ 100 사이의 값

    // === 배터리 색상 설정 변수 (기존) ===
    [System.Serializable]
    public class BatteryColorThreshold
    {
        public float percentage; // 해당 색상으로 변하는 기준 퍼센트 (예: 50% 이하)
        public Color color;      // 적용될 색상
    }

    // 퍼센트 기준과 색상을 설정하는 리스트 (인스펙터에서 관리)
    public List<BatteryColorThreshold> batteryColorThresholds = new List<BatteryColorThreshold>();

    // === 카메라/플래시 관련 변수 ===
    public GameObject cameraUIRoot; // 카메라 UI 전체 (Canvas)
    public Light flashLight;        // 씬에 추가할 플래시 라이트 오브젝트

    public float normalDrainRate = 1f; // 평상시 초당 배터리 소모율
    public float flashDrainIncrease = 5f; // 플래시 켤 때 추가되는 소모율

    // === Canvas Group 변수 (수정 및 추가) ===
    // 게임 오버
    public CanvasGroup gameOverCanvasGroup; // 게임 오버 패널의 Canvas Group
    public CanvasGroup inGameHudCanvasGroup; // 인게임 HUD (타이머, 배터리 등)의 CanvasGroup
    public float fadeOutTimeFactor = 1.0f; // 페이드 아웃/인 시간 조절 (클수록 느려짐)

    // === 내부 상태 변수 ===
    private float timeElapsed = 0f;
    private float currentBatteryLevel;
    private bool isGameOver = false;
    private bool isCameraOn = true;
    private bool isFlashOn = false; // 플래시는 꺼진 상태로 시작


    void Start()
    {
        currentBatteryLevel = initialBatteryLevel;
        Time.timeScale = 1;

        // HUD 투명도 초기화 및 활성화
        if (inGameHudCanvasGroup != null)
        {
            inGameHudCanvasGroup.alpha = 1f;
            SetCanvasGroupState(inGameHudCanvasGroup, true);
        }

        // 게임 오버 패널 투명도 초기화 및 비활성화
        if (gameOverCanvasGroup != null)
        {
            gameOverCanvasGroup.alpha = 0f;
            SetCanvasGroupState(gameOverCanvasGroup, false);
        }

        // 초기 카메라 UI 상태 설정
        SetCameraActive(true);

        // 초기 플래시 상태 설정
        if (flashLight != null) flashLight.enabled = false;

        // 배터리 UI 초기 업데이트 (색상 및 Fill)
        UpdateBatteryUI();

        if (recIndicator != null)
        {
            InvokeRepeating("BlinkRec", 0.1f, blinkInterval);
        }

        // 색상 리스트를 퍼센트가 낮은 순서(높은 우선순위)로 정렬
        batteryColorThresholds.Sort((a, b) => a.percentage.CompareTo(b.percentage));
    }

    void Update()
    {
        // 카메라가 꺼져 있거나 게임 오버 상태면 타이머/배터리 소모 정지
        if (isGameOver || !isCameraOn)
        {
            // 게임 오버 상태일 때는 HUD 페이드 아웃, 게임 오버 패널 페이드 인 로직을 실행
            if (isGameOver)
            {
                FadeOutHud();
                FadeInGameOverPanel();
            }
            return;
        }

        // 1. 타이머 업데이트 로직
        UpdateTimer();

        // 2. 배터리 소모 로직
        DrainBattery();
    }

    // Canvas Group의 상호작용 및 레이캐스트 설정을 일괄 처리
    private void SetCanvasGroupState(CanvasGroup cg, bool active)
    {
        if (cg == null) return;
        cg.interactable = active;
        cg.blocksRaycasts = active;
    }

    // 게임 오버 시 HUD를 페이드 아웃시키는 로직
    private void FadeOutHud()
    {
        if (inGameHudCanvasGroup == null) return;

        // Time.timeScale = 0 상태에서도 작동하도록 UnscaledDeltaTime 사용
        inGameHudCanvasGroup.alpha = Mathf.MoveTowards(
            inGameHudCanvasGroup.alpha,
            0f,
            Time.unscaledDeltaTime / fadeOutTimeFactor
        );

        // 투명도가 거의 0에 도달하면 상호작용을 막음
        if (inGameHudCanvasGroup.alpha < 0.01f)
        {
            SetCanvasGroupState(inGameHudCanvasGroup, false);
        }
    }

    // 게임 오버 시 패널을 페이드 인시키는 로직
    private void FadeInGameOverPanel()
    {
        if (gameOverCanvasGroup == null) return;

        // 페이드 인 시작 시 상호작용 활성화
        if (gameOverCanvasGroup.alpha == 0f)
        {
            SetCanvasGroupState(gameOverCanvasGroup, true);
        }

        // Time.timeScale = 0 상태에서도 작동하도록 UnscaledDeltaTime 사용
        gameOverCanvasGroup.alpha = Mathf.MoveTowards(
            gameOverCanvasGroup.alpha,
            1f,
            Time.unscaledDeltaTime / fadeOutTimeFactor
        );
    }

    void BlinkRec()
    {
        // recIndicator의 활성화 상태를 반전시킵니다.
        if (recIndicator != null)
        {
            recIndicator.enabled = !recIndicator.enabled;
        }
    }

    private void UpdateTimer()
    {
        timeElapsed += Time.deltaTime;

        int minutes = (int)(timeElapsed / 60f);
        int seconds = (int)(timeElapsed % 60f);
        int milliseconds = (int)((timeElapsed * 1000f) % 1000f);

        timerText.text = string.Format("{0:00}:{1:00}:{2:00}", minutes, seconds, milliseconds / 10);
    }

    private void DrainBattery()
    {
        // 소모율 계산: 플래시가 켜져 있으면 추가 소모율을 더함
        float currentDrainRate = normalDrainRate;
        if (isFlashOn)
        {
            currentDrainRate += flashDrainIncrease;
        }

        // 배터리 잔량 감소
        currentBatteryLevel -= currentDrainRate * Time.deltaTime;

        // 배터리 잔량이 0 이하로 내려가지 않도록 함
        currentBatteryLevel = Mathf.Max(0f, currentBatteryLevel);

        // UI 업데이트 함수 호출
        UpdateBatteryUI();

        // 3. 게임 오버 조건 체크
        if (currentBatteryLevel <= 0 && !isGameOver)
        {
            isGameOver = true;
            ShowGameOver();
        }
    }

    private void UpdateBatteryUI()
    {
        // Image Fill 방식 업데이트
        // currentBatteryLevel (0~100)을 fillAmount (0~1)로 변환
        if (batterySlider != null)
        {
            batterySlider.fillAmount = currentBatteryLevel / 100f;

            // 잔량 퍼센트 텍스트 업데이트
            batteryPercentText.text = Mathf.RoundToInt(currentBatteryLevel) + "%";

            // 배터리 색상 업데이트
            UpdateBatteryColor();
        }
    }

    private void UpdateBatteryColor()
    {
        Color targetColor = Color.white; // 기본 색상 (잔량이 높을 때)

        // 설정된 임계값 리스트를 반복하여 현재 잔량에 맞는 색상 찾기
        foreach (var threshold in batteryColorThresholds)
        {
            if (currentBatteryLevel <= threshold.percentage)
            {
                targetColor = threshold.color;
                break;
            }
        }

        // 배터리 이미지 색상 적용
        if (batterySlider != null)
        {
            batterySlider.color = targetColor;
        }
    }

    private void ShowGameOver()
    {
        Debug.Log("배터리가 모두 소모되었습니다. 게임 오버!");

        // 게임 오버 패널의 Canvas Group 활성화 (페이드 인 로직은 Update에서 실행)
        // isGameOver 플래그를 통해 Update의 페이드 인 로직이 시작됨

        // 게임 일시 정지 (UI 페이드는 Time.unscaledDeltaTime로 계속 진행됨)
        Time.timeScale = 0;

        // 깜빡이는 기능 정지
        CancelInvoke("BlinkRec");
        if (recIndicator != null)
        {
            recIndicator.enabled = false;
        }
    }

    // --- 새로운 카메라/플래시 제어 함수 ---

    // 그립 버튼 연결용: 카메라 On/Off 토글 (Q 키)
    public void ToggleCamera()
    {
        if (isGameOver) return;

        Debug.Log("ToggleCamera 호출됨. 토글 전 isCameraOn 상태: " + isCameraOn);

        isCameraOn = !isCameraOn;
        SetCameraActive(isCameraOn);

        if (recIndicator != null)
        {
            if (isCameraOn)
            {
                // 카메라 켤 때 REC 깜빡임 재시작
                InvokeRepeating("BlinkRec", 0.1f, blinkInterval);
            }
            else
            {
                // 카메라 끌 때 REC 깜빡임 정지
                CancelInvoke("BlinkRec");
                recIndicator.enabled = false;
            }
        }
    }

    private void SetCameraActive(bool active)
    {
        // 카메라 화면 전체 UI (Canvas)를 켜고 끕니다.
        if (cameraUIRoot != null)
        {
            cameraUIRoot.SetActive(active);
        }

        // 카메라가 꺼지면 플래시도 강제로 끕니다.
        if (!active)
        {
            TurnFlashOff();
        }
    }

    // 트리거 버튼 연결용: 플래시 On/Off 토글 (W 키)
    public void ToggleFlash()
    {
        // A. 플래시 토글 전에 isCameraOn 상태를 콘솔에 출력
        Debug.Log("ToggleFlash 호출. isCameraOn: " + isCameraOn + ", isFlashOn: " + isFlashOn);

        // B. 카메라가 꺼져 있거나 게임 오버 상태면 작동 중지
        if (isGameOver || !isCameraOn)
        {
            Debug.LogWarning("ToggleFlash 중지됨: 카메라 꺼짐 또는 게임 오버 상태.");
            return;
        }

        // C. 연결이 되어 있지 않으면 즉시 경고 출력 (Null 체크 강화)
        if (flashLight == null)
        {
            Debug.LogError("오류: Flash Light 오브젝트가 UIManager 스크립트의 Flash Light 슬롯에 연결되어 있지 않거나 연결이 해제되었습니다!");
            return;
        }

        isFlashOn = !isFlashOn;

        if (isFlashOn)
        {
            // 플래시 켜는 명령
            flashLight.enabled = true;
            Debug.Log("플래시 켜짐 최종 확인: Light.enabled = TRUE 설정됨.");
        }
        else
        {
            // 플래시 끄는 명령
            flashLight.enabled = false;
            Debug.Log("플래시 꺼짐 최종 확인: Light.enabled = FALSE 설정됨.");
        }
    }

    private void TurnFlashOff()
    {
        isFlashOn = false;
        if (flashLight != null) flashLight.enabled = false;
    }

    // --- 새 게임 시작 대기 및 버튼 이벤트 함수 ---

    // 게임 오버 애니메이션 후 N초 대기 또는 특정 이벤트 발생 시 호출될 함수
    public void continueToNewGame()
    {
        // 여기 애니메이터 끝나고 n초 후, 혹은 특정 상황에 게임 재시작 호출을 위해 이 함수를 불러주세요.
        // 그리고, 이 함수에서 실제로 새 게임을 시작하는 함수로 연결해야 합니다.
        // 예: Invoke("RestartLevel", 5f); 또는 SceneLoader.LoadScene("MainMenu");
    }

    // 게임 오버 패널의 '다시 시작' 버튼에 연결할 함수 (기존 RestartLevel)
    public void RestartLevel()
    {
        Time.timeScale = 1;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}