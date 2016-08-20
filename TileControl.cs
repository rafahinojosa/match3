using UnityEngine;
using System.Collections;

public class TileControl : MonoBehaviour
{
    public GridManager GridManager;
    public GridManager.XY MyXY;

    public void Move(GridManager.XY xy)
    {
        StartCoroutine(Moving(xy));
    }

    // A co-routine which moves the tile into its destination.
    IEnumerator Moving(GridManager.XY xy)
    {
        GridManager.ReportTileMovement(); // Report to the GridManager that this tile is moving so it doesn't check for matches.

        Vector2 destination = new Vector2(xy.X, xy.Y);
        bool moving = true;

        while (moving)
        {
            transform.position = Vector2.MoveTowards(transform.position, destination, 5f * Time.deltaTime);

            if (Vector2.Distance(transform.position, destination) <= 0.1f)
            {
                transform.position = destination;
                moving = false;
            }
            yield return null;
        }

        MyXY = xy;
        gameObject.name = xy.X + "/" + xy.Y; // Not necessary, just helps with the overview in the Hierarchy.
        GridManager.ReportTileStopped(); // Report to the GridManager that this tile is done moving.   
    }
}