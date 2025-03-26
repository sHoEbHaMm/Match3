using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class Matchable : Movable
{
    private MatchableType matchableType;
    private SpriteRenderer spriteRenderer;
    private Cursor cursor;

    // where is the matchable on the grid
    public Vector2Int gridPosition;

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();   
    }

    private void Start()
    {
        cursor = Cursor.Instance;
    }

    private void OnMouseDown()
    {
        cursor.SelectFirst(this);
    }

    private void OnMouseUp()
    {
        cursor.SelectFirst(null);
    }

    private void OnMouseEnter()
    {
        cursor.SelectSecond(this);
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
