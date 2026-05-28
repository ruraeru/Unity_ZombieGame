using UnityEngine;

public class MachineGun : MonoBehaviour, IItem
{
    public void Use(GameObject target)
    {
        // 전달 받은 게임 오브젝트로부터 PlayerShooter 컴포넌트를 가져오기 시도
        PlayerShooter playerShooter = target.GetComponent<PlayerShooter>();

        // PlayerShooter 컴포넌트가 있으며, 게임 매니저에 머신건이 설정되어 있으면
        if (playerShooter != null && GameManager.instance.machineGun != null)
        {
            Gun oldGun = playerShooter.gun;

            if (oldGun != null)
            {
                // 기존 총의 정보 백업 (위치, 회전, 부모)
                Transform gunPivot = oldGun.transform.parent;
                Vector3 localPos = oldGun.transform.localPosition;
                Quaternion localRot = oldGun.transform.localRotation;

                // 새 머신건 인스턴스화
                Gun newGun = Instantiate(GameManager.instance.machineGun, gunPivot);
                newGun.transform.localPosition = localPos;

                // M249 모델이 반대를 향하고 있으므로 Y축으로 180도 회전시켜 정면을 보게 합니다.
                // localRot을 곱하는 대신 명확하게 180도 값을 설정합니다.
                newGun.transform.localRotation = Quaternion.Euler(0, 180f, 0);

                // 기존 총에서 IK 마운트들을 새 총으로 이동 (월드 위치를 유지하여 손이 꼬이지 않게 true 설정)
                if (playerShooter.leftHandMount != null)
                    playerShooter.leftHandMount.SetParent(newGun.transform, true);
                if (playerShooter.rightHandMount != null)
                    playerShooter.rightHandMount.SetParent(newGun.transform, true);

                // 장착된 무기가 다시 아이템으로 취급되지 않도록 충돌체와 스크립트 제거
                Collider col = newGun.GetComponent<Collider>();
                Rotator rotator = newGun.GetComponent<Rotator>();
                if (rotator != null) Destroy(rotator);
                if (col != null) col.enabled = false;

                MachineGun pickupScript = newGun.GetComponent<MachineGun>();
                if (pickupScript != null) Destroy(pickupScript);

                // 새로운 총 할당 및 활성화
                playerShooter.gun = newGun;
                playerShooter.gun.gameObject.SetActive(true);

                // 기존 총 파괴
                Destroy(oldGun.gameObject);
            }
        }

        // 아이템으로 사용된 자신을 파괴
        Destroy(gameObject);
    }
}