using Photon.Pun;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace _Scripts
{
    public class ScenesManager : MonoBehaviour
    {
        public void LoadScene(string sceneName)
        {
            SceneManager.LoadScene(sceneName);
        
            if (PhotonNetwork.InRoom)
            {
                PhotonNetwork.LeaveRoom();
                PhotonNetwork.Disconnect();
            }
        }
    }
}
