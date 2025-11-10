using UnityEngine;
using System.Collections; // Coroutine을 사용하려면 필요합니다.

public class CameraFOVChanger : MonoBehaviour
{
    [Header("카메라 설정")]
    // FOV를 변경할 대상 카메라 컴포넌트입니다.
    // 'public'으로 선언되어 Unity 에디터에서 할당할 수 있습니다.
    public Camera targetCamera;

    [Header("FOV Lerp 설정")]
    // FOV의 시작 값 (A) 입니다.
    public float startFOV = 60f;
    // FOV의 목표 값 (B) 입니다.
    public float targetFOV = 30f;
    // FOV 전환에 걸리는 시간 (초) 입니다.
    public float transitionDuration = 1.0f;

    private bool isTransitioning = false; // 현재 전환 중인지 확인하는 플래그

    void Update()
    {
        // 'Y' 키를 누르고, 현재 전환 중이 아니라면 전환을 시작합니다.
        if (Input.GetKeyDown(KeyCode.Y) && !isTransitioning)
        {
            // 현재 FOV가 목표 FOV와 다를 때만 전환 시작 (선택 사항)
            if (targetCamera.fieldOfView != targetFOV)
            {
                StartCoroutine(LerpFOV(targetFOV));
            }
            // 이미 targetFOV라면, startFOV로 되돌리는 로직을 추가할 수도 있습니다.
            else if (targetCamera.fieldOfView == targetFOV)
            {
                StartCoroutine(LerpFOV(startFOV));
            }
        }
    }

    // FOV를 Lerp를 사용하여 부드럽게 전환하는 코루틴
    IEnumerator LerpFOV(float newFOV)
    {
        isTransitioning = true; // 전환 시작
        float timeElapsed = 0f;
        float currentFOV = targetCamera.fieldOfView;

        // FOV를 변경할 대상이 없다면 경고를 띄우고 종료합니다.
        if (targetCamera == null)
        {
            Debug.LogError("대상 카메라가 할당되지 않았습니다!");
            isTransitioning = false;
            yield break; // 코루틴 즉시 종료
        }

        while (timeElapsed < transitionDuration)
        {
            // Lerp 계산: 현재 시간 / 총 시간의 비율만큼 FOV를 보간합니다.
            // timeElapsed / transitionDuration는 0에서 1 사이의 값을 가집니다.
            targetCamera.fieldOfView = Mathf.Lerp(currentFOV, newFOV, timeElapsed / transitionDuration);

            timeElapsed += Time.deltaTime; // 다음 프레임까지의 시간을 더합니다.
            yield return null; // 다음 프레임까지 대기
        }

        // Lerp가 끝난 후, 정확히 목표 값으로 설정하여 오차를 없앱니다.
        targetCamera.fieldOfView = newFOV;
        isTransitioning = false; // 전환 완료
    }
}