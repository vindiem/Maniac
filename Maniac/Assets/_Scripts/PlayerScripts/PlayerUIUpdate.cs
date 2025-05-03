using System;
using UnityEngine;
using UnityEngine.Rendering.PostProcessing;
using UnityEngine.UI;

namespace _Scripts.PlayerScripts
{
    public class PlayerUIUpdate : MonoBehaviour
    {
        [Header("UI Elements")]
        [SerializeField] private Text healthText;
        [SerializeField] private Text roleText;
        [SerializeField] private Image roleImage;
        
        [SerializeField] private Image roleImage1;

        [Header("Inventory UI")]
        [SerializeField] private Image holdTrapImage;
        [SerializeField] private Image holdGunImage;
        [SerializeField] private GameObject helpText;
        [SerializeField] private Image murderAttackImage;

        [Header("Health UI")]
        [SerializeField] private Image healthBarImage;
        [SerializeField] private Image heartPanelImage;

        [Header("Reload System")]
        [SerializeField] private GameObject weaponSystem;
        [SerializeField] private GameObject inventorySystem;
        [SerializeField] private Text reloadText;
        [SerializeField] private Image reloadImage;

        [Header("Stamina System")]
        [SerializeField] private Text staminaText;
        [SerializeField] private Image staminaBarImage;
        [SerializeField] private Image staminaImage;

        [Header("Highlight System")]
        [SerializeField] private GameObject highlightText;

        [Header("Fear System")]
        [SerializeField] private GameObject fearOverlay;
        [SerializeField] private Text fearText;
        [SerializeField] private Image fearImage;

        [Header("Player Components")]
        [SerializeField] private Weapon weapon;
        [SerializeField] private PostProcessVolume postProcessVolume;

        private PlayerMovement playerMovement;
        private FearEffect fearEffect;
        private Vignette vignette;

        private void Awake()
        {
            playerMovement = GetComponent<PlayerMovement>();
            fearEffect = GetComponent<FearEffect>();
        }

        private void Start()
        {
            highlightText.SetActive(false);
            roleImage1.gameObject.SetActive(false);
            murderAttackImage.gameObject.SetActive(false);
        }

        public void UpdateUI(float health, PlayerRole playerRole, bool holdTrap, bool holdGun)
        {
            // Health update
            healthText.text = $"HP: {health}";
            healthBarImage.fillAmount = health / 100f;
            heartPanelImage.fillAmount = 1f - (health / 100f);

            // Role and reload update
            roleText.text = playerRole.GetRole().ToString();
            float nextFirePercent = weapon.GetNextFirePercent();
            reloadText.text = $"Next fire in: {(nextFirePercent <= 0f ? 0 : nextFirePercent * 100):F0}";
            reloadImage.fillAmount = nextFirePercent;

            // Stamina update
            float currentStamina = playerMovement.GetStamina()[0];
            float maxStamina = playerMovement.GetStamina()[1];
            float staminaPercent = currentStamina / maxStamina;

            staminaText.text = $"CS: {MathF.Round(currentStamina, 2)}";
            staminaBarImage.fillAmount = staminaPercent;
            staminaImage.fillAmount = (staminaPercent - 1) * -1;

            // Role-specific UI
            if (playerRole.GetRole() == PlayerRoleEnum.Murder)
            {
                roleImage.color = Color.red;
                weaponSystem.SetActive(false);
                inventorySystem.SetActive(false);
                fearOverlay.SetActive(false);
                
                // Apply red vignette for murder role
                if (postProcessVolume.profile.TryGetSettings(out vignette))
                {
                    vignette.color.value = Color.red;
                    vignette.intensity.value = 0.4f;
                }
                
                roleImage1.gameObject.SetActive(true);
                murderAttackImage.gameObject.SetActive(true);
                murderAttackImage.GetComponent<Image>().color = 
                    (playerMovement.GetAttackTimer() == 0 ? Color.green : Color.white);
                murderAttackImage.GetComponentInChildren<Text>().text = 
                    MathF.Round(playerMovement.GetAttackTimer(), 1).ToString();
            }
            else if (playerRole.GetRole() == PlayerRoleEnum.Victim)
            {
                roleImage.color = Color.green;
                holdTrapImage.color = holdTrap ? Color.green : Color.grey;
                holdGunImage.color = holdGun ? Color.green : Color.grey;

                // Show interaction highlight if object is being outlined
                var outline = GetComponentInChildren<Outline>();
                highlightText.SetActive(outline != null && outline.enabled);

                // Fear UI update
                float fearValue = fearEffect.GetCurrentFear();
                fearText.text = $"Fear: {Mathf.Round(fearValue)}";
                fearImage.fillAmount = fearValue / 100f;
            }
        }

        public void ShowHidePressButton()
        {
            helpText.SetActive(!helpText.activeSelf);
        }
    }
}
