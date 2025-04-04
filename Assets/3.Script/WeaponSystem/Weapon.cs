using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Weapon : MonoBehaviour
{
    public class Events
    {
        public Action<int> OnChangeAmmo;
    }
    
    public Events events = new Events();
    
    [Header("Gun Data")]
    public GunData data;
    
    [Header("Bullet Data")]
    [SerializeField] Bullet bulletSample;
    ObjectPool bulletPool;
    
    [Header("Visual")]
    public GameObject muzzleFlash;
    public float muzzleFlashDuration = 0.1f;
    
    public int CurrentAmmo { get; protected set; }
    public float CurrentFireRate { get; protected set; }
    
    protected float CurrentMuzzleFlashDuration { get; set; }

    public abstract bool Fire();

    public bool IsReadyToFire()
    {
        if (CurrentFireRate >= data.fireRate)
            return false;
        
        if (CurrentAmmo <= 0)
            return false;
        
        CurrentAmmo--;
        events.OnChangeAmmo?.Invoke(CurrentAmmo);
        
        CurrentFireRate = 0f;
        CurrentMuzzleFlashDuration = 0f;
        return true;
    }
    
    protected virtual void Awake()
    {
        CurrentAmmo = data.totalAmmo;
    }

    private bool isPoolInitialized = false;
    public Bullet GetBullet()
    {
        if (bulletSample is null)
        {
            Debug.LogWarning("Bullet Sample is null");
        }
        
        if (isPoolInitialized is false)
        {
            bulletPool = new ObjectPool();
            bulletPool.Initialize(
                null,
                bulletSample,
                "Bullet",
                4 
                );

            isPoolInitialized = true;
        }
        
        var bullet = bulletPool.Get() as Bullet;
        bullet.gameObject.SetActive(true);
        bullet.weapon = this; 

        return bullet;
    }
    
    public void ReturnToWeapon(Bullet bullet)
    {
        bulletPool.Return(bullet);
    }
}
