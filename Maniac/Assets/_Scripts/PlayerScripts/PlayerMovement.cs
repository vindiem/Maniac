using System;
using System.Collections;
using System.Collections.Generic;
using _Scripts.MultiplayerScripts;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace _Scripts.PlayerScripts
{
    [RequireComponent(typeof(CharacterController))]
    [RequireComponent(typeof(PlayerRole))]
    public class PlayerMovement : MonoBehaviour
    {
        private static readonly int IsRunning = Animator.StringToHash("isRunning");
        private static readonly int IsCrouching = Animator.StringToHash("isCrouching");
        private PhotonView photonView;

        [Header("Movement variables")] 
        [SerializeField] private float speed = 5f;
        [SerializeField] private float jumpHeight = 2f;
        [SerializeField] private float gravity = 9.81f;
        [SerializeField] private float mouseSensitivity = 100f;

        // Movement variables
        private CharacterController controller;
        private Vector3 velocity;
        private bool isGrounded;
        private Transform cameraTransform;
        private Animator animator;
        private float xRotation = 0f;
        private float defaultCharacterHeight = 0f;

        [Space(10)] 
        [Header("Health & damage")] 
        [HideInInspector] public float health = 100;
        
        // Attacking variables
        private bool isAttacking = false;
        [HideInInspector] public float attackDistance = 2.5f;
        private float damage = 35f;

        // : MonoBehaviour
        private PlayerRole playerRole;
        private PlayerUIUpdate playerUIUpdate;
        private PlayerTrapSystem playerTrapSystem;
        private PlayerOutlineSystem playerOutlineSystem;
        private GhostFreeMovement ghostFreeMovement;
        
        // Prefab
        [SerializeField] private GameObject stateObj;

        private void Awake()
        {
            photonView = GetComponent<PhotonView>();
        }

        private void Start()
        {
            controller = GetComponent<CharacterController>();
            cameraTransform = GetComponentInChildren<Camera>().transform;
            animator = GetComponentInChildren<Animator>();
            
            playerRole = GetComponent<PlayerRole>();
            playerUIUpdate = GetComponent<PlayerUIUpdate>();
            playerTrapSystem = GetComponent<PlayerTrapSystem>();
            playerOutlineSystem = GetComponent<PlayerOutlineSystem>();
            ghostFreeMovement = GetComponent<GhostFreeMovement>();
            
            defaultCharacterHeight = controller.height;
            
            Cursor.lockState = CursorLockMode.None;
            Cursor.lockState = Cursor.lockState == CursorLockMode.Locked ? 
                CursorLockMode.None : CursorLockMode.Locked;
            
            ghostFreeMovement.SetDieState(playerRole.GetRole());
        }

        private void Update()
        {
            isGrounded = controller.isGrounded;
            if (isGrounded && velocity.y < 0) velocity.y = -2f;

            #region Movement functions
            HandleMovement();
            HandleJump();
            ApplyGravity();
            HandleCamera();
            HandleCrouch();
            #endregion
            
            if (playerRole.GetRole() == PlayerRoleEnum.Murder) 
                HandleAttack();
            playerUIUpdate.UpdateUI(health, playerRole, 
                playerTrapSystem.GetTrapInHandsBool(), playerTrapSystem.GetGunInHandsBool());
            if (playerRole.GetRole() == PlayerRoleEnum.Victim) 
                playerTrapSystem.HandleInventory();
            
            CursorVisibility();

        }

        private void HandleAttack()
        {
            playerOutlineSystem.HighlightVictim(attackDistance);

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

                    if (distanceToTarget <= attackDistance &&
                        targetPlayer.GetComponent<PlayerRole>().GetRole() == PlayerRoleEnum.Victim)
                    {
                        // Damage player ("victim")
                        int viewId = targetPlayer.GetComponent<PhotonView>().ViewID;
                        photonView.RPC("TakeDamage", RpcTarget.AllBuffered, viewId, damage);
                    }
                    
                    // Making sound
                    SoundManager.instance.PlayMurderHitSound();
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

            animator.SetBool(IsRunning, move.magnitude > 0.1f);
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
            animator.SetBool(IsCrouching, isCrouching);
            controller.height = isCrouching ? 1.0f : defaultCharacterHeight;
            controller.center = isCrouching ? controller.center / 2.0f : controller.center * 2.0f;
            speed *= isCrouching ? 0.4f : 2.5f; 
        }

        private void Die()
        {
            // Death logic
            Debug.Log($"{name} died.");
            
            Cursor.lockState = CursorLockMode.None;
    
            // Check if photonView is null before calling RPC
            if (photonView != null)
            {
                photonView.RPC("RPC_Death", RpcTarget.All);
            }
            else
            {
                // Fallback if photonView is null
                Debug.LogWarning("PhotonView is null in Die() method!");
                RPC_Death(); // Call the method directly if RPC can't be sent
            }
            
            // win check
            RoomManager roomManager = FindObjectOfType<RoomManager>();
            if (roomManager != null)
            {
                roomManager.WinCheck();
            }

        }

        [PunRPC]
        private void RPC_Death()
        {
            // Add null checks for all component access
            // Disable child objects safely
            if (this != null && gameObject != null) // Check if this object still exists
            {
                Transform[] objects = GetComponentsInChildren<Transform>();
                if (objects != null)
                {
                    foreach (Transform obj in objects)
                    {
                        if (obj != null && obj.gameObject != null && obj.gameObject != this.gameObject && 
                            !obj.gameObject.CompareTag("MainCamera"))
                        {
                            obj.gameObject.SetActive(false);
                        }
                    }
                }
        
                // Safely disable controller
                if (controller != null && controller.enabled)
                {
                    controller.enabled = false;
                }
        
                // Safely disable components
                if (playerRole != null) playerRole.enabled = false;
                if (playerUIUpdate != null) playerUIUpdate.enabled = false;
                if (playerTrapSystem != null) playerTrapSystem.enabled = false;
                if (playerOutlineSystem != null) playerOutlineSystem.enabled = false;
        
                // Safely setting ghostFreeMovement dead
                if (ghostFreeMovement != null)
                {
                    ghostFreeMovement.SetIsDead();
                }
        
                PhotonNetwork.Instantiate(stateObj.name, transform.position, Quaternion.identity);
        
                // Adjust transform positions only if objects exist
                if (transform != null)
                {
                    transform.position = new Vector3(transform.position.x, 1.5f, transform.position.z);
                    transform.rotation = Quaternion.identity;
                }
        
                if (cameraTransform != null)
                {
                    cameraTransform.localPosition = new Vector3(0f, 0f, 0f);
                    cameraTransform.rotation = Quaternion.identity;
                    // Using identity instead of new Quaternion(0f, 0f, 0f, 0f)
                }
        
                // Safely disable this component
                PlayerMovement playerMovement = GetComponent<PlayerMovement>();
                if (playerMovement != null)
                {
                    playerMovement.enabled = false;
                }
                
            }
        }

        private void CursorVisibility()
        {
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                Cursor.lockState = Cursor.lockState == CursorLockMode.Locked ? 
                    CursorLockMode.None : CursorLockMode.Locked;
            }
        }
        
        // Animations' triggers helper
        private IEnumerator SetAnimatorBool(string nameOfAnimation)
        {
            animator.SetBool(nameOfAnimation, true);
            yield return new WaitForSecondsRealtime(0.2f);
            animator.SetBool(nameOfAnimation, false);
        }

        // Stun system
        private void OnTriggerEnter(Collider other)
        {
            if (playerRole == null || other.gameObject == null) return;
            
            if (playerRole.GetRole() == PlayerRoleEnum.Murder && other.CompareTag("Trap"))
            {
                int viewId = GetComponent<PhotonView>().ViewID;
                photonView.RPC("TakeDamage", RpcTarget.AllBuffered, viewId, MathF.Round(damage / 5));
                StunMurder();
                
                // Making sound
                SoundManager.instance.PlayTrapCloseSound();
            }
        }

        private void StunMurder()
        {
            Debug.Log($"{gameObject.name} (murder) stunned.");
            StartCoroutine(Stun(4f));
        }

        private IEnumerator Stun(float seconds)
        {
            animator.enabled = false;
            float defaultJumpHeight = jumpHeight;
            float defaultSpeed = speed;
            jumpHeight = 0f;
            speed = 0f;
            yield return new WaitForSeconds(seconds);
            
            animator.enabled = true;
            jumpHeight = defaultJumpHeight;
            speed = defaultSpeed;
        }
        
    }
}
