using UnityEngine;

public class CenterRayInteractor : MonoBehaviour
{
    [Header("카메라 & 입력")]
    public Camera cam;
    public float interactRange = 3f;
    public LayerMask interactMask = ~0;
    public KeyCode interactKey = KeyCode.F;

    [Header("디버그")]
    public bool debugRay = true;

    // 내부 상태
    private Transform lastHit;  // 직전 프레임에 맞았던 오브젝트

    void Reset() => cam = Camera.main;

    void Update()
    {
        if (!cam) cam = Camera.main;
        if (!cam) return;

        // 중앙에서 레이 쏘기
        Ray ray = new Ray(cam.transform.position, cam.transform.forward);
        if (debugRay)
            Debug.DrawRay(ray.origin, ray.direction * interactRange, Color.cyan);

        // 레이 충돌 감지
        if (Physics.Raycast(ray, out RaycastHit hit, interactRange, interactMask, QueryTriggerInteraction.Collide))
        {
            Transform hitObj = hit.collider.transform;

            // 새로운 오브젝트에 맞았을 때만 UI ON
            if (hitObj != lastHit)
            {
                // 이전 오브젝트의 UI OFF
                if (lastHit != null)
                    ToggleCanvas(lastHit, false);

                // 새로운 오브젝트의 UI ON
                ToggleCanvas(hitObj, true);
                lastHit = hitObj;
            }

            // 상호작용 키 입력 시 IInteractable 실행
            if (Input.GetKeyDown(interactKey) || Input.GetMouseButtonDown(0))
            {
                var interactable = hit.collider.GetComponentInParent<IInteractable>();
                if (interactable != null)
                {
                    interactable.Interact();
                    return;
                }

                hit.collider.gameObject.SendMessage("OnInteract", SendMessageOptions.DontRequireReceiver);
            }
        }
        else
        {
            // 아무것도 안 맞으면 이전 오브젝트의 UI 끄기
            if (lastHit != null)
            {
                ToggleCanvas(lastHit, false);
                lastHit = null;
            }
        }
    }

    /// <summary>
    /// 대상 오브젝트나 부모에 Canvas가 있으면 On/Off 전환
    /// </summary>
    private void ToggleCanvas(Transform target, bool active)
    {
        if (!target) return;

        Canvas canvas = target.GetComponentInChildren<Canvas>(true);
        if (canvas != null)
            canvas.gameObject.SetActive(active);
    }
}

// 인터랙트 가능한 대상
public interface IInteractable
{
    void Interact();
}