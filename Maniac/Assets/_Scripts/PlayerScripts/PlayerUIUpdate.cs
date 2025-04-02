using System;
using UnityEngine;
using UnityEngine.UI;

namespace _Scripts.PlayerScripts
{
    public class PlayerUIUpdate : MonoBehaviour
    {
        [Space(10)] 
        [Header("UI variables")] 
        [SerializeField] private Text healthText;
        [SerializeField] private Text roleText;
        [SerializeField] private Image holdTrapImage;
        [SerializeField] private Image holdGunImage;
        [SerializeField] private GameObject helpText;
        [SerializeField] private Image roleImage;
        
        [SerializeField] private Image healthBarImage;
        
        // reload
        [SerializeField] private GameObject weaponSystem;
        [SerializeField] private GameObject inventorySystem;
        [SerializeField] private Text reloadText;
        [SerializeField] private Image reloadImage;
        
        // stamina
        [SerializeField] private Text staminaText;
        [SerializeField] private Image staminaBarImage;
        
        [SerializeField] private Text highlightText;
        
        // fear
        [SerializeField] private GameObject fearOverlay;
        [SerializeField] private Text fearText;
        [SerializeField] private Image fearImage;
        
        // Light
        private Light lightSource;

        [SerializeField] private Weapon weapon; 
        private PlayerMovement playerMovement;
        private FearEffect fearEffect;

        private void Awake()
        {
            playerMovement = GetComponent<PlayerMovement>();
            fearEffect = GetComponent<FearEffect>();
        }

        private void Start()
        {
            lightSource = GetComponentInChildren<Light>();
            highlightText.gameObject.SetActive(false);
        }

        public void UpdateUI(float health, _Scripts.PlayerScripts.PlayerRole playerRole, bool holdTrap, bool holdGun)
        {
            // Health
            healthText.text = $"HP: {health}";
            healthBarImage.fillAmount = health / 100;
            
            // Role and reload
            roleText.text = $"{playerRole.GetRole()}";
            reloadText.text = $"Next fire in: {((weapon.GetNextFirePercent() * 100) <= 0 ? 0 : weapon.GetNextFirePercent() * 100)}";
            reloadImage.fillAmount = weapon.GetNextFirePercent();
            
            // Stamina
            staminaText.text = $"CS: {MathF.Round(playerMovement.GetStamina()[0], 2)}";
            staminaBarImage.fillAmount = playerMovement.GetStamina()[0] / playerMovement.GetStamina()[1];
        
            if (playerRole.GetRole() == _Scripts.PlayerScripts.PlayerRoleEnum.Murder)
            {
                roleImage.color = Color.red;
                weaponSystem.SetActive(false);
                inventorySystem.SetActive(false);
                fearOverlay.SetActive(false);
            }
            else if (playerRole.GetRole() == _Scripts.PlayerScripts.PlayerRoleEnum.Victim)
            {
                holdTrapImage.color = holdTrap ? Color.green : Color.grey;
                holdGunImage.color = holdGun ? Color.green : Color.grey;
            
                roleImage.color = Color.green;
                highlightText.gameObject.SetActive(GetComponentInChildren<Outline>().enabled ? true : false);
                
                fearText.text = $"Fear: {Mathf.Round(fearEffect.GetCurrentFear())}";
                fearImage.fillAmount = fearEffect.GetCurrentFear() / 100;
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
}
