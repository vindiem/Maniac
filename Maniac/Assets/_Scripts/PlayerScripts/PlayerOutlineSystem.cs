using System;
using Photon.Pun;
using UnityEngine;

namespace _Scripts.PlayerScripts
{
    public class PlayerOutlineSystem : MonoBehaviour
    {
        private PlayerTrapSystem playerTrapSystem;
        private Transform lastHighlightedPlayer = null;

        private void Start()
        {
            playerTrapSystem = GetComponent<PlayerTrapSystem>();
        }

        public void HighlightVictim(float attackDistance)
        {
            RaycastHit hit;
            if (Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out hit))
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
                            targetPlayer.GetComponent<PhotonView>().RPC("SetHighlight", 
                                RpcTarget.AllBuffered, true);
                        }

                        return;
                    }
                }
            }
            ResetHighlight();
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


