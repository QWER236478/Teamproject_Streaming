using UnityEngine;
using UnityEngine.XR; // 입력 감지용
using UnityEngine.XR.Interaction.Toolkit; // 상호작용 비활성화용
using System.Collections; // 시간 지연(Coroutine) 사용을 위해 추가

public class TrunkInteraction : MonoBehaviour
{
    [Header("시스템 연결")]
    public MonologueSystem monologueSystem;

    [Header("대사 설정")]
    [TextArea]
    public string openMessage = "일단 카메라부터 챙겨서 증거물들을 수집해야겠어.";

    [Header("애니메이션 설정")]
    public Animator trunkAnimator;
    public string animTriggerName = "Open"; // 애니메이터의 파라미터 이름 (기본값 Open)
    public float animationDuration = 1.5f;  // 가방이 열리는 데 걸리는 시간 (초)

    [Header("내부 아이템 (순차 잠금 해제)")]
    public BoxCollider cameraCollider;      // 카메라의 박스 콜라이더
    public BoxCollider documentCollider;    // 문서의 박스 콜라이더

    private bool isHovered = false; // 쳐다보고 있는지
    private bool wasPressed = false; // 버튼 중복 방지

    // 시작하자마자 내부 물건들의 콜라이더를 꺼서, 닫힌 가방을 뚫고 만져지는 것을 방지
    void Start()
    {
        if (cameraCollider != null) cameraCollider.enabled = false;
        if (documentCollider != null) documentCollider.enabled = false;
    }

    // XR Simple Interactable의 Hover 이벤트에 연결할 함수
    public void SetHoverState(bool state)
    {
        isHovered = state;
    }

    void Update()
    {
        // 1. 쳐다보고 있지 않으면 실행 안 함
        if (!isHovered) return;

        // 2. 오른쪽 컨트롤러 A버튼 감지
        InputDevice device = InputDevices.GetDeviceAtXRNode(XRNode.RightHand);
        bool isPressed = false;

        if (device.TryGetFeatureValue(CommonUsages.primaryButton, out bool pressedValue))
        {
            isPressed = pressedValue;
        }

        // 3. 버튼을 '딱' 눌렀을 때 실행
        if (isPressed && !wasPressed)
        {
            OpenTrunk();
        }

        wasPressed = isPressed;
    }

    void OpenTrunk()
    {
        // 1. 대사 출력
        if (monologueSystem != null)
        {
            monologueSystem.ShowMonologue(openMessage);
        }

        // 2. 애니메이션 실행
        if (trunkAnimator != null)
        {
            trunkAnimator.SetTrigger(animTriggerName);
        }

        // 3. 가방 열리는 시간만큼 기다렸다가 카메라만 켜기 (코루틴 시작)
        StartCoroutine(EnableCameraOnly());

        // 4. 상호작용 비활성화 
        // 스크립트 끄기
        this.enabled = false;

        // 상호작용 컴포넌트 끄기 (더 이상 Hover 이벤트 발생 안 함)
        var interactable = GetComponent<XRSimpleInteractable>();
        if (interactable != null)
        {
            interactable.enabled = false;
        }
    }

    // 애니메이션 시간 뒤에 실행될 함수
    IEnumerator EnableCameraOnly()
    {
        // 설정한 시간(초) 만큼 대기
        yield return new WaitForSeconds(animationDuration);

        // 카메라 콜라이더만 켜줌 (문서는 아직 켜지않음)
        if (cameraCollider != null)
        {
            cameraCollider.enabled = true;
        }
    }
}