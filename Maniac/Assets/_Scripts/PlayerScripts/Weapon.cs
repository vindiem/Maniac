using System;
using System.Collections;
using System.Collections.Generic;
using _Scripts.PlayerScripts;
using UnityEngine;
using Photon.Pun;

public class Weapon : MonoBehaviour
{
    private float damage = 5f;
    private float fireRate = 12.5f;
    private float maxDistance = 250f;
    private float nextFire = 0f;

    private void Update()
    {
        if (Input.GetButtonDown("Fire1") && nextFire <= 0f && !PhotonNetwork.IsMasterClient)
        {
            // Making sound
            SoundManager.instance.PlayShootSound();
            nextFire = fireRate;
            Fire();
        }
        nextFire -= Time.deltaTime;
    }

    private void Fire()
    {
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
                    Debug.Log(obj.name + " takes " + damage + " damage " + "view id " + viewId);
                }
            }
        }
    }

    public float GetNextFirePercent() => MathF.Round((nextFire / fireRate), 2);

}
