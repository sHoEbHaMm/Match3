using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class Matchable : Movable
{
    private MatchableType matchableType;
    private SpriteRenderer spriteRenderer;

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    public void SetType(MatchableType type)
    {
        matchableType = type;
        spriteRenderer.sprite = type.Sprite;
    }

    public MatchableType GetMatchableType()
    {
        return matchableType;
    }
}
