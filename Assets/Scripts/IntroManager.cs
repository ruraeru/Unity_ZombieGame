using UnityEngine;
using UnityEngine.SceneManagement; // 씬 전환을 위한 네임스페이스

public class IntroManager : MonoBehaviour
{
    // 오리지널 메인 씬 (Main) 로드
    public void LoadMainMode()
    {
        SceneManager.LoadScene("Main");
    }

    // 후쿠오카 맵 (Fukuoka_map) 로드
    public void LoadFukuokaMode()
    {
        SceneManager.LoadScene("Fukuoka_map");
    }
    public void ReStartGame()
    {
        SceneManager.LoadScene("Intro");
    }

    public void ExitGame()
    {
        Application.Quit();
    }
}
