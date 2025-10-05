using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Splines;
using DG.Tweening;

public class CardManager : MonoBehaviour
{
    [SerializeField] List<Cards> cards = new List<Cards>();

    [SerializeField] Upgrade _selectedCard;
    public Upgrade SelectedCard { get { return _selectedCard; } }

    [SerializeField] bool _hasBaseCard = false;
    public bool HasBaseCard { get { return _hasBaseCard; } }

    static CardManager _instance;
    public static CardManager Instance { get { return _instance; } }

    [Header("Spline UI")]
    [SerializeField] SplineContainer spline;
    [SerializeField] RectTransform canvasRect;
    [SerializeField] Camera mainCamera;
    [SerializeField] float moveDuration = 0.3f;

    [Header("Visuals")]
    [SerializeField] float dimAlpha = 0.5f;
    [SerializeField] float unselectedScale = 1.2f;
    [SerializeField] float selectedScale = 1.5f;

    [Header("Layout Spread")]
    [SerializeField] float minSpread = 0.1f;  // tightest for very few cards
    [SerializeField] float maxSpread = 0.8f;  // widest for many cards
    [SerializeField] int maxCardsForFullSpread = 5; // number of cards that reach full spread

    private List<RectTransform> spawnedCards = new List<RectTransform>();

    private void Awake()
    {
        if (_instance == null) _instance = this;
        else Destroy(gameObject);
    }
    
    // Card Logic things 
    
    public bool CheckForCard(Upgrade card)
    {
        foreach (Cards _card in cards)
        {
            if (_card._name == card && _card._hasCard) return true;
        }
        return false;
    }

    public void CollectCard(Upgrade card)
    {
        foreach (Cards _card in cards)
        {
            if (_card._name == card)
            {
                if (_card._hasCard) return; // Already collected

                _card._hasCard = true;
                AddCardToSpline(_card._cardUI);
                return;
            }
        }
    }

    public void SelectCard(int index)
    {
        if (!_hasBaseCard) _hasBaseCard = true;

        if (cards[index]._hasCard)
        {
            _selectedCard = (Upgrade)index;

            // Bring selected card to front so its not over lapping 
            RectTransform selectedRect = GetSelectedCardRect();
            if (selectedRect != null)
            {
                selectedRect.SetAsLastSibling();
            }

            UpdateSplineLayout();
        }
    }
    
    // DOTween Spline UI
    private void AddCardToSpline(GameObject cardUI) // first we add it to spline 
    {
        if (cardUI == null) return;

        cardUI.SetActive(true);

        RectTransform cardRect = cardUI.GetComponent<RectTransform>();
        if (cardRect == null) cardRect = cardUI.AddComponent<RectTransform>();

        // Parent to canvas if needed
        if (cardUI.transform.parent != canvasRect)
            cardUI.transform.SetParent(canvasRect, false);

        // Track active cards
        if (!spawnedCards.Contains(cardRect))
            spawnedCards.Add(cardRect);

        // CanvasGroup for fade
        CanvasGroup cg = cardUI.GetComponent<CanvasGroup>();
        if (cg == null) cg = cardUI.AddComponent<CanvasGroup>();
        cg.alpha = 0f;

        // Start offscreen
        cardRect.anchoredPosition = new Vector2(0, -500);
        cardRect.localRotation = Quaternion.identity;
        cardRect.localScale = Vector3.one * unselectedScale;

        // Fade in
        cg.DOFade(1f, moveDuration);

        UpdateSplineLayout();
    }

    private RectTransform GetSelectedCardRect()
    {
        foreach (Cards c in cards)
        {
            if (c._hasCard && c._name == _selectedCard)
            {
                return c._cardUI?.GetComponent<RectTransform>();
            }
        }
        return null;
    }

    private void UpdateSplineLayout()
    {
        spawnedCards.RemoveAll(c => c == null);

        RectTransform selectedRect = GetSelectedCardRect();
        int count = spawnedCards.Count;

        // --- Dynamic spread based on card count ---
        float spreadFraction = Mathf.Clamp01((float)count / maxCardsForFullSpread);
        float spreadWidth = Mathf.Lerp(minSpread, maxSpread, spreadFraction);
        float tStart = 0.5f - spreadWidth / 2f;
        float tEnd = 0.5f + spreadWidth / 2f;

        for (int i = 0; i < count; i++)
        {
            RectTransform cardRect = spawnedCards[i];
            if (cardRect == null) continue;

            // Adaptive t along spline
            float t = (count == 1) ? 0.5f : Mathf.Lerp(tStart, tEnd, (float)i / (count - 1));

            Vector3 worldPos = spline.EvaluatePosition(t);
            Vector2 screenPos = RectTransformUtility.WorldToScreenPoint(mainCamera, worldPos);
            RectTransformUtility.ScreenPointToLocalPointInRectangle(canvasRect, screenPos, null, out Vector2 uiPos);

            // Rotation along tangent
            Vector3 tangent = spline.EvaluateTangent(t);
            float angle = Mathf.Atan2(tangent.y, tangent.x) * Mathf.Rad2Deg;
            Quaternion targetRot = Quaternion.Euler(0f, 0f, angle);

            // Animate position and rotation
            cardRect.DOAnchorPos(uiPos, moveDuration).SetEase(Ease.OutQuad);
            cardRect.DOLocalRotateQuaternion(targetRot, moveDuration).SetEase(Ease.OutQuad);

            // -- Dim and scale 
            CanvasGroup cg = cardRect.GetComponent<CanvasGroup>();
            if (cg == null) cg = cardRect.gameObject.AddComponent<CanvasGroup>();

            bool isSelected = (cardRect == selectedRect);
            float targetAlpha = isSelected ? 1f : dimAlpha;
            float targetScale = isSelected ? selectedScale : unselectedScale;

            cg.DOFade(targetAlpha, moveDuration);

            // Scale with bounce for selected, smooth for unselected
            if (isSelected)
                cardRect.DOScale(targetScale, moveDuration).SetEase(Ease.OutBounce);
            else
                cardRect.DOScale(targetScale, moveDuration).SetEase(Ease.OutQuad);
        }
    }
}

[Serializable]
public class Cards
{
    public Upgrade _name;
    public bool _hasCard;
    public GameObject _cardUI;
}
