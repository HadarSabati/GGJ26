using UnityEngine;
using UnityEngine.SceneManagement; 
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    [Header("Settings UI")]
    public GameObject settingsPanel; 

    void Start()
    { 
        if (settingsPanel != null)
            settingsPanel.SetActive(false);

        Time.timeScale = 1f;
    }

    public void OpenSettings()
    {
        if (settingsPanel != null)
        {
            settingsPanel.SetActive(true);
            Time.timeScale = 0f;
        }
    }

    public void CloseSettings()
    {
        if (settingsPanel != null)
        {
            settingsPanel.SetActive(false);
            Time.timeScale = 1f; 
        }
    }

    public void RestartLevel()
    {
        Time.timeScale = 1f; 

        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }


    public void GoToMainMenu()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("MainMenu"); 
    }
}