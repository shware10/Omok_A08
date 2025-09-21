using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class OmokManager_Multi : MonoBehaviour
{
    public static OmokManager_Multi Instance;

    [Header("Board")]
    [SerializeField] public int N = 15;
    public LayerMask cellMask;
    private Board board;
    private Cell cell;

    public Button StartButton;

    [Header("Online")]
    public bool isHost = true;
    public StoneState curTurn = StoneState.Black;                                      // 처음 시작시 흑이 시작
    public StoneState myStone = StoneState.Black;
    public int turnNo = 0;

    [Header("Util Assets")]
    [SerializeField] private GameObject x_Marker; 

    public GameState curState;

    public delegate void OnGameStateChanged(GameState state);                       //게임 상태 변경을 알릴 델리게이트
    public event OnGameStateChanged OnStateChanged;

    public delegate void OnBoardStateChanged(int x, int y, StoneState player);      //보드 착수 완료를 알릴 델리게이트
    public event OnBoardStateChanged OnBoardChanged;

    public delegate void OnFobbidenSeletedEvent();
    public event OnFobbidenSeletedEvent OnForbbidenSeleted;                         //금지된 수 착수를 알릴 델리게이트


    private List<GameObject> activeXMarkers = new List<GameObject>();
    private GameObject activeLastMarker = null;

    public void SetState(GameState newState)
    {
        curState = newState;
        OnStateChanged?.Invoke(curState);
    }

    void Awake()
    {
        cell = GetComponent<Cell>();
        Instance = this;
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

        StartButton.onClick.AddListener(HostStartGame);

        NetworkManager networkManager = NetworkManager.Instance;
        networkManager.room_create_act += (int isSuccess) => { if (isSuccess == 1) isHost = true; };
        networkManager.room_join_act += (int isSuccess) => { if (isSuccess == 1) isHost = false; };
        networkManager.move_req_act += OnMoveReq;
        networkManager.move_com_act += OnMoveCommit;
        networkManager.game_result_act += OnGameResult;
        networkManager.game_start_act += OnGameStart;
    }

    void Update()
    {
        if (curState != GameState.None) return;
        if (myStone != curTurn) return;

        if (Input.GetMouseButtonDown(0))
        {
            if (isHost)
            {
                Debug.Log("호스트 클릭");
                TryPlace_Host();
            }
            else
            {
                Debug.Log("게스트 클릭");
                TryPlace_Guest();
            }
        }
    }

    void TryPlace_Host()
    {
        if (!RayToCell(out int x, out int y)) return;
        TryHostCommit(x, y);
    }

    void TryPlace_Guest()
    {
        if (!RayToCell(out int x, out int y)) return;
        // 호스트로 요청
        NetworkManager.Instance.Send_Move_Request((byte)x, (byte)y);
    }

    /// <summary>
    /// 해당 칸이 빈칸인지 먼저 확인하기위한 기능
    /// </summary>
    /// <param name="screenPos"></param>
    bool RayToCell(out int x, out int y)
    {
        x = y = -1;

        Ray r = Camera.main.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(r, out var hit, 100f, cellMask))
        {
            Cell cell = hit.collider.GetComponent<Cell>();
            if (cell == null) return false;

            x = cell.X; y = cell.Y;
            Debug.Log($"레이캐스트 실행 x : {x} / y : {y}");
        }
        return true;
    }

    void TryHostCommit(int x, int y)
    {
        if (curTurn == StoneState.Black)
        {
            if (board.IsForbiddenMove(x, y))   //렌쥬룰 체크
            {
                OnForbbidenSeleted?.Invoke();
                Debug.Log("금수입니다.");
                return;
            }
        }

        cell.SetLastMarker(false);
        board.Place(x, y, curTurn);                    // 보드에 수를 놓으면
        cell.SetLastMarker(true);
        Debug.Log("돌 놓기 완료");
        OnBoardChanged?.Invoke(x, y, curTurn);         // 보드 뷰 업데이트
        board.ShowBoard();

        //브로드 캐스트
        ushort nextTurnNo = (ushort)(turnNo + 1);
        byte colorByte = (byte)(curTurn == StoneState.Black ? 0 : 1);
        NetworkManager.Instance.Send_Move_Commit(nextTurnNo, (byte)x, (byte)y, colorByte);
        turnNo = nextTurnNo;

        if (board.CheckWin(x, y, curTurn))
        {
            byte result = (curTurn == StoneState.Black) ? (byte)1 : (byte)0;
            SetState(curTurn == StoneState.Black ? GameState.BlackWin : GameState.WhiteWin);
            //게스트로 브로드 캐스트
            NetworkManager.Instance.Send_Game_Result(result);
            return;
        }

        if (board.IsFull())
        {
            SetState(GameState.Draw);               //승리 없이 돌이 다 찼으면 무승부
                                                    //게스트로 브로드 캐스트
            NetworkManager.Instance.Send_Game_Result((byte)3);
            Debug.Log("무승부");
            return;
        }

        curTurn = curTurn == StoneState.Black ? StoneState.White : StoneState.Black;
    }

    void OnGameResult(byte result)
    {
        if (isHost) return;

        if (result == 1) SetState(GameState.BlackWin);
        else if (result == 2) SetState(GameState.WhiteWin);
        else if (result == 3) SetState(GameState.Draw);
    }

    //호스트가 게스트 요청 수신
    void OnMoveReq(byte x, byte y)
    {
        if (!isHost) return;

        TryHostCommit(x, y);
    }

    void OnMoveCommit(ushort recvTurnNo, byte bx, byte by, byte bcolor)
    {
        if (isHost) return;

        if (recvTurnNo <= turnNo) return; //중복/역순 보호
        turnNo = recvTurnNo;

        int x = bx, y = by;
        StoneState color = (bcolor == 0) ? StoneState.Black : StoneState.White;

        board.Place(x, y, bcolor == 0 ? StoneState.Black : StoneState.White);
        OnBoardChanged?.Invoke(x, y, bcolor == 0 ? StoneState.Black : StoneState.White);

        curTurn = (bcolor == 0) ? StoneState.White : StoneState.Black;
    }

    /// <summary>
    /// 서버에 송신 호스트에서만 실행
    /// </summary>
    public void HostStartGame()
    {
        if (!isHost) return;

        bool randBool = (Random.Range(0, 2) == 0);
        myStone = randBool ? StoneState.Black : StoneState.White;

        curTurn = StoneState.Black;
        turnNo = 0;
        board = new Board(N);
        SetState(GameState.None);

        byte blackIsHost = randBool ? (byte)1 : (byte)0;

        StartButton.gameObject.SetActive(false);
        Debug.Log("호스트가 게임을 시작");

        NetworkManager.Instance.Send_Game_Start(blackIsHost);
    }

    /// <summary>
    /// 수신될 서버에서 브로드 캐스트
    /// </summary>
    /// <param name="blackIsHost"></param>
    void OnGameStart(byte blackIsHost)
    {
        if (isHost) return;

        bool hostIsBlack = (blackIsHost == 1);

        myStone = hostIsBlack ? StoneState.White : StoneState.Black; //호스트가 블랙이면 나는 화이트

        curTurn = StoneState.Black;
        turnNo = 0;
        board = new Board(N);
        SetState(GameState.None);

        StartButton.gameObject.SetActive(false);
        Debug.Log("게스트도 게임을 시작");
    }

    private void UpdateForbiddenMarkers() // 흑돌 금수 위치 마커 생성 
    {
        foreach (var marker in activeXMarkers)
        {
            Destroy(marker);
        }
        activeXMarkers.Clear();

        if (curTurn != StoneState.Black)
            return;

        for (int i = 0; i < N; i++)
        {
            for (int j = 0; j < N; j++)
            {
                if (board.IsEmpty(i, j) && board.IsForbiddenMove(i, j))
                {
                    Vector3 worldPos = BoardToWorld(i, j);
                    worldPos.y -= 2f;
                    GameObject marerInstance = Instantiate(x_Marker, worldPos, Quaternion.identity);
                    activeXMarkers.Add(marerInstance);
                }
            }
        }
    }

    private Vector3 BoardToWorld(int x, int y) // World 좌표로 변환
    {
        float cellSize = 0.5f;
        Vector3 origin = Vector3.zero;

        return origin + new Vector3(x * cellSize, y * cellSize, 0);
    }
}
