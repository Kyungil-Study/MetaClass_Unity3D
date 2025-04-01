using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AssultRifle : MonoBehaviour
{
    [Header("Gun Data")]
    public GunData data;

    [Header("Visual")]
    public GameObject muzzleFlash;
    public float muzzleFlashDuration = 0.1f;

    public int CurrentAmmo { get; private set; }
    public float CurrentFireRate { get; private set; }
    private float CurrentMuzzleFlashDuration { get; set; }
    
    private Camera mainCam;

    // Start is called before the first frame update
    void Start()
    {
        mainCam = Camera.main;
        muzzleFlash.SetActive(true);
    }

    // Update is called once per frame
    void Update()
    {
        CurrentFireRate += Time.deltaTime;
        CurrentMuzzleFlashDuration += Time.deltaTime;

        if (CurrentMuzzleFlashDuration >= muzzleFlashDuration)
        {
            muzzleFlash.SetActive(false);
        }
    }


    public bool Fire()
    {
        if(CurrentFireRate < data.fireRate)
            return false;

        /*if (CurrentAmmo <= 0)
            return false;*/

        CurrentFireRate = 0f;

        CurrentAmmo--;
        
        muzzleFlash.transform.localRotation 
            *= Quaternion.AngleAxis(Random.Range(0,360), Vector3.right);
        
        muzzleFlash.SetActive(true);
        CurrentMuzzleFlashDuration = 0f;
        
        Ray ray = mainCam.ScreenPointToRay(Input.mousePosition);

        if (Physics.Raycast(ray, out RaycastHit hit, data.range, LayerMask.GetMask("Enemy")))
        {
            IDamagable enemy = CombatSystem.Instance.GetMonsterOrNull(hit.collider);
            if (enemy != null)
            {
                CombatEvent combatEvent = new CombatEvent
                {
                    Sender = Player.localPlayer,
                    Receiver = enemy,
                    Damage = data.damage,
                    HitPosition = hit.point
                };
                CombatSystem.Instance.AddCombatEvent(combatEvent);
            }
        }
        return true;
    }
    
    
}
