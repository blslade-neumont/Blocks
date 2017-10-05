using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class Points : MonoBehaviour
{
    [SerializeField] public Transform m_destination = null;
    [SerializeField] public Vector3 m_offset = Vector3.zero;
    [SerializeField] public Vector3 m_offsetEnter = Vector3.zero;
    [SerializeField] [Range(0.1f, 5.0f)] public float m_enterTime = 1.0f;
    [SerializeField] [Range(0.1f, 5.0f)] public float m_idleTime = 0.2f;
    [SerializeField] [Range(0.1f, 5.0f)] public float m_exitTime = 0.2f;

    PointsController m_owner = null;
    TextMeshPro m_textMeshPro = null;

    Vector3 m_startPosition;
    float m_timer = 0.0f;

    void Start()
    {
        m_textMeshPro = GetComponent<TextMeshPro>();
    }

    public void Initialize(Vector3 position, int value, PointsController owner)
    {
        m_startPosition = position + m_offset;
        transform.position = m_startPosition;
        m_owner = owner;

        if (m_textMeshPro == null)
        {
            m_textMeshPro = GetComponent<TextMeshPro>();
        }
        m_textMeshPro.text = value.ToString();

        StartCoroutine(DisplayCoroutine());
	}

    IEnumerator DisplayCoroutine()
    {
        m_timer = m_enterTime;
        while (m_timer > 0.0f)
        {
            m_timer = m_timer - Time.deltaTime;
            m_timer = Mathf.Max(m_timer, 0.0f);

            float interp = 1.0f - (m_timer / m_enterTime);
            transform.position = Vector3.LerpUnclamped(m_startPosition, m_startPosition + m_offsetEnter, interp);

            yield return null;
        }

        m_timer = m_idleTime;
        while (m_timer > 0.0f)
        {
            m_timer = m_timer - Time.deltaTime;
            m_timer = Mathf.Max(m_timer, 0.0f);

            //float interp = 1.0f - (m_timer / m_idleTime);

            yield return null;
        }

        m_startPosition = transform.position;
        m_timer = m_exitTime;
        while (m_timer > 0.0f)
        {
            m_timer = m_timer - Time.deltaTime;
            m_timer = Mathf.Max(m_timer, 0.0f);

            float interp = 1.0f - (m_timer / m_exitTime);
            if (m_destination)
            {
                transform.position = Vector3.LerpUnclamped(m_startPosition, m_destination.transform.position, interp);
            }

            yield return null;
        }

        m_owner.RemovePoints(this);
    }
}
