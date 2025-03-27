using System;
using System.Collections;
using UnityEngine;

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

    public event Action<int, int> OnGridInitialized; // Camera handler is a subscriber to this event
    public event Action<bool> ToggleLoadingScreen; // UI manager is a subscriber to this event
    public event Action<bool> FadeOutLoadingScreen; // UI manager is a subscriber to this event

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
}
