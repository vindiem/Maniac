using System.Collections;
using Photon.Pun;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace _Scripts
{
    public class ScenesManager : MonoBehaviour
    {
        public void LoadScene(string sceneName)
        {
            SoundManager.Instance.PlayBackButtonPressedSound();
            SceneManager.LoadScene(sceneName);
            
            if (!PhotonNetwork.InRoom) return;
            PhotonNetwork.LeaveRoom();
            PhotonNetwork.LeaveLobby();
            PhotonNetwork.Disconnect();
        }

        public void Quit()
        {
            if (!PhotonNetwork.InRoom)
            {
                PhotonNetwork.LeaveRoom();
                PhotonNetwork.LeaveLobby();
                PhotonNetwork.Disconnect();
            }
            
            Application.Quit();
            
        }
    }
}
