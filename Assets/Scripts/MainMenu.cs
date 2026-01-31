using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenu : MonoBehaviour
{
    public GameObject howToPanel;

    [Header("Tutorial Pages")]
    public GameObject page1; 
    public GameObject page2; 
    public GameObject page3; 

    public void StartGame()
    {
        SceneManager.LoadScene("Island");
    }

    public void QuitGame()
    {
        Application.Quit();
    }

    public void ShowHowToPanel()
    {
        howToPanel.SetActive(true);
        ShowPage1();
    }

    public void CloseHowToPanel()
    {
        howToPanel.SetActive(false);
    }

    public void ShowPage1()
    {
        page1.SetActive(true);
        page2.SetActive(false);
        page3.SetActive(false);
    }

    public void ShowPage2()
    {
        page1.SetActive(false);
        page2.SetActive(true);
        page3.SetActive(false);
    }

    public void ShowPage3()
    {
        page1.SetActive(false);
        page2.SetActive(false);  
        page3.SetActive(true);
    }
}