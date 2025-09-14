using UnityEngine;

public class BoardManager : MonoBehaviour
{
    public enum CellState { Empty, Black, White }

    [Header("Board")]
    [SerializeField] int rows = 15;
    [SerializeField] int cols = 15;
    [SerializeField] float cellSize = 0.642f;                 // 칸 한 변 길이
    [SerializeField] Transform origin;                    // 보드 좌하단 월드 좌표
    [SerializeField] bool use2D = true;                   // 2D(Physics2D) / 3D(Physics) 선택

    [Header("Prefabs & Parents")]
    [SerializeField] GameObject blackStonePrefab;
    [SerializeField] GameObject whiteStonePrefab;
    [SerializeField] Transform stonesParent;              // 인스턴스 부모(정리용, 선택)

    [Header("Collision")]
    [SerializeField] Collider2D boardCollider2D;          // 2D일 때 할당
    [SerializeField] Collider boardCollider3D;            // 3D일 때 할당
    [SerializeField] LayerMask boardMask = ~0;            // 3D Raycast 용

    Camera cam;
    CellState[,] board;
    CellState turn = CellState.Black;                     // 흑 선공

    void Awake()
    {
        cam = Camera.main;
        board = new CellState[rows, cols];
        if (origin == null) origin = transform;
        if (stonesParent == null) stonesParent = transform;
    }

    void Update()
    {
        // 마우스 클릭
        if (Input.GetMouseButtonDown(0))
            TryPlaceAtPointer(Input.mousePosition);

        // 모바일 터치
        if (Input.touchCount > 0)
        {
            var t = Input.GetTouch(0);
            if (t.phase == TouchPhase.Began)
                TryPlaceAtPointer(t.position);
        }
    }

    void TryPlaceAtPointer(Vector2 screenPos)
    {
        if (!TryGetWorldPoint(screenPos, out var worldPoint))
            return;
        if (!TryGetCell(worldPoint, out var cell)) 
            return;

        // (나중에) 렌주룰 33/44 체크 지점
        // if (!RuleValidator.IsLegal(board, cell, turn)) { ShowIllegalFx(); return; }

        PlaceStone(cell);
    }

    bool TryGetWorldPoint(Vector2 screenPos, out Vector3 worldPoint)
    {
        worldPoint = default;

        if (use2D)
        {
            // 2D(Orthographic) : 스크린→월드 변환 뒤 보드 콜라이더 내부만 허용
            var wp = cam.ScreenToWorldPoint(new Vector3(screenPos.x, screenPos.y, 0f));
            if (boardCollider2D != null && !boardCollider2D.OverlapPoint(wp)) 
                return false;

            worldPoint = wp;
            return true;
        }
        else
        {
            // 3D : 보드로 Raycast
            Ray ray = cam.ScreenPointToRay(screenPos);
            if (Physics.Raycast(ray, out var hit, 1000f, boardMask))
            {
                if (boardCollider3D != null && hit.collider != boardCollider3D) 
                    return false;

                worldPoint = hit.point;
                return true;
            }
            return false;
        }
    }

    /// <summary>
    /// 클릭 이벤트시 돌 생성 구역
    /// </summary>
    /// <param name="worldPoint"></param>
    /// <param name="cell"></param>
    /// <returns></returns>
    bool TryGetCell(Vector3 worldPoint, out Vector2Int cell)
    {
        // origin(좌하단 모서리) 기준, 셀크기 만큼 돌 생성할 구역 (클릭 이벤트 발생시)
        Vector3 local = worldPoint - origin.position;

        int col = Mathf.FloorToInt(local.x / cellSize);
        int row = Mathf.FloorToInt(local.y / cellSize);

        if (row < 0 || col < 0 || row >= rows || col >= cols)
        {
            cell = default;
            return false;
        }

        cell = new Vector2Int(row, col);
        return true;
    }

    /// <summary>
    /// 칸에서 돌이 생성되는 위치
    /// </summary>
    /// <param name="cell"></param>
    /// <returns></returns>
    Vector3 CellToWorldCenter(Vector2Int cell)
    {
        // 칸 중심 좌표가 좌 하단이므로 (0.5, 0.5) 만큼 더해서 생성 
        float x = (cell.y + 0.5f) * cellSize;
        float y = (cell.x + 0.5f) * cellSize;
        var p = origin.position + new Vector3(x, y, 0f);

        // 3D라면 원하는 z로 올려주기
        if (!use2D && stonesParent != null) p.z = stonesParent.position.z;

        return p;
    }

    #region 돌 생성
    /// <summary>
    /// 셀에 돌 색깔 번갈아 가면서 생성
    /// </summary>
    /// <param name="cell"></param>
    void PlaceStone(Vector2Int cell)
    {
        if (board[cell.x, cell.y] != CellState.Empty) 
            return;

        GameObject prefab = (turn == CellState.Black) ? blackStonePrefab : whiteStonePrefab;
        var pos = CellToWorldCenter(cell);
        Instantiate(prefab, pos, Quaternion.identity, stonesParent);

        board[cell.x, cell.y] = turn;
        turn = (turn == CellState.Black) ? CellState.White : CellState.Black;

        // 승, 패 로직 추가 위치!!! (아마도 여기일겁니다...?)
    }
    #endregion

    #region 디버그용 그리드 (육안 확인용)
    /// <summary>
    /// 디버그용 그리드 생성 (육안 확인용)
    /// </summary>
    void OnDrawGizmosSelected()
    {
        if (origin == null) return;

        Gizmos.color = new Color(1, 1, 1, 0.4f); // 그리드 기즈모 컬러 및 투명도 설정

        /* 그리드 가로, 세로 줄 그리기 */
        for (int r = 0; r <= rows; r++)
        {
            Vector3 a = origin.position + new Vector3(0, r * cellSize, 0);
            Vector3 b = origin.position + new Vector3(cols * cellSize, r * cellSize, 0);
            Gizmos.DrawLine(a, b);
        }
        for (int c = 0; c <= cols; c++)
        {
            Vector3 a = origin.position + new Vector3(c * cellSize, 0, 0);
            Vector3 b = origin.position + new Vector3(c * cellSize, rows * cellSize, 0);
            Gizmos.DrawLine(a, b);
        }
    }
    #endregion
}