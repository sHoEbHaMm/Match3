using System;
using System.Collections;
using UnityEngine;

public class ScoreManager : Singleton<ScoreManager>
{
    private AudioMixer audioMixer;
    private MatchableGrid matchableGrid;
    private UIManager uIManager;

    private int score;
    private int comboMultiplier = 0;

    public int Score
    {
        get
        {
            return score;
        }
    }

    private float timeSincePlayerLastScored;
    [SerializeField] private float maxComboTime;
    [SerializeField] private float currentComboTime;
    private bool isComboTimerActive;


    public event Action<int> OnScoreUpdated;
    public event Action<bool> ToggleComboUI;
    public event Action<float> OnComboChangeSlider;
    public event Action<int> OnComboChangeText;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        matchableGrid = (MatchableGrid)MatchableGrid.Instance;
        uIManager = UIManager.Instance;
        audioMixer = AudioMixer.Instance;

        matchableGrid.OnResolveRequested += ResolveMatch;

        ToggleComboUI?.Invoke(false);
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

    private void AddScore(int toAdd)
    {
        score += toAdd * IncreaseComboMultiplier();

        OnScoreUpdated?.Invoke(score);
        ToggleComboUI?.Invoke(true);
        OnComboChangeText?.Invoke(comboMultiplier);

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
        currentComboTime = maxComboTime - Mathf.Log(comboMultiplier) / 2;
        return comboMultiplier;
    }
}
