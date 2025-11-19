using UnityEngine;

public class SpotlightController : MonoBehaviour
{
    [SerializeField] private Animator animator;
    [SerializeField] private Light spotlight;

    private bool isOn = false;

    void Update()
    {
       
    }
    public void ToggleSpotlight()
    {
        // G키를 눌렀을 때 실행되던 코드를 그대로 가져옵니다.
        isOn = !isOn;
        animator.SetBool("isOn", isOn);

        Debug.Log("ToggleSpotlight 호출! 새 상태 (isOn): " + isOn);
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
