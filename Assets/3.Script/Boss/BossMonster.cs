using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor.Build.Content;
using UnityEngine;
using UnityEngine.Serialization;

public partial class BossMonster : MonoBehaviour , IDamagable , IAttackable
{
    public class BaseStat
    {
        public const int HIT_COUNT = 20;
        public int CurrentHp { get; set; }
        public int CurrentHitCount { get; set; }
    }
    public int hp;

    public class Events
    {
        /// <summary>
        /// int currentHp, int maxHp
        /// </summary>
        public Action<int,int> OnDamage;
        public Action<BossState.StateName> OnChangedState;
    }
    public readonly Events events = new Events();

    public GameObject GameObject => gameObject;
 
    public readonly BaseStat stat = new BaseStat();
    private List<HitBox> hitBoxes;
    
    public Animator Animator { get; private set; }
    private Dictionary<BossState.StateName, BossState> stateDic 
        = new Dictionary<BossState.StateName, BossState>();
    
    
    private BossState previousState;
    private BossState currentState;

    private List<IDamagable> hittedDamagables = new List<IDamagable>();

    public void BeginAttack()
    {
        hittedDamagables.Clear();
    }

    public void AddHitted(IDamagable target)
    {
        hittedDamagables.Add(target);
    }
    public bool IsHitted(IDamagable damagable)
    {
        if (hittedDamagables.Contains(damagable))
        {
            return true;
        }
        return false;
    }

    private void Awake()
    {
        Animator = GetComponentInChildren<Animator>();
        stat.CurrentHp = hp;
        CurrentSceneBossMonster = this;

        BossState[] myState =  gameObject.GetComponentsInChildren<BossState>(true);
        for (int i = 0; i < myState.Length; i++)
        {
            var state = myState[i];
            state.Initialize(this);
            stateDic.Add(state.Name, state);
        }
            
        ChangeState(BossState.StateName.IdleState);
    }

    private void Start()
    {
        ChangeHp(0);
        hitBoxes = GetComponentsInChildren<HitBox>(true).ToList();
        foreach (var hitbox in hitBoxes)
        {
            CombatSystem.Instance.RegisterMonster(hitbox, this);
        }
    }

    private void OnDestroy()
    {
        foreach (var hitbox in hitBoxes)
        {
            CombatSystem.Instance.UnregisterMonster(hitbox);
        }
    }

    public void TakeDamage(CombatEvent combatEvent)
    {
        var damage =combatEvent.Damage;
        switch (combatEvent.HitBox.DamageArea)
        {
            case DamageArea.Head:
                damage *= 2;
                break;
            case DamageArea.Body:
                break;
            case DamageArea.LeftArm:
            case DamageArea.RightArm:
            case DamageArea.LeftLeg:
            case DamageArea.RightLeg:
                damage += damage / 2;
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
        
        TakeDamageEvent evt = new TakeDamageEvent()
        {
            Taker = this,
            Damage = damage,
            HitPosition = combatEvent.HitPosition,
            HitNormal = combatEvent.HitNormal,
            HitBox = combatEvent.HitBox
        };
        CombatSystem.Instance.AddTakeDamageEvent(evt);
        ChangeHp(-damage);
    }

    public void ChangeState(BossState.StateName state)
    {
        previousState = currentState;
        
        if (previousState != null)
        {
            previousState.Exit();
            previousState.gameObject.SetActive(false);
        }
        
        currentState = stateDic[state];
        currentState.Enter();
        currentState.gameObject.SetActive(true);
    }
    
    public void ChangeHp(int amount)
    {
        stat.CurrentHp += amount;
        stat.CurrentHp = Mathf.Clamp(stat.CurrentHp, 0, hp);
        events.OnDamage?.Invoke(stat.CurrentHp, hp);

        if (stat.CurrentHp <= 0)
        {
            Animator.ResetTrigger(SCRATCH);
            Animator.ResetTrigger(BREATH);
            Animator.ResetTrigger(HIT);
            Animator.SetTrigger(DEAD);
        }
        else
        {
            stat.CurrentHitCount++;
            if (stat.CurrentHitCount >= BaseStat.HIT_COUNT)
            {
                stat.CurrentHitCount = 0;
                Animator.SetTrigger(HIT);
            }
        }
    }
    
    
}
