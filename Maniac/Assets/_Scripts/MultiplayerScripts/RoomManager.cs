using System;
using Photon.Pun;
using UnityEngine;
using Random = System.Random;

namespace _Scripts.MultiplayerScripts
{
    public class RoomManager : MonoBehaviourPunCallbacks
    {
        public GameObject player;
        public Transform[] spawnPoints;
        public GameObject roomCamera;
        public GameObject connectionScreen;
        public GameObject nickNameScreen;
        private string _nickname = "unnamed";

        private void Start()
        {
            roomCamera.SetActive(true);
            connectionScreen.SetActive(false);
            nickNameScreen.SetActive(true);
        }

        public void ChangeNickname(string name) => _nickname = name;

        public void JoinRoomButtonPressed()
        {
            nickNameScreen.SetActive(false);
            connectionScreen.SetActive(true);
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
            connectionScreen.SetActive(false);
            roomCamera.SetActive(false);
            SpawnPlayer();
        }

        private void SpawnPlayer()
        {
            Random random = new Random();
            int index = random.Next(0, spawnPoints.Length);
        
            GameObject _player = PhotonNetwork.Instantiate(player.name, spawnPoints[index].position, Quaternion.identity);
            _player.GetComponent<PlayerSetup>().SetupLocalPlayer();
            _player.GetComponent<PhotonView>().RPC("SetNickname_RPC", RpcTarget.AllBuffered, _nickname);
        }
    
    }
}
