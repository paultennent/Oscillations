//#define ENABLE_TIMERS

using UnityEngine;
using System.Collections;

public class rcMonoBehaviourBase : MonoBehaviour
{
    public bool Inited { get { return inited; } set { inited = value; } }
    private bool inited = true;

    public static bool bPrintTiming = false;

    //
    // AwakeVirtual
    //
    protected virtual void AwakeVirtual()
    {

    }

    void Awake()
    {
#if ENABLE_TIMERS
        float start = Time.realtimeSinceStartup;
        AwakeVirtual();
        float finish = Time.realtimeSinceStartup;
        float total = 1000.0f * (finish - start);
        if (total > 1)
            Debug.Log(name + "." + this.GetType() + ".Awake: " + total);
#else
        AwakeVirtual();
#endif
    }

    //
    // StartVirtual
    //
    protected virtual void StartVirtual()
    {
    }

    void Start()
    {
#if ENABLE_TIMERS
        float start = Time.realtimeSinceStartup;
        StartVirtual();
        float finish = Time.realtimeSinceStartup;
        float total = 1000.0f * (finish - start);
        if (total > 1)
            Debug.Log(name + "." + this.GetType() + ".Start: " + total);
#else
        StartVirtual();
#endif
    }

    //
    // OnDestroyVirtual
    //
    protected virtual void OnDestroyVirtual()
    {
    }

    void OnDestroy()
    {
        OnDestroyVirtual();
    }


    //
    // UpdateVirtual
    //
    protected virtual void UpdateVirtual()
    {
    }

    void Update()
    {
        if (Inited)
        {
#if ENABLE_TIMERS
            float start = Time.realtimeSinceStartup;
            //Profiler.BeginSample(name + "." + this.GetType() + ".UpdateVirtual");
            UpdateVirtual();
            //Profiler.EndSample();
            float finish = Time.realtimeSinceStartup;
            float total = 1000.0f * (finish - start);
            if (bPrintTiming && total > 1)
                Debug.Log(name + "." + this.GetType() + ".UpdateVirtual: " + total);
#else

            UpdateVirtual();
#endif
        }
    }

    //
    // FixedUpdateVirtual
    //
    protected virtual void FixedUpdateVirtual()
    {
    }

    void FixedUpdate()
    {
        if (Inited)
        {
#if ENABLE_TIMERS
            float start = Time.realtimeSinceStartup;
            //Profiler.BeginSample(name + "." + this.GetType() + ".FixedUpdateVirtual");
            FixedUpdateVirtual();
            //Profiler.EndSample();
            float finish = Time.realtimeSinceStartup;
            float total = 1000.0f * (finish - start);
            if (bPrintTiming && total > 1)
                Debug.Log(name + "." + this.GetType() + ".FixedUpdateVirtual: " + total);
#else
            FixedUpdateVirtual();
#endif
        }
    }

    //
    // LateUpdateVirtual
    //
    protected virtual void LateUpdateVirtual()
    {
    }

    void LateUpdate()
    {
        if (Inited)
        {
#if ENABLE_TIMERS
            float start = Time.realtimeSinceStartup;
            //Profiler.BeginSample(name + "." + this.GetType() + ".LateUpdateVirtual");
            LateUpdateVirtual();
            //Profiler.EndSample();
            float finish = Time.realtimeSinceStartup;
            float total = 1000.0f * (finish - start);
            if (bPrintTiming && total > 1)
                Debug.Log(name + "." + this.GetType() + ".LateUpdateVirtual: " + total);
#else
            LateUpdateVirtual();
#endif
        }
    }


    //
    // OnTriggerEnterVirtual
    //
    protected virtual void OnTriggerEnterVirtual(Collider other)
    {
    }

    void OnTriggerEnter(Collider other)
    {
        OnTriggerEnterVirtual(other);
    }

    //
    // OnTriggerStayVirtual
    //
    protected virtual void OnTriggerStayVirtual(Collider other)
    {
    }

    void OnTriggerStay(Collider other)
    {
        OnTriggerStayVirtual(other);
    }

    //
    // OnTriggerExitVirtual
    //
    protected virtual void OnTriggerExitVirtual(Collider other)
    {
    }

    void OnTriggerExit(Collider other)
    {
        OnTriggerExitVirtual(other);
    }

    //
    // OnCollisionEnterVirtual
    //
    protected virtual void OnCollisionEnterVirtual(Collision other)
    {
    }

    void OnCollisionEnter(Collision other)
    {
        OnCollisionEnterVirtual(other);
    }

    //
    // OnCollisionStayVirtual
    //
    protected virtual void OnCollisionStayVirtual(Collision other)
    {
    }

    void OnCollisionStay(Collision other)
    {
        OnCollisionStayVirtual(other);
    }

    //
    // OnTriggerExitVirtual
    //
    protected virtual void OnCollisionExitVirtual(Collision other)
    {
    }

    void OnCollisionExit(Collision other)
    {
        OnCollisionExitVirtual(other);
    }

    //
    // OnRenderObjectVirtual
    //
    protected virtual void OnRenderObjectVirtual()
    {
    }

    void OnRenderObject()
    {
        OnRenderObjectVirtual();
    }


    //
    // Those two functions aren't hooked up ON PURPOSE. DO NOT USE THEM IN YOUR CHILD CLASS.
    // Implement the unity OnEnable() or OnDisable() directly instead.
    // Be carefull though if for example a child AND a grand child needs it to not have them both implement the same function.
    // Making those virtual is causing all sorts of issues.
    //
    // OnEnableVirtual
    //
    protected virtual void OnEnableVirtual()
    {
        Debug.LogError("OnEnableVirtual is called on object \"" + gameObject.name + "\". Implement Unity OnEnable() function directly instead.");
    }

    //
    // OnDisableVirtual
    //
    protected virtual void OnDisableVirtual()
    {
        Debug.LogError("OnDisableVirtual is called on object \"" + gameObject.name + "\". Implement Unity OnDisable() function directly instead.");
    }

    // Run the given coroutine synchronously (ie. don't yield)
    public static void RunCoroutineSync(IEnumerator coroutine)
    {
        while (true)
        {
            if (!coroutine.MoveNext())
            {
                break;
            }
            object yielded = coroutine.Current;
            Debug.Log(yielded);
        }
    }

    // Run the given coroutine asynchronously (ie. yield to allow game objects a chance to update)
    public static IEnumerator RunCoroutine(IEnumerator coroutine)
    {
        while (true)
        {
            if (!coroutine.MoveNext())
            {
                yield break;
            }
            object yielded = coroutine.Current;
            Debug.Log(yielded);
            yield return coroutine.Current;
        }
    }
}
