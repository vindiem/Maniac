using System.Collections;
using System.Collections.Generic;
using _Scripts.MultiplayerScripts;
using UnityEngine;

namespace _Scripts.MultiplayerScripts.LobbyScripts
{
    public class RoomItemButton : MonoBehaviour
    {
        public string roomName;

        public void OnClick()
        {
            RoomList.Instance.JoinRoomByName(roomName);
            GameObject canvas = GameObject.FindGameObjectWithTag("CreateAndJoinCanvas");
            canvas.gameObject.SetActive(false);
        }
    }
}
