using UnityEngine;
using UnityEngine.UI; // UI 관련 코드

// 플레이어 캐릭터의 생명체로서의 동작을 담당
public class PlayerHealth : LivingEntity
{
    public Slider healthSlider; // 체력을 표시할 UI 슬라이더

    public AudioClip deathClip; // 사망 소리
    public AudioClip hitClip; // 피격 소리
    public AudioClip itemPickupClip; // 아이템 습득 소리

    private AudioSource playerAudioPlayer; // 플레이어 소리 재생기
    private Animator playerAnimator; // 플레이어의 애니메이터

    private PlayerMovement playerMovement; // 플레이어 움직임 컴포넌트
    private PlayerShooter playerShooter; // 플레이어 슈터 컴포넌트

    private void Awake()
    {
        // 사용할 컴포넌트를 가져오기
        playerAnimator = GetComponent<Animator>();
        playerAudioPlayer = GetComponent<AudioSource>();

        playerMovement = GetComponent<PlayerMovement>();
        playerShooter = GetComponent<PlayerShooter>();
    }

    protected override void OnEnable()
    {
        // LivingEntity의 OnEnable() 실행 (상태 초기화)
        base.OnEnable();

        healthSlider.gameObject.SetActive(true); // 체력 슬라이더 활성화
        healthSlider.maxValue = startingHealth; // 슬라이더 최대값 설정
        healthSlider.value = health; // 슬라이더 현재값 설정

        playerMovement.enabled = true; // 플레이어 움직임 활성화
        playerShooter.enabled = true; // 플레이어 슈터 활성화

        UpdateUI();
    }

    // 체력 UI 텍스트 갱신
    private void UpdateUI()
    {
        if (UIManager.instance != null)
        {
            UIManager.instance.UpdateHealthText(health, startingHealth);
        }
    }

    // 체력 회복
    public override void RestoreHealth(float newHealth)
    {
        // LivingEntity의 RestoreHealth() 실행 (체력 증가)
        base.RestoreHealth(newHealth);

        healthSlider.value = health; // 슬라이더 현재값 갱신
        UpdateUI();
    }

    // 데미지 처리
    public override void OnDamage(float damage, Vector3 hitPoint, Vector3 hitDirection)
    {
        if (!dead)
        {
            playerAudioPlayer.PlayOneShot(hitClip); // 피격 소리 재생
        }

        // LivingEntity의 OnDamage() 실행(데미지 적용)
        base.OnDamage(damage, hitPoint, hitDirection);

        healthSlider.value = health; // 슬라이더 현재값 갱신
        UpdateUI();
    }

    // 사망 처리
    public override void Die()
    {
        // LivingEntity의 Die() 실행(사망 적용)
        base.Die();

        healthSlider.gameObject.SetActive(false); // 체력 슬라이더 비활성화

        playerAudioPlayer.PlayOneShot(deathClip); // 사망 소리 재생
        playerAnimator.SetTrigger("Die"); // 사망 애니메이션 재생

        playerMovement.enabled = false; // 플레이어 움직임 비활성화
        playerShooter.enabled = false; // 플레이어 슈터 비활성화
    }

    private void OnTriggerEnter(Collider other)
    {
        // 아이템과 충돌한 경우 해당 아이템을 사용하는 처리
        if (!dead)
        {
            IItem item = other.GetComponent<IItem>(); // 충돌한 오브젝트에서 IItem 인터페이스 가져오기 시도

            // 충돌한 상대방으로부터 IItem 인터페이스가 있다면
            if (item != null)
            {
                item.Use(gameObject); // 아이템 사용, 자신을 전달
                playerAudioPlayer.PlayOneShot(itemPickupClip); // 아이템 습득 소리 재생
            }
        }
    }
}