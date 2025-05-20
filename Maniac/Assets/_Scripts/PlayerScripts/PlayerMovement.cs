using System;
using System.Collections;
using _Scripts.MultiplayerScripts.LobbyScripts;
using Photon.Pun;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Serialization;
using UnityEngine.UI;
using Random = UnityEngine.Random;

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
        [SerializeField] private float speed = 3.5f;
        [SerializeField] private float jumpHeight = 2f;
        [SerializeField] private float gravity = 9.81f;
        [SerializeField] private float mouseSensitivity = 100f;

        // Movement variables
        private CharacterController controller;
        private Vector3 velocity;
        private bool isGrounded;
        [SerializeField] private Transform cameraHolderTransform;
        private Animator animator;
        private float xRotation = 0f;
        private float defaultCharacterHeight = 0f;
        private float defaultSpeed = 0f;
        private float defaultJumpHeight = 0f;
        
        [Space(10)]
        [Header("Sprint variables")]
        [SerializeField] private float sprintSpeed = 8f;
        [SerializeField] private float acceleration = 10f;
        [SerializeField] private float stamina = 5f;
        [SerializeField] private float staminaRegenRate = .7f;
        [SerializeField] private float staminaDrainRate = 2f;
        [SerializeField] private float staminaThreshold = 1f;
        private bool canSprint = true;
        private float currentStamina;
        private bool isSprinting;
        private bool isCrouching;

        private bool stunned = false;

        [Space(10)] 
        [Header("Health & damage")] 
        [HideInInspector] public float health = 100;
        
        // Attacking variables
        [SerializeField] private Image damageFlashImage;
        private float attackCooldown = 3f;
        private float attackTimer = 0f;
        private bool isAttacking = false;
        private bool IsAttackReady => attackTimer <= 0f;
        [HideInInspector] public float attackDistance = 2.5f;
        private float damage = 35f;
        private float highlightAllPlayersTime = 15f;

        // : MonoBehaviour
        private PlayerRole playerRole;
        private PlayerUIUpdate playerUIUpdate;
        private PlayerTrapSystem playerTrapSystem;
        private PlayerOutlineSystem playerOutlineSystem;
        private GhostFreeMovement ghostFreeMovement;
        
        // Prefab
        [SerializeField] private GameObject stateObj;
        [SerializeField] private GameObject particleObj;
        
        [Header("Camera Bobbing")]
        [SerializeField] private float bobFrequency = 5f;
        [SerializeField] private float bobAmplitude = 0.05f;
        [SerializeField] private float bobSmoothing = 6f;
        private float bobSmoothingDefaultValue;

        private Vector3 initialCameraLocalPos;
        private float bobTimer;
        private Camera camera;
        
        [Header("Camera Tilt Settings")]
        [SerializeField] private float tiltAmount = 5f;
        [SerializeField] private float tiltSpeed = 4f;
        
        private const float DefaultFieldOfView = 50.0f;
        private const float ChangedFieldOfView = 60.0f;
        
        [Header("Jump variables")]
        [SerializeField] private float jumpCooldown = 1f;
        private bool canJump = true;

        private AudioSource footStepsAudioSource;
        
        // Prestart game
        [SerializeField] private Text preGameStartedCooldown;
        [SerializeField] private float secondsToStartGame = 50f;

        private void Awake()
        {
            photonView = GetComponent<PhotonView>();
            initialCameraLocalPos = cameraHolderTransform.localPosition;
            
            footStepsAudioSource = GetComponentInChildren<AudioSource>();
            photonView.RPC("FootStepsAudioSource", RpcTarget.AllBuffered, false);

            if (PhotonNetwork.IsMasterClient)
            {
                preGameStartedCooldown.gameObject.SetActive(false);
            }
        }

        private void Start()
        {
            controller = GetComponent<CharacterController>();
            animator = GetComponentInChildren<Animator>();
            
            //cameraTransform = GetComponentInChildren<Camera>().transform;
            camera = cameraHolderTransform.GetComponentInChildren<Camera>();
            camera.fieldOfView = DefaultFieldOfView;
            
            playerRole = GetComponent<PlayerRole>();
            playerUIUpdate = GetComponent<PlayerUIUpdate>();
            playerTrapSystem = GetComponent<PlayerTrapSystem>();
            playerOutlineSystem = GetComponent<PlayerOutlineSystem>();
            ghostFreeMovement = GetComponent<GhostFreeMovement>();
            
            defaultCharacterHeight = controller.height;
            defaultSpeed = speed;
            defaultJumpHeight = jumpHeight;
            
            Cursor.lockState = CursorLockMode.None;
            Cursor.lockState = Cursor.lockState == CursorLockMode.Locked ? 
                CursorLockMode.None : CursorLockMode.Locked;
            
            ghostFreeMovement.SetDieState(playerRole.GetRole());
            bobSmoothingDefaultValue = bobSmoothing;
            
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
            HandleSprint();
            #endregion

            if (playerRole.GetRole() == PlayerRoleEnum.Murder)
            {
                HandleAttack();

                if (Input.GetKeyDown(KeyCode.X) && highlightAllPlayersTime <= 0f)
                {
                    //playerOutlineSystem.HighlightAll();
                    photonView.RPC("HighlightAll", RpcTarget.AllBuffered);
                    highlightAllPlayersTime = 55f;
                }
                highlightAllPlayersTime -= Time.deltaTime;
                
                //
                if (preGameStartedCooldown != null && preGameStartedCooldown.enabled)
                {
                    StunPlayer(secondsToStartGame);
                    preGameStartedCooldown.gameObject.SetActive(true);
                    preGameStartedCooldown.text = MathF.Round((secondsToStartGame -= Time.deltaTime), 1).ToString();
                    Destroy(preGameStartedCooldown, secondsToStartGame);
                }

                if (PhotonNetwork.PlayerList.Length == 1 && preGameStartedCooldown == null)
                {
                    PhotonNetwork.LeaveRoom();
                    PhotonNetwork.LeaveLobby();
                    
                    Cursor.visible = true;
                    Cursor.lockState = CursorLockMode.None;
                    
                    SceneManager.LoadScene("Menu");
                }

            }
            else if (playerRole.GetRole() == PlayerRoleEnum.Victim)
            {
                playerTrapSystem.HandleInventory();
            }
            
            if (attackTimer > 0f)
                attackTimer -= Time.deltaTime;
            
            playerUIUpdate.UpdateUI(health, playerRole, 
                playerTrapSystem.GetTrapInHandsBool(), playerTrapSystem.GetGunInHandsBool());
            
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
            if (!IsAttackReady) yield break;

            isAttacking = true;
            attackTimer = attackCooldown;

            StartCoroutine(SetAnimatorBool("Attack"));

            RaycastHit hit;
            if (Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out hit, attackDistance + 0.1f))
            {
                if (hit.collider.CompareTag("Player") && hit.collider.gameObject != gameObject)
                {
                    Transform targetPlayer = hit.collider.transform;
                    float distanceToTarget = Vector3.Distance(transform.position, targetPlayer.position);

                    if (distanceToTarget <= attackDistance &&
                        targetPlayer.GetComponent<PlayerRole>().GetRole() == PlayerRoleEnum.Victim)
                    {
                        int viewId = targetPlayer.GetComponent<PhotonView>().ViewID;
                        photonView.RPC("TakeDamage", RpcTarget.AllBuffered, viewId, damage);
                    }

                    SoundManager.Instance.PlayMurderHitSound();
                }
            }

            isAttacking = false;
        }


        public float GetAttackTimer() => Mathf.Max(0f, attackTimer);

        [PunRPC]
        public void TakeDamage(int targetViewID, float damageAmount)
        {
            PhotonView targetPhotonView = PhotonView.Find(targetViewID);
            PlayerMovement targetPlayer = targetPhotonView.GetComponent<PlayerMovement>();
            
            if (targetPhotonView != null && !targetPlayer.GetStunnedState())
            {
                targetPlayer.health -= damageAmount;
                Debug.Log($"{targetPlayer.name} took {damageAmount} damage!");

                Vector3 spawnPosition = targetPlayer.transform.position + new Vector3(0f, 0.7f, 0f);
                GameObject particle = Instantiate(particleObj, spawnPosition, Quaternion.Euler(-90f, 0f, 0f));
                Destroy(particle, 0.5f);
                
                //Vector3 knockbackDir = (targetPlayer.transform.position - transform.position).normalized;

                if (PhotonNetwork.IsMasterClient != targetPhotonView)
                {
                    targetPlayer.ApplyKnockback(Vector3.forward, 5f);
                    targetPlayer.ApplyKnockback(Vector3.up, 3f);
                }
                
                targetPlayer.StartCoroutine(targetPlayer.CameraShake(0.25f, 0.1f));
                targetPlayer.StartCoroutine(targetPlayer.DamageFlash());

                if (PhotonNetwork.IsMasterClient == targetPhotonView)
                {
                    RoomManager roomManager = FindObjectOfType<RoomManager>();
                    if (roomManager != null)
                    {
                        roomManager.AddMurderMiss("shot");
                    }
                }
                
                if (targetPlayer.health <= 0) targetPlayer.Die();
            }
        }
        
        public IEnumerator CameraShake(float duration, float magnitude)
        {
            Vector3 originalPos = cameraHolderTransform.localPosition;
            float elapsed = 0f;

            while (elapsed < duration)
            {
                float x = Random.Range(-1f, 1f) * magnitude;
                float y = Random.Range(-1f, 1f) * magnitude;

                cameraHolderTransform.localPosition = originalPos + new Vector3(x, y, 0f);

                elapsed += Time.deltaTime;
                yield return null;
            }

            cameraHolderTransform.localPosition = originalPos;
        }
        
        public IEnumerator DamageFlash()
        {
            damageFlashImage.gameObject.SetActive(true);

            float elapsed = 0f;
            float duration = 1f;

            Color startColor = new Color(1f, 0f, 0f, 0.5f);
            Color endColor = new Color(1f, 0f, 0f, 0f);

            while (elapsed < duration)
            {
                damageFlashImage.color = Color.Lerp(startColor, endColor, elapsed / duration);
                elapsed += Time.deltaTime;
                yield return null;
            }

            damageFlashImage.gameObject.SetActive(false);
        }
        
        public void ApplyKnockback(Vector3 direction, float force)
        {
            StartCoroutine(KnockbackCoroutine(direction, force));
        }

        private IEnumerator KnockbackCoroutine(Vector3 direction, float force)
        {
            float knockTime = 0.2f;
            float elapsed = 0f;

            while (elapsed < knockTime)
            {
                controller.Move(direction * force * Time.deltaTime);
                elapsed += Time.deltaTime;
                yield return null;
            }
        }

        private void HandleMovement()
        {
            float moveX = Input.GetAxis("Horizontal");
            float moveZ = Input.GetAxis("Vertical");

            Vector3 move = transform.right * moveX + transform.forward * moveZ;
            controller.Move(move * speed * Time.deltaTime);

            bool isMoving = move.magnitude >= 0.1f;

            photonView.RPC("FootStepsAudioSource", RpcTarget.AllBuffered, isMoving);

            animator.SetBool(IsRunning, isMoving);
        }

        [PunRPC]
        public void FootStepsAudioSource(bool shouldUnmute)
        {
            footStepsAudioSource.mute = !shouldUnmute;
        }

        private void HandleSprint()
        {
            if (stunned) return;
            
            if (currentStamina <= 0)
                canSprint = false;

            if (currentStamina >= staminaThreshold)
                canSprint = true;

            if (Input.GetKey(KeyCode.LeftShift) && canSprint && currentStamina > 0)
            {
                isSprinting = true;
                currentStamina -= staminaDrainRate * Time.deltaTime;
            }
            else
            {
                isSprinting = false;
                currentStamina += staminaRegenRate * Time.deltaTime;
            }

            currentStamina = Mathf.Clamp(currentStamina, 0, stamina);

            float targetSpeed = isSprinting ? sprintSpeed : defaultSpeed;
            speed = Mathf.Lerp(speed, targetSpeed, Time.deltaTime * acceleration);
        }
        public float[] GetStamina() => new float[] { currentStamina, stamina };
        
        private void HandleJump()
        {
            if (Input.GetButtonDown("Jump") && isGrounded && canJump)
            {
                velocity.y = Mathf.Sqrt(jumpHeight * 2f * gravity);
                StartCoroutine(SetAnimatorBool("Jump"));
                StartCoroutine(JumpCooldown());
            }
        }

        private IEnumerator JumpCooldown()
        {
            canJump = false;
            yield return new WaitForSeconds(jumpCooldown);
            canJump = true;
        }

        private void ApplyGravity()
        {
            velocity.y += Physics.gravity.y * Time.deltaTime;
            controller.Move(velocity * Time.deltaTime);
        }

        private void HandleCamera()
        {
            // Get mouse input
            float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity * Time.deltaTime;
            float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity * Time.deltaTime;

            // Vertical rotation with clamp
            xRotation = Mathf.Clamp(xRotation - mouseY, -75f, 75f);

            // Calculate tilt on Z axis based on mouse X
            float targetTiltZ = -mouseX * tiltAmount;
            Quaternion targetTiltRotation = Quaternion.Euler(xRotation, 0f, targetTiltZ);

            // Smoothly apply rotation
            cameraHolderTransform.localRotation = Quaternion.Slerp(cameraHolderTransform.localRotation,
                targetTiltRotation, Time.deltaTime * tiltSpeed);

            // Reset tilt if it exceeds the allowed amount
            Vector3 currentEulerAngles = cameraHolderTransform.localRotation.eulerAngles;
            if (Mathf.Abs(Mathf.DeltaAngle(0f, currentEulerAngles.z)) > Mathf.Abs(tiltAmount))
            {
                // Set Z rotation to 0 while keeping X and Y as they are
                cameraHolderTransform.localRotation = Quaternion.Euler(currentEulerAngles.x, currentEulerAngles.y, 0f);
            }

            // Handle camera bobbing when moving
            float moveX = Input.GetAxis("Horizontal");
            float moveZ = Input.GetAxis("Vertical");
            bool isMoving = moveX != 0 || moveZ != 0;

            if (isMoving && isGrounded)
            {
                bobTimer += Time.deltaTime * bobFrequency;
                float bobOffsetY = Mathf.Sin(bobTimer) * bobAmplitude;
                float bobOffsetX = Mathf.Cos(bobTimer / 2f) * bobAmplitude;

                Vector3 targetPosition = initialCameraLocalPos + new Vector3(bobOffsetX, bobOffsetY, 0f);
                cameraHolderTransform.localPosition = Vector3.Lerp(cameraHolderTransform.localPosition, targetPosition,
                    Time.deltaTime * bobSmoothing);
            }
            else
            {
                bobTimer = 0f;
                cameraHolderTransform.localPosition = Vector3.Lerp(cameraHolderTransform.localPosition,
                    initialCameraLocalPos, Time.deltaTime * bobSmoothing);
            }

            // Rotate player horizontally
            transform.Rotate(Vector3.up * mouseX);

            // Adjust FOV and bob smoothing when sprinting
            if (isSprinting)
            {
                bobSmoothing = 1.2f;
                camera.fieldOfView = ChangedFieldOfView;
            }
            else
            {
                bobSmoothing = bobSmoothingDefaultValue;
                camera.fieldOfView = DefaultFieldOfView;
            }
        }


        private void HandleCrouch()
        {
            if (Input.GetKeyDown(KeyCode.C)) SetCrouchState(true);
            else if (Input.GetKeyUp(KeyCode.C)) SetCrouchState(false);
        }

        private void OnApplicationFocus(bool hasFocus)
        {
            if (!hasFocus && isCrouching)
                SetCrouchState(false);
        }

        private void SetCrouchState(bool isCrouching)
        {
            this.isCrouching = isCrouching;
            
            animator.SetBool(IsCrouching, isCrouching);
            controller.height = isCrouching ? 1.0f : defaultCharacterHeight;
            controller.center = isCrouching ? controller.center / 2.0f : controller.center * 2.0f;
            
            speed *= isCrouching ? 0.4f : 2.5f;
            defaultSpeed *= isCrouching ? 0.4f : 2.5f;
            sprintSpeed *= isCrouching ? 0.4f : 2.5f;
            
            cameraHolderTransform.localPosition = isCrouching 
                ? new Vector3(cameraHolderTransform.localPosition.x,
                                cameraHolderTransform.localPosition.y - 0.55f,
                                cameraHolderTransform.localPosition.z) 
                : new Vector3(cameraHolderTransform.localPosition.x,
                                cameraHolderTransform.localPosition.y + 0.55f,
                                cameraHolderTransform.localPosition.z);
            initialCameraLocalPos = cameraHolderTransform.localPosition;
        }

        private void Die()
        {
            // Death logic
            Debug.Log($"{name} died.");
            
            Cursor.lockState = CursorLockMode.None;
    
            // Check if photonView is null before calling RPC
            if (photonView != null && PhotonNetwork.IsMasterClient)
            {
                photonView.RPC("RPC_Death", RpcTarget.AllBuffered);
                
                //Vector3 stateSpawnPosition = new Vector3(transform.position.x, -0.5f, transform.position.z);
                Vector3 stateSpawnPosition = transform.position - Vector3.up * 1.5f;
                PhotonNetwork.Instantiate(stateObj.name, stateSpawnPosition, Quaternion.identity);
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
                        if (obj != null && obj.gameObject != null && obj.gameObject != gameObject && 
                            !obj.gameObject.name.Contains("Camera holder") && !obj.gameObject.CompareTag("MainCamera"))
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
        
                //PhotonNetwork.Instantiate(stateObj.name, transform.position, Quaternion.identity);
        
                // Adjust transform positions only if objects exist
                if (transform != null)
                {
                    transform.position = new Vector3(transform.position.x, 1.5f, transform.position.z);
                    transform.rotation = Quaternion.identity;
                }
        
                if (cameraHolderTransform != null)
                {
                    cameraHolderTransform.localPosition = new Vector3(0f, 0f, 0f);
                    cameraHolderTransform.localRotation = Quaternion.identity;
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
                StunPlayer(3f);
                
                RoomManager roomManager = FindObjectOfType<RoomManager>();
                if (roomManager != null)
                {
                    roomManager.AddMurderMiss("trap");
                }
                
                // Making sound
                SoundManager.Instance.PlayTrapActionSound();
            }
        }

        private void StunPlayer(float secs)
        {
            Debug.Log($"{gameObject.name} (player) stunned.");
            StartCoroutine(Stun(secs));
        }

        private IEnumerator Stun(float seconds)
        {
            animator.enabled = false;
            jumpHeight = 0f;
            speed = 0f;
            stunned = true;
            yield return new WaitForSeconds(seconds);
            stunned = false;
            animator.enabled = true;
            jumpHeight = defaultJumpHeight;
            speed = defaultSpeed;
        }

        public bool GetStunnedState() => stunned;

        private void OnApplicationQuit()
        {
            GameObject ghost = PhotonNetwork.Instantiate("Ghost", new Vector3(45, 45, 45), Quaternion.identity);
            ghost.gameObject.GetComponent<GhostFreeMovement>().SetIsDead();
            
            ghost.gameObject.GetComponent<GhostFreeMovement>().SetDieState(PhotonNetwork.IsMasterClient ? 
                PlayerRoleEnum.Murder : PlayerRoleEnum.Victim);
            
            // win check
            RoomManager roomManager = FindObjectOfType<RoomManager>();
            if (roomManager != null)
            {
                roomManager.WinCheck(true);
            }
            PhotonNetwork.LeaveRoom();
        }
    }
}
