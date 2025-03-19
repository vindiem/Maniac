using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PlayerUIUpdate : MonoBehaviour
{
    [Space(10)] 
    [Header("UI variables")] 
    [SerializeField] private Text healthText;
    [SerializeField] private Text roleText;
    //[SerializeField] private Text heldTrapText;
    [SerializeField] private Image holdTrapImage;
    [SerializeField] private GameObject helpText;
    [SerializeField] private Image roleImage;
    [SerializeField] private Image healthBarImage;
        
    // Light
    private Light lightSource;

    private void Start()
    {
        lightSource = GetComponentInChildren<Light>();
    }

    public void UpdateUI(float health, _Scripts.PlayerScripts.PlayerRole playerRole, bool heldTrap)
    {
        healthText.text = $"{health}";
        healthBarImage.fillAmount = health / 100;
        roleText.text = $"{playerRole.GetRole()}";
        if (playerRole.GetRole() == _Scripts.PlayerScripts.PlayerRoleEnum.Murder)
        {
            //heldTrapText.text = "Murders can't held traps";
            holdTrapImage.color = Color.grey;
            roleImage.color = Color.red;
        }
        else if (playerRole.GetRole() == _Scripts.PlayerScripts.PlayerRoleEnum.Victim)
        {
            if (heldTrap) holdTrapImage.color = Color.green;
            else holdTrapImage.color = Color.grey;
            roleImage.color = Color.green;
        }

        if (Input.GetMouseButtonDown(1))
        {
            lightSource.enabled = !lightSource.enabled;
            
            // Making sound
            SoundManager.instance.PlayLightSound();
        }
    }
    
    public void ShowHidePressButton()
    {
        bool isActive = helpText.gameObject.activeSelf;
        helpText.gameObject.SetActive(!isActive);
    }
    
}
