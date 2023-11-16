using UnityEngine;

public class Piece : MonoBehaviour
{
    public Board board { get; private set; }
    public TetrominoData data { get; private set; }
    public Vector3Int[] cells { get; private set; }
    public Vector3Int position { get; private set; }
    public int rotationIndex { get; private set; }

    public float stepDelay = 1f;
    public float moveDelay = 0.1f;
    public float lockDelay = 0.5f;

    private float stepTime;
    private float moveTime;
    private float lockTime;

    public void Initialize(Board board, Vector3Int position, TetrominoData data)
    {
        this.data = data;
        this.board = board;
        this.position = position;

        rotationIndex = 0;
        stepTime = Time.time + stepDelay;
        moveTime = Time.time + moveDelay;
        lockTime = 0f;

        if (cells == null) {
            cells = new Vector3Int[data.cells.Length];
        }

        for (int i = 0; i < cells.Length; i++) {
            cells[i] = (Vector3Int)data.cells[i];
        }
    }
    
    private void Update()
    {
        board.Clear(this);
        lockTime += Time.deltaTime;

        // Rotation
        if (Input.GetKeyDown(KeyCode.Q)) {
            Rotate(-1);
        }
        else if (Input.GetKeyDown(KeyCode.E)) {
            Rotate(1);
        }

        //Hard drop movement
        if (Input.GetKeyDown(KeyCode.Space)) {
           HardDrop();
        }

        // Allow the player to hold movement keys but only after a move delay
        // so it does not move too fast
        if (Time.time > moveTime) {
            HandleMoveInputs();
        }

        // Advance the piece to the next row every x seconds
        if (Time.time >= stepTime) {
            Step();
        }

        board.Set(this);

    }

    private void HandleMoveInputs()
    {
        // Soft drop movement
        if (Input.GetKey(KeyCode.S))
        {
            if (Move(Vector2Int.down)) {
                // Update the step time to prevent double movement
                stepTime = Time.time + stepDelay;
            }
        }

        // Left/right movement
        if (Input.GetKey(KeyCode.A)) {
            Move(Vector2Int.left);
        } else if (Input.GetKey(KeyCode.D)) {
            Move(Vector2Int.right);
        }
    }
    
    private void Step()
    {
        stepTime = Time.time + stepDelay;

        // Step down to the next row
        Move(Vector2Int.down);

        // Once the piece has been inactive for too long it becomes locked
        if (lockTime >= lockDelay)
        {
            Lock();
        }
    }

    private void HardDrop()
        {
            while (Move(Vector2Int.down)) {
                continue;
            }
            Lock();
        }

    private void Lock()
    {
        board.Set(this);
        board.ClearLines();
        board.SpawnPiece();
    }

    private bool Move(Vector2Int translation)
        {
            Vector3Int newPosition = position;
            newPosition.x += translation.x;
            newPosition.y += translation.y;

            bool valid = board.IsValidPosition(this,newPosition);

            // Only save the movement if the new position is valid
            if (valid)
            {
                position = newPosition;
                moveTime = Time.time + moveDelay;
                lockTime = 0f; // Optional reset lock timer for each move/rotation
            }

            return valid;
        }

    private void Rotate(int direction)
    {
        // Store the current rotation in case the rotation fails and we need to revert
        int originalRotation = rotationIndex;

        rotationIndex = Wrap(rotationIndex + direction, 0, 4);
        ApplyRotationMatrix(direction);

        // Revert the rotation if the wall kick tests fail
        if (!TestWallKicks(rotationIndex, direction))
        {
            rotationIndex = originalRotation; 
            ApplyRotationMatrix(-direction); //Negate the rotation by rotating in opposite direction
        }
    }

    private void ApplyRotationMatrix(int direction)
    {
        float[] matrix = Data.RotationMatrix;

        for(int i = 0; i < cells.Length; i++)
        {
            Vector3 cell = cells[i]; //Rotation for I and O piece is offset by 0.5 so a float is needed
            int x, y; 

            switch(data.tetromino)
            {
                case Tetromino.I:
                case Tetromino.O:
                    cell.x -= 0.5f;
                    cell.y -= 0.5f;
                    //CeilToInt to round upwards to prevent converting Int to Float
                    x = Mathf.CeilToInt((cell.x * matrix[0] * direction) + (cell.y * matrix[1] * direction)); 
                    y = Mathf.CeilToInt((cell.x * matrix[2] * direction) + (cell.y * matrix[3] * direction));
                    break;
                default:
                    //RoundToInt to prevent converting Int to Float
                    x = Mathf.RoundToInt((cell.x * matrix[0] * direction) + (cell.y * matrix[1] * direction)); 
                    y = Mathf.RoundToInt((cell.x * matrix[2] * direction) + (cell.y * matrix[3] * direction));
                    break;
            }

            cells[i] = new Vector3Int (x, y, 0);
        }
    }

    private bool TestWallKicks(int rotationIndex, int rotationDirection)
    {
        int wallKickIndex = GetWallKickIndex(rotationIndex, rotationDirection);

        for (int i = 0; i < data.wallKicks.GetLength(1); i++) //There are 5 tests for wall kicks in Data
        {
            Vector2Int translation = data.wallKicks[wallKickIndex, i];

            if (Move(translation))
            {
                return true;
            }
        }

        return false;
    }

    private int GetWallKickIndex(int rotationIndex, int rotationDirection)
    {
        int wallKickIndex = rotationIndex * 2;

        if (rotationDirection < 0)
        {
            wallKickIndex--;
        }

        return Wrap(wallKickIndex, 0, data.wallKicks.GetLength(0));
    }

    //Wrap around if below minium or above maximum
    private int Wrap(int input, int min, int max)
    {
        if (input < min)
        {
            return max - (min - input) % (max - min);
        }
        else
        {
            return min + (input - min) % (max - min);
        }
    }
}