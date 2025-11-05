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

    // === 배터리 색상 설정 변수 ===
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

    // === Canvas Group 변수 ===
    // 게임 오버
    public CanvasGroup gameOverCanvasGroup; // 게임 오버 패널의 Canvas Group
    public CanvasGroup inGameHudCanvasGroup; // 인게임 HUD (타이머, 배터리 등)의 CanvasGroup
    public float fadeOutTimeFactor = 1.0f; // 페이드 아웃/인 시간 조절 (클수록 느려짐)

    // === 줌 설정 (새로 추가) ===
    [Tooltip("카메라 컴포넌트를 연결해주세요. (XR Origin > Camera Offset > Main Camera)")]
    public Camera mainCamera;
    [Header("줌 설정")]
    [Tooltip("최대 줌 인 시 FOV 값 (좁은 시야)")]
    public float camMinFov = 30f;
    [Tooltip("기본/최대 줌 아웃 시 FOV 값 (넓은 시야)")]
    public float camMaxFov = 90f;
    [Tooltip("줌 전환에 걸리는 시간 (초)")]
    public float zoomDuration = 0.5f;
    [Tooltip("줌 전환 속도 곡선 (Lerp 시 사용)")]
    public AnimationCurve zoomCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);


    // === 내부 상태 변수 ===
    private float timeElapsed = 0f;
    private float currentBatteryLevel;
    private bool isGameOver = false;
    private bool isCameraOn = true;
    private bool isFlashOn = false; // 플래시는 꺼진 상태로 시작

    // === 줌 상태 변수 추가 ===
    private bool isZoomed = false; // 현재 줌 인 상태인지
    private float zoomTime = 0f;    // 줌 전환 경과 시간


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

        // 줌 기능 초기화: Main Camera 연결 및 FOV 기본값 설정
        if (mainCamera == null)
        {
            mainCamera = FindObjectOfType<Camera>();
        }
        if (mainCamera != null)
        {
            // 시작 시 최대 줌 아웃(기본) 상태로 설정
            mainCamera.fieldOfView = camMaxFov;
        }


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
            if (!isGameOver)
                return;
        }

        // 1. 타이머 업데이트 로직
        UpdateTimer();

        // 2. 배터리 소모 로직
        DrainBattery();

        // 3. 줌 전환 로직 (Lerp 구동)
        HandleZoomTransition();
    }

    // === 줌 전환 로직 (Update에서 Lerp를 구동) ===
    private void HandleZoomTransition()
    {
        if (mainCamera == null) return;

        // 줌 전환 경과 시간이 설정된 지속 시간보다 작을 때만 Lerp 진행
        if (zoomTime < zoomDuration)
        {
            zoomTime += Time.deltaTime;
            float normalizedTime = Mathf.Clamp01(zoomTime / zoomDuration);

            // AnimationCurve를 적용한 시간 값
            float curveValue = zoomCurve.Evaluate(normalizedTime);

            float startFOV, endFOV;

            if (isZoomed)
            {
                // 줌 인 전환 중: Max -> Min
                startFOV = camMaxFov;
                endFOV = camMinFov;
            }
            else
            {
                // 줌 아웃 전환 중: Min -> Max
                startFOV = camMinFov;
                endFOV = camMaxFov;
            }

            // Lerp를 사용하여 FOV를 부드럽게 전환
            mainCamera.fieldOfView = Mathf.Lerp(startFOV, endFOV, curveValue);
        }
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
        float currentDrainRate = normalDrainRate;
        if (isFlashOn)
        {
            currentDrainRate += flashDrainIncrease;
        }

        currentBatteryLevel -= currentDrainRate * Time.deltaTime;

        currentBatteryLevel = Mathf.Max(0f, currentBatteryLevel);

        UpdateBatteryUI();

        if (currentBatteryLevel <= 0 && !isGameOver)
        {
            isGameOver = true;
            ShowGameOver();
        }
    }

    private void UpdateBatteryUI()
    {
        if (batterySlider != null)
        {
            batterySlider.fillAmount = currentBatteryLevel / 100f;
            batteryPercentText.text = Mathf.RoundToInt(currentBatteryLevel) + "%";
            UpdateBatteryColor();
        }
    }

    private void UpdateBatteryColor()
    {
        Color targetColor = Color.white;

        foreach (var threshold in batteryColorThresholds)
        {
            if (currentBatteryLevel <= threshold.percentage)
            {
                targetColor = threshold.color;
                break;
            }
        }

        if (batterySlider != null)
        {
            batterySlider.color = targetColor;
        }
    }

    private void ShowGameOver()
    {
        Debug.Log("배터리가 모두 소모되었습니다. 게임 오버!");

        Time.timeScale = 0;

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
                InvokeRepeating("BlinkRec", 0.1f, blinkInterval);
            }
            else
            {
                CancelInvoke("BlinkRec");
                recIndicator.enabled = false;
            }
        }
    }

    private void SetCameraActive(bool active)
    {
        if (cameraUIRoot != null)
        {
            cameraUIRoot.SetActive(active);
        }

        if (!active)
        {
            TurnFlashOff();
            // 카메라가 꺼지면 줌 상태도 리셋 (Max FOV로 즉시 돌아감)
            if (mainCamera != null) mainCamera.fieldOfView = camMaxFov;
            isZoomed = false;
            zoomTime = 0f;
        }
    }

    // 트리거 버튼 연결용: 플래시 On/Off 토글 (W 키)
    public void ToggleFlash()
    {
        Debug.Log("ToggleFlash 호출. isCameraOn: " + isCameraOn + ", isFlashOn: " + isFlashOn);

        if (isGameOver || !isCameraOn)
        {
            Debug.LogWarning("ToggleFlash 중지됨: 카메라 꺼짐 또는 게임 오버 상태.");
            return;
        }

        if (flashLight == null)
        {
            Debug.LogError("오류: Flash Light 오브젝트가 UIManager 스크립트의 Flash Light 슬롯에 연결되어 있지 않거나 연결이 해제되었습니다!");
            return;
        }

        isFlashOn = !isFlashOn;

        if (isFlashOn)
        {
            flashLight.enabled = true;
            Debug.Log("플래시 켜짐 최종 확인: Light.enabled = TRUE 설정됨.");
        }
        else
        {
            flashLight.enabled = false;
            Debug.Log("플래시 꺼짐 최종 확인: Light.enabled = FALSE 설정됨.");
        }
    }

    private void TurnFlashOff()
    {
        isFlashOn = false;
        if (flashLight != null) flashLight.enabled = false;
    }

    // === 줌 인/아웃 토글 함수 (x 키에 연결) ===
    // 이 함수가 호출되면 줌 전환이 시작됩니다.
    public void ToggleZoom()
    {
        if (isGameOver) return;
        if (!isCameraOn)
        {
            Debug.LogWarning("줌 기능 중지됨: 카메라 꺼짐 상태.");
            return;
        }

        // 줌 상태를 반전시키고 (토글)
        isZoomed = !isZoomed;

        // 전환 시간을 0으로 리셋하여 Update()에서 새로운 Lerp 전환 시작
        zoomTime = 0f;

        Debug.Log($"ToggleZoom 호출. 새로운 줌 상태: {(isZoomed ? "줌 인" : "줌 아웃")}");
    }


    // --- 새 게임 시작 대기 및 버튼 이벤트 함수 ---

    public void continueToNewGame()
    {
        // ...
    }

    // 게임 오버 패널의 '다시 시작' 버튼에 연결할 함수
    public void RestartLevel()
    {
        Time.timeScale = 1;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}
