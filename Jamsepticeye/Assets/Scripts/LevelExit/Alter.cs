using DG.Tweening;
using UnityEngine;
using System.Collections.Generic;


public class Alter : MonoBehaviour
{
    [Header("Alter Settings")]
    [SerializeField] GameObject door;
    [SerializeField] Upgrade _requiredUpgrade;

    [Header("Alter Tween Settings")]
    [SerializeField] float _lowerBounds;
    [SerializeField] float _time;

    [Header("Door Tween Settings")]
    [SerializeField] float _upperBounds;
    [SerializeField] float _time2;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player") && CardManager.Instance.CheckForCard(_requiredUpgrade))
        {
            transform.DOMoveY(transform.position.y - _lowerBounds, _time).SetEase(Ease.OutExpo);
            door.transform.DOMoveY(door.transform.position.y + _upperBounds, _time2).SetEase(Ease.OutExpo);
        }
    }
}
