using System;
using UnityEngine;

// 점수와 게임 오버 여부를 관리하는 게임 매니저
public class GameManager : MonoBehaviour
{
    // 싱글톤 접근용 프로퍼티
    public static GameManager instance
    {
        get
        {
            // 만약 싱글톤 변수에 아직 오브젝트가 할당되지 않았다면
            if (m_instance == null)
            {
                // 씬에서 GameManager 오브젝트를 찾아 할당
                m_instance = FindObjectOfType<GameManager>();
            }

            // 싱글톤 오브젝트를 반환
            return m_instance;
        }
    }

    private static GameManager m_instance; // 싱글톤이 할당될 static 변수

    private int score = 0; // 현재 게임 점수
    public bool isGameover { get; private set; } // 게임 오버 상태
    public Gun machineGun; // 머신건 총 오브젝트

    // 콤보 관련 변수 및 이벤트
    public int currentCombo { get; private set; } // 현재 콤보 수
    public float comboTimer { get; private set; } // 콤보 유지 타이머
    public float comboDuration = 3f; // 콤보 유지 시간 (3초)
    public event Action<int> onComboChanged; // 콤보 변경 시 발생할 이벤트
    public event Action onComboReset; // 콤보 초기화 시 발생할 이벤트

    private void Awake()
    {
        // 씬에 싱글톤 오브젝트가 된 다른 GameManager 오브젝트가 있다면
        if (instance != this)
        {
            // 자신을 파괴
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        // 플레이어 캐릭터의 사망 이벤트 발생시 게임 오버
        FindObjectOfType<PlayerHealth>().onDeath += EndGame;
    }

    private void Update()
    {
        // 콤보 타이머 처리
        if (!isGameover && currentCombo > 0)
        {
            comboTimer -= Time.deltaTime;
            if (comboTimer <= 0)
            {
                ResetCombo();
            }
        }
    }

    // 콤보 추가
    public void AddCombo()
    {
        if (isGameover) return;

        currentCombo++;
        comboTimer = comboDuration; // 타이머 리셋

        // 콤보 변경 이벤트 알림
        if (onComboChanged != null)
        {
            onComboChanged(currentCombo);
        }
    }

    // 콤보 초기화
    private void ResetCombo()
    {
        currentCombo = 0;
        if (onComboReset != null)
        {
            onComboReset();
        }
    }

    // 점수를 추가하고 UI 갱신
    public void AddScore(int newScore)
    {
        // 게임 오버가 아닌 상태에서만 점수 증가 가능
        if (!isGameover)
        {
            // 점수 추가
            score += newScore;
            // 점수 UI 텍스트 갱신
            UIManager.instance.UpdateScoreText(score);
        }
    }

    // 게임 오버 처리
    public void EndGame()
    {
        // 게임 오버 상태를 참으로 변경
        isGameover = true;

        // 점수 기록 업데이트 및 저장
        UpdateRanking();

        // 게임 오버 UI를 활성화
        UIManager.instance.SetActiveGameoverUI(true);
    }

    // 상위 5개 점수 기록 업데이트 및 저장
    private void UpdateRanking()
    {
        int[] topScores = new int[5];

        // 기존 랭킹 불러오기
        for (int i = 0; i < 5; i++)
        {
            topScores[i] = PlayerPrefs.GetInt("HighScore" + i, 0);
        }

        // 현재 점수를 랭킹에 삽입
        for (int i = 0; i < 5; i++)
        {
            if (score > topScores[i])
            {
                // 하위 기록들 한 칸씩 밀어내기
                for (int j = 4; j > i; j--)
                {
                    topScores[j] = topScores[j - 1];
                }

                // 새로운 기록 삽입
                topScores[i] = score;
                break;
            }
        }

        // 랭킹 저장
        for (int i = 0; i < 5; i++)
        {
            PlayerPrefs.SetInt("HighScore" + i, topScores[i]);
        }
        PlayerPrefs.Save();
    }
}