using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Block : FiniteStateMachine
{
    [SerializeField] [Range(50, 5000)] public int m_points = 100;
    [SerializeField] [Range(50, 50000)] public int m_maxPoints = 500;
    [SerializeField] [Range(0.0f, 2.0f)] public float m_bounceModifier = 1.1f;
    [SerializeField] [Range(0.1f, 5.0f)] public float m_enterTime = 1.0f;
    [SerializeField] [Range(0.1f, 1.0f)] public float m_hitTime = 0.2f;

    public enum eState
    {
        INACTIVE,
        ENTER,
        ACTIVE,
        HIT
    }

    public enum eType
    {
        STANDARD
    }

    Collider m_collider = null;
    Blocks m_owner = null;

    eType m_type;
    float m_timer;
    Vector3 m_startPosition;
    Vector3 m_position;
    Vector3 m_startScale;

    void Awake()
    {
        InitializeStateMachine<eState>(eState.INACTIVE, true);
        AddTransitionsToState(eState.INACTIVE, new System.Enum[] { eState.ACTIVE, eState.ENTER });
        AddTransitionsToState(eState.ENTER, new System.Enum[] { eState.ACTIVE });
        AddTransitionsToState(eState.ACTIVE, new System.Enum[] { eState.HIT, eState.ENTER });
        AddTransitionsToState(eState.HIT, new System.Enum[] { eState.INACTIVE });
        m_startScale = transform.localScale;
    }

    void Start()
    {
        m_collider = GetComponent<Collider>();
    }

    public void Initialize(Vector3 position, eType type, Blocks owner)
    {
        m_type = type;

        m_position = position;
        m_startPosition.x = m_position.x;
        m_startPosition.y = m_position.y + 10.0f;
        m_startPosition.z = m_position.z;
        transform.position = m_startPosition;

        transform.localScale = m_startScale;
        
        m_owner = owner;

        SetState(eState.ENTER);
    }

    private void EnterENTER(Enum previousState)
    {
        transform.position = m_startPosition;
        m_timer = m_enterTime;
        m_timer += ((transform.position.y - 6) / 16) + Math.Abs(transform.position.x / 50);
    }

    private void UpdateENTER()
    {
        m_timer = m_timer - Time.deltaTime;
        m_timer = Mathf.Max(m_timer, 0.0f);
        var animProgress = clamp(1.0f - (m_timer / m_enterTime), 0, 1);
        float interp = Interpolation.BounceOut(animProgress);
        transform.position = Vector3.LerpUnclamped(m_startPosition, m_position, interp);
        
        if (m_timer == 0.0f)
        {
            SetState(eState.ACTIVE);
        }
    }
    private float clamp(float val, float min, float max)
    {
        return Math.Min(max, Math.Max(val, min));
    }

    private void EnterHIT(Enum previousState)
    {
        m_collider.enabled = false;
        Game.instance.AddPoints(transform.position, m_points);
        StartCoroutine(HitCoroutine());
    }

    IEnumerator HitCoroutine()
    {
        GetComponent<AudioSource>().Play();
        m_timer = m_hitTime;
        while (m_timer > 0.0f)
        {
            m_timer = m_timer - Time.deltaTime;
            m_timer = Mathf.Max(m_timer, 0.0f);
            //float interp = Interpolation.BackIn(1.0f - (m_timer / m_hitTime));
            float interp = 1.0f - (m_timer / m_hitTime);
            transform.localScale = Vector3.LerpUnclamped(m_startScale, Vector3.zero, interp);

            yield return null;
        }
        m_owner.RemoveBlock(this);

        SetState(eState.INACTIVE);
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Ball"))
        {
            switch (m_type)
            {
            case eType.STANDARD:
                var ball = collision.gameObject.GetComponent<Ball>();
                var hitCount = ball.m_hitCount++;
                m_points = (int)(Math.Ceiling((m_points * Math.Pow(m_bounceModifier, (double)hitCount)) / 10) * 10);
                m_points = Math.Min(m_points, m_maxPoints);

                SetState(eState.HIT);
                break;
                
            default:
                throw new NotImplementedException("Only standard blocks are implemented ATM");
            }
        }
    }
}
