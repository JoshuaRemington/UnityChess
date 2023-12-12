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
    public Stack<Move> moveHistory = new Stack<Move>();
    public Stack<int> captureIndexHistory = new Stack<int>();
    Bitboards bitboardObject = new Bitboards();

    private bool whiteToMove = true;
    private MoveGenerator m = new MoveGenerator();
    private Magic s = new Magic();

    private bool gameOver = false;

    // Start is called before the first frame update
    //private string standardFen = "rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR w KQkq - 0 1";
    private string fen = "rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR w KQkq - 0 1";
    private int perftDepth = 1;
    void Start()
    {
        parseFen(fen, ref board);
        bitboardObject.initiateBitboardStartPosition(fen);
        s.Create();
        m.StoreMoves();
        DateTime startTime = DateTime.Now;
        Move lastMove = MoveGenerator.lastMove;
        bool saveSideToMove = whiteToMove;
        string temp = Perft(perftDepth, lastMove, false).ToString();
        whiteToMove = saveSideToMove;
        DateTime endTime = DateTime.Now;
        TimeSpan elapsedTime = endTime - startTime;
        double timeTaken = elapsedTime.TotalMilliseconds;
        
        string temp2 = "\nExecution Time: \n" + timeTaken.ToString();
        text.text = temp + temp2;
        MoveGenerator.lastMove = lastMove;
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
            test = test.Contains(ar, test);
            if(!Move.SameMove(test, Move.nullMove))
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
    int startloc, endloc;
    Chess c;
    Move[] ar = new Move[256];
    void Update()
    {
        while(gameOver)
            ;
        if(whiteToMove) 
        {
            playAIMove();
            MoveGenerator.GenerateMoves(ref ar, bitboardObject);
        }
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
            endloc = this.TranslateMouseToPos(mousePosition);
            Move play = new Move(startloc,endloc);
            play = play.Contains(ar, play);
            if(!Move.SameMove(play, Move.nullMove))
            {
                playMoveOnBoards(play);
                MoveGenerator.GenerateMoves(ref ar, bitboardObject);
            } else {
                c.SetCoords(startloc);
            }
            selectedObject = null;
        }
        if(Input.GetKeyDown(KeyCode.G))
        {
            undoMoveOnBoards();
            MoveGenerator.GenerateMoves(ref ar, bitboardObject);
        }
        if(Input.GetKeyDown(KeyCode.H))
        {
            detectDescrepancy();
        }
    }

    public void undoMoveOnBoards()
    {
        if(moveHistory.Count == 0) return;
        Move play = moveHistory.Pop();
        int captureIndex = captureIndexHistory.Pop();
        bitboardObject.undoMove(play,captureIndex);
        if(moveHistory.Count != 0)
        {
            Move temp = moveHistory.Pop();
            MoveGenerator.lastMove = temp;
            moveHistory.Push(temp);
        }
        Chess piece = board[play.targetSquare].GetComponent<Chess>();
        board[play.startSquare] = board[play.targetSquare];
        board[play.targetSquare] = null;
        piece.SetCoords(play.startSquare);
        if(play.flag == Move.CastleFlag)
        {
            switch(captureIndex)
            {
                case 0: board[0] = board[2]; board[2] = null; c = board[0].GetComponent<Chess>(); c.SetCoords(0); break;
                case 7: board[7] = board[4]; board[4] = null; c = board[7].GetComponent<Chess>(); c.SetCoords(7); break;
                case 56: board[56] = board[58]; board[58] = null; c = board[56].GetComponent<Chess>(); c.SetCoords(56); break;
                case 63: board[63] = board[60]; board[60] = null; c = board[63].GetComponent<Chess>(); c.SetCoords(63); break;
            } 
        }
        else if(play.flag == Move.EnPassantCaptureFlag)
        {
            board[play.enPassantSquare] = Create(whiteToMove ? "white_pawn" : "black_pawn", play.enPassantSquare);
            captureIndex = 0;
        }
        else if(play.flag == Move.PromoteToQueenFlag)
        {
            if(whiteToMove)
                board[play.startSquare].GetComponent<SpriteRenderer>().sprite = c.white_pawn;
            else
                board[play.startSquare].GetComponent<SpriteRenderer>().sprite = c.black_pawn;
        }

        if(captureIndex != 0)
        {
            returnCapturedPiece(captureIndex, play.targetSquare);
        }
        whiteToMove = !whiteToMove;
    }

    public void returnCapturedPiece(int index, int square)
    {
        switch(index)
        {
            case 2: board[square] = Create("white_pawn", square); return;
            case 3: board[square] = Create("white_knight", square); return;
            case 4: board[square] = Create("white_bishop", square); return;
            case 5: board[square] = Create("white_rook", square); return;
            case 6: board[square] = Create("white_queen", square); return;
            case 8: board[square] = Create("black_pawn", square); return;
            case 9: board[square] = Create("black_knight", square); return;
            case 10: board[square] = Create("black_bishop", square); return;
            case 11: board[square] = Create("black_rook", square); return;
            case 12: board[square] = Create("black_queen", square); return;
            default: return;
        }
    }

    public void playMoveOnBoards(Move play)
    {
        moveHistory.Push(play);
        //int captureIndex = -1;
        if(board[endloc]) 
        {
            Chess temp = board[endloc].GetComponent<Chess>();
            //captureIndex = temp.pieceToBitboardValue;
            Destroy(board[endloc]);
        }
        board[endloc] = board[startloc];
        board[startloc] = null;
        c.SetCoords(endloc);
        whiteToMove = !whiteToMove;
        int interestSquare = bitboardObject.playMove(play);
        captureIndexHistory.Push(interestSquare);
        if(play.flag == Move.CastleFlag)
        {
            switch(interestSquare)
            {
                case 0: board[2] = board[0]; board[0] = null; c = board[2].GetComponent<Chess>(); c.SetCoords(2); break;
                case 7: board[4] = board[7]; board[7] = null; c = board[4].GetComponent<Chess>(); c.SetCoords(4); break;
                case 56: board[58] = board[56]; board[56] = null; c = board[58].GetComponent<Chess>(); c.SetCoords(58); break;
                case 63: board[60] = board[63]; board[63] = null; c = board[60].GetComponent<Chess>(); c.SetCoords(60); break;
            } 
        }
        else if(play.flag == Move.EnPassantCaptureFlag)
        {
            Chess temp = board[play.enPassantSquare].GetComponent<Chess>();
            Destroy(board[play.enPassantSquare]);
            board[play.enPassantSquare] = null;
        }
        else if(play.flag == Move.PromoteToQueenFlag)
        {
            if(!whiteToMove)
                board[play.targetSquare].GetComponent<SpriteRenderer>().sprite = c.white_queen;
            else
                board[play.targetSquare].GetComponent<SpriteRenderer>().sprite = c.black_queen;
        }
        MoveGenerator.lastMove = play;
    }

    public void playAIMove()
    {
        Move play = AI.rootNegaMax(5, ref bitboardObject);
        if(play == Move.nullMove) {gameOver = true; return;}
        startloc = play.startSquare;
        endloc = play.targetSquare;
        c = board[startloc].GetComponent<Chess>();
        playMoveOnBoards(play);
    }
    public ulong Perft(int depth, Move parentMove, bool print)
    {
        Move[] test = new Move[256];
        ulong n_moves, i;
        ulong nodes = 0;
        bool correctPath = print;
        MoveGenerator.lastMove = parentMove;
        n_moves = MoveGenerator.GenerateMoves(ref test, bitboardObject);
        //if(depth == perftDepth)
                //for(ulong var = 0; var < n_moves; var++)
                    //Debug.Log(standardChessBoardPosistions[test[var].startSquare] + standardChessBoardPosistions[test[var].targetSquare] + ": " + 1);
        /*
        if(depth == 1 && print)
            for(ulong var = 0; var < n_moves; var++)
                Debug.Log(standardChessBoardPosistions[test[var].startSquare] + standardChessBoardPosistions[test[var].targetSquare] + ": 1");
        if(1 == perftDepth && print)
            for(ulong var = 0; var < n_moves; var++)
                Debug.Log(standardChessBoardPosistions[test[var].startSquare] + standardChessBoardPosistions[test[var].targetSquare] + ": 1");
        if(depth == 1 && print)
            Debug.Log("THIS IS A SEPERATION OF MOVES");
            */
        if(n_moves == 0) return (ulong)0;

        if(depth == 1)
            return n_moves;
        for(i = 0; i < n_moves; i++)
        {
            Move play = test[i];
            int captureIndex = bitboardObject.playMove(play);
            ulong thisNodes = 0;
            /*
            if(depth == 4 && standardChessBoardPosistions[play.startSquare] == "b4" && standardChessBoardPosistions[play.targetSquare] == "b3")
                print = true;
            else if(correctPath && depth == 3 && standardChessBoardPosistions[play.startSquare] == "d6" && standardChessBoardPosistions[play.targetSquare] == "d5")
                print = true;
            else if(correctPath && depth == 2 && standardChessBoardPosistions[play.startSquare] == "b5" && standardChessBoardPosistions[play.targetSquare] == "b6")
                print = true;
            else
                print = false;
                */
            thisNodes = Perft(depth-1, play, print);
            /*
            if(print)
                Debug.Log(standardChessBoardPosistions[play.startSquare] + standardChessBoardPosistions[play.targetSquare] + ": " + thisNodes);
            if(depth == 100000 && print)
                for(ulong var = 0; var < n_moves; var++)
                    Debug.Log(standardChessBoardPosistions[test[var].startSquare] + standardChessBoardPosistions[test[var].targetSquare] + ": " + thisNodes);
            //if(depth == 2 && standardChessBoardPosistions[parentMove.startSquare] == "d6" && standardChessBoardPosistions[parentMove.targetSquare] == "d5")
              //  Debug.Log(standardChessBoardPosistions[play.startSquare] + standardChessBoardPosistions[play.targetSquare] + ": " + thisNodes);
              */
            //if(depth == perftDepth)
                //Debug.Log(standardChessBoardPosistions[play.startSquare] + standardChessBoardPosistions[play.targetSquare] + ": " + thisNodes);
                
            nodes += thisNodes;
            bitboardObject.undoMove(play,captureIndex);
        }
        return nodes;
    }

    private void parseFen(string fen, ref GameObject[] temp)
    {
        int i = 0;
        int j = 0;
        while(j < fen.Length && i < 64)
        {
            switch(fen[j])
            {
                case 'p': temp[63-i] = Create("black_pawn", 63-i); break;
                case 'n': temp[63-i] = Create("black_knight", 63-i); break;
                case 'b': temp[63-i] = Create("black_bishop", 63-i); break;
                case 'r': temp[63-i] = Create("black_rook", 63-i); break;
                case 'q': temp[63-i] = Create("black_queen", 63-i); break;
                case 'k': temp[63-i] = Create("black_king", 63-i); break;
                case 'P': temp[63-i] = Create("white_pawn", 63-i); break;
                case 'N': temp[63-i] = Create("white_knight", 63-i); break;
                case 'B': temp[63-i] = Create("white_bishop", 63-i); break;
                case 'R': temp[63-i] = Create("white_rook", 63-i); break;
                case 'Q': temp[63-i] = Create("white_queen", 63-i); break;
                case 'K': temp[63-i] = Create("white_king", 63-i); break;
                case '/': j++; continue;
                default: double storeValue = Char.GetNumericValue(fen[j]); i+= (int)storeValue; j++; continue;
            }
            i++;
            j++;
        }
        j++;
        if(j >= fen.Length) return;
        if(fen[j] == 'w') whiteToMove= true; 
        else whiteToMove = false;
        j += 2;
        while(fen[j] != ' ')
        {
            j++;
        }
        j++;
        if(fen[j] != '-')
        {
            int storeEnPassantTargetSquare = -1;
            string enPassantTargetSquare = fen.Substring(j, 2);
            for(int var = 0; var < standardChessBoardPosistions.Length; var++)
                if(enPassantTargetSquare == standardChessBoardPosistions[var])
                    storeEnPassantTargetSquare = var;
            
            if(storeEnPassantTargetSquare != -1)
            {
                Move setEnPassantPossible;
                if(storeEnPassantTargetSquare > 15 && storeEnPassantTargetSquare < 24)
                    setEnPassantPossible = new Move(storeEnPassantTargetSquare - 8, storeEnPassantTargetSquare + 8, Move.PawnTwoUpFlag);
                else
                    setEnPassantPossible = new Move(storeEnPassantTargetSquare + 8, storeEnPassantTargetSquare - 8, Move.PawnTwoUpFlag);
                
                MoveGenerator.lastMove = setEnPassantPossible;
            }
        }
    }

    public void detectDescrepancy()
    {
        Debug.Log("Checking for descrepency");
        for(int i = 0; i < 2; i++)
        {
            ulong checkingForDescrepancy = new ulong();
            checkingForDescrepancy |= bitboardObject.pawns[i];
            checkingForDescrepancy |= bitboardObject.knights[i];
            checkingForDescrepancy |= bitboardObject.bishops[i];
            checkingForDescrepancy |= bitboardObject.rooks[i];
            checkingForDescrepancy |= bitboardObject.queens[i];
            checkingForDescrepancy |= bitboardObject.kingBitboards[i];

            if(checkingForDescrepancy != bitboardObject.pieces[i])
                Debug.Log("we are off for color: " + ((i==0) ? "white" : "black"));

            if((bitboardObject.pawns[i] & ~(bitboardObject.pieces[i])) != 0)
                Debug.Log("Pawns not working for color: " + ((i==0) ? "white" : "black"));
        }
        Bitboards.printBitBoard(bitboardObject.pawns[0]);
        Bitboards.printBitBoard(bitboardObject.pawns[1]);
        Debug.Log("Last Move: " + MoveGenerator.lastMove.startSquare + " " + MoveGenerator.lastMove.targetSquare + " " + MoveGenerator.lastMove.flag + " " + MoveGenerator.lastMove.enPassantSquare);
    }

    public string[] standardChessBoardPosistions = 
    {
        "h1", "g1", "f1", "e1", "d1", "c1", "b1", "a1",
        "h2", "g2", "f2", "e2", "d2", "c2", "b2", "a2",
        "h3", "g3", "f3", "e3", "d3", "c3", "b3", "a3",
        "h4", "g4", "f4", "e4", "d4", "c4", "b4", "a4",
        "h5", "g5", "f5", "e5", "d5", "c5", "b5", "a5",
        "h6", "g6", "f6", "e6", "d6", "c6", "b6", "a6",
        "h7", "g7", "f7", "e7", "d7", "c7", "b7", "a7",
        "h8", "g8", "f8", "e8", "d8", "c8", "b8", "a8"
    };
}
