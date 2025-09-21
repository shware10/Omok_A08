using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class UIManager_AI : MonoBehaviour, ITurnStateListener, IGameStateListener
{
    [Header("Turn")]
    [SerializeField] private GameObject current_black;
    [SerializeField] private GameObject current_white;
    [SerializeField] private TextMeshProUGUI player_text_black;
    [SerializeField] private TextMeshProUGUI player_text_white;


    [Header("Result Panel")]
    [SerializeField] private GameObject win_or_lose_panel;
    [SerializeField] private GameObject win_black_image;
    [SerializeField] private GameObject win_white_image;
    [SerializeField] private TextMeshProUGUI win_state_text;

    [SerializeField] private GameObject draw_panel;

    [Header("StoneSelect")]
    [SerializeField] private Button blackButton;
    [SerializeField] private Button whiteButton;

    [Header("Difficulty")]
    [SerializeField] private Button easyButton;
    [SerializeField] private Button normalButton;
    [SerializeField] private Button hardButton;

    [Header("Start / Exit BTN")]
    [SerializeField] private Button exit_btn;

    [Header("Canvas Group")]
    [SerializeField] private CanvasGroup stoneCG;
    [SerializeField] private CanvasGroup difficultyCG;

    public delegate void StoneSelectedEvent(StoneState selectedStone);
    public event StoneSelectedEvent OnStoneSelected;

    public delegate void DifficultySelectedEvent(int difficulty);
    public event DifficultySelectedEvent OnDifficultySelected;

    void Start()
    {
        var SSListeners = FindObjectsByType<MonoBehaviour>(FindObjectsInactive.Include, FindObjectsSortMode.None)
                  .OfType<IStoneSelectListener>();
        foreach (var listener in SSListeners) OnStoneSelected += listener.OnStoneSelected;

        var DSListeners = FindObjectsByType<MonoBehaviour>(FindObjectsInactive.Include, FindObjectsSortMode.None)
                  .OfType<IDifficultySelectListener>();
        foreach (var listener in DSListeners) OnDifficultySelected += listener.OnDifficultySelected;

        blackButton.onClick.AddListener(() => OnStoneClicked(StoneState.Black));
        whiteButton.onClick.AddListener(() => OnStoneClicked(StoneState.White));

        easyButton.onClick.AddListener(() => OnDifficultyClicked(2));
        normalButton.onClick.AddListener(() => OnDifficultyClicked(3));
        hardButton.onClick.AddListener(() => OnDifficultyClicked(4));

        exit_btn.onClick.AddListener(OnExitBtn);

        Init();
    }

    void ActivateCG(CanvasGroup cg, bool activate)
    {
        cg.alpha = activate ? 1 : 0;
        cg.blocksRaycasts = activate;
    }

    void Init()
    {
        ActivateCG(stoneCG, true);
        ActivateCG(difficultyCG, false);
    }

    void OnStoneClicked(StoneState stone)
    {
        OnStoneSelected?.Invoke(stone);
        ActivateCG(stoneCG, false);
        ActivateCG(difficultyCG, true);
    }

    void OnDifficultyClicked(int difficulty)
    {
        OnDifficultySelected?.Invoke(difficulty);
        ActivateCG(difficultyCG, false);
    }

    private void OnExitBtn()
    {
        SceneManager.LoadScene(0);
    }

    public void OnTurnChanged(StoneState curStone)
    {
        Debug.Log(curStone.ToString());
        bool isBlack = curStone == StoneState.Black;
        current_black.SetActive(isBlack);
        current_white.SetActive(!isBlack);
    }

    public void OnStateChanged(GameState state)
    {
        switch (state)
        {
            case GameState.BlackWin:
                win_or_lose_panel.SetActive(true);
                win_black_image.SetActive(true);
                win_white_image.SetActive(false);
                break;
            case GameState.WhiteWin:
                win_or_lose_panel.SetActive(true);
                win_black_image.SetActive(false);
                win_white_image.SetActive(true);
                break;
            case GameState.Draw:
                draw_panel.SetActive(true);
                break;
        }
    }
}

