using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;
using Random = UnityEngine.Random;

public class PlayerSetup : MonoBehaviourPunCallbacks
{
    public PlayerMovement playerMovement;
    public GameObject playerCamera;
    public GameObject playerCanvas;
    
    [SerializeField] private string role = "victim";
    [SerializeField] private GameObject[] skins;
    
    private PhotonView photonView;

    private void Awake()
    {
        photonView = GetComponent<PhotonView>();
    }

    public void IsLocalPlayer()
    {
        if (photonView.IsMine)
        {
            playerMovement.enabled = true;
            playerCamera.gameObject.SetActive(true);
            playerCanvas.SetActive(true);
            FindPlayersAndSetRole();
            
        }
    }
    
    public void SetRoleAndSkin(string role)
    {
        this.role = role;
        Debug.Log($"{role} set to new player");
        
        Text[] texts = playerCanvas.GetComponentsInChildren<Text>();
        Text roleText = null;
        foreach (Text text in texts)
        {
            if (text.name == "Role") roleText = text;
        }
        roleText.text = $"Role: {role}";
        
        int randomSkin = (role == "victim") ? Random.Range(0, 3) : Random.Range(3, skins.Length);
        photonView.RPC("SetSkin_RPC", RpcTarget.AllBuffered, randomSkin);
        
    }
    
    [PunRPC]
    private void SetSkin_RPC(int skinIndex)
    {
        foreach (GameObject skin in skins) 
            skin.SetActive(false);
        
        skins[skinIndex].SetActive(true);
    }
    
    private void FindPlayersAndSetRole()
    {
        if (GameObject.FindGameObjectsWithTag("Player").Length > 1) 
            SetRoleAndSkin("victim");
        else SetRoleAndSkin("murder");
    }
}
