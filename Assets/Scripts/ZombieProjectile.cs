using UnityEngine;

// 좀비가 발사하는 투사체 스크립트
public class ZombieProjectile : MonoBehaviour
{
    public float speed = 10f; // 투사체 속도
    public float damage = 20f; // 데미지
    public float lifeTime = 5f; // 최대 생존 시간

    private void Start()
    {
        // 일정 시간이 지나면 자동으로 파괴
        Destroy(gameObject, lifeTime);
    }

    private void Update()
    {
        // 매 프레임 앞으로 이동
        transform.Translate(Vector3.forward * speed * Time.deltaTime);
    }

    private void OnTriggerEnter(Collider other)
    {
        // 플레이어 레이어(Player)와 충돌했는지 확인
        if (other.CompareTag("Player"))
        {
            // 상대방으로부터 IDamageable 컴포넌트 가져오기 시도
            IDamageable target = other.GetComponent<IDamageable>();

            if (target != null)
            {
                // 데미지 입히기
                Vector3 hitPoint = other.ClosestPoint(transform.position);
                Vector3 hitNormal = transform.position - other.transform.position;
                target.OnDamage(damage, hitPoint, hitNormal);
            }

            // 플레이어와 충돌 후 파괴
            Destroy(gameObject);
        }
        else if (!other.CompareTag("Item") && !other.CompareTag("Zombie"))
        {
            // 아이템이나 다른 좀비가 아닌 다른 장애물과 충돌 시 파괴
            Destroy(gameObject);
        }
    }
}
