using System;
using System.Collections;
using System.Collections.Generic;
using _Scripts.PlayerScripts;
using UnityEngine;
using Photon.Pun;

public class Weapon : MonoBehaviour
{
    [SerializeField] private Camera camera;
    [SerializeField] private float damage = 25f;
    private float fireRate = 2.5f;
    private float maxDistance = 250f;
    
    private float nextFire = 0f;

    private void Update()
    {
        if (Input.GetButton("Fire1") && nextFire <= 0f)
        {
            nextFire += fireRate;
            Fire();
        }
        nextFire -= Time.deltaTime;
    }

    private void Fire()
    {
        // Making sound
        //SoundManager.instance.PlayShootSound();
        RaycastHit hit;
        if (Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out hit, maxDistance))
        {
            GameObject obj = hit.collider.transform.gameObject;
            if (obj.CompareTag("Player") && obj != gameObject)
            {
                PhotonView photonView = obj.GetComponent<PhotonView>();
                PlayerMovement playerMovement = obj.GetComponent<PlayerMovement>();
                int viewId = obj.GetComponent<PhotonView>().ViewID;
                
                bool isOwnerMasterClient = photonView.Owner.IsMasterClient;
                Debug.Log($"Is Owner Master Client: {isOwnerMasterClient}");
                if (isOwnerMasterClient)
                {
                    photonView.RPC("TakeDamage", RpcTarget.AllBuffered, viewId, damage);
                    playerMovement.StunMurder();
                    Debug.Log(obj.name + " takes " + damage + " damage " + "view id " + viewId);
                }
            }
        }
    }
    
}
