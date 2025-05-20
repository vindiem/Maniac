using System;
using _Scripts.PlayerScripts;
using UnityEngine;

public class GhostFreeMovement : MonoBehaviour
{
    public float speed = 5f;
    public float sensitivity = 2f;

    public float minSpeed = 1f;
    public float maxSpeed = 20f;
    public float speedStep = 1f;

    private float rotationX = 0f;
    private float rotationY = 0f;

    private PlayerRoleEnum playerRoleEnum;

    public bool isDead = false;

    private void Update()
    {
        if (!isDead) return;

        float scroll = Input.GetAxis("Mouse ScrollWheel");
        if (Mathf.Abs(scroll) > 0.01f)
        {
            speed += scroll * speedStep;
            speed = Mathf.Clamp(speed, minSpeed, maxSpeed);
        }

        float moveX = Input.GetAxis("Horizontal"); // A, D
        float moveZ = Input.GetAxis("Vertical");   // W, S
        float moveY = 0;

        if (Input.GetKey(KeyCode.Space)) moveY = 1;
        if (Input.GetKey(KeyCode.LeftControl)) moveY = -1;

        Vector3 moveDirection = new Vector3(moveX, moveY, moveZ).normalized;
        transform.position += transform.TransformDirection(moveDirection) * speed * Time.deltaTime;

        rotationX += Input.GetAxis("Mouse X") * sensitivity;
        rotationY -= Input.GetAxis("Mouse Y") * sensitivity;
        rotationY = Mathf.Clamp(rotationY, -90f, 90f);

        transform.rotation = Quaternion.Euler(rotationY, rotationX, 0);
        
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Cursor.lockState = Cursor.lockState == CursorLockMode.Locked ? 
                CursorLockMode.None : CursorLockMode.Locked;
        }
    }

    public void SetIsDead()
    {
        isDead = true;
    }

    public void SetDieState(PlayerRoleEnum playerRoleEnum)
    {
        this.playerRoleEnum = playerRoleEnum;
    }
    public PlayerRoleEnum GetDieState() => playerRoleEnum;
}