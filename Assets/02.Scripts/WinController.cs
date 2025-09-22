using Unity.Mathematics;
using Unity.VisualScripting;
using UnityEngine;

public class WinController : MonoBehaviour
{
    //0 빈칸, 1 흑돌, 2 백돌
    public enum Stone { Empty = 0, Black = 1, White = 2 };

    #region Variables
    public int[,] board = new int[15, 15];

    [Serialize] private GameObject blackPrefab;
    [Serialize] private GameObject whitePrefab;

    [Serialize] private SpriteRenderer boardSprite;

    //마우스 좌표
    private Vector2 mp; //마우스 포지션

    private int arrX = 0, arrY = 0;

    private Vector2 bottomL;
    private Vector2 bottomR;
    private Vector2 topL;
    private Vector2 topR;
    #endregion Variables

    //15x15 보드판 초기화
    public void Init()
    {
        for (int x = 0; x < 15; x++)
        {
            for (int y = 0; y < 15; y++)
            {
                board[x, y] = (int)Stone.Empty;
            }
        }
    }

    private void Start()
    {
        GetBoardCorners(boardSprite);
    }

    //업데이트 함수가 아니라 스프라이트에서 마우스를 클릭 했을 때로
    //수정 필요 OnMouseDonw()? //스프라이트에 스크립트를 넣으면 가능할지도 ★★★★★★★★★★★★★★★★★★★★★★★★★
    //위에게 되면 예외 처리 할 필요가 없음 ★★★★★★★★★★★★★★★★★★★★★★★★★
    private void Update()
    {
        //좌표 비교를 위한 마우스위치 값 가져오기
        if (Input.GetMouseButtonDown(0))
        {
            mp = Input.mousePosition;
        }
    }

    //보드판 외부 클릭시 예외처리를 위한 좌표 저장
    void GetBoardCorners(SpriteRenderer sr)
    {
        //Bound 함수는 특정 오브젝트의 외곽선과 관련된 연산을 함
        Bounds bounds = sr.bounds;

        bottomL = new Vector2(bounds.min.x, bounds.min.y);
        bottomR = new Vector2(bounds.max.x, bounds.min.y);
        topL = new Vector2(bounds.min.x, bounds.max.y);
        topR = new Vector2(bounds.max.x, bounds.max.y);
    }

    
    //보드 내부의 좌표로 해야 오류가 안날 듯
    //마우스 좌표  ==>  보드 좌표로 변환 
    //보드판의 좌표 크기가 정수형으로 딱 떨어져야 되는...(정수 좌표에 돌을 놓음.)
    public bool WorldToBoard(Vector2 worldPos, out Vector2 boardPos)
    {
        //float 좌표를 Int좌표로 (정수값으로)
        arrX = Mathf.RoundToInt(worldPos.x);
        arrY = Mathf.RoundToInt(worldPos.y); // 또는 z 사용

        boardPos = new Vector2(arrX, arrY);

        //데이터를 저장하기 위한 Clamp사용
        arrX = Mathf.Clamp(arrX, 0, 14);
        arrY = Mathf.Clamp(arrY, 0, 14);

        //마우스가 판을 벗어나거나 || 배열 데이터가 empty가 아닌 좌표라면 false
        if (board[arrX, arrY] != 0 && (mp.x < bottomL.x || mp.x > bottomR.x || mp.y < bottomL.y || mp.y > topL.y))
        {
            boardPos = default;
            return false;
        }
        return true;
    }

    //while 문으로 PlaceStone이 true를 반환할 때 까지 돌을 두게 하면 됨
    //빈 공간이 아니면 바둑돌 놓는 함수 
    bool PlaceStone(int player, Vector2 mp)
    {
        if (WorldToBoard(mp, out var boardPos))
        {
            //클릭한 월드 좌표에 바둑돌 생성
            Instantiate(player == (int)Stone.Black ? blackPrefab
                : whitePrefab, boardPos, Quaternion.identity);

            //데이터 저장
            board[arrX, arrY] = player;
        }
        //turnFlag가 true일 동안 계속이라 하면 (while)쓰면 되긴함★★★★★★★★★★★★★★★★★★★★★★★★★

        //이겼으면 윈, 아니면 턴 바꾸기
        CheckWin(arrX, arrY, player);
        
        return true;
    }

    //승리 판정 함수 (가로세로, 대각선 체크 ==> 합산이 5면 승리)
    bool CheckWin(int x, int y, int player)
    {
        Vector2Int[] directions =
        {
            new (1, 0),
            new (0, 1),
            new (1, 1),
            new (-1, 1)
        };
        foreach (var dir in directions)
        {
            int count = 1;
            count += CountStones(x, y, dir.x, dir.y, player);
            count += CountStones(x, y, -dir.x, -dir.y, player);

            if (count >= 5)
            {
                Debug.Log($"Player {player} Wins!");
                //GameWin(player);
                return true;
            }
            else
            {
                //SwitchTurn();
            }
        }
        return false;
    }

    //한 쪽 방향으로만 카운트를 세서 CountStones를 호출한 함수 CheckWin의
    //count를 올려줌 (총 2번) 
    int CountStones(int x, int y, int dx, int dy, int player)
    {
        int cnt = 0;
        int nx = x + dx;
        int ny = y + dy;

        //player돌이 이어진 동안, 이어졌는지 체크하면서 cnt++
        while (nx >= 0 && nx < 15 && ny >= 0 && ny < 15 && board[nx, ny] == player)
        {
            cnt++;
            nx += dx;
            ny += dy;
        }
        return cnt;
    }
}
