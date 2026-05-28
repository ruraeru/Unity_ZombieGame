using UnityEngine;
using UnityEngine.SceneManagement; // 씬 전환을 위한 네임스페이스

public class IntroManager : MonoBehaviour
{
    public void StartGame()
    {
        // "Main" 이라는 이름의 씬으로 전환
        SceneManager.LoadScene("Main");
    }
    public void ReStartGame()
    {
        SceneManager.LoadScene("Intro");
    }
}
