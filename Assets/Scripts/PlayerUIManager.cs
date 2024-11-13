using TMPro;
using UnityEngine;
using Unity.Netcode;

public class PlayerUIManager : NetworkBehaviour
{
    [SerializeField] private TextMeshProUGUI healthText;
    [SerializeField] private TextMeshProUGUI ammoText;
    [SerializeField] private TextMeshProUGUI powerUpText;

    private PlayerSettings playerSettings;
    private Gun playerGun;

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

        // Mendapatkan referensi ke komponen yang relevan
        playerSettings = GetComponent<PlayerSettings>();
        playerGun = GetComponent<Gun>();

        // Inisialisasi tampilan awal UI
        UpdateHealthUI();
        UpdateAmmoUI();
        powerUpText.text = ""; // Kosongkan pada awal permainan
    }

    private void Update()
    {
        if (!IsOwner) return;

        // Update UI untuk health dan ammo di setiap frame
        UpdateHealthUI();
        UpdateAmmoUI();
    }

    // Ubah aksesibilitas metode ini menjadi `public`
    public void UpdateHealthUI()
    {
        healthText.text = "Health : " + playerSettings.currentHealth;
    }

    // Ubah aksesibilitas metode ini menjadi `public`
    public void UpdateAmmoUI()
    {
        ammoText.text = "Ammo : " + playerGun.currentAmmo;
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
