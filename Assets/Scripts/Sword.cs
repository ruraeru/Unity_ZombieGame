using UnityEngine;

public class Sword : MonoBehaviour, IItem
{
    public void Use(GameObject target)
    {
        PlayerShooter playerShooter = target.GetComponent<PlayerShooter>();

        if (playerShooter != null)
        {
            Gun gun = playerShooter.gun;

            if (gun != null)
            {
                gun.damageMultiplier *= 2; // 총 자체의 데미지 배율을 2배로 증가 (영구 수정 방지)
            }

        }
        Destroy(gameObject); // 아이템 사용 후 자신을 파괴
    }
}
