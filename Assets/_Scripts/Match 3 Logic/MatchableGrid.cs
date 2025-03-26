using System.Collections;
using Unity.Burst.CompilerServices;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.SocialPlatforms.Impl;

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

    // attempt to swap 2 matchables on the grid, see if they made a valid match, resolve any matches, if no matches, swap them back
    public IEnumerator TrySwap(Matchable[] toBeSwapped)
    {
        //  Make a local copy of what we're swapping so Cursor doesn't overwrite
        Matchable[] copies = new Matchable[2];
        copies[0] = toBeSwapped[0];
        copies[1] = toBeSwapped[1];

        //  yield until matchables animate swapping
        yield return StartCoroutine(Swap(copies));

        /*        //  check for a valid match
                Match[] matches = new Match[2];

                matches[0] = GetMatch(copies[0]);
                matches[1] = GetMatch(copies[1]);

                //   if we made valid matches, resolve them
                if (matches[0] != null)
                    StartCoroutine(score.ResolveMatch(matches[0]));

                if (matches[1] != null)
                    StartCoroutine(score.ResolveMatch(matches[1]));

                //  if there's no match, swap them back
                if (matches[0] == null && matches[1] == null)
                {
                    yield return StartCoroutine(Swap(copies));

                    // if swapping them back creates a match, find it and resolve, fill the grid and scan again
                    if (ScanForMatches())
                        StartCoroutine(FillAndScanGrid());
                }
                //  if there was a match, fill and scan the grid
                else
                    StartCoroutine(FillAndScanGrid());*/

        StartCoroutine(Swap(copies));
    }

    // coroutine that swaps 2 matchables in the grid
    private IEnumerator Swap(Matchable[] toBeSwapped)
    {
        //  swap them in the grid data structure
        SwapItemsAt(toBeSwapped[0].gridPosition, toBeSwapped[1].gridPosition);

        //  tell the matchables their new positions
        Vector2Int temp = toBeSwapped[0].gridPosition;
        toBeSwapped[0].gridPosition = toBeSwapped[1].gridPosition;
        toBeSwapped[1].gridPosition = temp;

        //  get the world positions of both
        Vector3[] worldPosition = new Vector3[2];
        worldPosition[0] = toBeSwapped[0].transform.position;
        worldPosition[1] = toBeSwapped[1].transform.position;

        //  move them to their new positions on screen
        StartCoroutine(toBeSwapped[0].MoveToPosition(worldPosition[1]));
        yield return StartCoroutine(toBeSwapped[1].MoveToPosition(worldPosition[0]));
    }
}
