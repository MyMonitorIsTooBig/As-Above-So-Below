using System;
using UnityEngine;
using System.Collections.Generic;

public class CardManager : MonoBehaviour
{
    [SerializeField] List<Cards> cards = new List<Cards>();

    [SerializeField] Upgrade _selectedCard;
    public Upgrade SelectedCard { get { return _selectedCard; } }

    [SerializeField] bool _hasBaseCard = false;
    public bool HasBaseCard {  get { return _hasBaseCard; } }

    static CardManager _instance;
    public static CardManager Instance {  get { return _instance; } }

    private void Awake()
    {
        if(_instance == null)
        {
            _instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public bool CheckForCard(Upgrade card)
    {
        foreach(Cards _card in cards)
        {
            if(_card._name == card && _card._hasCard)
            {
                return true;
            }
        }

        return false;
    }
    public void CollectCard(Upgrade card)
    {
        foreach(Cards _card in cards)
        {
            if(_card._name == card)
            {
                _card._hasCard = true;
                _card._cardUI.SetActive(true);
                return;
            }
        }
    }

    public void SelectCard(int index)
    {

        if (!_hasBaseCard) _hasBaseCard = true;

        foreach (Cards _card in cards)
        {
            if(_card._name == (Upgrade)index && _card._hasCard)
            {
                _selectedCard = (Upgrade)index;
                return;
            }
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


