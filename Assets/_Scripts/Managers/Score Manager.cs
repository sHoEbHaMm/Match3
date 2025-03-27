using System;
using System.Collections;
using UnityEngine;

public class ScoreManager : Singleton<ScoreManager>
{
    private int score;

    public int Score
    {
        get
        {
            return score;
        }
    }

    private MatchableGrid matchableGrid;
    private UIManager uIManager;

    public event Action<int> OnScoreUpdated;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        matchableGrid = (MatchableGrid)MatchableGrid.Instance;
        uIManager = UIManager.Instance;
        matchableGrid.OnResolveRequested += ResolveMatch;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void ResolveMatch(Match toResolve)
    {
        StartCoroutine(StartResolvingMatch(toResolve));
    }

    public IEnumerator StartResolvingMatch(Match toResolve)
    {
        Matchable matchable;

        for(int i = 0; i != toResolve.Count; i++)
        {
            matchable = toResolve.Matchables[i];

            // remove the matchables from the grid
            matchableGrid.RemoveItemAt(matchable.gridPosition);

            // move them off to the side of the screen
            if (i == toResolve.Count - 1)
                yield return StartCoroutine(matchable.MoveThemOff(uIManager.MatchableCollectionPoint));
            else
                StartCoroutine(matchable.MoveThemOff(uIManager.MatchableCollectionPoint));
        }

        // update players score
        AddScore(toResolve.Count * toResolve.Count);

        yield return null;
    }

    private void AddScore(int toAdd)
    {
        score += toAdd;
        OnScoreUpdated?.Invoke(score);
    }
}
