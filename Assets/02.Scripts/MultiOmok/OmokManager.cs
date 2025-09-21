using NUnit.Framework;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class OmokManager : MonoBehaviour
{
    public static OmokManager Instance;

    [Header("Board")]
    [SerializeField] private Transform boardOriginPoint;
    [SerializeField] public int N = 15;

    public LayerMask cellMask;

    Board board;
    public StoneState turn = StoneState.Black;                                      // 처음 시작시 흑이 시작

    public GameState curState;

    public delegate void OnGameStateChanged(GameState state);                       //게임 상태 변경을 알릴 델리게이트
    public event OnGameStateChanged OnStateChanged;

    public delegate void OnBoardStateChanged(int x, int y, StoneState player);      //보드 착수 완료를 알릴 델리게이트
    public event OnBoardStateChanged OnBoardChanged;

    public delegate void OnFobbidenSeletedEvent();
    public event OnFobbidenSeletedEvent OnForbbidenSeleted;                         //금지된 수 착수를 알릴 델리게이트

    [Header("Util Assets")]
    [SerializeField] private GameObject x_Marker;
    [SerializeField] private GameObject last_Marker;

    private List<GameObject> activeXMarkers = new List<GameObject>();
    private GameObject activeLastMarker = null;
    public void SetState(GameState newState)
    {
        curState = newState;
        OnStateChanged?.Invoke(curState);
    }

    void Awake()
    {
        board = new Board(N);
        SetState(GameState.None);
    }

    void Start()
    {
        //delegate 구독
        var BSListeners = FindObjectsByType<MonoBehaviour>(FindObjectsInactive.Include, FindObjectsSortMode.None)
                          .OfType<IBoardStateListener>();
        foreach (var listener in BSListeners) OnBoardChanged += listener.OnBoardChanged;

        var GSListeners = FindObjectsByType<MonoBehaviour>(FindObjectsInactive.Include, FindObjectsSortMode.None)
                          .OfType<IGameStateListener>();
        foreach (var listener in GSListeners) OnStateChanged += listener.OnStateChanged;

        var FSListeners = FindObjectsByType<MonoBehaviour>(FindObjectsInactive.Include, FindObjectsSortMode.None)
                  .OfType<IForbiddenSelectListener>();
        foreach (var listener in FSListeners) OnForbbidenSeleted += listener.OnForbbidenSeleted;
    }

    void Update()
    {
        if (curState == GameState.None)
        {
            if (Input.GetMouseButtonDown(0)) TryPlace();
        }
    }

    /// <summary>
    /// 해당 칸이 빈칸인지 먼저 확인하기위한 기능
    /// </summary>
    /// <param name="screenPos"></param>
    void TryPlace()
    {
        Ray r = Camera.main.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(r, out var hit, 100f, cellMask))
        {
            Cell cell = hit.collider.GetComponent<Cell>(); //cell이 없는 

            if (cell != null)
            {
                OnStone(cell.X, cell.Y);
            }
        }
    }

    void OnStone(int x, int y)
    {
        if (!board.IsEmpty(x, y))
        {
            Debug.Log("이미 돌이 놓여져있는 자리입니다");
            return;
        }
        if (turn == StoneState.Black)
        {
            if (board.IsForbiddenMove(x, y))   //렌쥬룰 체크
            {
                OnForbbidenSeleted?.Invoke();
                Debug.Log("금수입니다.");
                return;
            }
        }

        board.Place(x, y, turn);                    // 보드에 수를 놓으면
        Debug.Log("돌 놓기 완료");
        UpdateLastMarker(x, y);                     // 마지막 착수 지점 마커 업데이트
        OnBoardChanged?.Invoke(x, y, turn);         // 보드 뷰 업데이트
        board.ShowBoard();

        if (board.CheckWin(x, y, turn))
        {
            if (turn == StoneState.White)           //흰색 승리
            {
                SetState(GameState.WhiteWin);
                Debug.Log("백 승리");
                return;
            }
            else if (turn == StoneState.Black)      //검은색 승리
            {
                SetState(GameState.BlackWin);
                Debug.Log("흑 승리");
                return;
            }
        }

        if (board.IsFull())
        {
            SetState(GameState.Draw);               //승리 없이 돌이 다 찼으면 무승부
            Debug.Log("무승부");
            return;
        }

        turn = turn == StoneState.Black ? StoneState.White : StoneState.Black;

        UpdateForbiddenMarkers();
    }


    private void UpdateLastMarker(int x, int y) // 마지막 착수한 돌 위치 마커 생성 
    {

        Vector3 worldPos = BoardToWorld(x, y);
        worldPos.z = -2f;
        activeLastMarker = Instantiate(last_Marker, worldPos, boardOriginPoint.rotation);
    }

    private void UpdateForbiddenMarkers() // 흑돌 금수 위치 마커 생성 
    {
        foreach (var marker in activeXMarkers)
        {
            Destroy(marker);
        }
        activeXMarkers.Clear();

        if (turn != StoneState.Black)
            return;

        /* 15x15 좌표에 금수로 지정될 부분에 전부 금수 마커 생성 */
        for (int i = 0; i < N; i++)
        {
            for (int j = 0; j < N; j++)
            {
                if (board.IsEmpty(i, j) && board.IsForbiddenMove(i, j))
                {
                    Vector3 worldPos = BoardToWorld(i, j);
                    worldPos.z = -2f;
                    GameObject markerInstance = Instantiate(x_Marker, worldPos, boardOriginPoint.rotation);
                    activeXMarkers.Add(markerInstance);
                }
            }
        }
    }
    private Vector3 BoardToWorld(int x, int y) // World 좌표로 변환
    {
        float cellSize = 0.5f;
        Vector3 origin = boardOriginPoint.position;

        return origin + new Vector3(x * cellSize, y * cellSize, 0);
    }
}