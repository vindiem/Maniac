using System;
using System.Collections;
using Photon.Pun;
using UnityEngine;

namespace _Scripts.PlayerScripts
{
    public class PlayerOutlineSystem : MonoBehaviourPun
    {
        [Header("Outline Settings")]
        [SerializeField] private float yOffset = 1f;
        [SerializeField] private float maxAutoAimAngle = 45f;
        [SerializeField] private float autoAimSmoothness = 5f;
        [SerializeField] private float highlightDuration = 4f;
        [SerializeField] private float stopAimingDistance = 1f;

        private PlayerTrapSystem playerTrapSystem;
        private Transform lastHighlightedPlayer = null;
        private Camera mainCamera;
        [SerializeField] private GameObject mainCameraHolder; 

        private void Start()
        {
            playerTrapSystem = GetComponent<PlayerTrapSystem>();
            mainCamera = Camera.main; // Cache the main camera once
        }

        public void HighlightVictim(float attackDistance)
        {
            RaycastHit hit;
            if (Physics.Raycast(mainCamera.ScreenPointToRay(Input.mousePosition), out hit))
            {
                if (hit.collider.CompareTag("Player") && hit.collider.gameObject != gameObject)
                {
                    Transform targetPlayer = hit.collider.transform;
                    float distanceToTarget = Vector3.Distance(transform.position, targetPlayer.position);

                    if (distanceToTarget <= attackDistance &&
                        targetPlayer.GetComponent<PlayerRole>().GetRole() == PlayerRoleEnum.Victim)
                    {
                        if (lastHighlightedPlayer != targetPlayer)
                        {
                            ResetHighlight();
                            lastHighlightedPlayer = targetPlayer;
                            targetPlayer.GetComponent<PhotonView>().RPC("SetHighlight", RpcTarget.AllBuffered, true);
                        }

                        if (Input.GetMouseButton(1) && distanceToTarget > stopAimingDistance)
                        {
                            AutoAimAtTarget(targetPlayer);
                        }

                        return;
                    }
                }
            }

            ResetHighlight();
        }

        private void AutoAimAtTarget(Transform target)
        {
            Transform camTransform = mainCameraHolder.transform;

            // Calculate direction to the target with vertical offset
            Vector3 targetDirection = (target.position + Vector3.up * yOffset) - camTransform.position;
            Quaternion targetRotation = Quaternion.LookRotation(targetDirection);

            // Check if target is within aim angle
            float angleToTarget = Vector3.Angle(camTransform.forward, targetDirection.normalized);
            if (angleToTarget <= maxAutoAimAngle)
            {
                // Smoothly rotate the camera
                Quaternion smoothedRotation = Quaternion.Slerp(camTransform.rotation, targetRotation, Time.deltaTime * autoAimSmoothness);
                camTransform.rotation = Quaternion.Euler(smoothedRotation.eulerAngles.x, smoothedRotation.eulerAngles.y, 0f);

                // Smoothly rotate the player towards the target (horizontal only)
                Vector3 lookDirection = target.position - transform.position;
                lookDirection.y = 0f;
                Quaternion playerRotation = Quaternion.LookRotation(lookDirection);
                transform.rotation = Quaternion.Slerp(transform.rotation, playerRotation, Time.deltaTime * autoAimSmoothness);
            }
        }

        private void ResetHighlight()
        {
            if (lastHighlightedPlayer != null)
            {
                lastHighlightedPlayer.GetComponent<PhotonView>().RPC("SetHighlight", RpcTarget.AllBuffered, false);
                lastHighlightedPlayer = null;
            }
        }

        [PunRPC]
        public void HighlightAll()
        {
            GameObject[] players = GameObject.FindGameObjectsWithTag("Player");
            foreach (var player in players)
            {
                StartCoroutine(ResetHighlight(player));
            }
        }

        private IEnumerator ResetHighlight(GameObject targetPlayer)
        {
            targetPlayer.GetComponent<PhotonView>().RPC("SetHighlight", RpcTarget.AllBuffered, true);
            yield return new WaitForSeconds(highlightDuration);
            targetPlayer.GetComponent<PhotonView>().RPC("SetHighlight", RpcTarget.AllBuffered, false);
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
    }
}
