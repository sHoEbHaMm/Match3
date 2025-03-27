using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class Cursor : Singleton<Cursor>
{
    private SpriteRenderer spriteRenderer;

    private MatchableGrid grid;

    private Matchable[] selectedMatchables;

    //  These variables will be used to stretch and reposition the cursor to cover 2 matchables
    [SerializeField]
    private Vector2 verticalStretch = new Vector2Int(1, 2),
                    horizontalStretch = new Vector2Int(2, 1);

    [SerializeField]
    private Vector3 halfUp = Vector3.up / 2,
                halfDown = Vector3.down / 2,
                halfLeft = Vector3.left / 2,
                halfRight = Vector3.right / 2;

    private void Start()
    {
        grid = (MatchableGrid)MatchableGrid.Instance;
    }

    protected override void Initialize()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();

        spriteRenderer.enabled = false;

        selectedMatchables = new Matchable[2];
    }

    public void SelectFirst(Matchable toSelect)
    {
        selectedMatchables[0] = toSelect;

        if (!enabled || selectedMatchables[0] == null)
            return;

        transform.position = toSelect.transform.position;

        spriteRenderer.size = Vector2.one;
        spriteRenderer.enabled = true;
    }

    public void SelectSecond(Matchable toSelect)
    {
        selectedMatchables[1] = toSelect;

        if (!enabled || selectedMatchables[0] == null || selectedMatchables[1] == null || !selectedMatchables[0].Idle || !selectedMatchables[1].Idle || 
            selectedMatchables[0] == selectedMatchables[1])
            return;

        if (SelectedAreAdjacent())
            StartCoroutine(grid.TrySwap(selectedMatchables));

        SelectFirst(null);
    }

    //  check if the 2 selected matchables are adjacent
    private bool SelectedAreAdjacent()
    {
        //  if they are in the same column
        if (selectedMatchables[0].gridPosition.x == selectedMatchables[1].gridPosition.x)
        {
            //  if the 1st is above the 2nd
            if (selectedMatchables[0].gridPosition.y == selectedMatchables[1].gridPosition.y + 1)
            {
                spriteRenderer.size = verticalStretch;
                transform.position += halfDown;
                return true;
            }
            //  if the 1st is below the 2nd
            else if (selectedMatchables[0].gridPosition.y == selectedMatchables[1].gridPosition.y - 1)
            {
                spriteRenderer.size = verticalStretch;
                transform.position += halfUp;
                return true;
            }
        }
        //  if they are in the same row
        else if (selectedMatchables[0].gridPosition.y == selectedMatchables[1].gridPosition.y)
        {
            //  if the 1st is to the right of the 2nd
            if (selectedMatchables[0].gridPosition.x == selectedMatchables[1].gridPosition.x + 1)
            {
                spriteRenderer.size = horizontalStretch;
                transform.position += halfLeft;
                return true;
            }
            //  if the 1st is to the left of the 2nd
            else if (selectedMatchables[0].gridPosition.x == selectedMatchables[1].gridPosition.x - 1)
            {
                spriteRenderer.size = horizontalStretch;
                transform.position += halfRight;
                return true;
            }
        }
        return false;
    }
}
