using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public float moveSpeed = 5f;
    public float runSpeed = 7f;
    public float jumpPower = 5f;
    public float grvity = -9.81f;
    public float mouseSensitivity = 3f;

    float xRotation = 0f;
    CharacterController controller;
    Transform cam;
    Vector3 velocity;
    bool isGrounded;

    private float _nextHitTime;
    private Camera _cam;
    public Inventory inventory;
    InventoryUI invenUI;

    private void Awake()
    {
        //커서 잠금/ 숨기기 로직
        LockCursor();

        controller = GetComponent<CharacterController>();
        if (cam == null)
        {
            cam = GetComponentInChildren<Camera>()?.transform;
        }

        _cam = Camera.main;
        if(inventory == null) inventory = gameObject.AddComponent<Inventory>();
        invenUI = FindObjectOfType<InventoryUI>();
    }

    private void LockCursor()
    {
        // 1. 마우스 커서 숨기기
        Cursor.visible = false;

        // 2. 마우스 커서를 화면 중앙에 잠그기
        Cursor.lockState = CursorLockMode.Locked;

    }


    // Update is called once per frame
    void Update()
    {
        HandleMove();
        HandleLook();

         if (Input.GetKeyDown(KeyCode.Escape))
        {
             Cursor.visible = true;
             Cursor.lockState = CursorLockMode.None;
        }
    }


    void HandleMove()
    {
        isGrounded = controller.isGrounded;
        if (isGrounded && velocity.y < 0)
            velocity.y = -2f;
        float h = Input.GetAxis("Horizontal");
        float v = Input.GetAxis("Vertical");
        Vector3 move = transform.right * h + transform.forward * v;
        controller.Move(move * moveSpeed * Time.deltaTime);
        if (Input.GetButtonDown("Jump") && isGrounded)
            velocity.y = Mathf.Sqrt(jumpPower * -2f * grvity);
        velocity.y += grvity * Time.deltaTime;
        controller.Move(velocity * Time.deltaTime);
    }

    void HandleLook()
    {
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity;
        transform.Rotate(Vector3.up * mouseX);
        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, -80f, 80f);
        if (cam != null)
            cam.localRotation = Quaternion.Euler(xRotation, 0f, 0f);

    }
}