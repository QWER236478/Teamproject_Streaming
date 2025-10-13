using UnityEngine;

public class SpotlightController : MonoBehaviour
{
    [SerializeField] private Animator animator;
    [SerializeField] private Light spotlight;

    private bool isOn = false;

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.G)) // 이거는 자유롭게 바꾸어도 무방 -> 입력부!
        {
            // 상태 전환
            isOn = !isOn;
            animator.SetBool("isOn", isOn);
        }
    }

    // 애니메이션 이벤트용 메서드
    public void TurnLightOn()
    {
        if (spotlight != null)
            spotlight.enabled = true;
    }

    public void TurnLightOff()
    {
        if (spotlight != null)
            spotlight.enabled = false;
    }
}
