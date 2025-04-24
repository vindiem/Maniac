using System.Collections;
using UnityEngine;
using Photon.Pun;

namespace _Scripts.PlayerScripts
{
    public class LightController : MonoBehaviourPun
    {
        [Header("Light System")]
        private Light[] lightSources;

        private float maxLightUsage = 12f;
        private float cooldownTime = 0f;
        private float currentUsageTime = 0f;
        private bool isOnCooldown = false;
        private bool isLightOn = false;
        private bool isBlinking = false;

        private new PhotonView photonView;

        private void Awake()
        {
            photonView = GetComponent<PhotonView>();
        }

        private void Start()
        {
            cooldownTime = maxLightUsage / 2f;
            lightSources = GetComponentsInChildren<Light>();
            foreach (var lightSource in lightSources)
            {
                lightSource.enabled = false;
            }
        }

        private void Update()
        {
            if (!photonView.IsMine) return;

            if (Input.GetKeyDown(KeyCode.Z) && !isOnCooldown && !isBlinking)
            {
                ToggleLocalFlashlight(!isLightOn);
                photonView.RPC("SetFlashlightState", RpcTarget.Others, isLightOn);
            }

            if (isLightOn && !isOnCooldown && !isBlinking)
            {
                currentUsageTime += Time.deltaTime;
                if (currentUsageTime >= maxLightUsage)
                {
                    StartCoroutine(StartBlinkAndCooldown());
                }
            }
        }

        private void ToggleLocalFlashlight(bool turnOn)
        {
            isLightOn = turnOn;
            if (lightSources != null)
            {
                foreach (var lightSource in lightSources)
                {
                    lightSource.enabled = isLightOn;
                }
            }

            if (isLightOn)
                SoundManager.Instance.PlayLightSound();
        }

        [PunRPC]
        private void SetFlashlightState(bool turnOn)
        {
            isLightOn = turnOn;
            if (lightSources != null)
            {
                foreach (var lightSource in lightSources)
                {
                    lightSource.enabled = isLightOn;
                }
            }
        }

        private IEnumerator StartBlinkAndCooldown()
        {
            isOnCooldown = true;
            isBlinking = true;

            float blinkDuration = 2f;
            float blinkInterval = 0.2f;
            float elapsed = 0f;

            while (elapsed < blinkDuration)
            {
                ToggleLocalFlashlight(false);
                photonView.RPC("SetFlashlightState", RpcTarget.Others, false);
                yield return new WaitForSeconds(blinkInterval);

                ToggleLocalFlashlight(true);
                photonView.RPC("SetFlashlightState", RpcTarget.Others, true);
                yield return new WaitForSeconds(blinkInterval);

                elapsed += blinkInterval * 2;
            }

            ToggleLocalFlashlight(false);
            photonView.RPC("SetFlashlightState", RpcTarget.Others, false);

            currentUsageTime = 0f;
            isBlinking = false;

            yield return new WaitForSeconds(cooldownTime);
            isOnCooldown = false;
        }
    }
}
