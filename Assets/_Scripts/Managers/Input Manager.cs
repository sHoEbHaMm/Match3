using UnityEngine;
using UnityEngine.SceneManagement;

public class InputManager : MonoBehaviour
{
    public void PlayButton()
    {
        SceneManager.LoadScene((SceneManager.GetActiveScene().buildIndex + 1));
    }

    public void QuitButton()
    {
        Application.Quit();
    }
}
