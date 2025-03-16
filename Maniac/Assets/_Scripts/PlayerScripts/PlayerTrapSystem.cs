using System;
using System.Collections;
using Photon.Pun;
using UnityEngine;

namespace _Scripts.PlayerScripts
{
    public class PlayerTrapSystem : MonoBehaviour
    {
        private PhotonView photonView;
        
        [Header("Trap system variables")] 
        [SerializeField] private GameObject trapInHands;
        [SerializeField] private GameObject trapPrefab;
        private bool heldTrap = false;
        private float distanceTrap = 3f;

        private PlayerMovement playerMovement;

        private void Awake()
        {
            photonView = GetComponent<PhotonView>();
        }

        private void Start()
        {
            playerMovement = GetComponent<PlayerMovement>();
        }
        
        public float GetDistanceTrap() => distanceTrap;

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
                photonView.RPC("SyncTrap", RpcTarget.OthersBuffered,
                    newTrapView.ViewID, trapPosition);
                ;
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

        public void HandleTrap()
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
        
        public bool GetTrapInHandsBool() => heldTrap;
        
        // Draw gizmos
        private void OnDrawGizmosSelected() 
        {
            Debug.DrawRay(Camera.main.transform.position, Camera.main.transform.forward * distanceTrap, Color.green); 
            Debug.DrawRay(Camera.main.transform.position, Camera.main.transform.forward * playerMovement.attackDistance, Color.blue);
        }

    }
}
