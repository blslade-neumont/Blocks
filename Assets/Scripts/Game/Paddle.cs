using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Paddle : MonoBehaviour
{
    [SerializeField] [Range(0.1f, 2.0f)] public float m_speed = 1.0f;
    [SerializeField] public Transform m_limitMin = null;
    [SerializeField] public Transform m_limitMax = null;

    enum eState
    {
        INACTIVE,
        ACTIVE
    }

    eState state { get; set; }

    void Awake()
    {
        state = eState.INACTIVE;
    }

    void Start()
    {
        
    }

    void Update()
    {
        Vector3 mousePosition = Input.mousePosition;
        mousePosition.z = Mathf.Abs(Camera.main.transform.position.z - transform.position.z);
        Vector3 target = Camera.main.ScreenToWorldPoint(mousePosition);
        target.x = Mathf.Clamp(target.x, m_limitMin.position.x, m_limitMax.position.x);

        Vector3 position = transform.position;
        var oldX = position.x;

        float dx = Mathf.Min(Mathf.Abs(target.x - position.x), m_speed) * Mathf.Sign(target.x - position.x);
        position.x = position.x + dx;
        transform.position = position;

        var prevRotation = transform.rotation.z;
        var offsetRotation = (oldX - transform.position.x) * 100;
        var newRotation = clamp(((prevRotation * 9) + offsetRotation) / 10, -80, 80);
        transform.rotation = Quaternion.Euler(0, 0, newRotation);
    }
    private float clamp(float val, float min, float max)
    {
        return Math.Min(max, Math.Max(val, min));
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Ball"))
        {
            var ball = collision.gameObject.GetComponent<Ball>();
            ball.m_hitCount = 0;
        }
    }
}
