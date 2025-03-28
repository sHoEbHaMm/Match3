using UnityEngine;


/*
 * This class will both draw the cursor where it needs to be and handle input processing
 * This class is a Singleton so any other script can get a reference to this through Instance
 * It requires a sprite renderer, designed to be used with a 9-slice of 1 unity unit in size
 */

[RequireComponent(typeof(SpriteRenderer))]
public class Cursor : Singleton<Cursor>
{
    [Tooltip("Enables you to press the number keys while holding mouse to change types\r\n ")]
    //  activate the ability to edit the grid
    public bool cheatMode;

    private SpriteRenderer spriteRenderer;

    private MatchableGrid grid;
    private MatchablePool pool;

    private Matchable[] selectedMatchables;

    //  These variables will be used to stretch and reposition the cursor to cover 2 matchables
    [Tooltip("These variables will be used to stretch and reposition the cursor to cover 2 matchables")]
    [SerializeField]
    private Vector2 verticalStretch = new Vector2Int(1, 2),
                    horizontalStretch = new Vector2Int(2, 1);

    [SerializeField]
    private Vector3 halfUp = Vector3.up / 2,
                halfDown = Vector3.down / 2,
                halfLeft = Vector3.left / 2,
                halfRight = Vector3.right / 2;

    //  since this is a singleton, using Init instead of Awake to initialize stuff
    protected override void Initialize()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();

        spriteRenderer.enabled = false;

        selectedMatchables = new Matchable[2];
    }

    // get references to required gameobjects
    private void Start()
    {
        grid = (MatchableGrid)MatchableGrid.Instance;
        pool = (MatchablePool)MatchablePool.Instance;
    }

    private void Update()
    {
        if (!cheatMode || selectedMatchables[0] == null)
            return;

        //  press the number keys while holding mouse to change types

        if (Input.GetKeyDown(KeyCode.Alpha1))
            pool.ChangeType(selectedMatchables[0], pool.MatchableTypes[0]);

        if (Input.GetKeyDown(KeyCode.Alpha2))
            pool.ChangeType(selectedMatchables[0], pool.MatchableTypes[1]);

        if (Input.GetKeyDown(KeyCode.Alpha3))
            pool.ChangeType(selectedMatchables[0], pool.MatchableTypes[2]);

        if (Input.GetKeyDown(KeyCode.Alpha4))
            pool.ChangeType(selectedMatchables[0], pool.MatchableTypes[3]);

        if (Input.GetKeyDown(KeyCode.Alpha5))
            pool.ChangeType(selectedMatchables[0], pool.MatchableTypes[4]);

        if (Input.GetKeyDown(KeyCode.Alpha6))
            pool.ChangeType(selectedMatchables[0], pool.MatchableTypes[5]);

        if (Input.GetKeyDown(KeyCode.Alpha7))
            pool.ChangeType(selectedMatchables[0], pool.MatchableTypes[6]);
    }

    //  select the 1st of 2 matchables, move the cursor to it, reset the size, and activate the sprite
    public void SelectFirst(Matchable toSelect)
    {
        selectedMatchables[0] = toSelect;

        if (!enabled || selectedMatchables[0] == null)
            return;

        transform.position = toSelect.transform.position;

        spriteRenderer.size = Vector2.one;
        spriteRenderer.enabled = true;
    }

    //  select the 2nd of 2 matchables, if they are adjacent, swap them, then deselect
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
