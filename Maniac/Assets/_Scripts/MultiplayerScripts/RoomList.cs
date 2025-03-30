using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace _Scripts.MultiplayerScripts
{
    public class RoomList : MonoBehaviourPunCallbacks
    {
        public static RoomList Instance;
        
        public GameObject roomManagerGameObject;
        public RoomManager roomManager;
        
        [Header("UI")]
        public Transform roomListParent;
        public GameObject roomListItemPrefab;
        
        private List<RoomInfo> cachedRoomList = new List<RoomInfo>();

        private void Awake()
        {
            Instance = this;
        }

        private IEnumerator Start()
        {
            if (PhotonNetwork.InRoom)
            {
                PhotonNetwork.LeaveRoom();
                PhotonNetwork.Disconnect();
            }
            yield return new WaitUntil(() => !PhotonNetwork.IsConnected);

            PhotonNetwork.ConnectUsingSettings();
        }

        public override void OnConnectedToMaster()
        {
            base.OnConnectedToMaster();
            
            PhotonNetwork.JoinLobby();
            
        }

        public override void OnRoomListUpdate(List<RoomInfo> roomList)
        {
            if (cachedRoomList.Count <= 0)
            {
                cachedRoomList = roomList;
            }
            else
            {
                foreach (RoomInfo room in roomList)
                {
                    for (int i = 0; i < cachedRoomList.Count; i++)
                    {
                        if (cachedRoomList[i].Name == room.Name)
                        {
                            List<RoomInfo> newList = cachedRoomList;

                            if (room.RemovedFromList)
                            {
                                newList.Remove(newList[i]);
                            }
                            else
                            {
                                newList[i] = room;
                            }
                            cachedRoomList = newList;
                        }
                    }
                }
            }
            UpdateUI();
        }

        public void ChangeRoomToCreateName(string roomName)
        {
            roomManager.roomNameToJoin = roomName;
        }

        private void UpdateUI()
        {
            foreach (Transform roomItem in roomListParent)
            {
                Destroy(roomItem.gameObject);
            }

            foreach (var room in cachedRoomList)
            {
                GameObject roomItem = Instantiate(roomListItemPrefab, roomListParent);
                
                roomItem.transform.GetChild(0).GetComponent<Text>().text = room.Name;
                roomItem.transform.GetChild(1).GetComponent<Text>().text = room.PlayerCount + "/" + room.MaxPlayers;
                
                roomItem.GetComponent<RoomItemButton>().roomName = room.Name;
                
            }
        }

        public void JoinRoomByName(string roomName)
        {
            roomManager.roomNameToJoin = roomName;
            roomManagerGameObject.SetActive(true);
        }
        
    }
}