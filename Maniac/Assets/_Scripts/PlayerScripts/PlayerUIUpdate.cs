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
    [SerializeField] private Image holdGunImage;
    [SerializeField] private GameObject helpText;
    [SerializeField] private Image roleImage;
    [SerializeField] private Image healthBarImage;
    [SerializeField] private Text reloadText;
    
    [SerializeField] private Image reloadImage;
        
    // Light
    private Light lightSource;

    [SerializeField] private Weapon weapon; 

    private void Start()
    {
        lightSource = GetComponentInChildren<Light>();
    }

    public void UpdateUI(float health, _Scripts.PlayerScripts.PlayerRole playerRole, bool holdTrap, bool holdGun)
    {
        healthText.text = $"{health}";
        healthBarImage.fillAmount = health / 100;
        roleText.text = $"{playerRole.GetRole()}";
        reloadText.text = $"Next fire in: {weapon.GetNextFirePercent()}";
        reloadImage.fillAmount = weapon.GetNextFirePercent();
        
        if (playerRole.GetRole() == _Scripts.PlayerScripts.PlayerRoleEnum.Murder)
        {
            holdTrapImage.color = Color.grey;
            holdGunImage.color = Color.grey;
            roleImage.color = Color.red;
        }
        else if (playerRole.GetRole() == _Scripts.PlayerScripts.PlayerRoleEnum.Victim)
        {
            holdTrapImage.color = holdTrap ? Color.green : Color.grey;
            holdGunImage.color = holdGun ? Color.green : Color.grey;
            
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
