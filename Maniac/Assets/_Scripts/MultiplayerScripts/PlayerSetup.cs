using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;
using Random = UnityEngine.Random;

public class PlayerSetup : MonoBehaviourPunCallbacks
{
    [Header("Player Setup")]
    [SerializeField] private PlayerMovement playerMovement;
    [SerializeField] private GameObject playerCamera;
    [SerializeField] private GameObject playerCanvas;
    
    [Space(10)]
    [Header("Player skins & ui")]
    [SerializeField] private GameObject[] skins;
    [SerializeField] private Text roleText;
    private string _role = "victim";
    
    private PhotonView _photonView;
    [SerializeField] private GameObject trapInHands;
        
    private void Awake()
    {
        _photonView = GetComponent<PhotonView>();
        
        // Disable all sets
        playerMovement.enabled = false;
        playerCamera.SetActive(false);
        playerCanvas.SetActive(false);
        trapInHands.SetActive(false);
    }

    public void SetupLocalPlayer()
    {
        if (!_photonView.IsMine) return;

        playerMovement.enabled = true;
        playerCamera.SetActive(true);
        playerCanvas.SetActive(true);

        FindPlayersAndSetRole();
    }
    
    public void SetRoleAndSkin(string newRole)
    {
        _role = newRole;
        playerMovement.SetRole(newRole);
        Debug.Log($"{_role.ToUpper()} set to new player");
        
        if (roleText != null) roleText.text = $"Role: {_role}";
        else Debug.LogWarning("RoleText is not assigned!");

        if (skins == null || skins.Length == 0)
        {
            Debug.LogError("No skins assigned!");
            return;
        }

        int randomSkin = (_role == "victim") ? Random.Range(0, 3) : Random.Range(4, skins.Length);
        _photonView.RPC("SetSkin_RPC", RpcTarget.AllBuffered, randomSkin);
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
