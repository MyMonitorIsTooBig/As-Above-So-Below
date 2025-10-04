using UnityEngine;
using DG.Tweening;

public class TarotCardObject : MonoBehaviour
{
    [Header("Card Settings")]
    [SerializeField] Upgrade _cardType;
    [SerializeField] float _tweenAmt;
    [SerializeField] float _tweenTime;

    private void OnEnable()
    {
        transform.DOLocalMoveY(transform.position.y + _tweenAmt,_tweenTime).SetLoops(-1, LoopType.Yoyo).SetEase(Ease.InOutSine);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if(collision.CompareTag("Player"))
        {
            CardManager.Instance.CollectCard(_cardType);
            Destroy(gameObject);
        }
    }
}
