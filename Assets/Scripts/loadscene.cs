using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

public class loadscene : MonoBehaviour
{
    [SerializeField] private GameObject _loadingPanel;

    public void PlayGame()
    {
        if (_loadingPanel == null)
        {
            _loadingPanel = FindLoadingPanel();
        }

        if (_loadingPanel != null)
        {
            _loadingPanel.SetActive(true);
        }

        Debug.Log("MainMenu: Loading stage_1 scene...");
        SceneManager.LoadScene("stage_1");
    }

    private GameObject FindLoadingPanel()
    {
        foreach (GameObject root in SceneManager.GetActiveScene().GetRootGameObjects())
        {
            if (root.name == "LoadingPanel") return root;

            Transform[] children = root.GetComponentsInChildren<Transform>(true);
            foreach (Transform child in children)
            {
                if (child.name == "LoadingPanel") return child.gameObject;
            }
        }

        return GameObject.Find("LoadingPanel");
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
