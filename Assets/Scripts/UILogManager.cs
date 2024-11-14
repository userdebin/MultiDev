using TMPro; // Jika menggunakan TextMeshPro, gunakan ini
using UnityEngine;

public class UILogManager : MonoBehaviour
{
    public static UILogManager Instance { get; private set; }

    [SerializeField] private TextMeshProUGUI logText; // Pastikan menggunakan TextMeshPro - Text
    [SerializeField] private float logDisplayDuration = 3f; // Durasi tampilan log (dalam detik)

    private float displayTimer;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }

        // Sembunyikan log di awal
        logText.text = "";
    }

    private void Update()
    {
        // Jika ada teks log yang ditampilkan, mulai hitung waktu untuk menyembunyikannya
        if (displayTimer > 0)
        {
            displayTimer -= Time.deltaTime;
            if (displayTimer <= 0)
            {
                ClearLog(); // Bersihkan log setelah waktu habis
            }
        }
    }

    public void DisplayLog(string message)
    {
        logText.text = message;
        displayTimer = logDisplayDuration; // Reset timer setiap kali ada log baru
    }

    private void ClearLog()
    {
        logText.text = ""; // Hapus log dari UI
    }
}
