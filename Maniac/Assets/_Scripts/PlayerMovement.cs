using System;
using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using UnityEngine;
using UnityEngine.UI;
using Random = System.Random;

[RequireComponent(typeof(CharacterController))]
public class PlayerMovement : MonoBehaviour
{
    [SerializeField] private string role = "victim";
    [SerializeField] private GameObject[] skins;
    private int health = 100;
    //private GameObject attackTrigger;
    
    [Header("Movement")]
    public float speed = 5f;
    public float jumpHeight = 2f;
    public float gravity = 9.81f;
    public float mouseSensitivity = 100f;

    private CharacterController controller;
    private Vector3 velocity;
    private bool isGrounded;
    private Transform cameraTransform;
    private Animator animator;
    private float xRotation = 0f;
    private float defaultCharacterHeight = 0f;
    private PhotonView photonView;
    
    [Space]
    [Header("UI")]
    [SerializeField] private Text healthText;
    [SerializeField] private Text roleText;

    private void Awake()
    {
        SetRole();
    }

    private void Start()
    {
        controller = GetComponent<CharacterController>();
        cameraTransform = GetComponentInChildren<Camera>().transform;
        animator = GetComponentInChildren<Animator>();
        //attackTrigger = GetComponentInChildren<BoxCollider>().gameObject;
        //attackTrigger.gameObject.SetActive(false);
        photonView = GetComponent<PhotonView>();
        Cursor.lockState = CursorLockMode.Locked;
    
        defaultCharacterHeight = controller.height;
    }

    private void Update()
    {
        isGrounded = controller.isGrounded;
        if (isGrounded && velocity.y < 0) velocity.y = -2f;

        float moveX = Input.GetAxis("Horizontal");
        float moveZ = Input.GetAxis("Vertical");
        
        Vector3 move = transform.right * moveX + transform.forward * moveZ;
        controller.Move(move * speed * Time.deltaTime);
        
        // Jump
        if (Input.GetButtonDown("Jump") && isGrounded)
        {
            velocity.y = Mathf.Sqrt(jumpHeight * 2f * gravity);
            StartCoroutine(SetAnimatorBool("Jump"));
        }

        velocity.y -= gravity * Time.deltaTime;
        controller.Move(velocity * Time.deltaTime);
        
        bool isRunning = move.magnitude > 0.1f;
        animator.SetBool("isRunning", isRunning);

        if (!isRunning && isGrounded)
            animator.SetBool("isRunning", false);
        
        // Crouching
        if (Input.GetKeyDown(KeyCode.C))
        {
            animator.SetBool("isCrouching", true);
            controller.height = 1.0f;
            controller.center /= 2.0f;
            //cameraTransform.position = new Vector3(cameraTransform.position.x, 
                //cameraTransform.position.y - 0.4f, cameraTransform.position.z + 0.35f);
            speed /= 2.5f;
        }
        else if (Input.GetKeyUp(KeyCode.C))
        {
            animator.SetBool("isCrouching", false);
            controller.height = defaultCharacterHeight;
            controller.center *= 2.0f;
            //cameraTransform.position = new Vector3(0, 1.775f, 0.02f);
            speed *= 2.5f;
        }
        
        // Attack
        if (role == "murder")
        {
            if (Input.GetMouseButtonDown(0))
            {
                photonView.RPC("Attack", RpcTarget.All);
            }
        }
        
        // Camera movement
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity * Time.deltaTime;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity * Time.deltaTime;
        
        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, -75f, 75f);
        
        cameraTransform.localRotation = Quaternion.Euler(xRotation, 0f, 0f);
        transform.Rotate(Vector3.up * mouseX);
        
        // Death
        
        healthText.text = "Health: " + health.ToString();
        roleText.text = "Role: " + role;
        
    }

    public void SetRoleAndSkin(string role)
    {
        this.role = role;
        Debug.Log($"{role} set to new player");

        Random random = new Random();
        // set role and skin
        if (role == "victim")
        {
            int randomSkin = random.Next(0, 3);
            skins[randomSkin].gameObject.SetActive(true);
        }
        else
        {
            int randomSkin = random.Next(3, skins.Length);
            skins[randomSkin].gameObject.SetActive(true);
        }
    }

    private void SetRole()
    {
        if (GameObject.FindGameObjectsWithTag("Player").Length > 1) SetRoleAndSkin("victim");
        else SetRoleAndSkin("murder");
    }
    
    private IEnumerator SetAnimatorBool(string nameOfAnimation)
    {
        animator.SetBool(nameOfAnimation, true);
        yield return new WaitForSecondsRealtime(0.2f);
        animator.SetBool(nameOfAnimation, false);
    }

    /*private IEnumerator attackIEnumerator(float delayTime)
    {
        yield return new WaitForSeconds(delayTime / 2);
        attackTrigger.SetActive(true);
        yield return new WaitForSeconds(delayTime);
        attackTrigger.SetActive(false);
    }*/

    private void OnCollisionEnter(Collision other)
    {
        // Damage for victims from murder by hand
        if (other.gameObject.CompareTag("Attack trigger"))
        {
            Debug.Log("OnCollisionEnter");
            if (role == "victim")
            {
                photonView.RPC("TakeDamage", RpcTarget.All, 50);
            }
        }
    }

    [PunRPC]
    public void TakeDamage(int damage)
    {
        health -= damage;
        healthText.text = "Health: " + health.ToString();
        Debug.Log($"{role} taken damage");
        
        // Death logic
        if (health <= 0)
        {
            gameObject.GetComponent<PlayerMovement>().enabled = false;
            cameraTransform.position = 
                new Vector3(cameraTransform.position.x, cameraTransform.position.y + 20f, cameraTransform.position.z);
            
        }
    }

    /*[PunRPC]
    public void Attack()
    {
        StartCoroutine(SetAnimatorBool("Attack"));
        StartCoroutine(attackIEnumerator(0.85f));
    }*/
    
}
