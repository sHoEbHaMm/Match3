using System.Collections;
using UnityEngine;

/*
 * This class will manage the grid of matchables by inheriting grid mechanics from Grid System
 * It's also a Singleton which can be accessed through Instance
 */
public class MatchableGrid : GridSystem<Matchable>
{
    private MatchablePool pool;

    [SerializeField] private Vector3 offScreenOffset;

    private void Start()
    {
        pool = (MatchablePool)MatchablePool.Instance;
    }
    public IEnumerator PopulateGrid(bool allowMatches = false)
    {
        Matchable newMatchable;
        Vector3 onScreenPosition;

        for(int y = 0; y!=Dimensions.y; y++)
        {
            for(int x = 0; x!= Dimensions.x; x++)
            {
                // get a matchable from the pool
                newMatchable = pool.GetRandomMatchable();

                // position it on the screen
                onScreenPosition = transform.position + new Vector3(x, y);
                newMatchable.transform.position = onScreenPosition + offScreenOffset;

                // activate the matchable
                newMatchable.gameObject.SetActive(true);

                //tell this matchable where it is on the grid
                newMatchable.gridPosition = new Vector2Int(x, y);

                // place it in the grid data structure
                PutItemAt(newMatchable, x, y);

                MatchableType type = newMatchable.GetMatchableType();

                while(!allowMatches && isPartOfAMatch(newMatchable))
                {
                    // Change matchable type until it is not a match anymore
                    if(pool.NextType(newMatchable) == type)
                    {
                        Debug.LogWarning("Failed to find a type that didnt match at (" + x + "," + y + ")");
                        Debug.Break();
                        break;
                    }
                }

                // move the matchable to its on screen psotion
                StartCoroutine(newMatchable.MoveToPosition(onScreenPosition));

                yield return new WaitForSeconds(0.1f);
            }
        }
    }

    //TODO
    private bool isPartOfAMatch(Matchable matchable)
    {
        return false;
    }
}
