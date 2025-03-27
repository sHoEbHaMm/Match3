using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UIManager : Singleton<UIManager>
{
    private ScoreManager scoreManager;
    [SerializeField] private Transform matchableCollectionPoint;
    [SerializeField] private TMP_Text scoreText;

    public Transform MatchableCollectionPoint
    {
        get
        {
            return matchableCollectionPoint;
        }
    }

    private void Start()
    {
        scoreManager = ScoreManager.Instance;
        scoreManager.OnScoreUpdated += UpdateScoreText;
    }

    private void UpdateScoreText(int updateTo)
    {
        scoreText.text = "Score: " + updateTo.ToString();
    }
}
