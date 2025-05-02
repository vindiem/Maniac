using System;
using System.Collections;
using Photon.Pun;
using UnityEngine;

namespace _Scripts.PlayerScripts
{
    public class PlayerTrapSystem : MonoBehaviour
    {
        private PhotonView photonView;
        
        [Header("Inventory System")]
        [SerializeField] private GameObject trapInHands;
        [SerializeField] private GameObject gunInHands;
        [SerializeField] private GameObject trapPrefab;
        [SerializeField] private GameObject gunPrefab;
        private bool holdTrap = false;
        private bool holdGun = false;
        private float distanceTrap = 3f;
        
        // Current active inventory slot (1 - trap, 2 - gun)
        private int activeSlot = 0;

        private PlayerMovement playerMovement;

        private void Awake()
        {
            photonView = GetComponent<PhotonView>();
        }

        private void Start()
        {
            playerMovement = GetComponent<PlayerMovement>();
        }
        
        private void Update()
        {
            // Only process input for the local player
            if (!photonView.IsMine) return;
            
            // Handle switching between items
            if (Input.GetKeyDown(KeyCode.Alpha1) && holdTrap)
            {
                SwitchToSlot(1);
            }
            else if (Input.GetKeyDown(KeyCode.Alpha2) && holdGun)
            {
                SwitchToSlot(2);
            }
        }
        
        // Method to switch between inventory slots
        private void SwitchToSlot(int slot)
        {
            if (slot == activeSlot) return;
            
            activeSlot = slot;
            
            // Synchronize slot change over the network
            if (photonView.IsMine)
            {
                photonView.RPC("SyncActiveSlot", RpcTarget.All, slot);
            }
        }
        
        [PunRPC]
        private void SyncActiveSlot(int slot)
        {
            activeSlot = slot;
            
            // Update the display of objects in hands
            UpdateHandsVisibility();
        }
        
        // Updates the visibility of items in hands
        private void UpdateHandsVisibility()
        {
            trapInHands.SetActive(activeSlot == 1 && holdTrap);
            gunInHands.SetActive(activeSlot == 2 && holdGun);
        }
        
        public float GetDistanceTrap() => distanceTrap;

        [PunRPC]
        private void PickUpTrap(int trapViewID)
        {
            holdTrap = true;
            
            // If active slot is 1 or no active slot, show trap in hands
            if (activeSlot == 0 || activeSlot == 1)
            {
                // Synchronize inventory state over the network
                if (photonView.IsMine)
                {
                    photonView.RPC("SyncActiveSlot", RpcTarget.All, 1);
                }
                else
                {
                    // If this is not the local player, just update the active slot
                    activeSlot = 1;
                    UpdateHandsVisibility();
                }
            }

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
        private void PickUpGun(int gunViewID)
        {
            holdGun = true;
            
            // If active slot is 2 or no active slot, show gun in hands
            if (activeSlot == 0 || activeSlot == 2)
            {
                // Synchronize inventory state over the network
                if (photonView.IsMine)
                {
                    photonView.RPC("SyncActiveSlot", RpcTarget.All, 2);
                }
                else
                {
                    // If this is not the local player, just update the active slot
                    activeSlot = 2;
                    UpdateHandsVisibility();
                }
            }

            Debug.Log($"{PhotonNetwork.NickName} received RPC. ViewID: {gunViewID}");

            PhotonView gunView = PhotonView.Find(gunViewID);
            if (gunView == null)
            {
                Debug.LogWarning($"{PhotonNetwork.NickName} didn't find object with ViewID: {gunViewID}");
                return;
            }

            if (gunView.IsRoomView)
            {
                Debug.LogWarning(
                    $"Gun {gunView.ViewID} belongs to the scene. Requesting MasterClient to take ownership.");
                if (PhotonNetwork.IsMasterClient)
                    gunView.TransferOwnership(PhotonNetwork.LocalPlayer);
                else
                {
                    photonView.RPC("RequestOwnership", RpcTarget.MasterClient, gunView.ViewID);
                    return;
                }
            }
            else
                gunView.TransferOwnership(PhotonNetwork.LocalPlayer);

            StartCoroutine(DestroyAfterOwnership(gunView));
        }

        [PunRPC] 
        private void RequestOwnership(int objectViewID)
        { 
            PhotonView objectView = PhotonView.Find(objectViewID);
            if (objectView != null && PhotonNetwork.IsMasterClient) 
                objectView.TransferOwnership(PhotonNetwork.LocalPlayer);
        }
        
        // Coroutine to wait for ownership transfer and then destroy the object
        private IEnumerator DestroyAfterOwnership(PhotonView objectView)
        { 
            int attempts = 10; 
            while (!objectView.IsMine && attempts > 0) 
            { 
                Debug.Log($"Waiting for ownership of object {objectView.ViewID}... Attempts left: {attempts}"); 
                attempts--; 
                yield return new WaitForSeconds(0.2f);
            }
        
            Debug.Log($"{PhotonNetwork.NickName} now owns the object {objectView.ViewID}. Destroying..."); 
            if (objectView.IsMine || PhotonNetwork.IsMasterClient) 
            { 
                if (objectView != null && objectView.gameObject != null) 
                    PhotonNetwork.Destroy(objectView.gameObject);
                else 
                    Debug.LogWarning("Object already destroyed or null.");
            }
            else 
                Debug.LogWarning(
                    $"{PhotonNetwork.NickName} is not the owner and cannot destroy object {objectView.ViewID}.");
        }

        [PunRPC]
        private void PlaceTrap()
        {
            holdTrap = false;
            
            // If dropping the active item, update active slot
            if (activeSlot == 1)
            {
                int newSlot = holdGun ? 2 : 0;
                
                // Synchronize inventory state over the network
                if (photonView.IsMine)
                {
                    photonView.RPC("SyncActiveSlot", RpcTarget.All, newSlot);
                }
                else
                {
                    // If this is not the local player, just update the active slot
                    activeSlot = newSlot;
                    UpdateHandsVisibility();
                }
            }
            else
            {
                // Just update the visibility without changing the active slot
                UpdateHandsVisibility();
            }

            if (PhotonNetwork.IsMasterClient)
            {
                Vector3 trapPosition = transform.position + transform.forward;
                Quaternion trapRotation = Quaternion.Euler(-90f, 0f, 0f);

                GameObject newTrap = PhotonNetwork.Instantiate(trapPrefab.name, trapPosition, trapRotation);
                PhotonView newTrapView = newTrap.GetComponent<PhotonView>();

                Debug.Log($"MasterClient placed trap {newTrapView.ViewID} at {trapPosition}");

                // Notify all players that trap has been created
                photonView.RPC("SyncTrap", RpcTarget.OthersBuffered,
                    newTrapView.ViewID, trapPosition);
            }
        }
        
        [PunRPC]
        private void PlaceGun()
        {
            holdGun = false;
            
            // If dropping the active item, update active slot
            if (activeSlot == 2)
            {
                int newSlot = holdTrap ? 1 : 0;
                
                // Synchronize inventory state over the network
                if (photonView.IsMine)
                {
                    photonView.RPC("SyncActiveSlot", RpcTarget.All, newSlot);
                }
                else
                {
                    // If this is not the local player, just update the active slot
                    activeSlot = newSlot;
                    UpdateHandsVisibility();
                }
            }
            else
            {
                // Just update the visibility without changing the active slot
                UpdateHandsVisibility();
            }

            if (PhotonNetwork.IsMasterClient)
            {
                Vector3 gunPosition = transform.position + transform.forward;
                Quaternion gunRotation = Quaternion.identity;

                GameObject newGun = PhotonNetwork.Instantiate(gunPrefab.name, gunPosition, gunRotation);
                PhotonView newGunView = newGun.GetComponent<PhotonView>();

                Debug.Log($"MasterClient placed gun {newGunView.ViewID} at {gunPosition}");

                // Notify all players that the gun has been created
                photonView.RPC("SyncGun", RpcTarget.OthersBuffered,
                    newGunView.ViewID, gunPosition);
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
        
        [PunRPC]
        private void SyncGun(int gunViewID, Vector3 position)
        {
            PhotonView gunView = PhotonView.Find(gunViewID);
            if (gunView != null)
            {
                gunView.transform.position = position;
                gunView.gameObject.SetActive(true);
                Debug.Log($"Synced gun {gunViewID} at {position}");
            }
        }
        
        // Synchronize the inventory state (what player is holding and what's visible)
        [PunRPC]
        private void SyncInventoryState(bool hasGun, bool hasTrap, int activeSlotValue)
        {
            this.holdGun = hasGun;
            this.holdTrap = hasTrap;
            this.activeSlot = activeSlotValue;
            
            // Update visibility based on new values
            UpdateHandsVisibility();
        }

        // Main method to handle all inventory interactions
        public void HandleInventory()
        {
            // Only process input for the local player
            if (!photonView.IsMine) return;
            
            RaycastHit hit;
            if (Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out hit, distanceTrap))
            {
                // Pick up trap
                if (hit.collider.CompareTag("Trap"))
                {
                    if (Input.GetKeyDown(KeyCode.F) && !holdTrap)
                    {
                        PhotonView trapView = hit.collider.GetComponent<PhotonView>();
                        if (trapView != null)
                        {
                            photonView.RPC("PickUpTrap", RpcTarget.All, trapView.ViewID);
                            
                            // Making sound
                            SoundManager.Instance.PlayTrapCollectingSound();
                        }
                    }
                }
                // Pick up gun
                else if (hit.collider.CompareTag("Gun"))
                {
                    if (Input.GetKeyDown(KeyCode.F) && !holdGun)
                    {
                        PhotonView gunView = hit.collider.GetComponent<PhotonView>();
                        if (gunView != null)
                        {
                            photonView.RPC("PickUpGun", RpcTarget.All, gunView.ViewID);
                            
                            // Making sound - add gun pickup sound if you have one
                            // SoundManager.instance.PlayGunCollectSound();
                        }
                    }
                }
                
                // Drop item based on active slot
                if (Input.GetKeyDown(KeyCode.G))
                {
                    if (activeSlot == 1 && holdTrap)
                    {
                        photonView.RPC("PlaceTrap", RpcTarget.AllBuffered);
                        
                        // Making sound
                        SoundManager.Instance.PlayTrapPlacingSound();
                    }
                    else if (activeSlot == 2 && holdGun)
                    {
                        photonView.RPC("PlaceGun", RpcTarget.AllBuffered);
                        
                        // Making sound - add gun drop sound if you have one
                        // SoundManager.instance.PlayGunPlaceSound();
                    }
                }
            }
        }
        
        // For late joiners - synchronize the current state
        public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
        {
            if (stream.IsWriting)
            {
                // We own this player: send the others our data
                stream.SendNext(holdTrap);
                stream.SendNext(holdGun);
                stream.SendNext(activeSlot);
            }
            else
            {
                // Network player, receive data
                this.holdTrap = (bool)stream.ReceiveNext();
                this.holdGun = (bool)stream.ReceiveNext();
                this.activeSlot = (int)stream.ReceiveNext();
                
                // Update visibility based on received values
                UpdateHandsVisibility();
            }
        }
        
        // Getter methods for external scripts
        public bool GetTrapInHandsBool() => holdTrap;
        public bool GetGunInHandsBool() => holdGun;
        public int GetActiveSlot() => activeSlot;
    }
}