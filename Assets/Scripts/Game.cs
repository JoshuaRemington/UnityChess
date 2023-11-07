using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

using System;

public class Game : MonoBehaviour
{
    public GameObject chesspiece;
    public GameObject MoveTile;

    public Text text;

    private GameObject[] board = new GameObject[64]; 

    private GameObject[] moveTiles = new GameObject[64];
    Bitboards bitboardObject = new Bitboards();

    private bool whiteToMove = true;
    private MoveGenerator m = new MoveGenerator();
    private Magic s = new Magic();

    //private bool gameOver = false;

    // Start is called before the first frame update
    void Start()
    {
       board = new GameObject[] {
            Create("white_rook", 0), Create("white_knight", 1), Create("white_bishop", 2),
            Create("white_queen", 3), Create("white_king", 4), Create("white_bishop", 5),
            Create("white_knight", 6), Create("white_rook", 7), 
            Create("white_pawn", 8), Create("white_pawn", 9), Create("white_pawn", 10), 
            Create("white_pawn", 11), Create("white_pawn", 12), Create("white_pawn", 13), 
            Create("white_pawn", 14), Create("white_pawn", 15),
            null, null, null, null, null, null, null, null,
            null, null, null, null, null, null, null, null,
            null, null, null, null, null, null, null, null,
            null, null, null, null, null, null, null, null,
            Create("black_pawn", 48), Create("black_pawn", 49), Create("black_pawn", 50), 
            Create("black_pawn", 51), Create("black_pawn", 52), Create("black_pawn", 53), 
            Create("black_pawn", 54), Create("black_pawn", 55),
            Create("black_rook", 56), Create("black_knight", 57), Create("black_bishop", 58),
            Create("black_queen", 59), Create("black_king", 60), Create("black_bishop", 61),
            Create("black_knight", 62), Create("black_rook", 63)
        };
        bitboardObject.initiateBitboardStartPosition();
        s.Create();
        m.StoreMoves();
        DateTime startTime = DateTime.Now;
        string temp = Perft(4).ToString();
        DateTime endTime = DateTime.Now;
        TimeSpan elapsedTime = endTime - startTime;
        double timeTaken = elapsedTime.TotalMilliseconds;
        
        string temp2 = "\nExecution Time: \n" + timeTaken.ToString();
        text.text = temp + temp2;
        MoveGenerator.GenerateMoves(ref ar, bitboardObject);
    }

    public GameObject Create(string name, int pos) 
    {
        GameObject obj = Instantiate(chesspiece, new Vector3(0,0,-1), Quaternion.identity);
        Chess c = obj.GetComponent<Chess>();
        c.name = name;
        c.Activate(pos);
        return obj;
    }

    public void SetPositionEmpty(int pos)
    {
        board[pos] = null;
    }

    public GameObject GetPosition(int pos)
    {
        return board[pos];
    }

    public bool PosOnBoard(int pos)
    {
        if (pos < 0 || pos > 64) return false;
        
        return true;
    }

    public int TranslateMouseToPos(Vector3 mouse)
    {
        /*
        length of board: 10
        Length of each square: 1.25
        */
        if(mouse.x > 5.02 || mouse.x < -5.02 || mouse.y > 5.02 || mouse.y < -5.02)
            return -1;

        int row = (int)((mouse.y - 5) / 1.25);
        int col = (int)((mouse.x + 5) / 1.25);
        
        int pos = col - (row * 8);
        return pos;
    }

    public void createMoveTiles(int pos)
    {
        float newZ = -0.9f;
        for(int i = 0; i < 64; i++) {
            Move test = new Move(pos, i);
            if(test.Contains(ar, test) != -1)
                {
                    moveTiles[i] = Instantiate(MoveTile);
                    Vector3 newPosition = moveTiles[i].transform.position;
                    newPosition.z = newZ;
                    moveTiles[i].transform.position = newPosition;
                    MoveTile m = moveTiles[i].GetComponent<MoveTile>();
                    m.Place(i);
                }
        }
    }

    public void deleteMoveTiles()
    {
        for(int i = 0; i < 64; i++)
        {
            Destroy(moveTiles[i]);
            moveTiles[i] = null;
        }
    }
    

    public GameObject selectedObject;
    Vector3 offset;
    int startloc;
    Chess c;
    Move[] ar = new Move[256];
    void Update()
    {
        Vector3 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        if (Input.GetMouseButtonDown(0))
        {
            Collider2D targetObject = Physics2D.OverlapPoint(mousePosition);
            if (targetObject)
            {
                selectedObject = targetObject.transform.gameObject;
                c = selectedObject.GetComponent<Chess>();
                if(c.isWhite != whiteToMove)
                {
                    selectedObject = null;
                    return;
                }
                startloc = TranslateMouseToPos(selectedObject.transform.position);
                createMoveTiles(startloc);
                
                offset = selectedObject.transform.position - mousePosition;
            }
        }
        if(Input.GetMouseButton(0) && selectedObject)
        {
            selectedObject.transform.position = mousePosition + offset;
        }
        if (Input.GetMouseButtonUp(0) && selectedObject)
        {
            deleteMoveTiles();
            int endloc = this.TranslateMouseToPos(mousePosition);
            Move play = new Move(startloc,endloc);
            int validMove = play.Contains(ar, play);
            play.flag = validMove;
            if(validMove != -1)
            {
                int captureIndex = -1;
                if(board[endloc]) 
                {
                    Chess temp = board[endloc].GetComponent<Chess>();
                    captureIndex = temp.pieceToBitboardValue;
                    Destroy(board[endloc]);
                }    
                board[endloc] = board[startloc];
                board[startloc] = null;
                c.SetCoords(endloc);
                whiteToMove = !whiteToMove;
                bitboardObject.playMove(play);
                MoveGenerator.lastMove = play;
                MoveGenerator.GenerateMoves(ref ar,bitboardObject);
            } else {
                c.SetCoords(startloc);
            }
            selectedObject = null;
        }
    }

    public ulong Perft(int depth)
    {
        Move[] test = new Move[256];
        int n_moves, i;
        ulong nodes = 0;

        if(depth == 0)
            return 1ul;

        n_moves = MoveGenerator.GenerateMoves(ref test, bitboardObject);
        whiteToMove = !whiteToMove;
        for(i = 0; i < n_moves; i++)
        {
            Move play = test[i];
            int captureIndex = bitboardObject.playMove(play);
            nodes += Perft(depth-1);
            bitboardObject.undoMove(play,captureIndex);
        }
        whiteToMove = !whiteToMove;
        return nodes;
    }
}
