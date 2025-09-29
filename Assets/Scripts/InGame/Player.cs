using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(CapsuleCollider))]
public class Player : MonoBehaviour
{
    [Header("Move")]
    public float moveSpeed = 3.5f;
    public float runMultiplier = 1.7f;

    [Header("Mouse Look (Yaw Only)")]
    public float mouseSensitivity = 180f;   // 도/초 느낌 (Mouse X에 곱)
    public bool lockCursor = true;

    Rigidbody rb;
    float yaw;   // 수평 회전만

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        rb.useGravity = true;
        rb.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ; // 넘어짐 방지

        yaw = transform.eulerAngles.y;

        if (lockCursor)
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
    }

    void Update()
    {
        // --- 수평 회전: 마우스 좌/우 ---
        float mx = Input.GetAxis("Mouse X");      // 좌: -, 우: +
        yaw += mx * mouseSensitivity * Time.deltaTime;
        // yaw만 적용 (pitch/roll 없음)
        transform.rotation = Quaternion.Euler(0f, yaw, 0f);

        // 커서 토글(옵션)
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            lockCursor = !lockCursor;
            Cursor.lockState = lockCursor ? CursorLockMode.Locked : CursorLockMode.None;
            Cursor.visible = !lockCursor;
        }
    }

    void FixedUpdate()
    {
        // --- 이동: WASD ---
        float h = Input.GetAxisRaw("Horizontal"); // A/D
        float v = Input.GetAxisRaw("Vertical");   // W/S
        Vector3 dir = (transform.forward * v + transform.right * h).normalized;

        float speed = moveSpeed * (Input.GetKey(KeyCode.LeftShift) ? runMultiplier : 1f);

        // 현재 y속도(낙하/점프 없음) 유지하며 평면 이동
        Vector3 vel = dir * speed;
        vel.y = rb.velocity.y;

        rb.velocity = vel;
    }
}