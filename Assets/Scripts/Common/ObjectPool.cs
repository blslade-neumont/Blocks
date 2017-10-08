using UnityEngine;
using UnityEngine.Assertions;
using System.Collections;
using System.Collections.Generic;

public class ObjectPool : MonoBehaviour
{
    [SerializeField] public GameObject m_gameObject;
    [SerializeField] [Range(0, 200)] public int m_size = 20;

    private List<GameObject> m_pool = new List<GameObject>();

    void Awake()
    {
        for (int i = 0; i < m_size; i++)
        {
            Add();
        }
    }

    void Add()
    {
        // create object from prefab
        GameObject instance = Instantiate(m_gameObject);
        instance.transform.SetParent(transform);
        instance.SetActive(false);

        // add object to pool
        m_pool.Add(instance);
    }

    public GameObject GetObject()
    {
        Assert.IsTrue(m_pool.Count > 0);

        // get object from pool
        GameObject gameObject = m_pool[0];
        gameObject.SetActive(true);
        gameObject.transform.SetParent(null);

        // remove object from pool
        m_pool.RemoveAt(0);

        return gameObject;
    }

    public void ReturnObject(GameObject gameObject)
    {
        // add object back to pool
        m_pool.Add(gameObject);

        // set object under pool object
        gameObject.SetActive(false);
        gameObject.transform.SetParent(transform);
    }

    public void Clear()
    {
        foreach (var obj in m_pool.ToArray())
        {
            ReturnObject(obj);
        }
    }
}

