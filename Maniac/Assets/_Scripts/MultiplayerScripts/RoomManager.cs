using System;
using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using UnityEngine;
using Random = System.Random;

public class RoomManager : MonoBehaviourPunCallbacks
{
    public GameObject player;
    public Transform[] spawnPoints;
    
    private void Start()
    {
        Debug.Log("Connecting to photon...");
        PhotonNetwork.ConnectUsingSettings();
    }

    public override void OnConnectedToMaster()
    {
        Debug.Log("Connected to Master (Server)...");
        PhotonNetwork.JoinLobby();
    }

    public override void OnJoinedLobby()
    {
        PhotonNetwork.JoinOrCreateRoom("test", null, null);
        Debug.Log("Joined or created lobby");
    }

    public override void OnJoinedRoom()
    {
        Debug.Log("Joined room");
        
        Random random = new Random();
        int index = random.Next(0, spawnPoints.Length);
        
        GameObject _player = PhotonNetwork.Instantiate(player.name, spawnPoints[index].position, Quaternion.identity);
        _player.GetComponent<PlayerSetup>().IsLocalPlayer();
    }
    
}
