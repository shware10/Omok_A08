using Unity.VisualScripting;
using UnityEngine;

public class RenjuRule : MonoBehaviour
{
    // 팀원이 쉽게 렌주룰에 접근 가능하도록 싱글톤으로 만들어두었음!!
    public static RenjuRule Instance { get; private set; }

    private int rows;
    private int cols;

    /* 8방향 탐색을 위한 백터 (상, 하, 좌, 우, 좌상, 우상, 우하, 좌하) */
    /*  상, 우상, 좌, 좌하 4개로 각각 -1을 곱해서 음수로 만들면 반대방향 백터 4개를 또 만들수 있으므로 4개만 선언하였음 */
    private readonly int[,] directions = { { 0, 1 }, { 1, 1 }, { 1, 0 }, { 1, -1 } };

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // 씬 변경시 대상 인스턴스 객체 파괴 방지
        }
        else
        {
            Destroy(gameObject);
        }
    }

    /// <summary>
    /// 행과 열을 초기화
    /// </summary>
    public void Initialize()
    {

    }

}
