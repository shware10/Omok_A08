using UnityEngine;

public class BoardView : MonoBehaviour, IBoardStateListener
{
    public Cell[,] cells;

    int N = 15;

    void Start()
    {
        Cell[] cellArray = transform.GetComponentsInChildren<Cell>();
        cells = new Cell[N, N];

        for (int i = 0; i < cellArray.Length; ++i)
        {
            cells[(i / N), (i % N)] = cellArray[i];
            cellArray[i].X = i / N;
            cellArray[i].Y = i % N;
        }
    }

    public void OnBoardChanged(int x, int y, StoneState curStone)
    {
        cells[x, y].ActivateStone(curStone);
    }
}

