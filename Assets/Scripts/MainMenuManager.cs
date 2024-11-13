using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenuManager : MonoBehaviour
{
    [SerializeField] private Button playButton;
    [SerializeField] private Button exitButton;

    private void Awake()
    {
        // Menghubungkan tombol Play dengan metode StartGame
        playButton.onClick.AddListener(StartGame);

        // Menghubungkan tombol Exit dengan metode ExitGame
        exitButton.onClick.AddListener(ExitGame);
    }

    public void StartGame()
    {
        // Memuat scene "Lobby" ketika tombol Play diklik
        SceneManager.LoadScene("LobbyHall");
    }

    public void ExitGame()
    {
        // Keluar dari aplikasi
#if UNITY_EDITOR
        // Jika sedang di editor, hentikan pemutaran
        UnityEditor.EditorApplication.isPlaying = false;
#else
            // Jika di build, keluar dari aplikasi
            Application.Quit();
#endif
    }
}
