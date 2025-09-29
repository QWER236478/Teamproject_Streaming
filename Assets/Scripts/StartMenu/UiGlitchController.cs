using System.Collections;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Image))]
public class UiGlitchController : MonoBehaviour
{
    public Vector2 calmInterval = new Vector2(0.8f, 2.2f); // 발작 사이 간격
    public Vector2 burstIntensity = new Vector2(0.4f, 0.9f); // 발작 세기 범위
    public float burstTime = 0.25f; // 발작 유지 시간
    public float fadeOutTime = 0.35f; // 사그라드는 시간

    Material _mat;
    int _idIntensity;

    void Awake()
    {
        var img = GetComponent<Image>();
        _mat = new Material(img.material);   // 인스턴스화
        img.material = _mat;
        _idIntensity = Shader.PropertyToID("_Intensity");
        _mat.SetFloat(_idIntensity, 0f);
    }

    IEnumerator Start()
    {
        while (true)
        {
            // 대기
            yield return new WaitForSeconds(Random.Range(calmInterval.x, calmInterval.y));

            // 발작
            float peak = Random.Range(burstIntensity.x, burstIntensity.y);
            _mat.SetFloat(_idIntensity, peak);
            yield return new WaitForSeconds(burstTime);

            // 서서히 진정
            float t = 0f;
            float start = peak;
            while (t < fadeOutTime)
            {
                t += Time.deltaTime;
                float k = 1f - Mathf.Clamp01(t / fadeOutTime);
                _mat.SetFloat(_idIntensity, start * k);
                yield return null;
            }
            _mat.SetFloat(_idIntensity, 0f);
        }
    }
}