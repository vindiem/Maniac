using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using Photon.Pun;

namespace _Scripts.PlayerScripts
{
    public class FearEffect : MonoBehaviour
    {
        public float detectionRadius = 10f;
        public float maxFear = 100f;
        public float fearIncreaseRate = 5f;
        public float fearDecreaseRate = 2f;
        public float stunDuration = 2f;
        public Image fearOverlay;
        public Transform cameraTransform;

        private float currentFear = 50f;
        private bool isStunned = false;
        private Transform murderer;
        private Vector3 originalCameraPos;

        private void Start()
        {
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

            originalCameraPos = cameraTransform.localPosition;
        }

        private void Update()
        {
            if (GetComponent<PlayerRole>().GetRole() == PlayerRoleEnum.Murder) return;
            if (isStunned) return;

            murderer = FindNearestMurderer();

            if (murderer != null)
            {
                currentFear += fearIncreaseRate * Time.deltaTime;
                Debug.Log($"Murderer nearby! Fear increasing: {currentFear}");
            }
            else
            {
                // Плавное снижение страха, но не ниже 50
                currentFear -= fearDecreaseRate * Time.deltaTime;
                Debug.Log($"No murderer nearby. Fear decreasing: {currentFear}");
            }

            currentFear = Mathf.Clamp(currentFear, 50, maxFear); // Теперь не опускается ниже 50
            ApplyFearEffects();

            if (currentFear >= maxFear && !isStunned)
            {
                StartCoroutine(StunPlayer());
            }
        }

        public float GetCurrentFear() => currentFear;

        private Transform FindNearestMurderer()
        {
            Collider[] colliders = Physics.OverlapSphere(transform.position, detectionRadius);
            if (colliders.Length == 0) return null;

            foreach (Collider col in colliders)
            {
                PlayerRole role = col.GetComponent<PlayerRole>();
                if (role.gameObject == PhotonNetwork.IsMasterClient)
                {
                    Debug.Log($"Found player with role: {role.GetRole()}"); 
                    return col.transform;
                }
            }
            return null;
        }

        private void ApplyFearEffects()
        {
            // Затемнение экрана
            if (fearOverlay != null)
            {
                Color color = fearOverlay.color;
                color.a = Mathf.Lerp(0f, 0.5f, (currentFear - 50) / (maxFear - 50)); // Сдвигаем диапазон (50-100)
                fearOverlay.color = color;
            }

            // Тряска камеры (уменьшенная амплитуда)
            if (currentFear > 50f)
            {
                float shakeAmount = ((currentFear - 50) / (maxFear - 50)) * 0.05f;
                cameraTransform.localPosition = originalCameraPos + Random.insideUnitSphere * shakeAmount;
            }
            else
            {
                cameraTransform.localPosition = originalCameraPos;
            }
        }

        private IEnumerator StunPlayer()
        {
            isStunned = true;
            Debug.Log("Player stunned!");

            GetComponent<PlayerMovement>().enabled = false;
            yield return new WaitForSeconds(stunDuration);
            GetComponent<PlayerMovement>().enabled = true;

            isStunned = false;
            currentFear = 50f; // После шока страх сбрасывается до 50
        }
    }
}
