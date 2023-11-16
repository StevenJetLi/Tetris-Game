using UnityEngine;
using UnityEngine.Tilemaps;

public class Board : MonoBehaviour
{
    public Tilemap tilemap { get; private set; }
    public Piece activePiece { get; private set; }
    public TetrominoData[] tetrominoes;
    public Vector2Int boardSize = new Vector2Int(10,20); //Size of board 
    public Vector3Int spawnPosition = new Vector3Int(-1, 8, 0); //Spawns new pieces at the top-middle of the board
    

    //Draw a rectangle from the bottom left of the board (position) to the size of the board (boardSize)
    public RectInt Bounds 
    {
        get
        {
            Vector2Int position = new Vector2Int(-boardSize.x /2, -boardSize.y /2);
            return new RectInt(position, boardSize);
        }
    }

    private void Awake()
    {
        tilemap = GetComponentInChildren<Tilemap>();
        activePiece = GetComponentInChildren<Piece>();

        for (int i = 0; i < tetrominoes.Length; i++) {
            tetrominoes[i].Initialize();
        }
    }

    private void Start()
    {
        SpawnPiece();
    }

    public void SpawnPiece()
    {
        //Spawn random piece
        int random = Random.Range(0, tetrominoes.Length);
        TetrominoData data = tetrominoes[random];

        //Spawned piece is the active piece for rotations and movement
        activePiece.Initialize(this, spawnPosition, data);
        
        if (IsValidPosition(activePiece, spawnPosition))  {
            Set(activePiece);
        }
        else
        {
            GameOver();
        }
        
    }

    public void GameOver()
    {
        tilemap.ClearAllTiles();
    }

    public void Set(Piece piece)
    {
        for (int i = 0; i < piece.cells.Length; i++)
        {
            Vector3Int tilePosition = piece.cells[i] + piece.position;
            tilemap.SetTile(tilePosition, piece.data.tile);
        }
    }

    public void Clear(Piece piece)
    {
        for (int i = 0; i < piece.cells.Length; i++)
        {
            Vector3Int tilePosition = piece.cells[i] + piece.position;
            tilemap.SetTile(tilePosition, null);
        }
    }

    // The position is only valid if every cell is valid
    public bool IsValidPosition(Piece piece, Vector3Int position)
    {
        RectInt bounds = Bounds;

        for (int i = 0; i < piece.cells.Length; i++)
        {
            // Define the new position of the tile 
            Vector3Int tilePosition = piece.cells[i] + position;

            // Test if tile would go out of bounds 
            if (!bounds.Contains((Vector2Int)tilePosition)) {
                return false;
            }

            // Test if a tile already exists there
            if (tilemap.HasTile(tilePosition)) {
                return false;
            }
        }
        return true;
    }

    public void ClearLines()
    {
        RectInt bounds = Bounds;
        int row = bounds.yMin;

        // Clear from bottom to top
        while (row < bounds.yMax)
        {
            // Only advance to the next row if the current is not cleared because the tiles above will fall down when a row is cleared
            if (IsLineFull(row)) {
                LineClear(row);
            } else {
                row++;
            }

        }

    }

    public bool IsLineFull(int row)
    {
        RectInt bounds = Bounds;

        for (int col = bounds.xMin; col < bounds.xMax; col ++) {
            Vector3Int position = new Vector3Int(col, row, 0);
            // The line is not full if a tile is missing
            if (!tilemap.HasTile(position)) {
                return false;
            }
        }

        return true;
    }

 
    public void LineClear(int row)
    {
        RectInt bounds = Bounds;

        //Clear line 
        for (int col = bounds.xMin; col < bounds.xMax; col ++) {
            Vector3Int position = new Vector3Int(col, row, 0);
            tilemap.SetTile(position, null);
        }

        //Shift all rows above down by 1
        while (row < bounds.yMax)
        {
            for (int col = bounds.xMin; col < bounds.xMax; col ++) {
                Vector3Int position = new Vector3Int(col, row + 1, 0);
                TileBase above = tilemap.GetTile(position);

                position = new Vector3Int(col, row, 0);
                tilemap.SetTile(position, above);
            }
            row++;
        }

    }
}