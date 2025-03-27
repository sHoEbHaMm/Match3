using System.Collections;
using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class Matchable : Movable
{
    private MatchablePool pool; 

    private MatchableType matchableType;
    private SpriteRenderer spriteRenderer;
    private Cursor cursor;

    // where is the matchable on the grid
    public Vector2Int gridPosition;

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        pool = (MatchablePool)MatchablePool.Instance;
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

    public IEnumerator MoveThemOff(Transform toThisTransform)
    {
        // draw above others in the grid
        spriteRenderer.sortingOrder = 2;

        //move off the grid to the point
        yield return StartCoroutine(MoveToPosition(toThisTransform.position));

        // reset
        spriteRenderer.sortingOrder = 2;

        // return it back to the pool
        pool.ReturnObjectToPool(this);

        yield return null;
    }
}
