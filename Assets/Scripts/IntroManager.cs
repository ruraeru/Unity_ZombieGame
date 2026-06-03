using UnityEngine;
using UnityEngine.SceneManagement; // 씬 전환을 위한 네임스페이스

public class IntroManager : MonoBehaviour
{
    public GameObject mainMenuUI;
    public GameObject mapSelectionUI;

    public void ShowMapSelection()
    {
        if (mainMenuUI != null) mainMenuUI.SetActive(false);
        if (mapSelectionUI != null) mapSelectionUI.SetActive(true);
    }

    public void ShowMainMenu()
    {
        if (mainMenuUI != null) mainMenuUI.SetActive(true);
        if (mapSelectionUI != null) mapSelectionUI.SetActive(false);
    }

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

    public void UnLimitMode()
    {
        SceneManager.LoadScene("UnLimit");
    }

    public void ExitGame()
    {
        Application.Quit();
    }
}
