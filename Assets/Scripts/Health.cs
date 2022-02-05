using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using Kutil;

public class Health : MonoBehaviour {

    [SerializeField, ReadOnly] float _currentHealth;
    public float currentHealth {
        get { return _currentHealth; }
        protected set { _currentHealth = value; UpdateHealth(); }
    }
    [SerializeField] float maxHealth = 3;

    /// <summary>
    /// heal regeneration rate per second. stops when full. -1 disables
    /// </summary>
    public float regenRate = -1;
    public bool canRegen = false;
    public bool destroyOnDie = false;
    /// <summary>
    /// Seconds after a hit that future hits will be ignored
    /// </summary>
    public float hitInvincibleDur = 0.5f;
    protected float lastDamageTime = 0;
    [HideInInspector]
    public HitArgs lastHitArgs = null;
    public bool manualInvincible = false;

    bool isHitInvincible => hitInvincibleDur > 0 && Time.time < lastDamageTime + hitInvincibleDur;
    public bool isInvincible => manualInvincible || isHitInvincible;
    // max health negative means true invincibility
    public bool isDead => currentHealth <= 0 && maxHealth >= 0;
    public bool isHealthFull => currentHealth >= maxHealth;

    [Header("Events")]
    public UnityEvent dieEvent;
    public UnityEvent damageEvent;
    public UnityEvent healthUpdateEvent;

    private void Awake() {
        RestoreHealth();
    }
    private void Update() {
        if (canRegen && regenRate > 0 && !isHealthFull)
        {
            currentHealth = Mathf.Min(currentHealth + regenRate * Time.deltaTime, maxHealth);
        }
    }
    public void UpdateHealth() {
        healthUpdateEvent.Invoke();
    }
    public void RestoreHealth() {
        currentHealth = maxHealth;
    }
    public void Heal(float amount) {
        currentHealth += amount;
    }
    public void TakeDamage(HitArgs args) {
        Debug.Log($"{name} hit by {args.attacker} for {args.damage}", this);
        if (isDead) {
            Debug.Log($"{name} is already dead", this);
            return;
        } else if (isInvincible) {
            // Debug.Log(name + " is invincible", this);
            return;
        }
        lastHitArgs = args;
        currentHealth -= args.damage;
        lastDamageTime = Time.time;
        damageEvent.Invoke();
        if (currentHealth <= 0) {
            Die();
        }
    }
    public void Die() {
        dieEvent.Invoke();
        if (destroyOnDie) {
            Destroy(gameObject);
        }
    }
}

[System.Serializable]
public class HitArgs {
    public string attacker;
    public float damage;
    // public bool isDirect;
    // public Vector3 point;
    // public Vector3 velocity;
    // public GameObject hit;
}