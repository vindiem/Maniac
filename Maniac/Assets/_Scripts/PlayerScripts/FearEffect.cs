using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using Photon.Pun;

namespace _Scripts.PlayerScripts
{
    public class FearEffect : MonoBehaviourPunCallbacks
    {
        [Header("Fear Settings")]
        public float detectionRadius = 10f;
        public float maxFear = 100f;
        public float minFear = 50f;
        public float fearIncreaseRate = 5f;
        public float fearDecreaseRate = 2f;
        public float stunDuration = 2f;
        
        [Header("Visual Effect Settings")]
        [Range(0f, 1f)]
        public float maxOverlayAlpha = 0.7f; // Maximum darkness of the overlay
        [Range(0f, 0.1f)]
        public float maxShakeAmount = 0.02f; // Reduced max camera shake
        public float overlayTransitionSpeed = 2f; // Higher = faster transition
        
        [Header("References")]
        public Image fearOverlay;
        public Transform cameraTransform;

        private float currentFear = 50f;
        private bool isStunned = false;
        private Transform murderer;
        private Vector3 originalCameraPos;
        private PlayerMovement playerMovement;
        private float currentOverlayAlpha = 0f;

        private void Start()
        {
            // Cache component reference
            playerMovement = GetComponent<PlayerMovement>();
            
            if (cameraTransform == null)
            {
                Debug.LogError("FearEffect: CameraTransform is not assigned!");
                enabled = false;
                return;
            }

            if (fearOverlay == null)
            {
                Debug.LogWarning("FearEffect: FearOverlay is not assigned. Fear UI effect won't work.");
            }
            else
            {
                // Initialize overlay to be transparent
                Color startColor = fearOverlay.color;
                startColor.a = 0f;
                fearOverlay.color = startColor;
            }

            originalCameraPos = cameraTransform.localPosition;
        }

        private void Update()
        {
            // Skip if we are the murderer
            PlayerRole playerRole = GetComponent<PlayerRole>();
            if (playerRole != null && playerRole.GetRole() == PlayerRoleEnum.Murder) 
                return;
                
            if (isStunned) 
                return;

            // Find murderer
            murderer = FindNearestMurderer();

            // Update fear level
            if (murderer != null)
            {
                float distance = Vector3.Distance(transform.position, murderer.position);
                float fearFactor = 1.0f - (distance / detectionRadius); // More fear when closer
                currentFear += fearIncreaseRate * fearFactor * Time.deltaTime;
                //Debug.Log($"Murderer nearby at distance {distance}! Fear increasing: {currentFear}");
            }
            else
            {
                // Gradually decrease fear, but not below minFear
                currentFear -= fearDecreaseRate * Time.deltaTime;
                //Debug.Log($"No murderer nearby. Fear decreasing: {currentFear}");
            }

            // Clamp fear between minimum and maximum
            currentFear = Mathf.Clamp(currentFear, minFear, maxFear);
            
            // Apply visual effects based on fear level
            ApplyFearEffects();

            // Check if player should be stunned
            if (currentFear >= maxFear && !isStunned)
            {
                StartCoroutine(StunPlayer());
            }
        }

        public float GetCurrentFear() => currentFear;

        private Transform FindNearestMurderer()
        {
            Collider[] colliders = Physics.OverlapSphere(transform.position, detectionRadius);
            Transform closestMurderer = null;
            float closestDistance = detectionRadius;

            foreach (Collider col in colliders)
            {
                // Skip if it's our own collider
                if (col.gameObject == gameObject)
                    continue;
                    
                PhotonView photonView = col.GetComponent<PhotonView>();
                
                // Skip if photonView is null
                if (photonView == null)
                    continue;
                
                // Skip if Owner is null
                if (photonView.Owner == null)
                {
                    //Debug.LogWarning("Found a PhotonView with null Owner");
                    continue;
                }
                
                // Check if this player is controlled by MasterClient (murderer)
                if (photonView.Owner.IsMasterClient && !photonView.IsMine)
                {
                    float distance = Vector3.Distance(transform.position, col.transform.position);
                    if (distance < closestDistance)
                    {
                        closestDistance = distance;
                        closestMurderer = col.transform;
                        //Debug.Log($"Found murderer at distance {distance}");
                    }
                }
            }
            
            return closestMurderer;
        }

        private void ApplyFearEffects()
        {
            // Calculate target alpha based on fear level
            float targetAlpha = Mathf.Lerp(0f, maxOverlayAlpha, (currentFear - minFear) / (maxFear - minFear));
            
            // Screen darkening effect with smooth transition
            if (fearOverlay != null)
            {
                // Smoothly interpolate the current alpha toward the target alpha
                currentOverlayAlpha = Mathf.Lerp(currentOverlayAlpha, targetAlpha, Time.deltaTime * overlayTransitionSpeed);
                
                Color color = fearOverlay.color;
                color.a = currentOverlayAlpha;
                fearOverlay.color = color;
            }

            // Camera shake effect (intensity based on fear level, but more subtle)
            if (currentFear > minFear && cameraTransform != null)
            {
                // Non-linear scaling for more dramatic effect at high fear levels
                float fearPercentage = (currentFear - minFear) / (maxFear - minFear);
                float shakeAmount = Mathf.Pow(fearPercentage, 2) * maxShakeAmount; // Square makes it increase more at higher levels
                
                // Apply slight randomization to make shake less predictable
                if (fearPercentage > 0.3f) // Only shake when fear is above 30%
                {
                    // More subtle horizontal shake than vertical
                    Vector3 shake = new Vector3(
                        Random.Range(-1f, 1f) * shakeAmount * 0.7f, // Less horizontal shake
                        Random.Range(-1f, 1f) * shakeAmount,
                        0f
                    );
                    
                    cameraTransform.localPosition = originalCameraPos + shake;
                }
                else
                {
                    cameraTransform.localPosition = originalCameraPos;
                }
            }
            else if (cameraTransform != null)
            {
                cameraTransform.localPosition = originalCameraPos;
            }
        }

        private IEnumerator StunPlayer()
        {
            isStunned = true;
            Debug.Log("Player stunned from fear!");

            // Disable movement
            if (playerMovement != null)
                playerMovement.enabled = false;
                
            // Dramatic screen effect at max fear
            if (fearOverlay != null)
            {
                Color color = fearOverlay.color;
                color.a = maxOverlayAlpha;
                fearOverlay.color = color;
            }
                
            // Wait for stun duration
            yield return new WaitForSeconds(stunDuration);
            
            // Re-enable movement
            if (playerMovement != null)
                playerMovement.enabled = true;

            // Reset fear to minimum level after being stunned
            isStunned = false;
            currentFear = minFear;
        }
    }
}