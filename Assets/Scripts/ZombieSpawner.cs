using System.Collections.Generic;
using UnityEngine;

// 좀비 게임 오브젝트를 주기적으로 생성
public class ZombieSpawner : MonoBehaviour
{
    // 좀비 프리팹과 데이터를 한 쌍으로 관리하기 위한 구조체
    [System.Serializable]
    public struct ZombieType
    {
        public Zombie prefab; // 생성할 프리팹
        public ZombieData data; // 적용할 데이터
    }

    public ZombieType[] zombieTypes; // 생성 가능한 좀비 종류들
    public Transform[] spawnPoints; // 좀비 AI를 소환할 위치들

    private List<Zombie> zombies = new List<Zombie>(); // 생성된 좀비들을 담는 리스트
    private int wave; // 현재 웨이브

    private void Update()
    {
        // 게임 오버 상태일때는 생성하지 않음
        if (GameManager.instance != null && GameManager.instance.isGameover)
        {
            return;
        }

        // 좀비를 모두 물리친 경우 다음 스폰 실행
        if (zombies.Count <= 0)
        {
            SpawnWave();
        }

        // UI 갱신
        UpdateUI();
    }

    // 웨이브 정보를 UI로 표시
    private void UpdateUI()
    {
        // 현재 웨이브와 남은 적 수 표시
        UIManager.instance.UpdateWaveText(wave, zombies.Count);
    }

    // 현재 웨이브에 맞춰 좀비들을 생성
    private void SpawnWave()
    {
        wave++; // 웨이브 증가

        //현재 웨이브 * 1.5에 반올림한 개수만큼 좀비 생성
        int spawnCount = Mathf.RoundToInt(wave * 1.5f);

        // spawnCount만큼 좀비 생성
        for (int i = 0; i < spawnCount; i++)
        {
            CreateZombie();
        }
    }

    // 좀비를 생성하고 생성한 좀비에게 추적할 대상을 할당
    private void CreateZombie()
    {
        if (zombieTypes == null || zombieTypes.Length == 0) return;

        // 사용할 좀비 타입 랜덤으로 결정
        ZombieType selectedType = zombieTypes[Random.Range(0, zombieTypes.Length)];

        // 생성할 위치를 랜덤으로 결정
        Transform spawnPoint = spawnPoints[Random.Range(0, spawnPoints.Length)];

        // 선택된 프리팹으로부터 좀비 생성
        Zombie zombie = Instantiate(selectedType.prefab, spawnPoint.position, spawnPoint.rotation);

        // 생성한 좀비의 능력치 설정
        zombie.Setup(selectedType.data);

        // 생성된 좀비를 리스트에 추가
        zombies.Add(zombie);

        // 좀비의 onDeath 이벤트에 익명 메서드 등록
        // 사망한 좀비를 리스트에서 제거
        zombie.onDeath += () => zombies.Remove(zombie);
        // 사망한 좀비를 10초 뒤에 파괴
        zombie.onDeath += () => Destroy(zombie.gameObject, 10f);
        // 좀비 사망 시 점수 추가
        zombie.onDeath += () => GameManager.instance.AddScore(100);
    }
}