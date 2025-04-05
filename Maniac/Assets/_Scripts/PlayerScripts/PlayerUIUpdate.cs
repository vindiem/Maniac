using System;
using UnityEngine;
using UnityEngine.UI;

namespace _Scripts.PlayerScripts
{
    public class PlayerUIUpdate : MonoBehaviour
    {
        [Space(10)]
        [Header("UI Elements")]
        [SerializeField] private Text healthText;
        [SerializeField] private Text roleText;
        [SerializeField] private Image roleImage;
    
        [Space(10)]
        [Header("Inventory UI")]
        [SerializeField] private Image holdTrapImage;
        [SerializeField] private Image holdGunImage;
        [SerializeField] private GameObject helpText;

        [Space(10)]
        [Header("Health UI")]
        [SerializeField] private Image healthBarImage;

        [Space(10)]
        [Header("Reload System")]
        [SerializeField] private GameObject weaponSystem;
        [SerializeField] private GameObject inventorySystem;
        [SerializeField] private Text reloadText;
        [SerializeField] private Image reloadImage;

        [Space(10)]
        [Header("Stamina System")]
        [SerializeField] private Text staminaText;
        [SerializeField] private Image staminaBarImage;

        [Space(10)]
        [Header("Highlight System")]
        [SerializeField] private Text highlightText;

        [Space(10)]
        [Header("Fear System")]
        [SerializeField] private GameObject fearOverlay;
        [SerializeField] private Text fearText;
        [SerializeField] private Image fearImage;

        [Space(10)]
        [Header("Light System")]
        private Light lightSource;

        [Space(10)]
        [Header("Player Components")]
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
                SoundManager.Instance.PlayLightSound();
            }
        }
    
        public void ShowHidePressButton()
        {
            bool isActive = helpText.gameObject.activeSelf;
            helpText.gameObject.SetActive(!isActive);
        }
    
    }
}
