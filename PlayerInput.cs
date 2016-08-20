using UnityEngine;

public class PlayerInput : MonoBehaviour
{
    private GridManager gridManager;
    public LayerMask Tiles;
    private GameObject activeTile;

    void Awake()
    {
        gridManager = GetComponent<GridManager>();
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Mouse0))
        {
            if (activeTile == null)
                SelectTile();
            else
                AttemptMove();
        }
        else if (Input.GetKeyDown(KeyCode.Mouse1))
            activeTile = null;
    }

    // Tries to select a tile if the players left-clicks and no other tile is selected.
    void SelectTile()
    {

        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit2D hit = Physics2D.GetRayIntersection(ray, 50f, Tiles);
        if (hit)
            activeTile = hit.collider.gameObject;
    }

    // Tries to select and move a tile if the player left-clicks and another tile has already been selected.
    void AttemptMove()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit2D hit = Physics2D.GetRayIntersection(ray, 50f, Tiles);
        if (hit)
        {
            if (Vector2.Distance(activeTile.transform.position, hit.collider.gameObject.transform.position) <= 1.25f)
            {
                TileControl activeControl = activeTile.GetComponent<TileControl>();
                TileControl hitControl = hit.collider.gameObject.GetComponent<TileControl>();

                GridManager.XY activeXY = activeControl.MyXY;
                GridManager.XY hitXY = hitControl.MyXY;

                activeControl.Move(hitXY);
                hitControl.Move(activeXY);

                gridManager.SwitchTiles(hitXY, activeXY);

                activeTile = null;
            }
        }
    }
}