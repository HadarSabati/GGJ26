using UnityEngine;
using UnityEngine.SceneManagement; 

public class MainMenu : MonoBehaviour
{
    public void StartGame()
    {
        SceneManager.LoadScene("Island");
    }

    public void QuitGame()
    {
        Application.Quit(); 
        Debug.Log("Game is exiting...");
    }
}