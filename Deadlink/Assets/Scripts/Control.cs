using UnityEngine;
using UnityEngine.SceneManagement;

public class Control : MonoBehaviour
{
    public GameObject mainMenu;
    public GameObject settingsMenu;

    public void NextScene()
    {
        SceneManager.LoadScene("Main", LoadSceneMode.Single);
    }

    public void Settings()
    {
        settingsMenu.SetActive(true);
        mainMenu.SetActive(false);
    }
}
