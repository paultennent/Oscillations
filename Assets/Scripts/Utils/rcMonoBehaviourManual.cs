using UnityEngine;
using System.Collections;

public class rcMonoBehaviourManual : MonoBehaviour
{
    public virtual void AwakeManual() { }
    public virtual void StartManual() { }
    public virtual void UpdateManual(float zRealDT, float zDT) { }  // zRealDT is the "real delta time" i.e. doesn't scale when Time.timeScale is changed, but does get set to zero when paused
    public virtual void LateUpdateManual() { }
    public virtual void FixedUpdateManual() { }
}
