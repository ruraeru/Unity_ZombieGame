using UnityEngine;

// 플레이어의 액티브 스킬을 관리하는 스크립트
public class PlayerSkill : MonoBehaviour
{
    public KeyCode skillKey = KeyCode.Q; // 스킬 사용 키
    public float cooldown = 5f; // 쿨타임 (5초)
    private float lastSkillTime; // 마지막 사용 시점

    public float shockwaveRadius = 7f; // 충격파 반경
    public float shockwaveDamage = 50f; // 충격파 데미지
    public float knockbackForce = 15f; // 밀쳐내는 힘
    public LayerMask zombieLayer; // 좀비 레이어

    public ParticleSystem shockwaveEffect; // 충격파 시각 효과
    public AudioClip shockwaveSound; // 충격파 소리 효과
    private AudioSource audioSource; // 소리 재생용 컴포넌트

    private void Awake()
    {
        // 소리 재생을 위한 AudioSource 가져오기 (없으면 추가)
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }
    }

    private void Update()
    {
        // 쿨타임 계산
        float timeSinceLastSkill = Time.time - lastSkillTime;
        float cooldownRemaining = Mathf.Max(0, cooldown - timeSinceLastSkill);

        // UI 업데이트
        if (UIManager.instance != null)
        {
            UIManager.instance.UpdateSkillUI(cooldownRemaining);
        }

        // 스킬 사용 입력 및 쿨타임 확인
        if (Input.GetKeyDown(skillKey) && cooldownRemaining <= 0)
        {
            CastShockwave();
            lastSkillTime = Time.time;
        }
    }

    private void CastShockwave()
    {
        // 시각 효과 재생
        if (shockwaveEffect != null)
        {
            // 새로운 이펙트 복사본 생성 (바닥에 묻히지 않게 높이를 1.0f로 상향)
            ParticleSystem effectInstance = Instantiate(shockwaveEffect, transform.position + Vector3.up * 1.0f, Quaternion.identity);
            
            // 모든 카메라가 볼 수 있는 Default 레이어로 강제 설정
            effectInstance.gameObject.layer = 0; 
            
            effectInstance.gameObject.SetActive(true);
            effectInstance.Play();
            
            Destroy(effectInstance.gameObject, 2f);
        }

        // 소리 효과 재생
        if (shockwaveSound != null)
        {
            audioSource.PlayOneShot(shockwaveSound);
        }

        // 주변 좀비들 검색
        Collider[] colliders = Physics.OverlapSphere(transform.position, shockwaveRadius, zombieLayer);

        foreach (Collider collider in colliders)
        {
            Zombie zombie = collider.GetComponent<Zombie>();
            if (zombie != null)
            {
                // 좀비를 플레이어 반대 방향으로 밀쳐냄
                Vector3 knockbackDir = (zombie.transform.position - transform.position).normalized;
                knockbackDir.y = 0; // 바닥 수평 방향으로만 밀침

                // 데미지 입히기
                zombie.OnDamage(shockwaveDamage, zombie.transform.position, -knockbackDir);
                
                // 넉백 적용
                zombie.ApplyKnockback(knockbackDir, knockbackForce);
            }
        }
    }

    // 에디터에서 범위를 시각적으로 확인하기 위함
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, shockwaveRadius);
    }
}
