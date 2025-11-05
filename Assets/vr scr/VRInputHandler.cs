using UnityEngine;
using UnityEngine.InputSystem;

public class VRInputHandler : MonoBehaviour
{
    public UIManager uiManager;

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
        if (uiManager == null)
        {
            return;
        }

        // === Q 키 처리 (카메라 토글) ===
        if (Input.GetKeyDown(KeyCode.Q))
        {
            uiManager.ToggleCamera();
            Debug.Log("Q 키: 카메라 토글");
        }

        // === W 키 처리 (플래시 토글) ===
        if (Input.GetKeyDown(KeyCode.W))
        {
            uiManager.ToggleFlash();
            Debug.Log("W 키: 플래시 토글");
        }

        // === X 키 처리 (줌 토글) ===  <--- 줌인/줌아웃 기능 추가
        // X 키를 누르는 순간 한 번만 호출
        if (Input.GetKeyDown(KeyCode.X))
        {
            uiManager.ToggleZoom();
            Debug.Log("X 키: 줌 토글");
        }
    }


    private void OnGripPressed(InputAction.CallbackContext context)
    {
        if (uiManager != null)
        {
            uiManager.ToggleCamera();
        }
    }

    private void OnTriggerPressed(InputAction.CallbackContext context)
    {
        if (uiManager != null)
        {
            uiManager.ToggleFlash();
        }
    }
}
