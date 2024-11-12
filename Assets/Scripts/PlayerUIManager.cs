using TMPro;
using UnityEngine;
using Unity.Netcode;

public class PlayerUIManager : NetworkBehaviour
{
    [SerializeField] private TextMeshProUGUI healthText;
    [SerializeField] private TextMeshProUGUI ammoText;
    [SerializeField] private TextMeshProUGUI powerUpText;

    private int localCurrentHealth;
    private int localCurrentAmmo;

    private void Start()
    {
        // Pastikan hanya pemain lokal yang meng-update UI ini
        if (!IsOwner)
        {
            healthText.gameObject.SetActive(false);
            ammoText.gameObject.SetActive(false);
            powerUpText.gameObject.SetActive(false);
            return;
        }

        // Inisialisasi tampilan awal UI
        UpdateHealthUI();
        UpdateAmmoUI();
        powerUpText.text = ""; // Kosongkan pada awal permainan
    }

    // Method untuk memperbarui nilai health secara lokal
    public void SetHealth(int health)
    {
        localCurrentHealth = health;
        UpdateHealthUI();
    }

    // Method untuk memperbarui nilai ammo secara lokal
    public void SetAmmo(int ammo)
    {
        localCurrentAmmo = ammo;
        UpdateAmmoUI();
    }

    private void UpdateHealthUI()
    {
        healthText.text = "Health : " + localCurrentHealth;
    }

    private void UpdateAmmoUI()
    {
        ammoText.text = "Ammo : " + localCurrentAmmo;
    }

    public void DisplayPowerUpMessage(string powerUpName)
    {
        powerUpText.text = powerUpName;
        CancelInvoke(nameof(ClearPowerUpMessage));
        Invoke(nameof(ClearPowerUpMessage), 3f); // Pesan power-up akan hilang setelah 3 detik
    }

    private void ClearPowerUpMessage()
    {
        powerUpText.text = "";
    }
}
