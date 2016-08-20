
using UnityEngine;
using System.Collections.Generic;

public class GridManager : MonoBehaviour
{
    public MainUI MainUI;

    private int score;
    // A simple class to handle the coordinates.
    public class XY 
    {
        public int X;
        public int Y;

        public XY (int x, int y)
        {
            X = x;
            Y = y;
        }
    }
     
    // The tile class. Keeps track of the tile type, the GameObject, and its controller.
    public class Tile
    {
        public int TileType;
        public GameObject GO;
        public TileControl TileControl;

        public Tile ()
        {            
            TileType = -1;            
        }

        public Tile (int tileType, GameObject go, TileControl tileControl)
        {
            TileType = tileType;
            GO = go;
            TileControl = tileControl;
        }
    }

    public GameObject[] TilePrefabs;

    public int GridWidth;
    public int GridHeight;
    public Tile[,] Grid;

    private int movingTiles;

    void Awake()
    {
        CreateGrid();
    }

    void CreateGrid()
    {
        Grid = new Tile[GridWidth, GridHeight];        

        for (int x = 0; x < GridWidth; x++)
        {
            for (int y = 0; y < GridHeight; y++)
            {
                int randomTileType = Random.Range(0, TilePrefabs.Length);
                GameObject go = Instantiate(TilePrefabs[randomTileType], new Vector2(x, y), Quaternion.identity) as GameObject;
                TileControl tileControl = go.GetComponent<TileControl>();                
                Grid[x, y] = new Tile(randomTileType, go , tileControl);
                tileControl.GridManager = this;
                tileControl.MyXY = new XY(x, y);
                go.name = x + "/" + y;
            }
        }
        CheckMatches();
    }

    public void SwitchTiles(XY firstXY, XY secondXY)
    {
        Tile firstTile = new Tile(Grid[firstXY.X, firstXY.Y].TileType, Grid[firstXY.X, firstXY.Y].GO, Grid[firstXY.X, firstXY.Y].TileControl);
        Tile secondTile = new Tile(Grid[secondXY.X, secondXY.Y].TileType, Grid[secondXY.X, secondXY.Y].GO, Grid[secondXY.X, secondXY.Y].TileControl);

        Grid[firstXY.X, firstXY.Y] = secondTile;
        Grid[secondXY.X, secondXY.Y] = firstTile;
    }

    public void CheckMatches()
    {
        List<XY> checkingTiles = new List<XY>(); // Tiles that are currently being considered for a match-3.
        List<XY> tilesToDestroy = new List<XY>(); // Tiles that are confirmed match-3s and will be destroyed.

        // Vertical check
        for (int x = 0; x < GridWidth; x++)
        {
            int currentTileType = -1;
            int lastTileType = -1;

            if (checkingTiles.Count >= 3)
                tilesToDestroy.AddRange(checkingTiles);

            checkingTiles.Clear();

            for (int y = 0; y < GridHeight; y++)
            {
                currentTileType = Grid[x, y].TileType;

                if (currentTileType != lastTileType)
                {
                    if (checkingTiles.Count >= 3)
                        tilesToDestroy.AddRange(checkingTiles);

                    checkingTiles.Clear();
                }

                checkingTiles.Add(new XY(x, y));
                lastTileType = currentTileType;
            }
        }

        checkingTiles.Clear();

        // Horizontal check
        for (int y = 0; y < GridHeight; y++)
        {
            int currentTileType = -1;
            int lastTileType = -1;

            if (checkingTiles.Count >= 3)
            {
                for (int i = 0; i < checkingTiles.Count; i++)
                {
                    if (!tilesToDestroy.Contains(checkingTiles[i]))
                        tilesToDestroy.Add(checkingTiles[i]);
                }
            }

            checkingTiles.Clear();

            for (int x = 0; x < GridWidth; x++)
            {
                currentTileType = Grid[x, y].TileType;

                if (currentTileType != lastTileType)
                {
                    if (checkingTiles.Count >= 3)
                    {
                        for (int i = 0; i < checkingTiles.Count; i++)
                        {
                            if (!tilesToDestroy.Contains(checkingTiles[i]))
                                tilesToDestroy.Add(checkingTiles[i]);

                        }
                    }
                    checkingTiles.Clear();
                }

                checkingTiles.Add(new XY(x, y));
                lastTileType = currentTileType;
            }
        }

        if (tilesToDestroy.Count != 0)
            DestroyMatches(tilesToDestroy);
        else
            ReplaceTiles();
    }

    void DestroyMatches(List<XY> tilesToDestroy)
    {
        for (int i = 0; i < tilesToDestroy.Count; i++)
        {
            Destroy(Grid[tilesToDestroy[i].X, tilesToDestroy[i].Y].GO);
            Grid[tilesToDestroy[i].X, tilesToDestroy[i].Y] = new Tile();

            if (i <= 2)
                AddScore(10);
            else
                AddScore(20);

        }
        GravityCheck();
    }

    void AddScore(int amount)
    {
        score += amount;
        MainUI.SetScoreText(score);
    }

    void ReplaceTiles()
    {
        for (int x = 0; x < GridWidth; x++)
        {
            int missingTileCount = 0;

            for (int y = 0; y < GridHeight; y++)
            {
                if (Grid[x, y].TileType == -1)
                    missingTileCount++;
            }

            for (int i = 0; i < missingTileCount; i++)
            {
                int tileY = GridHeight - missingTileCount + i;
                int randomTileType = Random.Range(0, TilePrefabs.Length);
                GameObject go = Instantiate(TilePrefabs[randomTileType], new Vector2(x, GridHeight + i), Quaternion.identity) as GameObject;
                TileControl tileControl = go.GetComponent<TileControl>();
                tileControl.GridManager = this;
                tileControl.Move(new XY(x, tileY));
                Grid[x, tileY] = new Tile(randomTileType, go, tileControl);
                go.name = x + "/" + tileY;
            }
        }
    }

    void GravityCheck()
    {
        for (int x = 0; x < GridWidth; x++)
        {
            int missingTileCount = 0;

            for (int y = 0; y < GridHeight; y++)
            {
                if (Grid[x, y].TileType == -1)
                    missingTileCount++;
                else
                {
                    if (missingTileCount >= 1)
                    {
                        Tile tile = new Tile(Grid[x, y].TileType, Grid[x, y].GO, Grid[x, y].TileControl);
                        Grid[x, y].TileControl.Move(new XY(x, y - missingTileCount));
                        Grid[x, y - missingTileCount] = tile;
                        Grid[x, y] = new Tile();
                    }
                }
            }
        }
        ReplaceTiles();
    }

    public void ReportTileMovement()
    {
        movingTiles++;
    }

    // If tiles have been moving, we'll check for matches once they are all done.
    public void ReportTileStopped()
    {
        movingTiles--;

        if (movingTiles == 0)
            CheckMatches();
    }

    public void CheckForLegalMoves()
    {
        // Vertical check
        for (int x = 0; x < GridWidth; x++)
        {
            int secondToLastType = -1;
            int lastType = -2;
            int currentType = -3;

            for (int y = 0; y < GridHeight; y++)
            {
                currentType = Grid[x, y].TileType;
                if (lastType == currentType)
                {
                    if (CheckForTileType(x, y - 3, currentType))
                        return;
                    if (CheckForTileType(x + 1, y - 2, currentType))
                        return;
                    if (CheckForTileType(x - 1, y - 2, currentType))
                        return;
                    if (CheckForTileType(x, y + 2, currentType))
                        return;
                    if (CheckForTileType(x + 1, y + 1, currentType))
                        return;
                    if (CheckForTileType(x - 1, y + 1, currentType))
                        return;
                }
                else if (secondToLastType == currentType)
                {
                    if (CheckForTileType(x + 1, y - 1, currentType))
                        return;
                    if (CheckForTileType(x - 1, y - 1, currentType))
                        return;
                }
                secondToLastType = lastType;
                lastType = currentType;
            }
        }

        // Horizontal check
        for (int y = 0; y < GridHeight; y++)
        {
            int secondToLastType = -1;
            int lastType = -2;
            int currentType = -3;

            for (int x = 0; x < GridWidth; x++)
            {
                currentType = Grid[x, y].TileType;
                if (lastType == currentType)
                {
                    if (CheckForTileType(x - 3, y, currentType))
                        return;
                    if (CheckForTileType(x - 2, y + 1, currentType))
                        return;
                    if (CheckForTileType(x - 2, y - 1, currentType))
                        return;
                    if (CheckForTileType(x + 2, y, currentType))
                        return;
                    if (CheckForTileType(x + 1, y + 1, currentType))
                        return;
                    if (CheckForTileType(x + 1, y - 1, currentType))
                        return;
                }
                else if (secondToLastType == currentType)
                {
                    if (CheckForTileType(x - 1, y + 1, currentType))
                        return;
                    if (CheckForTileType(x - 1, y - 1, currentType))
                        return;
                }
                secondToLastType = lastType;
                lastType = currentType;
            }
        }

        // No matches? Shuffle!
        ShuffleGrid();
    }

    bool CheckForTileType(int x, int y, int tileType)
    {
        if (x >= 0 && x < GridWidth && y >= 0 && y < GridHeight)
            return Grid[x, y].TileType == tileType;
        else
            return false;
    }

    void ShuffleGrid()
    {
        List<XY> xyList = new List<XY>();

        for (int x = 0; x < GridWidth; x++)
        {
            for (int y = 0; y < GridWidth; y++)
            {
                xyList.Add(new XY(x, y));
            }
        }

        for (int x = 0; x < GridWidth; x++)
        {
            for (int y = 0; y < GridWidth; y++)
            {
                System.Random rnd = new System.Random();
                int index = rnd.Next(xyList.Count);
                XY xy = xyList[index];
                Grid[x, y].TileControl.Move(xy);
                xyList.RemoveAt(index);
            }
        }
    }

}