using UnityEngine;
using UnityEngine.UI; // Text / Image / TMP 모두 Graphic 상속

[RequireComponent(typeof(Graphic))]
public class UIBlinkSimple : MonoBehaviour
{
    [Tooltip("true=켜짐/꺼짐, false=부드러운 펄스")]
    public bool hardBlink = true;

    [Tooltip("하드: on/off 유지 시간(초), 부드러움: 한 사이클 시간(초) 느낌")]
    public float interval = 0.5f;

    [Range(0f, 1f)] public float minAlpha = 0.2f;
    [Range(0f, 1f)] public float maxAlpha = 1f;

    public bool useUnscaledTime = true; // 일시정지 중에도 깜빡임

    Graphic g;
    float timer; bool on = true;

    void Awake() { g = GetComponent<Graphic>(); }
    void OnEnable() { timer = 0f; on = true; SetA(maxAlpha); }

    void Update()
    {
        float dt = useUnscaledTime ? Time.unscaledDeltaTime : Time.deltaTime;

        if (hardBlink)
        {
            timer += dt;
            if (timer >= interval)
            {
                timer = 0f;
                on = !on;
                SetA(on ? maxAlpha : minAlpha);
            }
        }
        else
        {
            float t = (useUnscaledTime ? Time.unscaledTime : Time.time) % interval / interval;
            float a = Mathf.Lerp(minAlpha, maxAlpha, Mathf.PingPong(t * 2f, 1f));
            SetA(a);
        }
    }

    void SetA(float a)
    {
        var c = g.color;
        c.a = a;
        g.color = c;
    }
}