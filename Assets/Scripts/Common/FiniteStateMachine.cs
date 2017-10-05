using UnityEngine;
using System.Collections;
using System;
using System.Collections.Generic;
using System.Reflection;

public class FiniteStateMachine : MonoBehaviour
{
    class State
    {
        public Enum id = null;

        public Action updateMethod = () => { };
        public Action<Enum> enterMethod = (state) => { };
        public Action<Enum> exitMethod = (state) => { };
        public bool forceTransition = false;
        public List<Enum> transitions = null;

        public State(Enum id)
        {
            this.id = id;
            this.transitions = new List<Enum>();
        }
    }

    private Dictionary<Enum, State> m_states;

    private State m_currentState = null;
    private bool m_inTransition = false;
    private bool m_initialized = false;
    private bool m_debugMode = false;

    private Action OnUpdate = null;

    public Enum state { get { return this.m_currentState.id; } }

    protected virtual void Update()
    {
        this.OnUpdate();
    }

    private bool Initialized()
    {
        if (!m_initialized)
        {
            Debug.LogError(this.GetType().ToString() + ": StateMachine is not initialized. You need to call InitializeStateMachine( bool debug, bool allowMultiTransition = false )");
            return false;
        }
        return true;
    }

    private static T GetMethodInfo<T>(object obj, Type type, string method, T Default) where T : class
    {
        Type baseType = type;
        MethodInfo methodInfo = baseType.GetMethod(method, BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance);
        if (methodInfo != null)
        {
            return Delegate.CreateDelegate(typeof(T), obj, methodInfo) as T;
        }
        return Default;
    }

    protected void InitializeStateMachine<T>(Enum initialState, bool debug)
    {
        if (this.m_initialized == true)
        {
            Debug.LogError("The StateMachine component on " + this.GetType().ToString() + " is already initialized.");
            return;
        }
        this.m_initialized = true;

        var values = Enum.GetValues(typeof(T));
        this.m_states = new Dictionary<Enum, State>();
        for (int i = 0; i < values.Length; i++)
        {
            this.m_initialized = this.CreateNewState((Enum)values.GetValue(i));
        }
        this.m_currentState = this.m_states[initialState];
        this.m_inTransition = false;
        this.m_debugMode = debug;

        this.m_currentState.enterMethod(m_currentState.id);
        this.OnUpdate = this.m_currentState.updateMethod;
        if (this.m_debugMode == true)
        {
            Debug.Log("StateMachine : " + this.GetType().ToString() + " initialized with " + this.m_currentState.id + " state.");
        }
    }

    private bool CreateNewState(Enum newstate)
    {
        if (this.Initialized() == false) { return false; }
        if (this.m_states.ContainsKey(newstate) == true)
        {
            Debug.Log("State " + newstate + " is already registered in " + this.GetType().ToString());
            return false;
        }
        State s = new State(newstate);
        Type type = this.GetType();
        s.enterMethod = FiniteStateMachine.GetMethodInfo<Action<Enum>>(this, type, "Enter" + newstate, DoNothingEnterExit);
        s.updateMethod = FiniteStateMachine.GetMethodInfo<Action>(this, type, "Update" + newstate, DoNothingUpdate);
        s.exitMethod = FiniteStateMachine.GetMethodInfo<Action<Enum>>(this, type, "Exit" + newstate, DoNothingEnterExit);
        this.m_states.Add(newstate, s);
        return true;
    }

    protected bool AddTransitionsToState(Enum sourceState, Enum[] transitions, bool forceTransition = false)
    {
        if (this.Initialized() == false) { return false; }
        if (this.m_states.ContainsKey(sourceState) == false) { return false; }
        State s = m_states[sourceState];
        s.forceTransition = forceTransition;
        foreach (Enum t in transitions)
        {
            if (s.transitions.Contains(t) == true)
            {
                Debug.LogError("State: " + sourceState + " already contains a transition for " + t + " in " + this.GetType().ToString());
                continue;
            }
            s.transitions.Add(t);
        }
        return true;
    }

    protected bool IsLegalTransition(Enum fromstate, Enum tostate)
    {
        if (this.Initialized() == false) { return false; }

        if (this.m_states.ContainsKey(fromstate) && this.m_states.ContainsKey(tostate))
        {
            if (this.m_states[fromstate].forceTransition == true || this.m_states[fromstate].transitions.Contains(tostate) == true)
            {
                return true;
            }
        }
        return false;
    }

    public bool SetState(Enum newstate, bool forceTransition = false)
    {
        if (this.Initialized() == false) { return false; }

        if (this.m_inTransition)
        {
            if (this.m_debugMode == true)
            {
                Debug.LogWarning(this.GetType().ToString() + " requests transition to state " + newstate +
                        " when still transitioning");
            }
            return false;
        }

        if (forceTransition || this.IsLegalTransition(this.m_currentState.id, newstate))
        {
            if (this.m_debugMode == true)
            {
                Debug.Log(this.GetType().ToString() + " transition: " + this.m_currentState.id + " => " + newstate);
            }

            State transitionSource = this.m_currentState;
            State transitionTarget = this.m_states[newstate];
            this.m_inTransition = true;
            this.m_currentState.exitMethod(transitionTarget.id);
            transitionTarget.enterMethod(transitionSource.id);
            this.m_currentState = transitionTarget;

            if (transitionTarget == null || transitionSource == null)
            {
                Debug.LogError(this.GetType().ToString() + " cannot finalize transition; source or target state is null!");
            }
            else
            {
                this.m_inTransition = false;
            }
        }
        else
        {
            Debug.LogError(this.GetType().ToString() + " requests transition: " + this.m_currentState.id + " => " + newstate + " is not a defined transition!");
            return false;
        }

        this.OnUpdate = this.m_currentState.updateMethod;
        return true;
    }

    private static void DoNothingUpdate() { }
    private static void DoNothingEnterExit(Enum state) { }
}