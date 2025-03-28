using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*
 * This class will manage the grid of matchables by inheriting grid mechanics from Grid System
 * It's also a Singleton which can be accessed through Instance
 */
public class MatchableGrid : GridSystem<Matchable>
{
    //Score manager listens to this event when a resolve is requested
    public event Action<Match> OnResolveRequested;

    //  The pool of Matchables with which to populate the grid
    private MatchablePool pool;
    private AudioMixer audioMixer;

    [Tooltip("A distance offscreen where the matchables will be spawned")]
    //  A distance offscreen where the matchables will be spawned
    [SerializeField] private Vector3 offScreenOffset;

    // a list of all possible moves
    private List<Matchable> possibleMoves;

    // get references to required gameobjects at start
    private void Start()
    {
        pool = (MatchablePool)MatchablePool.Instance;
        audioMixer = AudioMixer.Instance;
    }

    //  Populate the grid with matchables from the pool
    //  Optionally allow or not allow matches to be made while populating
    public IEnumerator PopulateGrid(bool allowMatches = false, bool initialPopulation = false)
    {
        Matchable newMatchable;
        Vector3 onScreenPosition;

        // list of new matchables added during population
        List<Matchable> newMatchables = new List<Matchable>();

        for(int y = 0; y!=Dimensions.y; y++)
        {
            for(int x = 0; x!= Dimensions.x; x++)
            {
                if(IsEmpty(x,y))
                {
                    // get a matchable from the pool
                    newMatchable = pool.GetRandomMatchable();

                    //position matchable off screen
                    newMatchable.transform.position = transform.position + new Vector3(x, y) + offScreenOffset;

                    // activate the matchable
                    newMatchable.gameObject.SetActive(true);

                    //tell this matchable where it is on the grid
                    newMatchable.gridPosition = new Vector2Int(x, y);

                    // place it in the grid data structure
                    PutItemAt(newMatchable, x, y);

                    newMatchables.Add(newMatchable);

                    MatchableType type = newMatchable.GetMatchableType();

                    while (!allowMatches && isPartOfAMatch(newMatchable))
                    {
                        // Change matchable type until it is not a match anymore
                        if (pool.NextType(newMatchable) == type)
                        {
                            Debug.LogWarning("Failed to find a type that didnt match at (" + x + "," + y + ")");
                            Debug.Break();
                            break;
                        }
                    }
                }
            }
        }

        // move each matchable to its on screen position and wait until the last one has finished moving
        for(int i = 0; i != newMatchables.Count; i++)
        {
            // position it on the screen
            onScreenPosition = transform.position + new Vector3(newMatchables[i].gridPosition.x, newMatchables[i].gridPosition.y);

            // play a landing sound after a delay when the matchable lands in postion
            audioMixer.PlayDelayedSound(SoundEffects.land, 1f / newMatchables[i].Speed);

            // move the matchable to its on screen position
            if (i == newMatchables.Count - 1)
                yield return StartCoroutine(newMatchables[i].MoveToPosition(onScreenPosition));
            else
                StartCoroutine(newMatchables[i].MoveToPosition(onScreenPosition));

            // pause for 1/10 of a second for a coool effect
            if (initialPopulation)
                yield return new WaitForSeconds(0.1f);
        }
    }

    // This funtion checks if the matchable being populated is part of a match or not
    private bool isPartOfAMatch(Matchable toMatch)
    {
        int horizontalMatches = 0, verticalMatches = 0;

        // First check left side
        horizontalMatches += CountMatchesInDirection(toMatch, Vector2Int.left);

        // Now check right side
        horizontalMatches += CountMatchesInDirection(toMatch, Vector2Int.right);

        if (horizontalMatches > 1)
            return true;

        // Check upward direction
        verticalMatches += CountMatchesInDirection(toMatch, Vector2Int.up);

        // Check downward direction
        verticalMatches += CountMatchesInDirection(toMatch, Vector2Int.down);

        if (verticalMatches > 1)
            return true;

        return false;
    }

    // This function counts the number of matches on the grid starting from the matchable toMatch moving in the direction indicated
    private int CountMatchesInDirection(Matchable toMatch, Vector2Int direction)
    {
        int matches = 0;

        Vector2Int position = toMatch.gridPosition + direction;

        while (BoundsCheck(position) && !IsEmpty(position) && GetItemAt(position).GetMatchableType() == toMatch.GetMatchableType())
        {
            matches++;
            position += direction;
        }

        return matches;
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

        //  check for a valid match
        Match[] matches = new Match[2];

        matches[0] = GetMatch(copies[0]);
        matches[1] = GetMatch(copies[1]);

        //   if we made valid matches, resolve them
        if (matches[0] != null)
            OnResolveRequested?.Invoke(matches[0]);

        if (matches[1] != null)
            OnResolveRequested?.Invoke(matches[1]);

        //  if there's no match, swap them back
        if (matches[0] == null && matches[1] == null)
        {
            yield return StartCoroutine(Swap(copies));

            // if swapping them back creates a match, find it and resolve, fill the grid and scan again
            if (ScanForMatches())
                StartCoroutine(FillAndScanGrid());
        }
        else
        {
            //  if there was a match, fill and scan the grid
            StartCoroutine(FillAndScanGrid());
        }
    }

    // This coroutine refills the grid after matches, checks for more matches, resolves them, refills again and so on
    private IEnumerator FillAndScanGrid()
    {
        CollapseGrid();
        yield return StartCoroutine(PopulateGrid(true));

        // Scan the grid for chain reactions
        if (ScanForMatches())
        {
            // collapse, repopulate and scan grid again
            StartCoroutine(FillAndScanGrid());
        }
        else
        {
            //if this there are no possible moves, game over
            CheckPossibleMoves();
        }
    }

    public void CheckPossibleMoves()
    {
        if (ScanForMoves() == 0)
        {
            // no moves!
            GameManager.Instance.NoMoreMoves();
        }
    }

    /*
     * Go through each column left to right,
     * search from bottom up to find an empty space,
     * then look above the empty space, and up through the rest of the column,
     * until you find a non empty space.
     * Move the matchable at the non empty space into the empty space,
     * then continue looking for empty spaces
     */
    private void CollapseGrid()
    {
        for(int x = 0; x != Dimensions.x; x++)
        {
            for(int yEmpty = 0; yEmpty != Dimensions.y - 1; yEmpty++)
            {
                if(IsEmpty(x, yEmpty))
                {
                    for(int yNotEmpty = yEmpty + 1; yNotEmpty != Dimensions.y; yNotEmpty++)
                    {
                        if(!IsEmpty(x, yNotEmpty) && GetItemAt(x, yNotEmpty).Idle)
                        {
                            // Move the matchable from NotEmpty to Empty
                            MoveMatchableToPostion(GetItemAt(x, yNotEmpty), x, yEmpty);
                            break;
                        }
                    }
                }

            }
        }
    }

    private void MoveMatchableToPostion(Matchable toMove, int x, int y)
    {
        // move matchable to its new postion
        MoveItemTo(toMove.gridPosition, new Vector2Int(x, y));

        // update the matchable's internal grid data
        toMove.gridPosition = new Vector2Int(x, y);

        // start animation to move it on screen
        StartCoroutine(toMove.MoveToPosition(transform.position + new Vector3(x, y)));

        // play a landing sound after a delay when the matchable lands in postion
        audioMixer.PlayDelayedSound(SoundEffects.land, 1f / toMove.Speed);
    }

    // Get a match containing this matchable, or null if it isn't part of any match
    private Match GetMatch(Matchable toMatch)
    {
        Match newMatch = new Match(toMatch);

        Match horizontalMatch, verticalMatch;

        // First get horizontal matches to left and right
        horizontalMatch = GetMatchesInDirection(newMatch,toMatch, Vector2Int.left);
        horizontalMatch.Merge(GetMatchesInDirection(newMatch,toMatch, Vector2Int.right));

        horizontalMatch.orientation = Orientation.horizontal;

        if (horizontalMatch.Count > 1)
        {
            newMatch.Merge(horizontalMatch);

            //scan for vertical branches
            GetBranches(newMatch, horizontalMatch, Orientation.vertical);
        }

        // Then get vertical matches to up and down
        verticalMatch = GetMatchesInDirection(newMatch,toMatch, Vector2Int.up);
        verticalMatch.Merge(GetMatchesInDirection(newMatch,toMatch, Vector2Int.down));

        verticalMatch.orientation = Orientation.vertical;

        if (verticalMatch.Count > 1)
        {
            newMatch.Merge(verticalMatch);

            // scan for horizontal branches
            GetBranches(newMatch, verticalMatch, Orientation.horizontal);
        }

        if (newMatch.Count == 1)
            return null;

        return newMatch;
    }

    // Add each matching matchable in the direction to a match and return it
    private Match GetMatchesInDirection(Match tree, Matchable toMatch, Vector2Int direction)
    {
        Match match = new Match();
        Vector2Int position = toMatch.gridPosition + direction;
        Matchable next;

        while (BoundsCheck(position) && !IsEmpty(position))
        {
            next = GetItemAt(position);

            if (next.GetMatchableType() == toMatch.GetMatchableType() && next.Idle)
            {
                //  only add matchables that aren't already part of the main match tr
                if (!tree.Contains(next))
                    match.AddMatchable(next);
                else
                    //  otherwise this branch still needs to be considered valid, so increment the count without adding the matchable to the list
                    match.AddUnlisted();

                position += direction;
            }
            else
                break;
        }
        return match;
    }

    //  After a horizontal or vertical match is found, scan it for branches in the perpendicular orientation
    private void GetBranches(Match tree, Match branchToSearch, Orientation perpendicular)
    {
        Match branch;

        foreach(Matchable matchable in branchToSearch.Matchables)
        {
            branch = GetMatchesInDirection(tree, matchable, perpendicular == Orientation.horizontal ? Vector2Int.left : Vector2Int.down);
            branch.Merge(GetMatchesInDirection(tree, matchable, perpendicular == Orientation.horizontal ? Vector2Int.right : Vector2Int.up));

            branch.orientation = perpendicular;

            //  If a valid branch is found, merge it into the tree and scan it for more branches
            if (branch.Count > 1)
            {
                tree.Merge(branch);
                GetBranches(tree, branch, perpendicular == Orientation.horizontal ? Orientation.vertical : Orientation.horizontal);
            }
        }
    }

    // scan the grid for any matches and resolve them
    private bool ScanForMatches()
    {
        bool madeAMatch = false;
        Matchable toMatch;
        Match match;

        // iterate through the grid, looking for non-empty and idle matchable
        for(int y = 0; y != Dimensions.y; y++)
        {
            for(int x = 0; x != Dimensions.x; x++)
            {
                if(!IsEmpty(x, y))
                {
                    toMatch = GetItemAt(x, y);

                    if (!toMatch.Idle)
                        continue;
                    
                    // try to match and resolve
                    match = GetMatch(toMatch);

                    if (match != null)
                    {
                        madeAMatch = true;
                        OnResolveRequested?.Invoke(match);
                    }

                }
            }
        }

        return madeAMatch;
    }


    //  match everything on the grid and resolve it
    public void MatchEverything()
    {
        Match everything = new Match();

        for (int y = 0; y != Dimensions.y; ++y)
            for (int x = 0; x != Dimensions.x; ++x)
                if (BoundsCheck(x, y) && !IsEmpty(x, y) && GetItemAt(x, y).Idle)
                    everything.AddMatchable(GetItemAt(x, y));

        OnResolveRequested?.Invoke(everything);
        StartCoroutine(FillAndScanGrid());

        audioMixer.PlaySound(SoundEffects.score);
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

        //play swap sound without delay
        audioMixer.PlaySound(SoundEffects.swap);

        //  move them to their new positions on screen
        StartCoroutine(toBeSwapped[0].MoveToPosition(worldPosition[1]));
        yield return StartCoroutine(toBeSwapped[1].MoveToPosition(worldPosition[0]));
    }

    //  scan for all possible moves
    private int ScanForMoves()
    {
        possibleMoves = new List<Matchable>();

        //  scan through the entire grid
        //  if a matchable can move, add it to the list of possible moves
        for (int y = 0; y != Dimensions.y; ++y)
            for (int x = 0; x != Dimensions.x; ++x)
                if (BoundsCheck(x, y) && !IsEmpty(x, y) && CanMove(GetItemAt(x, y)))
                    possibleMoves.Add(GetItemAt(x, y));

        return possibleMoves.Count;
    }

    //  check if this matchable can move to form a valid match
    private bool CanMove(Matchable toCheck)
    {
        //  Can this matchable move in any of the 4 directions?
        if
        (
                CanMove(toCheck, Vector2Int.up)
            || CanMove(toCheck, Vector2Int.right)
            || CanMove(toCheck, Vector2Int.down)
            || CanMove(toCheck, Vector2Int.left)
        )
            return true;

        return false;
    }

    //  Can this matchable move in 1 direction?
    private bool CanMove(Matchable toCheck, Vector2Int direction)
    {
        //  Look 2 and 3 positions away straight ahead
        Vector2Int position1 = toCheck.gridPosition + direction * 2,
                    position2 = toCheck.gridPosition + direction * 3;

        if (IsAPotentialMatch(toCheck, position1, position2))
            return true;

        //  What is the clockwise direction?
        Vector2Int cw = new Vector2Int(direction.y, -direction.x),
                    ccw = new Vector2Int(-direction.y, direction.x);

        //  Look diagonally clockwise
        position1 = toCheck.gridPosition + direction + cw;
        position2 = toCheck.gridPosition + direction + cw * 2;

        if (IsAPotentialMatch(toCheck, position1, position2))
            return true;

        //  Look diagonally both ways
        position2 = toCheck.gridPosition + direction + ccw;

        if (IsAPotentialMatch(toCheck, position1, position2))
            return true;

        //  Look diagonally counterclockwise
        position1 = toCheck.gridPosition + direction + ccw * 2;

        if (IsAPotentialMatch(toCheck, position1, position2))
            return true;

        return false;
    }

    //  Will these matchables form a potential match?
    private bool IsAPotentialMatch(Matchable toCompare, Vector2Int position1, Vector2Int position2)
    {
        if
        (
            BoundsCheck(position1) && BoundsCheck(position2)
            && !IsEmpty(position1) && !IsEmpty(position2)
            && GetItemAt(position1).Idle && GetItemAt(position2).Idle
            && GetItemAt(position1).GetMatchableType() == toCompare.GetMatchableType() && GetItemAt(position2).GetMatchableType() == toCompare.GetMatchableType()
        )
            return true;

        return false;
    }
}
