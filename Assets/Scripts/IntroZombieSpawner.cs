using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// 인트로 씬 전용 좀비 스포너
// 플레이어 추적 없이 무작위 위치로 배회하는 좀비들을 생성합니다.
public class IntroZombieSpawner : MonoBehaviour
{
    [System.Serializable]
    public struct ZombieType
    {
        public Zombie prefab;
        public ZombieData data;
    }

    public ZombieType[] zombieTypes; // 생성할 좀비 종류들
    public float spawnRadius = 15f; // 스폰 범위
    public float spawnInterval = 0.2f; // 스폰 간격 (대폭 감소)
    public int maxZombies = 100; // 최대 유지 좀비 수 (대폭 증가)
    public float introSpeed = 10f; // 인트로 좀비 이동 속도 (대폭 증가)
    public LivingEntity targetEntity; // 좀비들이 향할 목표 (중앙 오브젝트)

    private List<Zombie> spawnedZombies = new List<Zombie>();

    private void Start()
    {
        // 일정 주기로 스폰 시작
        StartCoroutine(SpawnLoop());
    }

    private IEnumerator SpawnLoop()
    {
        while (true)
        {
            // 최대 수보다 적을 때만 스폰
            if (spawnedZombies.Count < maxZombies)
            {
                SpawnZombie();
            }
            yield return new WaitForSeconds(spawnInterval);
        }
    }

    private void SpawnZombie()
    {
        if (zombieTypes == null || zombieTypes.Length == 0) return;

        // 랜덤하게 좀비 타입 선택
        ZombieType selectedType = zombieTypes[Random.Range(0, zombieTypes.Length)];

        // 반지름 내 랜덤 위치 계산
        Vector2 randomCircle = Random.insideUnitCircle * spawnRadius;
        Vector3 spawnPos = new Vector3(randomCircle.x, 0, randomCircle.y) + transform.position;

        // 좀비 생성
        Zombie zombie = Instantiate(selectedType.prefab, spawnPos, Quaternion.identity);
        zombie.Setup(selectedType.data);

        // 목표 설정
        if (targetEntity != null)
        {
            zombie.SetTarget(targetEntity);
        }

        // 인트로용 속도 강제 설정 (막 뛰어다니게 함)
        UnityEngine.AI.NavMeshAgent agent = zombie.GetComponent<UnityEngine.AI.NavMeshAgent>();
        if (agent != null)
        {
            agent.speed = introSpeed;
        }
        
        spawnedZombies.Add(zombie);

        // 좀비가 죽으면 리스트에서 제거 (사망 시 처리는 기존 Zombie 로직 활용)
        zombie.onDeath += () => spawnedZombies.Remove(zombie);
    }
}
