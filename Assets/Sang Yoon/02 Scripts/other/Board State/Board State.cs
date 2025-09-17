using UnityEngine;

public class BoardState : MonoBehaviour
{
    public enum Cell { Empty, SelectPos, Black, White }
    public Cell cell;
    private Cell[,] board;

    public const int boardSize = 15; // 오목 대회 좌,우 표준 규격
    private int row, col;

    [SerializeField] private Camera cam;
    private Collider2D hit;
    private void Awake()
    {
        board = new Cell[boardSize, boardSize];
        hit = GetComponent<Collider2D>();
    }
    void Start()
    {

    }

    void Update()
    {
        switch (cell)
        {
            case Cell.Empty:
                HandleClick("Position_Selector");
                break;
            case Cell.SelectPos:

                //OnClick();
                break;
            case Cell.Black:

                break;
            case Cell.White:

                break;
        }
    }

    private void ChangeState()
    {
        CreatePositionSelector(name);
    }

    public void OnCellClicked(string Name)
    {
        if (cell != Cell.Empty) return;

        if (cell == Cell.Empty)
        {
            CreatePositionSelector(Name);
        }

        cell = Cell.SelectPos;
    }
    private GameObject CreatePositionSelector(string Name)
    {
        GameObject prefab = Resources.Load<GameObject>(Name);

        return Instantiate(prefab, transform.position, Quaternion.identity);
    }


    public void HandleClick(string Name)
    {
        int cellMask = LayerMask.GetMask("Cell");

        if (Input.GetMouseButtonDown(0))
        {
            Debug.Log(cellMask);
            Vector3 mousePos = Input.mousePosition;
            //Vector2 worldPos = new Vector2(mousePos.x, mousePos.y);

            hit = Physics2D.OverlapPoint(mousePos, cellMask);

            OnCellClicked("Position_Selector");

        }
    }

    private bool InBounds(int row, int col) =>
        (0 <= row && row < boardSize) && (0 <= col && col < boardSize);

    //private void OnRay(string Name)
    //{

    //    if(Input.GetMouseButtonDown(0))
    //    {
    //        Vector2 worldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
    //        Collider2D hit = Physics2D.OverlapPoint(worldPos);

    //        if (hit != null)
    //        {
    //            hit.GetComponent<BoardState>().OnCellClicked(Name);
    //        }
    //    }
    //}
}
