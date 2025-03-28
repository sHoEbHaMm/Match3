using System.Collections;
using UnityEngine;


/*
 * These are the things in the grid that the player will match by swapping them around
 * 
 * They require a sprite to be drawn on the screen
 * 
 */
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
    }

    // get references of required gameobjects
    private void Start()
    {
        cursor = Cursor.Instance;
        pool = (MatchablePool)MatchablePool.Instance;
    }

    //  when the player clicks, select this as the first selected
    private void OnMouseDown()
    {
        cursor.SelectFirst(this);
    }

    //  when the player releases the click, select nothing
    private void OnMouseUp()
    {
        cursor.SelectFirst(null);
    }

    //  when the player drags the mouse, select this as the second selected
    //  (if using a mouse, this is actually called on every entry, even if they're not dragging, but cursor will filter this behaviour out)
    private void OnMouseEnter()
    {
        cursor.SelectSecond(this);
    }

    //  set the type, performed by the pool
    public void SetType(MatchableType type)
    {
        matchableType = type;
        spriteRenderer.sprite = type.Sprite;
    }

    //getter for type
    public MatchableType GetMatchableType()
    {
        return matchableType;
    }

    //move the matchables off screen once matched
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
