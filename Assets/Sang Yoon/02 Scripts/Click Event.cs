using System;
using UnityEngine;
using UnityEngine.Rendering;

public class ClickEvent : MonoBehaviour
{
    public enum CellState { Empty, Black, White}

    [Header("Board")]
    [SerializeField] int rows = 15;
    [SerializeField] int cols = 15;
    [SerializeField] float cellSize = 1f;
    [SerializeField] Transform originPos;
    [SerializeField] bool use2D = true;

    [Header("Prefabs & Parents")]
    [SerializeField] GameObject selectPosPrefeb;
    [SerializeField] GameObject blackStonePrefab;
    [SerializeField] GameObject whiteStonePrefab;
    [SerializeField] Transform stonesParent;

    [Header("Collision")]
    [SerializeField] Collider2D boardCollider2D;
    [SerializeField] Collider boardCollider3D;
    [SerializeField] LayerMask boardMask = ~0;

    private Camera cam;
    private CellState[,] board;
    private CellState turn = CellState.Black;

    private void Awake()
    {
        cam = Camera.main;
        board = new CellState[rows, cols];
        if (originPos == null) originPos = transform;
        if (stonesParent == null) stonesParent = transform;
    }

    private void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            TryPlaceAtPointer(Input.mousePosition);
        }

        if (Input.touchCount > 0)
        {
            var t = Input.GetTouch(0);
            if(t.phase == TouchPhase.Began)
                TryPlaceAtPointer(t.position);
        }
    }

    private void TryPlaceAtPointer(Vector2 screenPos)
    {
        if (!TryGetWorldPoint(screenPos, out var worldPoint))
            return;
        if (!tryGetCell(worldPoint, out var cell))
            return;

        // 렌주룰 체크 지점
    }

    private bool TryGetWorldPoint(Vector2 screenPos, out Vector3 worldPoint)
    {
        worldPoint = default; // ?

        if (use2D)
        {

            var wp = cam.ScreenToWorldPoint(new Vector3(screenPos.x, screenPos.y, 0f));
            if(boardCollider2D != null && !boardCollider2D.OverlapPoint(wp))
                return false;

            worldPoint = wp;
            return true;
        }
        else
        {
            Ray ray = cam.ScreenPointToRay(screenPos);
            if (Physics.Raycast(ray, out var hit, 1000f, boardMask))
            {
                if(boardCollider3D != null && hit.collider != boardCollider3D)
                    return false;
                worldPoint = hit.point;
                return true;
            }
            return false;
        }
    }
    private bool tryGetCell(Vector3 worldPoint, out Vector2Int cell)
    {
        Vector3 local = worldPoint - originPos.position;

        int col = Mathf.FloorToInt((local.x / cellSize) + 0.5f);
        int row = Mathf.FloorToInt((local.y / cellSize) + 0.5f);

        if (row < 0 || col < 0 || row >= rows || col >= cols)
        {
            cell = default;
            return false;
        }

        cell = new Vector2Int(row, col);
        return true;
    }
    private Vector3 CellToWorldCenter(Vector2Int cell)
    {
        float x = (cell.y + 0.5f) * cellSize;
        float y = (cell.x + 0.5f) * cellSize;
        var p = originPos.position + new Vector3(x, y, 0f);

        if (!use2D && stonesParent != null) p.z = stonesParent.position.z;

        return p;
    }

    private void PlaceStone(Vector2Int cell)
    {
        if (board[cell.x, cell.y] != CellState.Empty)
            return;

        GameObject prefab = (turn == CellState.Black) ? blackStonePrefab : whiteStonePrefab;
        var pos = CellToWorldCenter(cell);
        Instantiate(prefab, pos, Quaternion.identity, stonesParent);

        board[cell.x, cell.y] = turn;
        turn = (turn == CellState.Black) ? CellState.White : CellState.Black;
    }

    void OnDrawGizmosSelected()
    {
        if (originPos == null) return;
        Gizmos.color = new Color(1, 1, 1, 0.35f);
        for (int r = 0; r <= rows; r++)
        {
            Vector3 a = originPos.position + new Vector3(0, r * cellSize, 0);
            Vector3 b = originPos.position + new Vector3(cols * cellSize, r * cellSize, 0);
            Gizmos.DrawLine(a, b);
        }
        for (int c = 0; c <= cols; c++)
        {
            Vector3 a = originPos.position + new Vector3(c * cellSize, 0, 0);
            Vector3 b = originPos.position + new Vector3(c * cellSize, rows * cellSize, 0);
            Gizmos.DrawLine(a, b);
        }
    }
}
