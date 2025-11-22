using UnityEngine;
using UnityEngine.UI;

public class TVGlitchEffect : MonoBehaviour
{
    [Header("시스템 연결")]
    public UIManager uiManager;
    [Header("연결")]
    public RawImage noiseImage;
    public CanvasGroup noiseCanvas;
    public Transform targetLight;   // 몬스터의 전구(Radar)
    public Camera playerCamera;

    [Header("감지 설정")]
    public float maxDistance = 15.0f; // 거리
    [Range(0f, 1f)] public float monsterViewAngle = 0.8f; // 몬스터 시야각 (0.8 = 앞만 봄, 0.5 = 넓게 봄)
    [Header("배터리 설정")]
    public float extraDrainRate = 15.0f;
    [Header("멀미 방지 설정 (중요!)")]
    [Range(0f, 1f)] public float maxIntensity = 0.2f; // ★최대 투명도 (0.2 추천: 아주 흐릿하게)
    public float fadeSpeed = 2.0f; // 서서히 켜지는 속도 (낮을수록 부드러움)
    public float shakeAmount = 0.05f; // ★흔들림 강도 (낮을수록 덜 어지러움)

    private bool isInMap = false; // 맵 구역 체크용

    void Start()
    {
        if (playerCamera == null) playerCamera = Camera.main;
        if (noiseCanvas != null) noiseCanvas.alpha = 0f;
    }

    void Update()
    {
        if (noiseCanvas == null || targetLight == null) return;

        float targetAlpha = 0f;

        // 1. 맵 안에 있고 + 거리가 가까우면 계산 시작
        if (isInMap)
        {
            float dist = Vector3.Distance(transform.position, targetLight.position);

            if (dist <= maxDistance)
            {
                // 2. [핵심 변경] 몬스터가 나를 보고 있는가?
                // (몬스터의 앞방향 벡터와, 몬스터->플레이어 방향 벡터 비교)
                Vector3 dirToPlayer = (playerCamera.transform.position - targetLight.position).normalized;
                float dot = Vector3.Dot(targetLight.forward, dirToPlayer);

                // 몬스터가 나를 보고 있다면 (각도 안쪽이면)
                if (dot >= monsterViewAngle)
                {
                    targetAlpha = 1f;
                    if (uiManager != null)
                    {
                        // Time.deltaTime을 곱해야 프레임 상관없이 초당 15만큼 깎입니다.
                        uiManager.DrainBatteryImmediate(extraDrainRate * Time.deltaTime);
                    }
                }
            }
        }

        // 3. 투명도 적용 (최대치를 maxIntensity로 제한해서 은은하게 만듦)
        float finalAlpha = Mathf.Lerp(noiseCanvas.alpha, targetAlpha, Time.deltaTime * fadeSpeed);
        noiseCanvas.alpha = finalAlpha; // 캔버스 그룹 투명도 조절

        // 4. 색깔 투명도 조절 (이중 장치)
        if (noiseImage != null)
        {
            // 너무 진하지 않게 maxIntensity 곱하기
            float visualAlpha = finalAlpha * maxIntensity;
            noiseImage.color = new Color(1, 1, 1, visualAlpha);

            // 5. 흔들기 (보일 때만 + 살살 흔들기)
            if (visualAlpha > 0.01f)
            {
                Rect uv = noiseImage.uvRect;
                // shakeAmount만큼만 살짝살짝 움직임 (멀미 방지)
                uv.x = Random.Range(-shakeAmount, shakeAmount);
                uv.y = Random.Range(-shakeAmount, shakeAmount);
                noiseImage.uvRect = uv;
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("MapZone")) isInMap = true;
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("MapZone")) isInMap = false;
    }

}