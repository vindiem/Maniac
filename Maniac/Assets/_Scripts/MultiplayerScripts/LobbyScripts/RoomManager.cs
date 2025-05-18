using System;
using System.Collections;
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
        
        public GameObject winScreen;
        public Text winText;
        public float showDuration = 0.6f;
        
        private int victimsCount = 0, murdersCount = 0;
        
        private bool stillPlaying = true;
        private float gameDuration = 0f;
        
        public Text murdersText;
        public Text shotsAtTheMurderText;
        public Text gotTrappedText;

        public Text maxTimeAlive;
        public Text numbersOfSurvivors;

        private int shotsAtMurder = 0;
        private int murderTrapped = 0;

        private void Start()
        {
            roomCamera.SetActive(false);
            winScreen.SetActive(false);
        }

        private void Update()
        {
            if (!stillPlaying)
            {
                gameDuration += Time.deltaTime;
            }
        }

        public void AddMurderMiss(string miss)
        {
            if (miss == "trap")
            {
                murderTrapped++;
            }
            else if (miss == "shot")
            {
                shotsAtMurder++;
            }
            else Debug.LogError(miss);
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
        public void WinScreen(PlayerRoleEnum playerRoleEnum)
        {
            stillPlaying = false;
            
            SoundManager.Instance.PlayGameOverSound();
            winScreen.SetActive(true);
            winScreen.transform.localScale = Vector3.zero;

            if (playerRoleEnum == PlayerRoleEnum.Victim)
                winText.text = "Victims won!";
            else if (playerRoleEnum == PlayerRoleEnum.Murder)
                winText.text = "Murder won!";
            
            // WinScreen UI
            GhostFreeMovement[] ghosts = GameObject.FindObjectsOfType<GhostFreeMovement>();
            murdersText.text = $"\tMurders: <color=red>{ghosts.Length}</color>";
            shotsAtTheMurderText.text = $"\tShots at the killer: <color=red>{shotsAtMurder}</color>";
            gotTrappedText.text = $"\tGetting trapped: <color=red>{murderTrapped}</color>";

            maxTimeAlive.text = $"\tMax. survival time: <color=green>{MathF.Round(gameDuration, 2)}</color> seconds";
            GameObject[] players = GameObject.FindGameObjectsWithTag("Player");
            int number = players.Length - ghosts.Length;
            numbersOfSurvivors.text = $"\tNumber of survivors: <color=green>{number}</color>";

            StartCoroutine(AnimateWinScreen());
        }

        private IEnumerator AnimateWinScreen()
        {
            float elapsed = 0f;
            Vector3 startScale = Vector3.zero;
            Vector3 endScale = Vector3.one;

            while (elapsed < showDuration)
            {
                elapsed += Time.deltaTime;
                winScreen.transform.localScale = Vector3.Lerp(startScale, endScale, elapsed / showDuration);
                yield return null;
            }

            winScreen.transform.localScale = endScale;
        }

        public void SpawnPlayer()
        {
            Debug.Log("Spawn player!");
            
            Random random = new Random();
            int index = random.Next(0, spawnPoints.Length);
        
            GameObject _player = PhotonNetwork.Instantiate(player.name, spawnPoints[index].position, Quaternion.identity);
            _player.GetComponent<PlayerSetup>().SetupLocalPlayer();
            
            string _nickname = PlayerPrefs.GetString("username");
            _player.GetComponent<PhotonView>().RPC("SetNickname_RPC", RpcTarget.AllBuffered, _nickname);
        }

    }
}
