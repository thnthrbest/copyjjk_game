using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

public class loadscene : MonoBehaviour
{
    public void PlayGame()
    {
        Debug.Log("MainMenu: Loading stage_1 scene...");
        SceneManager.LoadScene("stage_1");
    }
    public void Setup()
    {
        Debug.Log("MainMenu: Loading setup scene...");
        SceneManager.LoadScene("setup");
    }
    public void OpenTutorial()
    {
        Debug.Log("MainMenu: Loading tutorial scene...");
        SceneManager.LoadScene("tutorial");
    }

}
