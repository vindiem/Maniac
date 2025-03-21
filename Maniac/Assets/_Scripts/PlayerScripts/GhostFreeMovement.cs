using System;
using _Scripts.PlayerScripts;
using UnityEngine;

public class GhostFreeMovement : MonoBehaviour
{
    public float speed = 5f;
    public float sensitivity = 2f;
    
    private float rotationX = 0f;
    private float rotationY = 0f;

    private PlayerRoleEnum playerRoleEnum;
    
    public bool isDead = false;

    private void Update()
    {
        if (!isDead) return;
        
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