
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerStats : MonoBehaviour, IDamageable
{
    //every stat
    [Header("Every Stat, Set Up In Editor")]
    public statPair speed;
    public statPair damage;
    public statPair atkSpeed;
    public statPair money;
    public statPair moneyOnKill;

    [Header("Don't worry about this!")]
    public List<statPair> stats;

    // all the things to handel player taking damage and stuff 
    private bool isInvincible = false;
    public float invincibilityDuration = 1f;
    
    private SpriteRenderer spriteRenderer;
    private Coroutine flashCoroutine;


    public delegate void OnPlayerHurt();
    public static OnPlayerHurt onPlayerHurt;

    public delegate void OnStatUpdate();
    public static OnStatUpdate onStatUpdate;
    
    private void Awake()
    {
        //populate list with all stats 
        stats = new List<statPair> { speed, damage, atkSpeed, money, moneyOnKill };
        onStatUpdate?.Invoke();
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    //takes in a stat name and value then sets the stats associated with the name to the given value
    public void updateStats(statName name, float value)
    {
        foreach(var pair in stats) if (pair.name == name) pair.value += value;

        onStatUpdate?.Invoke();


    }

    //takes in a stat name and returns the stat associated with the name if there is one
    public statPair getStats(statName name)
    {
        foreach (var pair in stats) if (pair.name == name) return pair;
        
        return null;
    }

    public void TakeDamage(int amount, Vector2 dmgLocation)
    {
        onPlayerHurt?.Invoke();

        if (isInvincible) return;
        isInvincible = true;

        
        if (flashCoroutine != null) StopCoroutine(flashCoroutine);
        flashCoroutine = StartCoroutine(FlashSprite());

        // logic for removing money
        if (getStats(statName.MONEY).value > 0)
        {
            

            // takes random amount between 0 - 1
            int _randNum = UnityEngine.Random.Range(0, 2);

            updateStats(statName.MONEY, -_randNum);

            if (getStats(statName.MONEY).value < 0) updateStats(statName.MONEY, 0);

        }
    }
    
    public void TakeDamage(float amount, Vector2 dmgLocation)
    {
        onPlayerHurt?.Invoke();

        if (isInvincible) return;
        isInvincible = true;
        
        
        if (flashCoroutine != null) StopCoroutine(flashCoroutine);
        flashCoroutine = StartCoroutine(FlashSprite());


    }

    private IEnumerator FlashSprite()
    {
        float flashInterval = 0.1f; // seconds between flashes
        float elapsedTime = 0f;

        while (elapsedTime < invincibilityDuration)
        {
            spriteRenderer.color = Color.clear;
            yield return new WaitForSeconds(flashInterval);
            spriteRenderer.color = Color.white;
            yield return new WaitForSeconds(flashInterval);

            elapsedTime += flashInterval * 2;
        }

        spriteRenderer.color = Color.white;
        isInvincible = false;
    }


}


//custom class used for serialized stat pair combinations
[Serializable]
public class statPair
{
    public statName name;
    public float value;
}

//custom enum used in statPair combinations
public enum statName
{
    SPEED, DAMAGE, ATTACKSPEED, MONEY, MONEYONKILL, 
}


