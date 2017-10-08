using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityRandom = UnityEngine.Random;

public class Ball : FiniteStateMachine
{
    [SerializeField] [Range(0.0f, 10.0f)] public float m_startVelocity = 5.0f;
    [SerializeField] [Range(-1.0f, 1.0f)] public float m_velocityChange = 0.01f;

    [SerializeField] public Game m_game;

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

    public Rigidbody m_rigidbody = null;
    public AudioSource m_audioSource = null;
    public Balls m_owner = null;
    public int m_hitCount = 0;

    eType m_type = eType.STANDARD;

    void Awake()
    {
        InitializeStateMachine<eState>(eState.INACTIVE, true);
        AddTransitionsToState(eState.INACTIVE, new System.Enum[] { eState.ACTIVE, eState.ENTER });
        AddTransitionsToState(eState.ENTER, new System.Enum[] { eState.ACTIVE });
        AddTransitionsToState(eState.ACTIVE, new System.Enum[] { eState.HIT, eState.ENTER });
        AddTransitionsToState(eState.HIT, new System.Enum[] { eState.INACTIVE });
    }

	void Start()
    {
        m_rigidbody = GetComponent<Rigidbody>();
        m_audioSource = GetComponent<AudioSource>();
    }

    public void Initialize(Vector3 position, Vector3 direction, eType type, Balls owner)
    {
        transform.position = position;
        if (m_rigidbody == null)
        {
            m_rigidbody = GetComponent<Rigidbody>();
        }
        m_rigidbody.AddForce(direction * (m_startVelocity * 100.0f));
        m_type = type;
        m_owner = owner;
                
        SetState(eState.ENTER);
    }

    private void UpdateINACTIVE()
    {
        SetState(eState.ACTIVE);
    }

    private void OnCollisionEnter(Collision collision)
    {
        var other = collision.gameObject;
        switch (m_type)
        {
        case eType.STANDARD:
            if (other.CompareTag("Exit"))
            {
                if (this.m_game) this.m_game.SetState(Game.eState.GAMEOVER);
                Destroy(this.gameObject);
                break;
            }
            else
            {
                m_audioSource.Play();

                Quaternion qr = Quaternion.AngleAxis(UnityRandom.Range(-2.0f, 2.0f), Vector3.forward);
                var newVel = qr * m_rigidbody.velocity;
                if (other.CompareTag("Paddle")) newVel *= 1 + m_velocityChange;
                m_rigidbody.velocity = newVel;
                break;
            }
            
        default:
            throw new NotImplementedException("Only standard balls are supported ATM");
        }
    }
}
