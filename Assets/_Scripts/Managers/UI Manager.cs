using UnityEngine;
using UnityEngine.UI;
using TMPro;

/*
 * This class will handle all the in-game UI
 * 
 * This class inherits from Singleton so any other script can access it easily through UIManager.Instance
 */

public class UIManager : Singleton<UIManager>
{
    private ScoreManager scoreManager;
    private GameManager gameManager;
    
    [SerializeField] private Transform matchableCollectionPoint;

    [SerializeField] private TMP_Text scoreText;
    [SerializeField] private TMP_Text comboText;
    [SerializeField] private TMP_Text highestComboText;
    [Tooltip("For game over screen")]
    [SerializeField] private TMP_Text FinalComboText;

    [SerializeField] private Slider comboSlider;

    [SerializeField] private Fader loadingScreen;

    [SerializeField] private GameObject gameOverScreen;
    [SerializeField] private GameObject deadLockScreen;

    public Transform MatchableCollectionPoint
    {
        get
        {
            return matchableCollectionPoint;
        }
    }

    // references to required game objects
    private void Start()
    {
        scoreManager = ScoreManager.Instance;
        gameManager = GameManager.Instance;

        scoreManager.OnScoreUpdated += UpdateScoreText;
        scoreManager.ToggleComboUI += ToggleComboElements;
        scoreManager.OnComboChangeSlider += UpdateComboSlider;
        scoreManager.OnComboChangeText += UpdateComboText;

        gameManager.ToggleLoadingScreen += HideLoadingScreen;
        gameManager.FadeOutLoadingScreen += OnFadeOutLoadingScreen;
        gameManager.OnGameOver += LoadGameOverUI;
        gameManager.OnDeadlockReached += ShowDeadLockScreen;

        gameOverScreen.SetActive(false);
        deadLockScreen.SetActive(false);
    }


    // game over related
    private void LoadGameOverUI(bool value)
    {
        if(value)
        {
            gameOverScreen.SetActive(true);

            // and then restart the game
            gameManager.RestartGame();
        }
    }


    private void ShowDeadLockScreen(bool value)
    {
        FinalComboText.text = "Highest Combo Reached\n" + scoreManager.HighestCombo.ToString() + "x";

        if(value)
            deadLockScreen.SetActive(true);
    }


    // loading screen related
    private void OnFadeOutLoadingScreen(bool value)
    {
        StartCoroutine(loadingScreen.Fade(0));
    }

    private void HideLoadingScreen(bool shouldHide)
    {
        loadingScreen.Hide(!shouldHide);
    }

    // Score related
    private void UpdateComboText(int combo, int highestCombo)
    {
        comboText.text = combo + "x";
        highestComboText.text = highestCombo + "x";
    }

    private void UpdateScoreText(int updateTo)
    {
        scoreText.text = updateTo.ToString();
    }

    private void UpdateComboSlider(float value)
    {
        comboSlider.value = value;
    }

    private void ToggleComboElements(bool show)
    {
        comboSlider.gameObject.SetActive(show);
    }
}
