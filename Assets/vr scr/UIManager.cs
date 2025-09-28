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
    public Image recIndicator; // 새로 추가: REC 표시 이미지 오브젝트
    public float blinkInterval = 0.5f; // 깜빡이는 간격 (0.5초마다)

    // 배터리
    public Slider batterySlider;
    public TextMeshProUGUI batteryPercentText;
    public float initialBatteryLevel = 100f;
    public float drainRatePerSecond = 1f; // 초당 소모될 배터리 양 (예: 1초에 1%)

    // 게임 오버
    public GameObject gameOverPanel; // 인스펙터에서 비활성화된 게임 오버 패널을 연결

    // === 내부 상태 변수 ===
    private float timeElapsed = 0f;
    private float currentBatteryLevel;
    private bool isGameOver = false;

    void Start()
    {
        currentBatteryLevel = initialBatteryLevel;
        gameOverPanel.SetActive(false); // 시작 시 게임 오버 패널 숨기기
        Time.timeScale = 1; // 혹시 모를 상황 대비하여 게임 속도 정상화
        if (recIndicator != null)
        {
            InvokeRepeating("BlinkRec", 0.1f, blinkInterval);
        }
    }

    void Update()
    {
        if (isGameOver)
        {
            return; // 게임 오버 상태면 아무것도 하지 않음
        }

        // 1. 타이머 업데이트 로직
        UpdateTimer();

        // 2. 배터리 소모 로직
        DrainBattery();
    }
    void BlinkRec()
    {
        // recIndicator의 활성화 상태를 반전시킵니다.
        recIndicator.enabled = !recIndicator.enabled;
    }
    private void UpdateTimer()
    {
        timeElapsed += Time.deltaTime;

        int minutes = (int)(timeElapsed / 60f);
        int seconds = (int)(timeElapsed % 60f);
        int milliseconds = (int)((timeElapsed * 1000f) % 1000f);

        // 이미지에 맞게 시간 형식 (분:초:밀리초)으로 설정
        timerText.text = string.Format("{0:00}:{1:00}:{2:00}", minutes, seconds, milliseconds / 10);
    }

    private void DrainBattery()
    {
        // 배터리 잔량 감소
        currentBatteryLevel -= drainRatePerSecond * Time.deltaTime;

        // UI 업데이트
        batterySlider.value = currentBatteryLevel;
        batteryPercentText.text = Mathf.RoundToInt(currentBatteryLevel) + "%";

        // 3. 게임 오버 조건 체크
        if (currentBatteryLevel <= 0)
        {
            currentBatteryLevel = 0; // 0 이하로 내려가지 않도록 고정
            isGameOver = true;
            ShowGameOver();
        }
    }

    private void ShowGameOver()
    {
        Debug.Log("배터리가 모두 소모되었습니다. 게임 오버!");
        gameOverPanel.SetActive(true);
        Time.timeScale = 0;

        // 새로 추가: 깜빡이는 기능 정지
        CancelInvoke("BlinkRec");
        if (recIndicator != null)
        {
            recIndicator.enabled = false; // REC 표시등을 완전히 끕니다.
        }
    }

    // --- 버튼 이벤트 함수 ---

    // 게임 오버 패널의 '다시 시작' 버튼에 연결할 함수
    public void RestartLevel()
    {
        // 현재 씬을 다시 로드하여 게임 재시작
        Time.timeScale = 1; // 멈춘 게임 시간을 다시 흐르게 한 후
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}