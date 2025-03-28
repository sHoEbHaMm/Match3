using System;
using System.Collections;
using UnityEngine;


/*
 * This class will manage the player's score and resolving matches
 * 
 * This class inherits from Singleton so any other script can access it easily through ScoreManager.Instance
 */
public class ScoreManager : Singleton<ScoreManager>
{
    private AudioMixer audioMixer;
    private MatchableGrid matchableGrid;
    private UIManager uIManager;

    // actual score, and a combo multiplier
    private int score;
    private int comboMultiplier = 0;

    //highest combo so far
    private int highestComboMultiplier = 0;

    //getter for highestcombo
    public int HighestCombo
    {
        get
        {
            return highestComboMultiplier;
        }
    }

    //getter for score
    public int Score
    {
        get
        {
            return score;
        }
    }

    // how much time has passed since the player last scored?
    private float timeSincePlayerLastScored;

    //  how much time should we allow before resetting the combo multiplier?
    [SerializeField] private float maxComboTime;
    [SerializeField] private float currentComboTime;

    //  is the combo timer currently running?
    private bool isComboTimerActive;

    // Events 
    public event Action<int> OnScoreUpdated;
    public event Action<bool> ToggleComboUI;
    public event Action<float> OnComboChangeSlider;
    public event Action<int, int> OnComboChangeText;

    //  get references to other game objects in Start and subscribe to events
    void Start()
    {
        matchableGrid = (MatchableGrid)MatchableGrid.Instance;
        uIManager = UIManager.Instance;
        audioMixer = AudioMixer.Instance;

        matchableGrid.OnResolveRequested += ResolveMatch;

        ToggleComboUI?.Invoke(false);
    }


    public void ResolveMatch(Match toResolve)
    {
        StartCoroutine(StartResolvingMatch(toResolve));
    }

    // coroutine for resolving a match
    public IEnumerator StartResolvingMatch(Match toResolve)
    {
        Matchable matchable;

        //play resolve sound
        audioMixer.PlaySound(SoundEffects.resolve);

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

    // add an amount to the score
    private void AddScore(int toAdd)
    {
        score += toAdd * IncreaseComboMultiplier();

        OnScoreUpdated?.Invoke(score);
        ToggleComboUI?.Invoke(true);
        OnComboChangeText?.Invoke(comboMultiplier, highestComboMultiplier);

        timeSincePlayerLastScored = 0;

        if (!isComboTimerActive)
            StartCoroutine(ComboTimer());

        // play score sound
        audioMixer.PlaySound(SoundEffects.score);
    }

    // This corountine counts up to max combo time before resetting the combo multiplier
    private IEnumerator ComboTimer()
    {
        isComboTimerActive = true;

        do
        {
            timeSincePlayerLastScored += Time.deltaTime;
            //update fill bar
            OnComboChangeSlider?.Invoke(1 - timeSincePlayerLastScored / currentComboTime);
            yield return null;
        }
        while (timeSincePlayerLastScored < currentComboTime);

        comboMultiplier = 0;
        isComboTimerActive = false;

        ToggleComboUI?.Invoke(false);
    }

    private int IncreaseComboMultiplier()
    {
        ++comboMultiplier;

        if (highestComboMultiplier < comboMultiplier)
            highestComboMultiplier = comboMultiplier;

        currentComboTime = maxComboTime - Mathf.Log(comboMultiplier) / 2;
        return comboMultiplier;
    }
}
