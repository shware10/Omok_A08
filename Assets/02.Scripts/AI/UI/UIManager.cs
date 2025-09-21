using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    public Button blackButton;
    public Button whiteButton;

    public Button easyButton;
    public Button normalButton;
    public Button hardButton;

    public CanvasGroup stoneCG;
    public CanvasGroup difficultyCG;

    public delegate void StoneSelectedEvent(StoneState selectedStone);
    public event StoneSelectedEvent OnStoneSelected;

    public delegate void DifficultySelectedEvent(int difficulty);
    public event DifficultySelectedEvent OnDifficultySelected;

    void Start()
    {
        blackButton.onClick.AddListener(OnBlackSelected);
        whiteButton.onClick.AddListener(OnWhiteSelected);

        easyButton.onClick.AddListener(OnEasyClicked);
        normalButton.onClick.AddListener(OnNormalClicked);
        hardButton.onClick.AddListener(OnHardClicked);
    }

    void OnBlackSelected()
    {
        OnStoneSelected?.Invoke(StoneState.Black);
    }

    void OnWhiteSelected()
    {
        OnStoneSelected?.Invoke(StoneState.White);
    }

    void OnEasyClicked()
    {
        OnDifficultySelected?.Invoke(2);
    }

    void OnNormalClicked()
    {
        OnDifficultySelected?.Invoke(3);
    }

    void OnHardClicked()
    {
        OnDifficultySelected?.Invoke(4);
    }

}

