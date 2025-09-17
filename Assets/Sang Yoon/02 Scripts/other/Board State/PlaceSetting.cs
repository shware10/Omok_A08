//using System.Collections;
//using Unity.Mathematics;
//using UnityEngine;

//public class PlaceSetting : MonoBehaviour
//{
//    //public enum Cell { Empty, SelectPos, Black, White }
//    //public Cell cell;
//    public enum Phase { BlackTurn, WhiteTurn, EndTurn }
//    public Phase phase;

//    //public const int boardSize = 15; // 오목 대회 좌,우 표준 규격
//    //private int row, col;

//    private Camera cam;
//    private GameObject selectPos;
//    [SerializeField] private GameObject selectPosPrefab;




//    private void Awake()
//    {
//        //Cell[,] board = new Cell[boardSize, boardSize];
//        //phase = Phase.PosSelectTurn;
//    }
//    private void Start()
//    {
//        //selectPos = Instantiate(selectPosPrefab);
//    }

//    private void Update()
//    {
//        switch (phase)
//        {
//            case Phase.PosSelectTurn:
//                ChangeTurn(Cell.SelectPos);
//                break;
//            case Phase.BlackTurn:
//                ChangeTurn(Cell.Black);
//                break;
//            case Phase.WhiteTurn:
//                ChangeTurn(Cell.White);
//                break;
//            case Phase.EndTurn:

//                break;
//        }
//    }


//    private void ChangeTurn(Cell turnState)
//    {
//        OnClick();
//    }

//    private void OnClick()
//    {
//        if (cell == Cell.Empty && Input.GetMouseButtonUp(0))
//        {
//            CreatePositionSelector("Position_Selector");

//            if (cell == Cell.SelectPos && phase == Phase.BlackTurn && Input.GetMouseButtonUp(0))
//            {
//                CreatePositionSelector("Black_Rock");
//            }
//        }
//    }

//    public void OnCellClicked(Vector2Int idx, Vector3 cellPos)
//    {
//        selectPos = cellPos. 
//    }

//    /// <summary>
//    /// 프리팹 이름으로 오브젝트 생성
//    /// </summary>
//    /// <param name="Name"></param>
//    /// <param name="mousePos"></param>
//    /// <param name="quaternion"></param>
//    /// <returns>
//    /// "Resourses" 파일내부의 프리팹
//    /// </returns>
//    /// 
//    //public GameObject CreatePositionSelector(string Name)
//    //{
//    //    GameObject prefab = Resources.Load<GameObject>(Name);

//    //    return Instantiate(prefab);
//    //}

//    /// <summary>
//    /// 마우스 클릭 위치 World 좌표로 변환
//    /// </summary>
//    /// <param name="world"></param>
//    /// <returns></returns>
//    /// 
//    //private bool TryGetMouseWorldPos(out Vector3 world)
//    //{
//    //    cam = Camera.main;
//    //    var ray = cam.ScreenPointToRay(Input.mousePosition);

//    //    Plane plane = new Plane(Vector3.forward, Vector3.zero);
//    //    if (plane.Raycast(ray, out float dist))
//    //    {
//    //        world = ray.GetPoint(dist);
//    //        world.z = 0f;
//    //        return true;
//    //    }

//    //    world = default;
//    //    return false;
//    //}
//    private bool inBounds(int row, int col) =>
//        row <= boardSize && col <= boardSize;
//}