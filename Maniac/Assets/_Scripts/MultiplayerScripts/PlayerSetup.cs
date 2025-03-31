using _Scripts.PlayerScripts;
using Photon.Pun;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;

namespace _Scripts.MultiplayerScripts
{
    public class PlayerSetup : MonoBehaviourPunCallbacks
    {
        [Header("Player Setup")]
        [SerializeField] private 
            _Scripts.PlayerScripts.PlayerMovement playerMovement;
        [SerializeField] private MonoBehaviour[] disableOnSetup;
        [SerializeField] private GameObject playerCamera;
        [SerializeField] private GameObject playerCanvas;
    
        [Space(10)]
        [Header("Player skins & ui")]
        [SerializeField] private GameObject[] skins;
        [SerializeField] private Text roleText;
        //private PlayerRoleEnum _role = PlayerRoleEnum.Victim;
        private PlayerRole playerRole;
        
        private PhotonView _photonView;
        [SerializeField] private GameObject trapInHands;
        [SerializeField] private GameObject gunInHands;
        private string nickname;
        
        private void Awake()
        {
            _photonView = GetComponent<PhotonView>();
            playerRole = GetComponent<PlayerRole>();
            
            // Disable all sets
            playerMovement.enabled = false;
            
            playerCamera.SetActive(false);
            playerCanvas.SetActive(false);
            
            trapInHands.SetActive(false);
            gunInHands.SetActive(false);

            foreach (MonoBehaviour t in disableOnSetup)
            {
                t.enabled = false;
            }
        }

        public void SetupLocalPlayer()
        {
            if (!_photonView.IsMine) return;

            playerMovement.enabled = true;
            playerCamera.SetActive(true);
            playerCanvas.SetActive(true);
            
            foreach (MonoBehaviour t in disableOnSetup)
            {
                t.enabled = true;
            }

            FindPlayersAndSetRole();
        }
        
        private void FindPlayersAndSetRole()
        {
            int playerCount = PhotonNetwork.PlayerList.Length;
            SetRoleAndSkin(playerCount > 1 ? PlayerRoleEnum.Victim : PlayerRoleEnum.Murder);
        }

        private void SetRoleAndSkin(PlayerRoleEnum newRole)
        {
            playerRole.SetRole(newRole);
            Debug.Log($"{playerRole.GetRole()} set to new player");
        
            if (roleText != null) roleText.text = $"Role: {playerRole.GetRole()}";
            else Debug.LogWarning("RoleText is not assigned!");

            if (skins == null || skins.Length == 0)
            {
                Debug.LogError("No skins assigned!");
                return;
            }

            int randomSkin = (playerRole.GetRole() == PlayerRoleEnum.Victim) ? Random.Range(0, 3) : Random.Range(4, skins.Length);
            _photonView.RPC("SetSkin_RPC", RpcTarget.AllBuffered, randomSkin);
        }
    
        [PunRPC]
        private void SetSkin_RPC(int skinIndex)
        {
            foreach (GameObject skin in skins) 
                skin.SetActive(false);
        
            skins[skinIndex].SetActive(true);

            if (photonView.IsMine)
            {
                SkinnedMeshRenderer[] skinnedMeshRenderers = skins[skinIndex].GetComponentsInChildren<SkinnedMeshRenderer>();
                foreach (var skinnedMeshRenderer in skinnedMeshRenderers)
                {
                    skinnedMeshRenderer.enabled = false;
                }
            }
            
        }

        [PunRPC]
        public void SetNickname_RPC(string nickname)
        {
            this.nickname = nickname;
            gameObject.GetComponentInChildren<TextMeshPro>().text = nickname + $" [{playerRole.GetRole()}]";
        }
        
    }
}
