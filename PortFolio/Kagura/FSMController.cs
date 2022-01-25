using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Kagura.Library.Common.Util;

public abstract class FSMController : MonoBehaviour
{
    [SerializeField] protected RenewalUnit MasterUnitProperty;
    [SerializeField] protected Enumerations.eFSMState CurrentStateType { get; set; }
    [SerializeField] protected Enumerations.eFSMState PreviewStateType { get; set; }
    [SerializeField] protected RenewalFSMState CurrentStateFSM;
    [SerializeField] protected RenewalFSMState PreveiwStateFSM;
    [SerializeField] protected Dictionary<Enumerations.eFSMState, RenewalFSMState> DicFSM = new Dictionary<Enumerations.eFSMState, RenewalFSMState>();

    Coroutine NowFSMRoutine;
    public void InitializeFSMController()
    {
        CurrentStateType = Enumerations.eFSMState.State_None;
        PreviewStateType = Enumerations.eFSMState.State_None;
        CurrentStateFSM = null;
        PreveiwStateFSM = null;
        DicFSM.Clear();
        NowFSMRoutine = null;
    }
    public void ChangeFSMState(Enumerations.eFSMState _afterState, float _delayTime = 0f)
    {
        if (!DicFSM.ContainsKey(_afterState))
            return;

        RenewalFSMState nextStateFSM;
        if (DicFSM.TryGetValue(_afterState, out nextStateFSM) == false)
            return;

        if (CurrentStateFSM != null)
        {
            CurrentStateFSM.EndFSMAction();
            StopFSM();
        }
        PreveiwStateFSM = CurrentStateFSM;
        if (PreveiwStateFSM != null)
            PreviewStateType = PreveiwStateFSM.FSMStateType;
        CurrentStateFSM = nextStateFSM;
        if (CurrentStateFSM != null)
        {
            CurrentStateType = CurrentStateFSM.FSMStateType;
            CurrentStateFSM.InitFSMAction(_delayTime);
            NowFSMRoutine = StartCoroutine(CurrentStateFSM.UpdateCurrentFSM());
        }
    }
    public void StopFSM()
    {
        if (NowFSMRoutine == null)
            return;
        StopCoroutine(NowFSMRoutine);
        NowFSMRoutine = null;
    }
    protected abstract void SettingFSMState();
    public Enumerations.eFSMState GetCurrnetState()
    {
        return CurrentStateType;
    }
}
