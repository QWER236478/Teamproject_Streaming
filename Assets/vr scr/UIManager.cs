using UnityEngine;
using UnityEngine.UI;
using TMPro; // TextMeshPro 사용
using UnityEngine.SceneManagement; // 씬 관리
using System.Collections.Generic;
using System.Collections;
using System.IO;
using UnityEngine.XR; // [중요] VR 입력 처리를 위해 필수

public class UIManager : MonoBehaviour
{
    // ==================================================================================
    // 1. 사진 촬영 및 갤러리 설정
    // ==================================================================================
    [Header("사진 촬영 설정")]
    public CanvasGroup photoDisplayCanvasGroup; // 촬영된 사진을 보여줄 패널
    public RawImage photoDisplayImage;          // 촬영된 사진을 표시할 RawImage
    public float photoDisplayDuration = 3f;     // 사진 표시 시간
    public string screenshotFolderName = "Screenshots"; // 저장 폴더명
    public Animator cameraStatus;               // 카메라 상태 애니메이터

    // ==================================================================================
    // 2. UI 요소 (타이머, 배터리, REC)
    // ==================================================================================
    [Header("UI 표시 요소")]
    public TextMeshProUGUI timerText;           // 타이머 텍스트
    public Image recIndicator;                  // REC 깜빡임 이미지
    public float blinkInterval = 0.5f;          // 깜빡임 간격

    [Header("배터리 설정")]
    public Image batterySlider;                 // 배터리 게이지 이미지
    public TextMeshProUGUI batteryPercentText;  // 배터리 퍼센트 텍스트
    public float initialBatteryLevel = 100f;    // 초기 배터리 양

    [System.Serializable]
    public class BatteryColorThreshold
    {
        public float percentage; // 기준 퍼센트
        public Color color;      // 적용 색상
    }
    public List<BatteryColorThreshold> batteryColorThresholds = new List<BatteryColorThreshold>();

    // ==================================================================================
    // 3. 카메라 기능 (플래시, 줌, 화면 페이드)
    // ==================================================================================
    [Header("카메라 시스템")]
    public GameObject cameraUIRoot;             // 카메라 UI 전체 부모
    public Light flashLight;                    // 실제 플래시 라이트 오브젝트

    public float normalDrainRate = 1f;          // 평상시 소모율
    public float flashDrainIncrease = 5f;       // 플래시 켤 때 추가 소모율
    public float fadeDuration = 1.0f;           // UI 페이드 시간

    [Header("줌 설정")]
    [Tooltip("카메라 컴포넌트를 연결해주세요. (없으면 자동 검색)")]
    public Camera mainCamera;
    public float camMinFov = 30f;               // 줌 인 FOV
    public float camMaxFov = 90f;               // 줌 아웃 FOV (기본)
    public float zoomDuration = 0.5f;           // 줌 전환 시간
    public AnimationCurve zoomCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);

    // ==================================================================================
    // 4. 사운드 및 효과
    // ==================================================================================
    [Header("사운드 및 효과")]
    public AudioSource audioSource;
    public AudioClip photoCaptureSound;
    public CanvasGroup shutterFlashCanvasGroup; // 촬영 시 하얀 화면 효과 (선택사항)
    public float visualFlashDuration = 0.1f;

    // ==================================================================================
    // 5. 게임 상태 관리 (게임오버, HUD)
    // ==================================================================================
    [Header("게임 상태 UI")]
    public CanvasGroup gameOverCanvasGroup;
    public CanvasGroup inGameHudCanvasGroup;
    public float fadeOutTimeFactor = 1.0f;

    // ==================================================================================
    // 내부 변수
    // ==================================================================================
    private float timeElapsed = 0f;
    private float currentBatteryLevel;
    private bool isGameOver = false;
    private bool isCameraOn = true;
    private bool isFlashOn = false;
    private bool isPhotoTaken = false;

    // 줌 관련 변수
    private bool isZoomed = false;
    private float zoomTime = 0f;
    private float zoomStartFov;

    // VR 입력 관련 변수
    private InputDevice targetDevice; // 오른쪽 컨트롤러
    private bool wasGripPressed = false;

    // ==================================================================================
    // Start Function
    // ==================================================================================
    void Start()
    {
        // 1. 기본 상태 초기화
        currentBatteryLevel = initialBatteryLevel;
        Time.timeScale = 1;

        // 2. HUD 초기화
        if (inGameHudCanvasGroup != null)
        {
            inGameHudCanvasGroup.alpha = 1f;
            SetCanvasGroupState(inGameHudCanvasGroup, true);
        }

        // 3. 게임 오버 패널 초기화
        if (gameOverCanvasGroup != null)
        {
            gameOverCanvasGroup.alpha = 0f;
            SetCanvasGroupState(gameOverCanvasGroup, false);
        }

        // 4. 카메라 및 플래시 초기화
        SetCameraActive(true);
        if (flashLight != null) flashLight.enabled = false;

        // 5. 줌 기능 초기화
        if (mainCamera == null) mainCamera = FindObjectOfType<Camera>();
        if (mainCamera != null)
        {
            mainCamera.fieldOfView = camMaxFov;
            zoomStartFov = camMaxFov;
        }
        zoomTime = zoomDuration; // 시작 시 줌 애니메이션 방지

        // 6. 배터리 UI 초기화
        UpdateBatteryUI();
        batteryColorThresholds.Sort((a, b) => a.percentage.CompareTo(b.percentage));

        // 7. 사진 결과창 초기화
        if (photoDisplayCanvasGroup != null)
        {
            photoDisplayCanvasGroup.alpha = 0f;
            SetCanvasGroupState(photoDisplayCanvasGroup, false);
        }

        // 8. REC 깜빡임 시작
        if (recIndicator != null)
        {
            InvokeRepeating("BlinkRec", 0.1f, blinkInterval);
        }
    }

    // ==================================================================================
    // Update Function
    // ==================================================================================
    void Update()
    {
        // VR 입력 감지 (그립 버튼 등)
        HandleVRInput();

        // 게임 오버 상태거나 카메라 꺼짐 상태 체크
        if (isGameOver || !isCameraOn)
        {
            if (isGameOver)
            {
                FadeOutHud();
                FadeInGameOverPanel();
            }
            if (!isGameOver) return;
        }

        // 메인 로직 실행
        UpdateTimer();
        DrainBattery();
        HandleZoomTransition();
    }

    // ==================================================================================
    // VR 입력 처리 함수 (오른쪽 그립 -> 플래시)
    // ==================================================================================
    private void HandleVRInput()
    {
        // 컨트롤러 장치가 유효하지 않으면 다시 찾기
        if (!targetDevice.isValid)
        {
            List<InputDevice> devices = new List<InputDevice>();
            InputDevices.GetDevicesAtXRNode(XRNode.RightHand, devices); // 오른손 찾기

            if (devices.Count > 0)
            {
                targetDevice = devices[0];
            }
        }

        // 장치가 연결되어 있다면 입력 확인
        if (targetDevice.isValid)
        {
            bool isGripPressedNow;
            // 그립 버튼 상태 가져오기
            if (targetDevice.TryGetFeatureValue(CommonUsages.gripButton, out isGripPressedNow))
            {
                // 버튼을 처음 누르는 순간 (Rising Edge)
                if (isGripPressedNow && !wasGripPressed)
                {
                    ToggleFlash(); // 플래시 토글
                }
                wasGripPressed = isGripPressedNow; // 상태 업데이트
            }
        }
    }

    // ==================================================================================
    // 기능 함수: 줌, 플래시, 카메라 토글
    // ==================================================================================

    // 줌 전환 로직 (Update에서 계속 호출)
    private void HandleZoomTransition()
    {
        if (mainCamera == null) return;
        if (cameraStatus != null) cameraStatus.SetBool("IsZoom", isZoomed);

        if (zoomTime < zoomDuration)
        {
            zoomTime += Time.deltaTime;
            float normalizedTime = Mathf.Clamp01(zoomTime / zoomDuration);
            float curveValue = zoomCurve.Evaluate(normalizedTime);

            float endFOV = isZoomed ? camMinFov : camMaxFov;
            mainCamera.fieldOfView = Mathf.Lerp(zoomStartFov, endFOV, curveValue);
        }
    }

    // 줌 토글 (외부 호출 가능, X키 등)
    public void ToggleZoom()
    {
        if (isGameOver) return;
        if (!isCameraOn) return;

        isZoomed = !isZoomed;
        zoomStartFov = mainCamera.fieldOfView;
        zoomTime = 0f; // 애니메이션 시작
    }

    // 플래시 토글 (VR 그립, W키 등)
    public void ToggleFlash()
    {
        if (isGameOver || !isCameraOn) return;
        if (flashLight == null)
        {
            Debug.LogError("Flash Light가 연결되지 않았습니다.");
            return;
        }

        isFlashOn = !isFlashOn;
        flashLight.enabled = isFlashOn;
    }

    // 플래시 강제 끄기
    private void TurnFlashOff()
    {
        isFlashOn = false;
        if (flashLight != null) flashLight.enabled = false;
    }

    // 카메라 전원 토글 (Q키, 그립 등)
    public void ToggleCamera()
    {
        if (isGameOver) return;

        isCameraOn = !isCameraOn;
        SetCameraActive(isCameraOn);

        // REC 표시 관리
        if (recIndicator != null)
        {
            if (isCameraOn) InvokeRepeating("BlinkRec", 0.1f, blinkInterval);
            else
            {
                CancelInvoke("BlinkRec");
                recIndicator.enabled = false;
            }
        }
    }

    private void SetCameraActive(bool active)
    {
        if (cameraUIRoot != null) cameraUIRoot.SetActive(active);

        if (!active)
        {
            TurnFlashOff(); // 카메라 꺼지면 플래시도 끔
            if (mainCamera != null) mainCamera.fieldOfView = camMaxFov; // 줌 리셋
            isZoomed = false;
            zoomTime = zoomDuration;
            zoomStartFov = camMaxFov;
        }
    }

    // ==================================================================================
    // 기능 함수: 타이머, 배터리
    // ==================================================================================
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
        if (isFlashOn) currentDrainRate += flashDrainIncrease;

        currentBatteryLevel -= currentDrainRate * Time.deltaTime;
        currentBatteryLevel = Mathf.Max(0f, currentBatteryLevel);

        UpdateBatteryUI();

        if (currentBatteryLevel <= 0 && !isGameOver)
        {
            isGameOver = true;
            ShowGameOver();
        }
    }

    // 배터리 즉시 소모 (이벤트용)
    public void DrainBatteryImmediate(float amount)
    {
        if (isGameOver) return;
        currentBatteryLevel -= amount;
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
        if (batterySlider != null) batterySlider.color = targetColor;
    }

    // ==================================================================================
    // 기능 함수: 사진 촬영
    // ==================================================================================
    public void TakePicture()
    {
        if (isGameOver || !isCameraOn || isPhotoTaken) return;
        isPhotoTaken = true;
        StartCoroutine(CaptureAndDisplayPhoto());
    }

    private IEnumerator CaptureAndDisplayPhoto()
    {
        // 1. HUD 숨김
        yield return StartCoroutine(FadeCanvasGroup(inGameHudCanvasGroup, 0f, fadeDuration * 0.5f));
        yield return new WaitForEndOfFrame();

        // 2. 소리 재생
        PlaySound(photoCaptureSound);

        // 3. 캡처
        Texture2D screenshotTexture = ScreenCapture.CaptureScreenshotAsTexture();

        if (screenshotTexture != null)
        {
            // 결과창에 표시
            if (photoDisplayImage != null)
            {
                photoDisplayImage.texture = screenshotTexture;
                photoDisplayImage.SetNativeSize();
            }

            // 파일 저장
            SaveScreenshot(screenshotTexture);

            // 결과창 보여주기
            yield return StartCoroutine(FadeCanvasGroup(photoDisplayCanvasGroup, 1f, fadeDuration * 0.5f));
            yield return new WaitForSecondsRealtime(photoDisplayDuration);

            // 결과창 숨기기 & HUD 복구
            yield return StartCoroutine(FadeCanvasGroup(photoDisplayCanvasGroup, 0f, fadeDuration * 0.5f));
            yield return StartCoroutine(FadeCanvasGroup(inGameHudCanvasGroup, 1f, fadeDuration * 0.5f));

            Destroy(screenshotTexture);
        }
        else
        {
            // 실패 시 HUD 복구
            StartCoroutine(FadeCanvasGroup(inGameHudCanvasGroup, 1f, fadeDuration * 0.5f));
        }

        isPhotoTaken = false;
    }

    private void SaveScreenshot(Texture2D screenshotTexture)
    {
        if (screenshotTexture == null) return;

        byte[] bytes = screenshotTexture.EncodeToPNG();
        string folderPath = Path.Combine(Application.persistentDataPath, screenshotFolderName);
        if (!Directory.Exists(folderPath)) Directory.CreateDirectory(folderPath);

        string fileName = $"Screenshot_{System.DateTime.Now:yyyyMMdd_HHmmss}.png";
        string filePath = Path.Combine(folderPath, fileName);

        try
        {
            File.WriteAllBytes(filePath, bytes);
            Debug.Log($"스크린샷 저장됨: {filePath}");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"저장 실패: {e.Message}");
        }
    }

    private void PlaySound(AudioClip clip)
    {
        if (audioSource != null && clip != null) audioSource.PlayOneShot(clip);
    }

    // ==================================================================================
    // 유틸리티 및 기타 함수
    // ==================================================================================

    // CanvasGroup 페이드 코루틴
    private IEnumerator FadeCanvasGroup(CanvasGroup cg, float endAlpha, float duration)
    {
        if (cg == null) yield break;
        float startAlpha = cg.alpha;
        float time = 0;

        if (endAlpha > startAlpha) SetCanvasGroupState(cg, true);

        while (time < duration)
        {
            time += Time.unscaledDeltaTime;
            float t = time / duration;
            cg.alpha = Mathf.Lerp(startAlpha, endAlpha, t);
            yield return null;
        }

        cg.alpha = endAlpha;
        if (endAlpha == 0) SetCanvasGroupState(cg, false);
    }

    private void SetCanvasGroupState(CanvasGroup cg, bool active)
    {
        if (cg == null) return;
        cg.interactable = active;
        cg.blocksRaycasts = active;
    }

    // 게임 오버 처리
    private void ShowGameOver()
    {
        Debug.Log("게임 오버!");
        Time.timeScale = 0;
        CancelInvoke("BlinkRec");
        if (recIndicator != null) recIndicator.enabled = false;
    }

    private void FadeOutHud()
    {
        if (inGameHudCanvasGroup == null) return;
        inGameHudCanvasGroup.alpha = Mathf.MoveTowards(inGameHudCanvasGroup.alpha, 0f, Time.unscaledDeltaTime / fadeOutTimeFactor);
        if (inGameHudCanvasGroup.alpha < 0.01f) SetCanvasGroupState(inGameHudCanvasGroup, false);
    }

    private void FadeInGameOverPanel()
    {
        if (gameOverCanvasGroup == null) return;
        if (gameOverCanvasGroup.alpha == 0f) SetCanvasGroupState(gameOverCanvasGroup, true);
        gameOverCanvasGroup.alpha = Mathf.MoveTowards(gameOverCanvasGroup.alpha, 1f, Time.unscaledDeltaTime / fadeOutTimeFactor);
    }

    void BlinkRec()
    {
        if (recIndicator != null) recIndicator.enabled = !recIndicator.enabled;
    }

    // 재시작 버튼 연결용
    public void RestartLevel()
    {
        Time.timeScale = 1;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    // 다음 게임 진행 (필요 시 구현)
    public void continueToNewGame() { }
}