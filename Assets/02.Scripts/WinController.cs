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

    //마우스 좌표
    private Vector2 mp;
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

    private void Update()
    {
        //좌표 비교를 위한 마우스위치 값 가져오기
        if (Input.GetMouseButtonDown(0))
        {
            mp = Input.mousePosition;
        }
    }

    //마우스 좌표  ==>  보드 좌표로 변환 
    //보드판이 0,0에 있다고 가정. //아니라면 코드 수정 필요
    public Vector2Int WorldToBoard(Vector2 worldPos)
    {
        //float 좌표를 Int좌표로 (정수값으로)
        int x = Mathf.RoundToInt(worldPos.x);
        int y = Mathf.RoundToInt(worldPos.y); // 또는 z 사용

        //Clamp 사용해서 0~14로 값 고정  ||  마우스가 판을 나가면 인식 X로?
        /*
        x = Mathf.Clamp(x, 0, 14);
        y = Mathf.Clamp(y, 0, 14);
        */
        return new Vector2Int(x, y);
    }

    //빈 공간이 아니면 바둑돌 놓는 함수 
    //★이겼는지 체크만 남음 => 체크하는 함수 있음★
    bool PlaceStone(int x, int y, int player, Vector2 mp)
    {
        //empty가 아닌 곳이면 false 반환
        if (board[x, y] != 0)
            return false;

        //데이터 저장
        board[x, y] = player;

        //클릭한 좌표값 변환
        Vector2 pos = WorldToBoard(mp);

        //클릭한 좌표에 바둑돌 생성
        Instantiate(player == (int)Stone.Black ? blackPrefab 
            : whitePrefab, pos, Quaternion.identity);
         
        //이겼으면 윈, 아니면 턴 바꾸기
        if (CheckWin(x, y, player))
        {
            //GameWin(player);
        }
        else
        {
            //SwitchTurn();
        }
        return true;
    }

    //수정 필요
    //승리 판정 함수 (가로세로, 대각선 체크 ==> 합산이 5면 승리)
    bool CheckWin(int x, int y, int player)
    {
        Vector2Int[] directions =
        {
            new Vector2Int(1, 0),
            new Vector2Int(0, 1),
            new Vector2Int(1, 1),
            new Vector2Int(-1, 1)
        };
        foreach (var dir in directions)
        {
            int count = 1;
            count += CountStones(x, y, dir.x, dir.y, player);
            count += CountStones(x, y, -dir.x, -dir.y, player);

            if (count >= 5)
            {
                Debug.Log($"Player {player} Wins!");
                return true;
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
