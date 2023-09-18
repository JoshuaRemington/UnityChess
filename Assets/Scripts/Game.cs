using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Game : MonoBehaviour
{
    public GameObject chesspiece;

    private GameObject[] board = new GameObject[64]; 

    //private string currentPlayer = "white";

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
    

    public GameObject selectedObject;
    Vector3 offset;
    int startloc;
    void Update()
    {
        Vector3 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        if (Input.GetMouseButtonDown(0))
        {
            Collider2D targetObject = Physics2D.OverlapPoint(mousePosition);
            if (targetObject)
            {
                selectedObject = targetObject.transform.gameObject;
                startloc = TranslateMouseToPos(selectedObject.transform.position);
                
                offset = selectedObject.transform.position - mousePosition;
            }
        }
        if(Input.GetMouseButton(0) && selectedObject)
        {
            selectedObject.transform.position = mousePosition + offset;
        }
        if (Input.GetMouseButtonUp(0) && selectedObject)
        {
            Chess c = selectedObject.GetComponent<Chess>();
            MoveGenerator generate = new MoveGenerator();
            int endloc = this.TranslateMouseToPos(mousePosition);
            Move test = new Move(startloc,endloc);
            Move[] ar = generate.GenerateMoves(1);
            bool validMove = test.Contains(ar, test);
            if(validMove)
            {
                c.SetCoords(endloc);
            } else {
                c.SetCoords(startloc);
            }
            selectedObject = null;
        }
    }
}
