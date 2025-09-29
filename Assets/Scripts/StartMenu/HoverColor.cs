using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[RequireComponent(typeof(Graphic))]
public class HoverColor : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [Header("색상 설정")]
    public Color normalColor = new Color32(87, 87, 87, 255);   // 기본
    public Color hoverColor = new Color32(255, 255, 255, 255); // 마우스 올라왔을 때

    Graphic g;

    void Awake()
    {
        g = GetComponent<Graphic>();
        g.color = normalColor;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        g.color = hoverColor;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        g.color = normalColor;
    }
}