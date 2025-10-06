using NUnit.Framework;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

public class CorpseManager : MonoBehaviour
{
    [SerializeField]
    List<Death> _corpses = new List<Death>();

    static CorpseManager _instance;
    public static CorpseManager Instance { get { return _instance; } }

    // turns object into Instanced singleton so scripts can call this without having a direct reference
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

    // called to delete every registered corpse
    [ContextMenu("Delete Everything")]
    public void DeleteAllCorpses()
    {
        foreach(var corpse in _corpses)
        {
            Destroy(corpse.gameObject);
        }

        _corpses.Clear();
    }

    // called by corpses when created and destroyed
    public void AddToList(Death corpse)
    {
        _corpses.Add(corpse);
    }
    public void RemoveFromlist(Death corpse)
    {
        if(_corpses.Contains(corpse)) _corpses.Remove(corpse);
    }

    public void resetLevel()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}
