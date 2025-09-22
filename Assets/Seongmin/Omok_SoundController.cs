using UnityEngine;
using UnityEngine.UI;
public class Omok_SoundController : MonoBehaviour
{
    public static Omok_SoundController Instance { get; private set; }

    public AudioSource audioSource;
    public AudioClip clickSound;
    public AudioClip enterSound;
    public AudioClip placeStone;

    private void Awake()
    {
        // 싱글톤 패턴
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject); // 씬 넘어가도 유지 (원하면 제거해도 됨)
    }

    // 바둑돌 두는 소리
    public void PlayStoneSound()
    {
        audioSource.PlayOneShot(placeStone);
    }

    // 버튼 클릭 소리
    public void PlayClickSound()
    {
        audioSource.PlayOneShot(clickSound);
    }

    // 마우스 올릴 때 소리
    public void PlayEnterSound()
    {
        audioSource.PlayOneShot(enterSound);
    }
}
