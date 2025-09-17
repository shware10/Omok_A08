using UnityEngine;

public class BoardManager : MonoBehaviour
{
    [Header("Board")]
    [SerializeField] private int rows = 15;                       // 좌우 행열 크기
    [SerializeField] private int cols = 15;                       // 좌우 행열 크기
    [SerializeField] private float cellSize = 0.642f;             // 보드 칸 한 변 길이
    [SerializeField] private Transform origin;                    // 보드 좌하단기준 그리드 위치
    [SerializeField] private bool use2D = true;                   // 2D(Physics2D) / 3D(Physics) 선택

    [Header("Prefabs & Parents")]
    [SerializeField] private GameObject blackStonePrefab;
    [SerializeField] private GameObject whiteStonePrefab;
    [SerializeField] private Transform stonesParent;              // 인스턴스 프리팹 부모(흑돌, 백돌 등등)

    [Header("Collision")]
    [SerializeField] private Collider2D boardCollider2D;          // 2D일 때 할당
    [SerializeField] private Collider boardCollider3D;            // 3D일 때 할당
    [SerializeField] private LayerMask boardMask = ~0;            // 3D Raycast 용

    Camera cam;
    public StoneState[,] board;
    StoneState turn = StoneState.Black;                             // 처음 시작시 흑이 시작

    void Awake()
    {
        cam = Camera.main;
        board = new StoneState[rows, cols];
        if (origin == null) origin = transform;
        if (stonesParent == null) stonesParent = transform;

        //if (Board.Instance != null)
        //{
        //    Board.Instance.Initialize(board, rows, cols);
        //}
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
            TryPlaceAtPointer(Input.mousePosition);
    }

    /// <summary>
    /// 해당 칸이 빈칸인지 먼저 확인하기위한 기능
    /// </summary>
    /// <param name="screenPos"></param>
    void TryPlaceAtPointer(Vector2 screenPos)
    {
        // 게임이 끝나면 상호작용 없애야함

        if (!TryGetWorldPoint(screenPos, out var worldPoint))
            return;
        if (!TryGetCell(worldPoint, out var cell)) 
            return;

        if (turn == StoneState.Black)
        {
            //if (Board.Instance.IsForbiddenBlackRock(cell.x, cell.y))
            //{
            //    Debug.Log("해당 위치는 금수입니다");
            //    // 금수 텍스트 UI 로직 위치???
            //    return;
            //}
        }

        PlaceStone(cell);
    }

    /// <summary>
    /// 스크린 → 월드 좌표 변환 및 보드내부만 클릭 허용
    /// </summary>
    /// <param name="screenPos"></param>
    /// <param name="worldPoint"></param>
    /// <returns></returns>
    bool TryGetWorldPoint(Vector2 screenPos, out Vector3 worldPoint)
    {
        worldPoint = default;

        if (use2D)
        {
            // 2D : 스크린 좌표 → 월드 좌표 변환
            // 보드 콜라이더 내부만 허용
            var wp = cam.ScreenToWorldPoint(new Vector3(screenPos.x, screenPos.y, 0f));
            if (boardCollider2D != null && !boardCollider2D.OverlapPoint(wp)) 
                return false;

            worldPoint = wp;
            return true;
        }
        else
        {
            // 3D전환시에 레이케스트
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
        if (board[cell.x, cell.y] != StoneState.Empty) 
            return;

        GameObject prefab = (turn == StoneState.Black) ? blackStonePrefab : whiteStonePrefab;
        var pos = CellToWorldCenter(cell);
        Instantiate(prefab, pos, Quaternion.identity, stonesParent);

        board[cell.x, cell.y] = turn;
        turn = (turn == StoneState.Black) ? StoneState.White : StoneState.Black;

        // 승, 패 로직 추가 위치!!! (아마도 여기일겁니다...?)-------------------------------------------------------------------- ✔✔✔✔✔
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

// 모바일 터치가 필요하다면 Update문에 추가 하면됩니다.

//if (Input.touchCount > 0)
//{
//    var t = Input.GetTouch(0);
//    if (t.phase == TouchPhase.Began)
//        TryPlaceAtPointer(t.position);
//}