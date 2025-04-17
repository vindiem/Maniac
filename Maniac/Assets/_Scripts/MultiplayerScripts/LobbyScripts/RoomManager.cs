using System;
using System.Collections.Generic;
using _Scripts.PlayerScripts;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;
using Random = System.Random;

namespace _Scripts.MultiplayerScripts.LobbyScripts
{
    public class RoomManager : MonoBehaviourPunCallbacks
    {
        public GameObject player;
        public Transform[] spawnPoints;
        public GameObject roomCamera;
        public GameObject connectionScreen;
        public GameObject nickNameScreen;
        private string _nickname = "unnamed";
        
        public GameObject winScreen;
        public Text winText;
        private int victimsCount = 0, murdersCount = 0;
        
        public string roomNameToJoin = "Room";

        private void Start()
        {
            roomCamera.SetActive(true);
            connectionScreen.SetActive(false);
            nickNameScreen.SetActive(true);
            
            winScreen.SetActive(false);
        }

        public void WinCheck()
        {
            if (PhotonNetwork.PlayerList.Length >= 2)
            {
                GameObject[] players = GameObject.FindGameObjectsWithTag("Player");
                List<GhostFreeMovement> deadGhosts = new List<GhostFreeMovement>();
                foreach (var t in players)
                {
                    if (t.GetComponent<GhostFreeMovement>().isDead)
                        deadGhosts.Add(t.GetComponent<GhostFreeMovement>());
                }
                Debug.Log($"Players: {players.Length} dead ghosts: {deadGhosts.Count}");

                if (deadGhosts.Count >= 1)
                {
                    foreach (GhostFreeMovement ghost in deadGhosts)
                    {
                        bool c = ghost.GetDieState() == PlayerRoleEnum.Victim;
                        bool v = ghost.GetDieState() == PlayerRoleEnum.Murder;
                        if (c) victimsCount++;
                        if (v) murdersCount++;
                    }
                    Debug.Log($"{deadGhosts.Count} dead ghosts, " +
                              $"({victimsCount} victims, {murdersCount} murders, players: {players.Length})");

                    if (victimsCount == players.Length - 1)
                    {
                        //WinScreen(PlayerRoleEnum.Murder);
                        GetComponent<PhotonView>().RPC("WinScreen", RpcTarget.AllBuffered, PlayerRoleEnum.Murder);
                        Debug.Log($"Murder win");
                    }
                    else if (murdersCount == 1)
                    {
                       //WinScreen(PlayerRoleEnum.Victim);
                       GetComponent<PhotonView>().RPC("WinScreen", RpcTarget.AllBuffered, PlayerRoleEnum.Victim);
                       Debug.Log($"Victim win");
                    }
                }
            }
            
        }

        [PunRPC]
        private void WinScreen(PlayerRoleEnum playerRoleEnum)
        {
            winScreen.SetActive(true);
            if (playerRoleEnum == PlayerRoleEnum.Victim)
            {
                winText.text = "Victims won!";
            }
            else if (playerRoleEnum == PlayerRoleEnum.Murder)
            {
                winText.text = "Murder won!";
            }
        }
        
        public void ChangeNickname(string name) => _nickname = name;

        public void JoinRoomButtonPressed()
        {
            nickNameScreen.SetActive(false);
            connectionScreen.SetActive(true);
            Debug.Log("Connecting to photon...");

            try
            {
                PhotonNetwork.JoinOrCreateRoom(roomNameToJoin, null, null);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                PhotonNetwork.LeaveRoom();
                PhotonNetwork.LeaveLobby();
                PhotonNetwork.Disconnect();
                throw;
            }
        }

        public override void OnJoinedRoom()
        {
            Debug.Log("Joined room");
            
            if (connectionScreen == null && roomCamera == null) return;
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

        private void OnApplicationQuit()
        {
            OnLeftRoom();
        }

        public override void OnLeftRoom()
        {
            base.OnLeftRoom();

            if (!PhotonNetwork.InRoom) return;
            PhotonNetwork.LeaveRoom();
            PhotonNetwork.LeaveLobby();
            PhotonNetwork.Disconnect();
        }
    }
}
