using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;
using Random = UnityEngine.Random;

public class PlayerSetup : MonoBehaviourPunCallbacks
{
    public PlayerMovement playerMovement;
    public GameObject playerCamera;
    public GameObject playerCanvas;
    
    [Space(10)]
    [SerializeField] private string role = "victim";
    [SerializeField] private GameObject[] skins;
    [SerializeField] private Text roleText;
    
    private PhotonView photonView;

    private void Awake()
    {
        photonView = GetComponent<PhotonView>();
    }

    public void SetupLocalPlayer()
    {
        if (!photonView.IsMine) return;

        playerMovement.enabled = true;
        playerCamera.SetActive(true);
        playerCanvas.SetActive(true);

        FindPlayersAndSetRole();
    }
    
    public void SetRoleAndSkin(string newRole)
    {
        role = newRole;
        Debug.Log($"{role} set to new player");
        
        if (roleText != null) roleText.text = $"Role: {role}";
        else Debug.LogWarning("RoleText is not assigned!");

        if (skins == null || skins.Length == 0)
        {
            Debug.LogError("No skins assigned!");
            return;
        }

        int randomSkin = (role == "victim") ? Random.Range(0, 3) : Random.Range(4, skins.Length);
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
        int playerCount = PhotonNetwork.PlayerList.Length;
        SetRoleAndSkin(playerCount > 1 ? "victim" : "murder");
    }
}
