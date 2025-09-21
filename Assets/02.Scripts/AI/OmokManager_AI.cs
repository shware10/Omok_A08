using UnityEngine;
using System.Collections;
using System.Linq;

public class OmokManager_AI : MonoBehaviour,IStoneSelectListener, IDifficultySelectListener
{
    public static OmokManager_AI Instance;
    public int N = 15;

    [SerializeField] private GameState curState;

    [SerializeField] private Board_AI board;

    [SerializeField] private StoneState selectedStone;
    [SerializeField] private Player_AI player;
    [SerializeField] private OmokAI_AI ai;

    private int turn = 0;

    public delegate void OnGameStateChanged(GameState state);
    public event OnGameStateChanged OnStateChanged;

    public delegate void OnBoardStateChanged(int x, int y, StoneState curStone);
    public event OnBoardStateChanged OnBoardChanged;

    public delegate void OnTurnStateChanged(StoneState curStone);
    public event OnTurnStateChanged OnTurnChanged;

    public void SetState(GameState newState)
    {
        curState = newState;
        OnStateChanged?.Invoke(curState);
    }
    public void OnStoneSelected(StoneState selectedStone)
    {
        this.selectedStone = selectedStone;
    }

    void Awake()
    {
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

        var TSListeners = FindObjectsByType<MonoBehaviour>(FindObjectsInactive.Include, FindObjectsSortMode.None)
                  .OfType<ITurnStateListener>();
        foreach (var listener in TSListeners) OnTurnChanged += listener.OnTurnChanged;


        board = new Board_AI(N);
    }

    public void LoopStart()
    {
        SetState(GameState.None);
        StartCoroutine(nameof(GameLoop));
    }

    IEnumerator GameLoop()
    {
        while(curState == GameState.None)
        {
            if((StoneState)(turn%2) == selectedStone)
            {
                yield return player.Think(board, selectedStone, OnStone);           //플레이어 턴
            }
            else
            {
                yield return ai.Think(board, (StoneState)(turn % 2), OnStone);      //ai 턴
            }

            yield return null;
        }
    }

    void OnStone(int x, int y)
    {
        StoneState curStone = (StoneState)(turn % 2);
        if (!board.TryPlace(x, y, curStone)) return;

        OnBoardChanged?.Invoke(x, y, curStone);

        if (board.IsFive(x,y,curStone))
        {
            if(curStone == StoneState.White) SetState(GameState.WhiteWin);
            else                             SetState(GameState.BlackWin);
            return;
        }

        if(board.IsFull())
        {
            SetState(GameState.Draw);
            return;
        }

        turn++;
        OnTurnChanged?.Invoke((StoneState)(turn % 2));
    }

    public void OnDifficultySelected(int difficulty)
    {
        ai.maxDepth = difficulty;
        LoopStart();
    }
}