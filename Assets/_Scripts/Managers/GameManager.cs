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

    [SerializeField] private Vector2Int dimensions = Vector2Int.one;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        // get references to other important game objects
        pool = (MatchablePool)MatchablePool.Instance;
        grid = (MatchableGrid)MatchableGrid.Instance;

        // set up the scene
        StartCoroutine(Setup());
    }

    private IEnumerator Setup()
    {
        // Put a loading screen here

        // Pool the matchables
        pool.PoolObjects(dimensions.x * dimensions.y * 2);

        // Create grid
        grid.InitializeGrid(dimensions);

        yield return null;

        StartCoroutine(grid.PopulateGrid(false, true));

        // remove loading screen
    }
}
