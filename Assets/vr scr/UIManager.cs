using UnityEngine;
using UnityEngine.UI;
using TMPro; // TextMeshPro를 사용하려면 필요
using UnityEngine.SceneManagement; // 씬 관리를 위해 필요 (재시작 기능)

public class UIManager : MonoBehaviour
{
    // === UI 요소 연결 변수 ===
    // 타이머
    public TextMeshProUGUI timerText;
    // REC 깜빡임
    public Image recIndicator; // REC 표시 이미지 오브젝트
    public float blinkInterval = 0.5f; // 깜빡이는 간격 (0.5초마다)

    // 배터리
    public Slider batterySlider;
    public TextMeshProUGUI batteryPercentText;
    public float initialBatteryLevel = 100f;

    // === 카메라/플래시 관련 변수 ===
    public GameObject cameraUIRoot; // 카메라 UI 전체 (Canvas)
    public Light flashLight;       // 씬에 추가할 플래시 라이트 오브젝트

    public float normalDrainRate = 1f; // 평상시 초당 배터리 소모율
    public float flashDrainIncrease = 5f; // 플래시 켤 때 추가되는 소모율

    // 게임 오버
    public GameObject gameOverPanel; // 인스펙터에서 비활성화된 게임 오버 패널

    // === 내부 상태 변수 ===
    private float timeElapsed = 0f;
    private float currentBatteryLevel;
    private bool isGameOver = false;
    private bool isCameraOn = true;
    private bool isFlashOn = false; // 플래시는 꺼진 상태로 시작


    void Start()
    {
        currentBatteryLevel = initialBatteryLevel;
        gameOverPanel.SetActive(false);
        Time.timeScale = 1;

        // 초기 카메라 UI 상태 설정
        SetCameraActive(true);
        // 초기 플래시 상태 설정: 코드 상으로 비활성화합니다.
        if (flashLight != null) flashLight.enabled = false;

        if (recIndicator != null)
        {
            InvokeRepeating("BlinkRec", 0.1f, blinkInterval);
        }
    }

    void Update()
    {
        // 카메라가 꺼져 있거나 게임 오버 상태면 타이머/배터리 소모 정지
        if (isGameOver || !isCameraOn)
        {
            if (!isGameOver)
                return;
        }

        // 1. 타이머 업데이트 로직
        UpdateTimer();

        // 2. 배터리 소모 로직
        DrainBattery();
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

        // UI 업데이트
        batterySlider.value = currentBatteryLevel;
        batteryPercentText.text = Mathf.RoundToInt(currentBatteryLevel) + "%";

        // 3. 게임 오버 조건 체크
        if (currentBatteryLevel <= 0)
        {
            currentBatteryLevel = 0;
            isGameOver = true;
            ShowGameOver();
        }
    }

    private void ShowGameOver()
    {
        Debug.Log("배터리가 모두 소모되었습니다. 게임 오버!");
        gameOverPanel.SetActive(true);
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

    // --- 버튼 이벤트 함수 ---

    // 게임 오버 패널의 '다시 시작' 버튼에 연결할 함수
    public void RestartLevel()
    {
        Time.timeScale = 1;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}
