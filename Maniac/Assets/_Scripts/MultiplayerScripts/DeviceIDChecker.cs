using Photon.Pun;
using Photon.Realtime;
using UnityEngine;
using ExitGames.Client.Photon;
using UnityEngine.SceneManagement;

public class DeviceIDChecker : MonoBehaviourPunCallbacks
{
    private const string DeviceIdKey = "DeviceID";
    private string deviceID;

    private void Start()
    {
        deviceID = SystemInfo.deviceUniqueIdentifier;
        PhotonNetwork.ConnectUsingSettings();
    }

    public override void OnConnectedToMaster()
    {
        PhotonNetwork.JoinOrCreateRoom("TestRoom", new RoomOptions { MaxPlayers = 10 }, TypedLobby.Default);
    }

    public override void OnJoinedRoom()
    {
        Debug.Log("JoinedRoom");

        // Setting Custom Property with deviceID
        Hashtable customProperties = new Hashtable { { DeviceIdKey, deviceID } };
        PhotonNetwork.LocalPlayer.SetCustomProperties(customProperties);

        // Check other players
        foreach (Player player in PhotonNetwork.PlayerListOthers)
        {
            if (player.CustomProperties.ContainsKey(DeviceIdKey))
            {
                if (player.CustomProperties[DeviceIdKey].ToString() == deviceID)
                {
                    Debug.LogWarning("Leave room (same device)");
                    PhotonNetwork.LeaveRoom();
                    SceneManager.LoadScene("Menu");
                    return;
                }
            }
        }
    }

    public override void OnPlayerPropertiesUpdate(Player targetPlayer, Hashtable changedProps)
    {
        if (changedProps.ContainsKey(DeviceIdKey))
        {
            string updatedDeviceID = changedProps[DeviceIdKey].ToString();
            if (updatedDeviceID == deviceID && targetPlayer != PhotonNetwork.LocalPlayer)
            {
                Debug.LogWarning("Leave room (same device)");
                PhotonNetwork.LeaveRoom();
                SceneManager.LoadScene("Menu");
            }
        }
    }
}