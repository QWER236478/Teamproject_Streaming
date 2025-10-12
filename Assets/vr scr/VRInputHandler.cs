using UnityEngine;
using UnityEngine.InputSystem;

public class VRInputHandler : MonoBehaviour
{
    public UIManager uiManager;

    // VR 컨트롤러 액션을 위한 변수 (향후 연결 시 사용)
    public InputActionProperty gripAction;
    public InputActionProperty triggerAction;

    void OnEnable()
    {
        // VR 액션 활성화 및 이벤트 연결
        gripAction.action.Enable();
        triggerAction.action.Enable();

        gripAction.action.performed += OnGripPressed;
        triggerAction.action.performed += OnTriggerPressed;
    }

    void OnDisable()
    {
        // VR 액션 비활성화 및 이벤트 연결 해제
        gripAction.action.performed -= OnGripPressed;
        triggerAction.action.performed -= OnTriggerPressed;

        gripAction.action.Disable();
        triggerAction.action.Disable();
    }

    // Update 함수: 키보드 입력 체크를 위해 사용 (안정적인 임시 테스트용)
    void Update()
    {
        // UIManager 연결 체크
        if (uiManager == null)
        {
            return;
        }

        // === Q 키 처리 (카메라 토글) ===
        // 키를 누르는 순간 한 번만 호출 (가장 안정적인 단일 호출 방법)
        if (Input.GetKeyDown(KeyCode.Q))
        {
            uiManager.ToggleCamera();
            Debug.Log("Q 키: 카메라 토글");
        }

        // === W 키 처리 (플래시 토글) ===
        // 키를 누르는 순간 한 번만 호출 (가장 안정적인 단일 호출 방법)
        if (Input.GetKeyDown(KeyCode.W))
        {
            uiManager.ToggleFlash();
            Debug.Log("W 키: 플래시 토글");
        }
    }


    // VR Grip Action 연결 함수 (Q 키 대체)
    private void OnGripPressed(InputAction.CallbackContext context)
    {
        if (uiManager != null)
        {
            // VR 컨트롤러가 연결되면 이 함수가 호출됩니다.
            uiManager.ToggleCamera();
        }
    }

    // VR Trigger Action 연결 함수 (W 키 대체)
    private void OnTriggerPressed(InputAction.CallbackContext context)
    {
        if (uiManager != null)
        {
            // VR 컨트롤러가 연결되면 이 함수가 호출됩니다.
            uiManager.ToggleFlash();
        }
    }
}
