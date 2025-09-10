using System.Collections;
using UnityEngine;

public class PlaceSetting : MonoBehaviour
{
    public enum Cell { Empty, Black, White }
    public enum Phase { BlackTurn, WhiteTurn, EndTurn }
    public Phase phase;

    public const int boardSize = 15; // 오목 대회 좌,우 표준 규격
    private int row, col;


    private bool inBounds(int row, int col) => row <= boardSize && col <= boardSize;


    private void Awake()
    {
        Cell[,] board = new Cell[boardSize, boardSize];
        phase = Phase.BlackTurn;
    }
    private void Start()
    {
        
    }

    private void Update()
    {
        switch (phase)
        {
            case Phase.BlackTurn:
                ChangeTurn(Cell.Black);
                break;
            case Phase.WhiteTurn:
                ChangeTurn(Cell.White);
                break;
            case Phase.EndTurn:

                break;
        }        
    }

    private void OnClick()
    {

    }

    private void ChangeTurn(Cell turnState)
    {
        if (Input.GetMouseButtonUp(0)) ;
    }
}
