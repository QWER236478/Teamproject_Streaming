using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.XR; // VR 입력
using UnityEngine.XR.Interaction.Toolkit; // XR 상호작용

[RequireComponent(typeof(XRSimpleInteractable))]
public class HideAndSeekManager : MonoBehaviour
{
    [Header("상호작용 설정 (리모컨)")]
    public GameObject targetRemote; // 획득해야 할 리모컨 오브젝트
    public MonologueSystem monologueSystem; // 실패 시 대사 출력
    [TextArea] public string failMessage = "전원을 끄려면 리모컨이 필요하다.";

    [Header("붉은 시야광 (Spot Light)")]
    public Light redSpotLight;
    public Light redPointLight;
    public Light redAreaLight;

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
    public Material screenMaterial;

    [Header("사운드 클립")]
    public AudioClip RadarON;
    public AudioClip PowerOff;

    [Header("사운드 옵션")]
    public float radarVolume = 1.0f;
    public float sfxVolume = 1.0f;
    [Range(0f, 1f)] public float spatialBlend = 1f;
    public float minDistance = 10f;
    public float maxDistance = 100f;
    public AudioRolloffMode rolloff = AudioRolloffMode.Linear;

    // 내부 변수
    private Quaternion initLocalRot;
    private bool isDead = false;
    private Transform headTransform;
    private float spotInitIntensity;

    private AudioSource radarSource;
    private AudioSource sfxSource;

    // XR 및 상태 제어 변수
    private bool isHovered = false;
    private bool wasPressed = false;
    private bool isActive = false; // WakeUp() 호출 전까지는 false

    void Awake()
    {
        if (redSpotLight)
            initLocalRot = redSpotLight.transform.localRotation;

        if (headObject)
            headTransform = headObject.transform;

        spotInitIntensity = maxIntensity;

        // 오디오 소스 2개 생성 (루프용, 효과음용)
        radarSource = gameObject.AddComponent<AudioSource>();
        radarSource.playOnAwake = false;
        radarSource.loop = true;
        radarSource.spatialBlend = spatialBlend;
        radarSource.volume = radarVolume;
        radarSource.minDistance = minDistance;
        radarSource.maxDistance = maxDistance;
        radarSource.rolloffMode = rolloff;

        sfxSource = gameObject.AddComponent<AudioSource>();
        sfxSource.playOnAwake = false;
        sfxSource.loop = false;
        sfxSource.spatialBlend = spatialBlend;
        sfxSource.volume = sfxVolume;
    }

    void Start()
    {
        // [중요] 시작할 때는 모든 빛과 소리를 꺼둡니다. (대기 상태)
        if (redSpotLight) redSpotLight.enabled = false;
        if (redPointLight) redPointLight.enabled = false;
        if (redAreaLight) redAreaLight.enabled = false;

        TurnOffScreenEmission(); // 처음엔 화면도 꺼둠 (필요 시 켜두려면 이 줄 삭제)
    }

    // =================================================================
    // 외부 트리거(TVwomanStart)가 호출하는 함수
    // =================================================================
    public void WakeUp()
    {
        if (isActive || isDead) return; // 이미 켜졌거나 죽었으면 무시
        isActive = true;

        // 1. 라이트 켜기
        if (redSpotLight)
        {
            redSpotLight.enabled = true;
            redSpotLight.intensity = maxIntensity;
            redSpotLight.range = maxRange;
            redSpotLight.spotAngle = angle;
            redSpotLight.transform.localRotation = Quaternion.Euler(pitchDown, 0f, 0f) * initLocalRot;
        }
        if (redPointLight) redPointLight.enabled = true;
        if (redAreaLight) redAreaLight.enabled = true;

        // 2. 소리 켜기
        if (RadarON && radarSource)
        {
            radarSource.clip = RadarON;
            radarSource.Play();
        }

        // 3. 머티리얼 켜기
        if (screenMaterial) screenMaterial.EnableKeyword("_EMISSION");
        if (screenMaterial && screenMaterial.HasProperty("_EmissionColor"))
            screenMaterial.SetColor("_EmissionColor", Color.white); // 흰색 또는 원래 색
    }

    // =================================================================
    // XR Interaction Toolkit 이벤트 연결용 (Hover Entered / Exited)
    // =================================================================
    public void SetHoverState(bool state)
    {
        isHovered = state;
    }

    void Update()
    {
        // 죽었거나, 아직 깨어나지 않았다면 작동 안 함
        if (isDead || !isActive) return;

        HandleEffect(); // 깜빡임, 회전
        HandleInput();  // A버튼 감지
    }

    void HandleEffect()
    {
        // 조명 깜빡임
        if (redSpotLight && enableFlicker)
        {
            float n = Mathf.PerlinNoise(Time.time * flickerSpeed, 0f);
            float var = Mathf.Lerp(1f - flickerAmount, 1f + flickerAmount, n);
            redSpotLight.intensity = spotInitIntensity * var;
        }

        // 머리 회전
        if (enableRotation && headTransform)
            headTransform.Rotate(Vector3.up * rotationSpeed * Time.deltaTime, Space.Self);
    }

    void HandleInput()
    {
        // 바라보고 있지 않으면 입력 체크 안 함
        if (!isHovered) return;

        // 오른쪽 컨트롤러 A버튼 감지
        var inputDevices = new List<InputDevice>();
        InputDevices.GetDevicesAtXRNode(XRNode.RightHand, inputDevices);

        bool isPressed = false;
        if (inputDevices.Count > 0)
        {
            if (inputDevices[0].TryGetFeatureValue(CommonUsages.primaryButton, out bool pressedValue))
            {
                isPressed = pressedValue;
            }
        }

        // 버튼을 '딱' 눌렀을 때 (중복 방지)
        if (isPressed && !wasPressed)
        {
            TryInteract();
        }
        wasPressed = isPressed;
    }

    void TryInteract()
    {
        // 리모컨이 씬에 있고, 비활성화(획득) 되었는지 확인
        bool hasRemote = (targetRemote != null && !targetRemote.activeSelf);

        if (hasRemote)
        {
            Die(); // 성공: 적 처치
        }
        else
        {
            // 실패: 메시지 출력
            if (monologueSystem != null)
            {
                monologueSystem.ShowMonologue(failMessage);
            }
        }
    }

    public void Die()
    {
        if (isDead) return;
        isDead = true;

        // 애니메이션
        if (animator) animator.SetTrigger(deathTriggerName);

        // 모든 조명 끄기
        if (redSpotLight) redSpotLight.enabled = false;
        if (redPointLight) redPointLight.enabled = false;
        if (redAreaLight) redAreaLight.enabled = false;

        // 소리 정지 및 사망음 재생
        if (radarSource && radarSource.isPlaying)
            radarSource.Stop();

        if (PowerOff && sfxSource)
            sfxSource.PlayOneShot(PowerOff, sfxVolume);

        // 화면 끄기
        TurnOffScreenEmission();

        // 상호작용 비활성화 (더 이상 클릭 안 되게)
        var interactable = GetComponent<XRSimpleInteractable>();
        if (interactable) interactable.enabled = false;
    }

    void TurnOffScreenEmission()
    {
        if (!screenMaterial) return;

        if (screenMaterial.HasProperty("_EmissionColor"))
            screenMaterial.SetColor("_EmissionColor", Color.black);

        screenMaterial.DisableKeyword("_EMISSION");
    }
}