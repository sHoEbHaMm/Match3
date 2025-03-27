using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UIManager : Singleton<UIManager>
{
    private ScoreManager scoreManager;
    [SerializeField] private Transform matchableCollectionPoint;
    [SerializeField] private TMP_Text scoreText;
    [SerializeField] private TMP_Text comboText;
    [SerializeField] private Slider comboSlider;

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
        scoreManager.ToggleComboUI += ToggleComboElements;
        scoreManager.OnComboChangeSlider += UpdateComboSlider;
        scoreManager.OnComboChangeText += UpdateComboText;
    }

    private void UpdateComboText(int updateTo)
    {
        comboText.text = updateTo + "x";
    }

    private void UpdateScoreText(int updateTo)
    {
        scoreText.text = updateTo.ToString();
    }

    private void UpdateComboSlider(float value)
    {
        comboSlider.value = value;
    }

    private void ToggleComboElements(bool show)
    {
        comboSlider.gameObject.SetActive(show);
    }
}
