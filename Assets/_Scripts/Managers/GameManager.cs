using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

/*
 * This class will set up the scene and initialize objects
 * 
 * This class inherits from Singleton so any other script can access it easily through GameManager.Instance
 */
public class GameManager : Singleton<GameManager>
{
    private MatchablePool pool;
    private MatchableGrid grid;
    private Cursor cursor;
    private AudioMixer audioMixer;

    [SerializeField] private Vector2Int gridDimensions = Vector2Int.one;

    [Header("When a deadlock happens in the game, the game will match everything \nas a reward to the player for reaching the deadlock." +
        "\nThis value indicates how long the game should wait before \nmatching everything so that the player can confirm whether \na deadlock has been actually reached (in Secs)")]
    [SerializeField] private float WaitBeforeMatchingEverything = 10f;

    [Header("Time to wait before showing gameover screen (in Secs)")]
    [SerializeField] private float WaitBeforeGameoverScreen = 3f;

    public event Action<int, int> OnGridInitialized; // Camera handler is a subscriber to this event

    // UI manager is a subscriber to these events
    public event Action<bool> ToggleLoadingScreen;
    public event Action<bool> FadeOutLoadingScreen; 
    public event Action<bool> OnGameOver;
    public event Action<bool> OnDeadlockReached;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        // get references to other important game objects
        pool = (MatchablePool)MatchablePool.Instance;
        grid = (MatchableGrid)MatchableGrid.Instance;

        cursor = Cursor.Instance;
        audioMixer = AudioMixer.Instance;

        // set up the scene
        StartCoroutine(Setup());
    }

    private IEnumerator Setup()
    {
        //disable user input
        cursor.enabled = false;

        // Put a loading screen here
        ToggleLoadingScreen(true);

        // Pool the matchables
        pool.PoolObjects(gridDimensions.x * gridDimensions.y * 2);

        // Create grid
        grid.InitializeGrid(gridDimensions);

        // fade out loading screen
        FadeOutLoadingScreen?.Invoke(true);

        //start BGM
        audioMixer.PlayMusic();

        yield return null;

        // populate the grid
        StartCoroutine(grid.PopulateGrid(false, true));

        // setup camera
        OnGridInitialized?.Invoke(gridDimensions.x, gridDimensions.y);

        // remove loading screen
        ToggleLoadingScreen(false);

        // enable user input
        cursor.enabled = true;

    }

    // Called when the grid reaches a dead lock
    public void NoMoreMoves()
    {

        StartCoroutine(HandleDeadlock());
    }

    public void GameOver()
    {
        OnGameOver?.Invoke(true);
    }

    private IEnumerator HandleDeadlock()
    {
        cursor.enabled = false;

        OnDeadlockReached?.Invoke(true);

        yield return new WaitForSeconds(WaitBeforeMatchingEverything);

        // reward the player for achieving a deadlock
        grid.MatchEverything();

        yield return new WaitForSeconds(WaitBeforeGameoverScreen);

        GameOver();
    }

    public void RestartGame()
    {
        SceneManager.LoadScene(0);
    }
}
