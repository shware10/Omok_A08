using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MenuManager : MonoBehaviour
{
    [Header("Panel")]
    public GameObject MainCanvas;
    public GameObject LoginCanvas;
    public GameObject SignUpCanvas;
    public GameObject StoreCanvas;
    public GameObject LobbyCanvas;

    [Header("Player Info")]
    [SerializeField] private GameObject panel_playerinfo;
    [SerializeField] private TextMeshProUGUI user_name;

    [Header("Login Element")]
    public TMP_InputField LoginUsername;
    public TMP_InputField LoginPassword;

    [Header("SignUp Elment")]
    public TMP_InputField SignUpUsername;
    public TMP_InputField SignUpPassword;
    public TMP_InputField SignUpPasswordConfirm;

    [Header("Error Text")]
    public TextMeshProUGUI LoginWarning;
    public TextMeshProUGUI SignUpWarning;

    void Awake()
    {
        ShowLogin();
    }

    void OnEnable()
    {
        SceneManager.sceneLoaded += ExitCallBack;
    }

    void OnDisable()
    {
        SceneManager.sceneLoaded -= ExitCallBack;
    }

    void Update()
    {

    }

    private void Reset()
    {
        MainCanvas.SetActive(false);
        LoginCanvas.SetActive(false);
        SignUpCanvas.SetActive(false);
        StoreCanvas.SetActive(false);
        LobbyCanvas.SetActive(false);
        LoginWarning.gameObject.SetActive(false);
        SignUpWarning.gameObject.SetActive(false);
    }

    public void SignIn()
    {
        string username = LoginUsername.text;
        string password = LoginPassword.text;
        SigninData str = new SigninData(username, password);
        StartCoroutine(NetworkManager.Instance.Signin(str, ShowMain, SignInFalied));

    }
    public void SignUp()
    {
        string username = SignUpUsername.text;
        string password = SignUpPassword.text;
        SignupData str = new SignupData(username, password);
        StartCoroutine(NetworkManager.Instance.Signup(str, ShowLogin, SignInFalied));

    }
    public void SignInFalied(int i)
    {
        if (i == 0) LoginWarning.text = "�̸����� �߸��Ǿ����ϴ�.";
        if (i == 1) LoginWarning.text = "��й�ȣ�� �߸��Ǿ����ϴ�.";
        LoginWarning.gameObject.SetActive(true);
    }
    public void SignUpFalied(int i)
    {
        if (i == 0) SignUpWarning.text = "�̸����� �ߺ��Ǿ����ϴ�..";
        if (i == 1) SignUpWarning.text = "��й�ȣ�� �߸��Ǿ����ϴ�.";
        SignUpWarning.gameObject.SetActive(true);
    }
    public void ShowMain()
    {
        Reset();
        user_name.text = NetworkManager.Instance.player_name;
        panel_playerinfo.SetActive(true);
        MainCanvas.SetActive(true);
    }
    public void ShowLogin()
    {
        Reset();
        LoginCanvas.SetActive(true);
    }
    public void ShowSignUp()
    {
        Reset();
        SignUpCanvas.SetActive(true);
    }
    public void ShowStore()
    {
        Reset();
        StoreCanvas.SetActive(true);
    }
    public void ShowLobby()
    {
        Reset();
        LobbyCanvas.SetActive(true);
    }

    /// <summary> AI / 멀티 플레이 씬에서 다시 로비로 돌아왔을 시 , 로비를 띄우는 콜백 함수 </summary>
    private void ExitCallBack(Scene scene, LoadSceneMode mode)
    {
        if (NetworkManager.Instance.isLogin)
        {
            this.ShowMain();
        }
    }
}
