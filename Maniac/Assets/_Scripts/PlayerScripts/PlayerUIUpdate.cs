using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PlayerUIUpdate : MonoBehaviour
{
    [Space(10)] 
    [Header("UI variables")] 
    [SerializeField] private Text healthText;
    [SerializeField] private Text roleText;
    [SerializeField] private Text heldTrapText;
    
    public void UpdateUI(float health, _Scripts.PlayerScripts.PlayerRole playerRole, bool heldTrap)
    {
        healthText.text = $"Health: {health}";
        roleText.text = $"Role: {playerRole.GetRole()}";
        if (playerRole.GetRole() == _Scripts.PlayerScripts.PlayerRoleEnum.Murder) 
            heldTrapText.text = "Murders can't held traps";
        else if (playerRole.GetRole() == _Scripts.PlayerScripts.PlayerRoleEnum.Victim) 
            heldTrapText.text = $"Held trap: {heldTrap}";
    }
}
