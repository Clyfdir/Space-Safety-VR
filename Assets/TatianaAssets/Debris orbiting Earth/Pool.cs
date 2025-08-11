///   this script added by Tatiana Gvozdenko, Hochschule Darmstadt, SoSe25
///   P6, Group project: Safe Space
///   script is copied from Udemy course, with minor changes : https://www.udemy.com/course/design-patterns-for-game-programming/ , lessons 34-35
///   Created: 08.06.2025
///   Last Change: 08.06.2025
///   ESA PROJECT STAGE:
///   Last Change: 11.08.2025

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class PoolItem
{
    public GameObject prefab;
    public int amount;
    public bool expandable;
}

public class Pool : MonoBehaviour
{

    public static Pool Instance;
    public List<PoolItem> items;
    public List<GameObject> pooledItems;

    void Awake()
    {
        Instance = this;
    }

    public GameObject Get(string tag)
    {
        for (int i = 0; i < pooledItems.Count; i++)
        {
            if (!pooledItems[i].activeInHierarchy && pooledItems[i].tag == tag)
            {
                return pooledItems[i];
            }
        }

        foreach (PoolItem item in items)
        {
            if (item.prefab.tag == tag && item.expandable)
            {
                GameObject obj = Instantiate(item.prefab, transform);// parent to Pool
                obj.SetActive(false);
                pooledItems.Add(obj);
                return obj;
            }
        }

        return null;
    }

    // Use this for initialization
    void Start()
    {
        pooledItems = new List<GameObject>();
        foreach (PoolItem item in items)
        {
            for (int i = 0; i < item.amount; i++)
            {
                GameObject obj = Instantiate(item.prefab, transform);// parent to Pool
                obj.SetActive(false);
                pooledItems.Add(obj);
            }
        }
    }
}