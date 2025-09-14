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
    public float mouseSensitivity = 180f;   // ��/�� ���� (Mouse X�� ��)
    public bool lockCursor = true;

    Rigidbody rb;
    float yaw;   // ���� ȸ����

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        rb.useGravity = true;
        rb.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ; // �Ѿ��� ����

        yaw = transform.eulerAngles.y;

        if (lockCursor)
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
    }

    void Update()
    {
        // --- ���� ȸ��: ���콺 ��/�� ---
        float mx = Input.GetAxis("Mouse X");      // ��: -, ��: +
        yaw += mx * mouseSensitivity * Time.deltaTime;
        // yaw�� ���� (pitch/roll ����)
        transform.rotation = Quaternion.Euler(0f, yaw, 0f);

        // Ŀ�� ���(�ɼ�)
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            lockCursor = !lockCursor;
            Cursor.lockState = lockCursor ? CursorLockMode.Locked : CursorLockMode.None;
            Cursor.visible = !lockCursor;
        }
    }

    void FixedUpdate()
    {
        // --- �̵�: WASD ---
        float h = Input.GetAxisRaw("Horizontal"); // A/D
        float v = Input.GetAxisRaw("Vertical");   // W/S
        Vector3 dir = (transform.forward * v + transform.right * h).normalized;

        float speed = moveSpeed * (Input.GetKey(KeyCode.LeftShift) ? runMultiplier : 1f);

        // ���� y�ӵ�(����/���� ����) �����ϸ� ��� �̵�
        Vector3 vel = dir * speed;
        vel.y = rb.velocity.y;

        rb.velocity = vel;
    }
}