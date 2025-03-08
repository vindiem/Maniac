using System;
using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(CharacterController))]
public class PlayerMovement : MonoBehaviour
{
    private int health = 100;
    
    [Header("Movement")]
    [SerializeField] private float speed = 5f;
    [SerializeField] private float jumpHeight = 2f;
    [SerializeField] private float gravity = 9.81f;
    [SerializeField] private float mouseSensitivity = 100f;
    
    private CharacterController controller;
    private Vector3 velocity;
    private bool isGrounded;
    private Transform cameraTransform;
    private Animator animator;
    private float xRotation = 0f;
    private float defaultCharacterHeight = 0f;
    
    [Space(10)]
    [Header("UI")]
    [SerializeField] private Text healthText;
    [SerializeField] private Text roleText;

    private void Start()
    {
        controller = GetComponent<CharacterController>();
        cameraTransform = GetComponentInChildren<Camera>().transform;
        animator = GetComponentInChildren<Animator>();
        Cursor.lockState = CursorLockMode.Locked;
        defaultCharacterHeight = controller.height;
    }

    private void Update()
    {
        isGrounded = controller.isGrounded;
        if (isGrounded && velocity.y < 0) velocity.y = -2f;

        HandleMovement();
        HandleJump();
        ApplyGravity();
        HandleCamera();
        HandleCrouch();
        
        healthText.text = $"Health: {health}";
        
    }
    
    private void HandleMovement()
    {
        float moveX = Input.GetAxis("Horizontal");
        float moveZ = Input.GetAxis("Vertical");

        Vector3 move = transform.right * moveX + transform.forward * moveZ;
        controller.Move(move * speed * Time.deltaTime);

        animator.SetBool("isRunning", move.magnitude > 0.1f);
    }

    private void HandleJump()
    {
        if (Input.GetButtonDown("Jump") && isGrounded)
        {
            velocity.y = Mathf.Sqrt(jumpHeight * 2f * gravity);
            StartCoroutine(SetAnimatorBool("Jump"));
        }
    }

    private void ApplyGravity()
    {
        velocity.y += Physics.gravity.y * Time.deltaTime;
        controller.Move(velocity * Time.deltaTime);
    }

    private void HandleCamera()
    {
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity * Time.deltaTime;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity * Time.deltaTime;

        xRotation = Mathf.Clamp(xRotation - mouseY, -75f, 75f);
        cameraTransform.localRotation = Quaternion.Euler(xRotation, 0f, 0f);
        transform.Rotate(Vector3.up * mouseX);
    }

    private void HandleCrouch()
    {
        if (Input.GetKeyDown(KeyCode.C)) SetCrouchState(true);
        else if (Input.GetKeyUp(KeyCode.C)) SetCrouchState(false);
    }

    private void SetCrouchState(bool isCrouching)
    {
        animator.SetBool("isCrouching", isCrouching);
        controller.height = isCrouching ? 1.0f : defaultCharacterHeight;
        controller.center = isCrouching ? controller.center / 2.0f : controller.center * 2.0f;
        speed *= isCrouching ? 0.4f : 2.5f;
    }
    
    private IEnumerator SetAnimatorBool(string nameOfAnimation)
    {
        animator.SetBool(nameOfAnimation, true);
        yield return new WaitForSecondsRealtime(0.2f);
        animator.SetBool(nameOfAnimation, false);
    }
    
}
