using UnityEngine;
using System.Collections;

public class HideAndSeekManager : MonoBehaviour
{
    [Header("붉은 시야광 (Spot Light)")]
    public Light redSpotLight; // 몬스터 머리 스포트라이트
    public Light redPointLight; // 보조 포인트 라이트
    public Light redAreaLight; //보조 라이트(적이 있다는 것을 나타내기 위함


    [Tooltip("부채꼴을 약간 아래로 기울이고 싶으면 각도(+는 아래로)")]
    public float pitchDown = 10f;

    [Header("빛 세기 설정")]
    public float maxIntensity = 8f;
    public float maxRange = 15f;
    public float angle = 60f;

    [Header("기본 깜빡임")]
    public bool enableFlicker = true;
    public float flickerSpeed = 10f;
    public float flickerAmount = 0.25f;

    [Header("머리 회전")]
    public GameObject headObject;
    public bool enableRotation = true;
    public float rotationSpeed = 30f;

    [Header("애니메이션")]
    public Animator animator;
    public string deathTriggerName = "Die";

    [Header("화면 머티리얼(Emission 끌 대상)")]
    public Material screenMaterial; // 에셋 직접 변경

    [Header("사운드 클립")]
    public AudioClip RadarON;    // 시작부터 무한 루프
    public AudioClip PowerOff;   // Die 시 1회

    [Header("사운드 옵션")]
    [Tooltip("RadarON 루프 볼륨 (클립이 낮게 믹싱되었다면 1보다 크게 사용 가능)")]
    public float radarVolume = 1.0f;
    [Tooltip("PowerOff 원샷 볼륨")]
    public float sfxVolume = 1.0f;
    [Tooltip("3D=1, 2D=0 (루프/원샷 둘 다 적용)")]
    [Range(0f, 1f)] public float spatialBlend = 1f;
    [Tooltip("3D 감쇠 최소/최대 거리 (루프에 적용)")]
    public float minDistance = 10f;
    public float maxDistance = 100f;
    public AudioRolloffMode rolloff = AudioRolloffMode.Linear;

    private Quaternion initLocalRot;
    private bool isDead = false;
    private Transform headTransform;
    private float spotInitIntensity;

    // 오디오소스(루프/효과음)
    private AudioSource radarSource;
    private AudioSource sfxSource;

    void Awake()
    {
        if (redSpotLight)
            initLocalRot = redSpotLight.transform.localRotation;

        if (headObject)
            headTransform = headObject.transform;

        spotInitIntensity = maxIntensity;

        // Radar 루프용 소스
        radarSource = gameObject.AddComponent<AudioSource>();
        radarSource.playOnAwake = false;
        radarSource.loop = true;
        radarSource.spatialBlend = spatialBlend;
        radarSource.volume = radarVolume;
        radarSource.minDistance = minDistance;
        radarSource.maxDistance = maxDistance;
        radarSource.rolloffMode = rolloff;

        // SFX 원샷용 소스
        sfxSource = gameObject.AddComponent<AudioSource>();
        sfxSource.playOnAwake = false;
        sfxSource.loop = false;
        sfxSource.spatialBlend = spatialBlend;
        sfxSource.volume = sfxVolume;
    }

    void Start()
    {
        // 라이트 초기화
        if (redSpotLight)
        {
            redSpotLight.intensity = maxIntensity;
            redSpotLight.range = maxRange;
            redSpotLight.spotAngle = angle;
            redSpotLight.enabled = true;
            redSpotLight.transform.localRotation = Quaternion.Euler(pitchDown, 0f, 0f) * initLocalRot;
        }
        if (redPointLight)
            redPointLight.enabled = true;

        // RadarON 루프 재생
        if (RadarON && radarSource)
        {
            radarSource.clip = RadarON;
            radarSource.volume = radarVolume;       // 시작부터 크게
            radarSource.spatialBlend = spatialBlend;
            radarSource.minDistance = minDistance;
            radarSource.maxDistance = maxDistance;
            radarSource.rolloffMode = rolloff;
            radarSource.Play();
        }
    }

    void Update()
    {
        if (isDead) return;

        // 미세 깜빡임
        if (redSpotLight && enableFlicker)
        {
            float n = Mathf.PerlinNoise(Time.time * flickerSpeed, 0f);
            float var = Mathf.Lerp(1f - flickerAmount, 1f + flickerAmount, n);
            redSpotLight.intensity = spotInitIntensity * var;
        }

        // 머리 회전
        if (enableRotation && headTransform)
            headTransform.Rotate(Vector3.up * rotationSpeed * Time.deltaTime, Space.Self);

        // 테스트: R → 사망
        if (Input.GetKeyDown(KeyCode.R))
            Die();
    }

    void Die()
    {
        if (isDead) return;
        isDead = true;

        // 애니메이션 트리거
        if (animator) animator.SetTrigger(deathTriggerName);

        // 라이트 끄기
        if (redSpotLight) redSpotLight.enabled = false;
        if (redPointLight) redPointLight.enabled = false;
        if (redAreaLight) redAreaLight.enabled = false;

        // 레이더 루프 정지
        if (radarSource && radarSource.isPlaying)
            radarSource.Stop();

        // PowerOff 재생 (원샷: 오브젝트가 사라져도 끝까지 재생되길 원하면 PlayClipAtPoint로 대체 가능)
        if (PowerOff && sfxSource)
            sfxSource.PlayOneShot(PowerOff, sfxVolume);

        // 스크린 Emission Off
        TurnOffScreenEmission();
    }

    void TurnOffScreenEmission()
    {
        if (!screenMaterial) return;

        if (screenMaterial.HasProperty("_EmissionColor"))
            screenMaterial.SetColor("_EmissionColor", Color.black);

        screenMaterial.DisableKeyword("_EMISSION");
    }
}