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

        [Space(10)] 
        [Header("UI variables")] 
        [SerializeField] private Text healthText;

        [SerializeField] private Text roleText;
        [SerializeField] private Text heldTrapText;

        [Space(10)] 
        [Header("Attack and take damage variables")] 
        [HideInInspector] public float health = 100;

        [Space(10)]
        private PlayerRole playerRole;
        //private PlayerRoleEnum role;
        private bool isAttacking = false;
        private float attackDistance = 2.5f;
        private float damage = 50f;

        private Transform lastHighlightedVictim = null;

        [Space(10)] 
        [Header("Trap system variables")] 
        [SerializeField] private GameObject trapInHands;

        [SerializeField] private GameObject trapPrefab;
        private bool heldTrap = false;
        private float distanceTrap = 3f;

        private void Start()
        {
            photonView = GetComponent<PhotonView>();
            controller = GetComponent<CharacterController>();
            cameraTransform = GetComponentInChildren<Camera>().transform;
            animator = GetComponentInChildren<Animator>();
            playerRole = GetComponent<PlayerRole>();
            
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
            if (playerRole.GetRole() == PlayerRoleEnum.Murder) 
                HandleAttack();
            UpdateUI();
            if (playerRole.GetRole() == PlayerRoleEnum.Victim) 
                HandleTrap();
            //CursorVisibility();

        }

        private void UpdateUI()
        {
            healthText.text = $"Health: {health}";
            roleText.text = $"Role: {playerRole.GetRole()}";
            if (playerRole.GetRole() == PlayerRoleEnum.Murder) heldTrapText.text = "Murders can't held traps";
            else if (playerRole.GetRole() == PlayerRoleEnum.Victim) heldTrapText.text = $"Held Trap: {heldTrap}";
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

                    if (distanceToTarget <= attackDistance &&
                        targetPlayer.GetComponent<PlayerRole>().GetRole() == PlayerRoleEnum.Victim)
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
                    //Debug.Log($"{hit.collider.name}, >> {hit.collider}, {hit.collider.gameObject.name}");

                    Transform targetPlayer = hit.collider.transform;
                    float distanceToTarget = Vector3.Distance(transform.position, targetPlayer.position);

                    if (distanceToTarget <= attackDistance &&
                        targetPlayer.GetComponent<PlayerRole>().GetRole() == PlayerRoleEnum.Victim)
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

        /*private void CursorVisibility()
        {
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                Cursor.visible = !Cursor.visible;
                CursorLockMode mode = Cursor.visible ? CursorLockMode.Locked : CursorLockMode.None;
                Cursor.lockState = mode;
            }
        }*/

        // Imitation animations' triggers
        private IEnumerator SetAnimatorBool(string nameOfAnimation)
        {
            animator.SetBool(nameOfAnimation, true);
            yield return new WaitForSecondsRealtime(0.2f);
            animator.SetBool(nameOfAnimation, false);
        }

        // FUCKING Trap system
        private void OnTriggerEnter(Collider other)
        {
            if (playerRole.GetRole() == PlayerRoleEnum.Murder && other.CompareTag("Trap"))
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
            heldTrap = true;
            trapInHands.SetActive(true);

            Debug.Log($"{PhotonNetwork.NickName} received RPC. ViewID: {trapViewID}");

            PhotonView trapView = PhotonView.Find(trapViewID);
            if (trapView == null)
            {
                Debug.LogWarning($"{PhotonNetwork.NickName} didn't find object with ViewID: {trapViewID}");
                return;
            }

            if (trapView.IsRoomView)
            {
                Debug.LogWarning(
                    $"Trap {trapView.ViewID} belongs to the scene. Requesting MasterClient to take ownership.");
                if (PhotonNetwork.IsMasterClient)
                    trapView.TransferOwnership(PhotonNetwork.LocalPlayer);
                else
                {
                    photonView.RPC("RequestOwnership", RpcTarget.MasterClient, trapView.ViewID);
                    return;
                }
            }
            else
                trapView.TransferOwnership(PhotonNetwork.LocalPlayer);

            StartCoroutine(DestroyAfterOwnership(trapView));
        }

        [PunRPC]
        private void RequestOwnership(int trapViewID)
        {
            PhotonView trapView = PhotonView.Find(trapViewID);
            if (trapView != null && PhotonNetwork.IsMasterClient)
                trapView.TransferOwnership(PhotonNetwork.LocalPlayer);
        }

        private IEnumerator DestroyAfterOwnership(PhotonView trapView)
        {
            int attempts = 10;
            while (!trapView.IsMine && attempts > 0)
            {
                Debug.Log($"Waiting for ownership of trap {trapView.ViewID}... Attempts left: {attempts}");
                attempts--;
                yield return new WaitForSeconds(0.2f);
            }

            Debug.Log($"{PhotonNetwork.NickName} now owns the trap {trapView.ViewID}. Destroying...");
            if (trapView.IsMine || PhotonNetwork.IsMasterClient)
            {
                //PhotonNetwork.Destroy(trapView.gameObject);
                if (trapView != null && trapView.gameObject != null)
                    PhotonNetwork.Destroy(trapView.gameObject);
                else
                    Debug.LogWarning("Trap already destroyed or null.");
            }
            else
                Debug.LogWarning(
                    $"{PhotonNetwork.NickName} is not the owner and cannot destroy trap {trapView.ViewID}.");

        }

        [PunRPC]
        private void PlaceTrap()
        {
            heldTrap = false;
            trapInHands.SetActive(false);

            if (PhotonNetwork.IsMasterClient)
            {
                Vector3 trapPosition = transform.position + transform.forward;
                Quaternion trapRotation = Quaternion.Euler(-90f, 0f, 0f);

                GameObject newTrap = PhotonNetwork.Instantiate(trapPrefab.name, trapPosition, trapRotation);
                PhotonView newTrapView = newTrap.GetComponent<PhotonView>();

                Debug.Log($"MasterClient placed trap {newTrapView.ViewID} at {trapPosition}");

                // Massage all players, that trap has been created
                if (newTrapView != null)
                    photonView.RPC("SyncTrap", RpcTarget.OthersBuffered,
                        newTrapView.ViewID, trapPosition);
            }
        }

        [PunRPC]
        private void SyncTrap(int trapViewID, Vector3 position)
        {
            PhotonView trapView = PhotonView.Find(trapViewID);
            if (trapView != null)
            {
                trapView.transform.position = position;
                trapView.gameObject.SetActive(true);
                Debug.Log($"Synced trap {trapViewID} at {position}");
            }
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
                            photonView.RPC("PickUpTrap", RpcTarget.All, trapView.ViewID);
                        }
                    }
                }
                else if (Input.GetKeyDown(KeyCode.G) && heldTrap)
                {
                    photonView.RPC("PlaceTrap", RpcTarget.AllBuffered);
                }
            }
        }

        // Draw gizmos
        private void OnDrawGizmosSelected()
        {
            Debug.DrawRay(Camera.main.transform.position, Camera.main.transform.forward * distanceTrap, Color.green);
            Debug.DrawRay(Camera.main.transform.position, Camera.main.transform.forward * attackDistance, Color.blue);
        }

    }
}
