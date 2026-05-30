using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement; // 씬 관리자 관련 코드
using UnityEngine.UI; // UI 관련 코드

// 필요한 UI에 즉시 접근하고 변경할 수 있도록 허용하는 UI 매니저
public class UIManager : MonoBehaviour
{
    // 싱글톤 접근용 프로퍼티
    public static UIManager instance
    {
        get
        {
            if (m_instance == null)
            {
                m_instance = FindObjectOfType<UIManager>();
            }

            return m_instance;
        }
    }

    private static UIManager m_instance; // 싱글톤이 할당될 변수

    public Text ammoText; // 탄약 표시용 텍스트
    public Text scoreText; // 점수 표시용 텍스트
    public Text waveText; // 적 웨이브 표시용 텍스트
    public GameObject gameoverUI; // 게임 오버시 활성화할 UI 
    public Text rankingText; // 랭킹 표시용 텍스트 (새로 추가)
    public Text comboText; // 콤보 표시용 텍스트 (새로 추가)
    public Text healthText; // 체력 표시용 텍스트 (새로 추가)
    public Text damageText; // 공격력 표시용 텍스트 (새로 추가)
    public CanvasGroup screenFadeGroup; // 화면 페이드 효과용 캔버스 그룹
    public Text skillCooldownText; // 스킬 쿨타임 표시용 텍스트 (새로 추가)

    private void Start()
    {
        // 콤보 이벤트 구독
        if (GameManager.instance != null)
        {
            GameManager.instance.onComboChanged += UpdateComboUI;
            GameManager.instance.onComboReset += ResetComboUI;
        }

        // 초기 상태에서는 콤보 텍스트 숨기기
        if (comboText != null) comboText.gameObject.SetActive(false);
    }

    // 콤보 UI 갱신
    public void UpdateComboUI(int combo)
    {
        if (comboText == null) return;

        comboText.gameObject.SetActive(true);
        string message = combo + " COMBO!";

        // 버프 단계별 메시지 추가 (5배수)
        if (combo >= 25) message += "\n<color=yellow>GOD MODE!</color>";
        else if (combo >= 20) message += "\n<color=red>VAMPIRISM!</color>";
        else if (combo >= 15) message += "\n<color=orange>POWER UP!</color>";
        else if (combo >= 10) message += "\n<color=red>BERSERKER!</color>";
        else if (combo >= 5) message += "\n<color=cyan>ADRENALINE!</color>";

        comboText.text = message;
    }

    // 콤보 UI 초기화
    public void ResetComboUI()
    {
        if (comboText != null)
        {
            comboText.gameObject.SetActive(false);
        }
    }

    // 체력 텍스트 갱신
    public void UpdateHealthText(float health, float maxHealth)
    {
        if (healthText != null)
        {
            healthText.text = "HP : " + Mathf.FloorToInt(health) + " / " + Mathf.FloorToInt(maxHealth);
        }
    }

    // 공격력 텍스트 갱신
    public void UpdateDamageText(float damage)
    {
        if (damageText != null)
        {
            damageText.text = "DMG : " + damage;
        }
    }

    // 스킬 UI 갱신
    public void UpdateSkillUI(float cooldownRemaining)
    {
        if (skillCooldownText == null) return;

        if (cooldownRemaining > 0)
        {
            skillCooldownText.text = "Q : " + cooldownRemaining.ToString("F1") + "s";
            skillCooldownText.color = Color.red;
        }
        else
        {
            skillCooldownText.text = "Q : READY";
            skillCooldownText.color = Color.cyan;
        }
    }

    // 탄약 텍스트 갱신
    public void UpdateAmmoText(int magAmmo, int remainAmmo)
    {
        ammoText.text = magAmmo + "/" + remainAmmo;
    }

    // 점수 텍스트 갱신
    public void UpdateScoreText(int newScore)
    {
        scoreText.text = "Score : " + newScore;
    }

    // 적 웨이브 텍스트 갱신
    public void UpdateWaveText(int waves, int count)
    {
        waveText.text = "Wave : " + waves + "\nEnemy Left : " + count;
    }

    // 게임 오버 UI 활성화
    public void SetActiveGameoverUI(bool active)
    {
        gameoverUI.SetActive(active);

        // 게임 오버 UI가 활성화될 때 랭킹 텍스트 갱신
        if (active && rankingText != null)
        {
            string rankString = "--- TOP 5 RANKING ---\n";
            for (int i = 0; i < 5; i++)
            {
                int rankScore = PlayerPrefs.GetInt("HighScore" + i, 0);
                rankString += (i + 1) + "st : " + rankScore + "\n";
            }
            rankingText.text = rankString;
        }
    }

    // 게임 재시작
    public void GameRestart()
    {
        SceneManager.LoadScene("Intro");
    }

    // 화면을 서서히 어둡게 만드는 코루틴
    public IEnumerator DrawFadeScreen()
    {
        if (screenFadeGroup == null)
        {
            yield break;
        }

        float fadeDuration = 5f; // 5초 동안 아주 천천히 페이드
        float timer = 0f;

        // 페이드 시작 시 활성화 (투명한 상태로 시작)
        screenFadeGroup.gameObject.SetActive(true);
        screenFadeGroup.alpha = 0f;

        while (timer < fadeDuration)
        {
            timer += Time.deltaTime;
            screenFadeGroup.alpha = timer / fadeDuration;
            yield return null;
        }

        screenFadeGroup.alpha = 1f;
    }
}