using System;
using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
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
        private float damage = 50f;

        // : MonoBehaviour
        private PlayerRole playerRole;
        private PlayerUIUpdate playerUIUpdate;
        private PlayerTrapSystem playerTrapSystem;
        private PlayerOutlineSystem playerOutlineSystem;
        private GhostFreeMovement ghostFreeMovement;
        
        // Prefab
        [SerializeField] private GameObject stateObj;

        private void Start()
        {
            photonView = GetComponent<PhotonView>();
            controller = GetComponent<CharacterController>();
            cameraTransform = GetComponentInChildren<Camera>().transform;
            animator = GetComponentInChildren<Animator>();
            
            playerRole = GetComponent<PlayerRole>();
            playerUIUpdate = GetComponent<PlayerUIUpdate>();
            playerTrapSystem = GetComponent<PlayerTrapSystem>();
            playerOutlineSystem = GetComponent<PlayerOutlineSystem>();
            ghostFreeMovement = GetComponent<GhostFreeMovement>();
            
            defaultCharacterHeight = controller.height;
            ghostFreeMovement.enabled = false;
            
            Cursor.lockState = CursorLockMode.None;
            Cursor.lockState = Cursor.lockState == CursorLockMode.Locked ? 
                CursorLockMode.None : CursorLockMode.Locked;
            
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
            photonView.RPC("RPC_Death", RpcTarget.All);
        }
        
        // + death animation
        [PunRPC]
        public void RPC_Death()
        {
            // Victims' win system or Murder's
            Debug.Log("DIEED");
            
            Transform[] objects = GetComponentsInChildren<Transform>();
            foreach (Transform obj in objects)
            {
                if (obj.gameObject != this.gameObject && !obj.gameObject.CompareTag("MainCamera"))
                {
                    obj.gameObject.SetActive(false);
                }
            }
            controller.enabled = false;
            
            playerRole.enabled = false;
            playerUIUpdate.enabled = false;
            playerTrapSystem.enabled = false;
            playerOutlineSystem.enabled = false;
            ghostFreeMovement.enabled = true;
            
            cameraTransform.localPosition = new Vector3(0f, 0f, 0f);
            cameraTransform.transform.rotation = new Quaternion(0f, 0f, 0f, 0f);
            PlayerMovement playerMovement = GetComponent<PlayerMovement>();
            playerMovement.enabled = false;
            
            PhotonNetwork.Instantiate(stateObj.name, transform.position, Quaternion.identity);
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
                photonView.RPC("TakeDamage", RpcTarget.AllBuffered, viewId, damage / 3);
                StunMurder();
                
                // Making sound
                SoundManager.instance.PlayTrapCloseSound();
            }
        }

        public void StunMurder()
        {
            Debug.Log($"{gameObject.name} (murder) stunned.");
            StartCoroutine(Stun(7f));
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
