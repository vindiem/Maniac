using System;
using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

[RequireComponent(typeof(CharacterController))]
public class PlayerMovement : MonoBehaviour
{
    private PhotonView photonView;

    [Header("Movement variables")] 
    [SerializeField] private float speed = 5f;
    [SerializeField] private float jumpHeight = 2f;
    [SerializeField] private float gravity = 9.81f;
    [SerializeField] private float mouseSensitivity = 100f;

    private float defaultSpeed = 5f;
    private CharacterController controller;
    private Vector3 velocity;
    private bool isGrounded;
    private Transform cameraTransform;
    private Animator animator;
    private float xRotation = 0f;
    private float defaultCharacterHeight = 0f;

    [Space(10)] [Header("UI variables")] 
    [SerializeField] private Text healthText;
    [SerializeField] private Text roleText;

    [Space(10)] [Header("Attack and take damage variables")] 
    [HideInInspector] public float health = 100;
    [HideInInspector] public string role = "victim"; // set in PlayerSetup.cs
    private bool isAttacking = false;
    private float attackDistance = 2.5f;
    private float damage = 50f;

    private Transform lastHighlightedVictim = null;

    [Space(10)] [Header("Trap system variables")]
    private bool heldTrap = false;
    [SerializeField] private GameObject trapInHands;
    [SerializeField] private GameObject trapPrefab;
    private float distanceTrap = 3f;

    private void Start()
    {
        photonView = GetComponent<PhotonView>();
        controller = GetComponent<CharacterController>();
        cameraTransform = GetComponentInChildren<Camera>().transform;
        animator = GetComponentInChildren<Animator>();
        Cursor.lockState = CursorLockMode.Locked;
        defaultCharacterHeight = controller.height;
        defaultSpeed = speed;
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
        if (role == "murder") HandleAttack();
        UpdateUI();
        if (role == "victim") HandleTrap();
        CursorVisibility();
        
    }

    private void UpdateUI()
    {
        healthText.text = $"Health: {health}";
        roleText.text = $"Role: {role}";
    }

    private void HandleAttack()
    {
        HighlightVictim();

        if (Input.GetMouseButtonDown(0) && !isAttacking)
        {
            StartCoroutine(Attack());
        }
    }

    private IEnumerator Attack()
    {
        isAttacking = true;
        StartCoroutine(SetAnimatorBool("Attack"));

        RaycastHit hit;
        if (Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out hit, attackDistance + 0.1f))
        {
            // If object - player, and it's not current player
            if (hit.collider.CompareTag("Player") && hit.collider.gameObject != gameObject)
            {
                Transform targetPlayer = hit.collider.transform;
                float distanceToTarget = Vector3.Distance(transform.position, targetPlayer.position);

                if (distanceToTarget <= attackDistance && targetPlayer.GetComponent<PlayerMovement>().role == "victim")
                {
                    // Damage player ("victim")
                    int viewId = targetPlayer.GetComponent<PhotonView>().ViewID;
                    photonView.RPC("TakeDamage", RpcTarget.AllBuffered, viewId, damage);
                }
            }
        }

        yield return new WaitForSeconds(3f);
        isAttacking = false;
    }

    [PunRPC]
    private void TakeDamage(int targetViewID, float damageAmount)
    {
        // Find player by ViewID and damage him
        PhotonView targetPhotonView = PhotonView.Find(targetViewID);
        if (targetPhotonView != null)
        {
            PlayerMovement targetPlayer = targetPhotonView.GetComponent<PlayerMovement>();
            targetPlayer.health -= damageAmount;
            Debug.Log($"{targetPlayer.name} took {damageAmount} damage!");

            if (targetPlayer.health <= 0) targetPlayer.Die();
        }
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

    private void Die()
    {
        Debug.Log($"{name} died.");
        gameObject.SetActive(false);
    }

    // Outline's functions

    #region Region

    private void HighlightVictim()
    {
        RaycastHit hit;
        if (Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out hit))
        {
            if (hit.collider.CompareTag("Player") && hit.collider.gameObject != gameObject)
            {
                Debug.Log($"{hit.collider.name}, >> {hit.collider}, {hit.collider.gameObject.name}");

                Transform targetPlayer = hit.collider.transform;
                float distanceToTarget = Vector3.Distance(transform.position, targetPlayer.position);

                if (distanceToTarget <= attackDistance && targetPlayer.GetComponent<PlayerMovement>().role == "victim")
                {
                    if (lastHighlightedVictim != targetPlayer)
                    {
                        ResetHighlight();
                        lastHighlightedVictim = targetPlayer;
                        targetPlayer.GetComponent<PhotonView>().RPC("SetHighlight", RpcTarget.AllBuffered, true);
                    }

                    return;
                }
            }
        }

        ResetHighlight();
    }

    private void ResetHighlight()
    {
        if (lastHighlightedVictim != null)
        {
            lastHighlightedVictim.GetComponent<PhotonView>().RPC("SetHighlight", RpcTarget.AllBuffered, false);
            lastHighlightedVictim = null;
        }
    }

    [PunRPC]
    private void SetHighlight(bool highlight)
    {
        Outline outline = GetComponentInChildren<Outline>();
        if (outline != null)
        {
            outline.enabled = highlight;
        }
    }

    #endregion

    private void CursorVisibility()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Cursor.visible = !Cursor.visible;
            CursorLockMode mode = Cursor.visible ? CursorLockMode.Locked : CursorLockMode.None;
        }
    }
    
    public void SetRole(string newRole)
    {
        role = newRole;
    }

    // Imitation animations' triggers
    private IEnumerator SetAnimatorBool(string nameOfAnimation)
    {
        animator.SetBool(nameOfAnimation, true);
        yield return new WaitForSecondsRealtime(0.2f);
        animator.SetBool(nameOfAnimation, false);
    }

    // Trap system
    private void OnTriggerEnter(Collider other)
    {
        if (role == "murder" && other.CompareTag("Trap"))
        {
            int viewId = GetComponent<PhotonView>().ViewID;
            photonView.RPC("TakeDamage", RpcTarget.AllBuffered, viewId, damage);
            StunMurder();
        }
    }

    private void StunMurder()
    {
        Debug.Log($"{gameObject.name} stunned.");
        StartCoroutine(Stun(7f));
    }

    private IEnumerator Stun(float seconds)
    {
        speed = 0f;
        animator.enabled = false;
        yield return new WaitForSeconds(seconds);
        speed = defaultSpeed;
        animator.enabled = true;
    }

    [PunRPC]
    private void PickUpTrap(int trapViewID)
    {
        PhotonView trapView = PhotonView.Find(trapViewID);
        if (trapView == null) return;
        GameObject trapObject = trapView.gameObject;

        if (!trapView.IsMine)
        {
            trapView.TransferOwnership(PhotonNetwork.LocalPlayer);
            StartCoroutine(WaitForOwnershipAndDestroy(trapView));
        }
        else
        {
            HandleTrapPickup();
            PhotonNetwork.Destroy(trapObject);
        }
    }

    private IEnumerator WaitForOwnershipAndDestroy(PhotonView trapView)
    {
        while (!trapView.IsMine) 
        {
            // Wait for 1 frame
            yield return null;
        }
        
        HandleTrapPickup();
        PhotonNetwork.Destroy(trapView.gameObject);
    }

    [PunRPC]
    private void HandleTrapPickup()
    {
        heldTrap = true;
        trapInHands.SetActive(true);
        Debug.Log($"Trap picked up by: {PhotonNetwork.LocalPlayer.NickName} | heldTrap = {heldTrap} | trapInHands Active = {trapInHands.activeSelf}");
    }

    [PunRPC]
    private void PlaceTrap()
    {
        heldTrap = false;
        trapInHands.SetActive(false);
        Vector3 trapPosition = transform.position + transform.forward;
        
        Quaternion trapRotation = Quaternion.Euler(-90f, 0f, 0f);
        GameObject newTrap = PhotonNetwork.Instantiate(trapPrefab.name, trapPosition, trapRotation);
        
        Debug.Log($"{name} placed trap {trapPosition}: {newTrap.name}");
    }

    private void HandleTrap()
    {
        RaycastHit hit;
        if (Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out hit, distanceTrap))
        {
            if (hit.collider.CompareTag("Trap"))
            {
                if (Input.GetKeyDown(KeyCode.F) && !heldTrap)
                {
                    PhotonView trapView = hit.collider.GetComponent<PhotonView>();
                    if (trapView != null)
                    {
                        photonView.RPC("PickUpTrap", RpcTarget.AllBuffered, trapView.ViewID);
                    }
                }
            }
            else if (Input.GetKeyDown(KeyCode.G) && heldTrap)
            {
                photonView.RPC("PlaceTrap", RpcTarget.MasterClient);
            }
        }
    }

    private void OnDrawGizmosSelected()
    {
        Debug.DrawRay(Camera.main.transform.position, Camera.main.transform.forward * distanceTrap, Color.green);
        Debug.DrawRay(Camera.main.transform.position, Camera.main.transform.forward * attackDistance, Color.blue);
    }
}
