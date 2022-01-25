using System.Collections.Generic;
using UnityEngine;

using Kagura.Library.Common.Util;

public abstract class RenewalUnit : MonoBehaviour
{
    [Header("BattleMode Group")]
    [SerializeField] protected BattleModeBase BattleModeProperty;
    public Enumerations.eBattleState BattleModeStateProperty
    {
        get
        {
            return BattleModeProperty.BattleStateProperty;
        }
    }

    [Header("Controller Group")]
    [SerializeField] protected RenewalStatController StatControllerProperty;
    [SerializeField] protected SkillController SkillControllerProperty;
    [SerializeField] protected ModelController ModelControllerProperty;
    [SerializeField] protected FSMController FSMControllerProperty;
    [SerializeField] protected RenewalPushController PushControllerProperty;
    [SerializeField] protected HitBoxController HitBoxControllerProperty;
    [SerializeField] protected ScanTargetController ScanTargetControllerProperty;
    [SerializeField] protected AtkRangeAreaController AtkAreaControllerProperty;
    [SerializeField] protected RenewalAreaController AreaControllerProperty;
    [SerializeField] protected Rigidbody RigidBodyProperty;

    [Header("Value Group")]
    [SerializeField] private int DefaultMassValue = 10;
    /// <summary>
    /// For Unit Pause
    /// </summary>
    private readonly float DefaultTimeScale = 1.2f;
    private float TimeScale = 1.2f;
    public float TimeScaleProperty
    {
        get
        {
            return TimeScale;
        }
        protected set
        {
            if (value > 0f)
                TimeScale = Mathf.Max(DefaultTimeScale, value);
            else
                TimeScale = DefaultTimeScale;
            ModelControllerProperty.SettingTimeScale(TimeScale);
        }
    }
    private bool UnitPause { get; set; } = false;
    public bool UnitPauseProperty
    {
        get
        {
            return UnitPause;
        }
        set
        {
            UnitPause = value;
            if (UnitPause)
                ModelControllerProperty.SettingTimeScale(0f);
            else
                ModelControllerProperty.SettingTimeScale(TimeScaleProperty);
        }
    }

    /// <summary>
    /// For Unit Control
    /// </summary>
    public long NowHpProperty
    {
        get
        {
            return StatControllerProperty.NowHp;
        }
    }

    public Vector3 UnitLocalPosProperty
    {
        get
        {
            Vector3 outValue = transform.localPosition;
            outValue.z = 0;
            return outValue;
        }
        set
        {
            Vector3 inputValue = value;
            inputValue.z = ((int)inputValue.y);
            transform.localPosition = inputValue;
        }
    }
    public Vector3 UnitModelHitPosProperty
    {
        get
        {
            return UnitLocalPosProperty + UnitHitBoxCenterPosProperty;
        }

    }
    public Vector3 UnitHitBoxCenterPosProperty
    {
        get
        {
            return HitBoxControllerProperty.GetHitBoxCenter();
        }
    }

    [SerializeField] protected RenewalUnit TargetUnit;
    public RenewalUnit TargetUnitProperty
    {
        get
        {
            return TargetUnit;
        }
        set
        {
            TargetUnit = value;
        }
    }

    public bool IdleMode;

    protected virtual void InitializeGroup()
    {
        InitializeUnitPause();
        InitialzieUnitTimeScale();
        InitializeRigidBody();

        StatControllerProperty.InitializeStatController();
        SkillControllerProperty.InitializeSkillController();
        ModelControllerProperty.InitializeModelController();
        FSMControllerProperty.InitializeFSMController();
        HitBoxControllerProperty.InitializeHitBoxController();
        PushControllerProperty.InitializePushController();
        ScanTargetControllerProperty.InitializeScanTargetContorller();
        AtkAreaControllerProperty.InitializeAtkAreaController();
        AreaControllerProperty.InitializeAreaController();
    }
    protected abstract void SettingStat();
    protected abstract void SettingSkill();
    protected abstract void SettingModel();
    protected abstract void SettingFSM();
    protected abstract void SettingAtkArea();
    protected abstract void SettingHitBox();
    protected abstract void SettingArea();

    #region BattleMode Method
    /// <summary>
    /// 투사체(공격 판정) 가져오는 함수
    /// </summary>
    /// <returns></returns>
    public Transform GetBattleRootTransform()
    {
        return BattleModeProperty.BattleRootProperty;
    }
    #endregion

    #region TargetUnit Method
    [HideInInspector]
    public bool TargetUnitLost
    {
        get
        {
            return TargetUnitProperty == null || TargetUnitProperty.NowHpProperty <= 0;
        }
    }
    public bool CheckTargetUnit(RenewalUnit _enterAtkAreaUnit)
    {
        if (TargetUnitLost)
            return false;
        return _enterAtkAreaUnit.Equals(TargetUnitProperty);
    }
    public virtual bool CheckTargetUnitLost()
    {
        return TargetUnitLost;
    }
    #endregion

    #region Unit GameControl Method
    public virtual void SettingGameSpeed(float _gameSpeed)
    {
        TimeScaleProperty = _gameSpeed;
    }
    public void SettingUnitPause()
    {
        UnitPauseProperty = true;
    }
    public void SettingUnitPlay()
    {
        InitializeUnitPause();
    }
    protected void InitializeUnitPause()
    {
        UnitPauseProperty = false;
    }
    public void SettingActiveOnUnit()
    {
        gameObject.SetActive(true);
    }
    public void SettingActiveOffUnit()
    {
        gameObject.SetActive(false);
    }
    void InitialzieUnitTimeScale()
    {
        TimeScaleProperty = DefaultTimeScale;
    }
    public virtual void SettingMasterUnitDieState()
    {
        PushControllerProperty.SettingMasterUnitDieState();
        HitBoxControllerProperty.SettingMasterUnitDieState();
        ScanTargetControllerProperty.SettingMasterUnitDieState();
        AtkAreaControllerProperty.SettingMasterUnitDieState();
        AreaControllerProperty.SettingMasterUnitDieState();
    }
    #endregion

    #region Ease Method
    public virtual string GetPackageName()
    {
        return string.Empty;
    }
    public virtual string GetAssetBundleName()
    {
        return string.Empty;
    }
    public virtual string GetEffectPath(string _fileName)
    {
        return string.Empty;
    }
    public string GetCommonEffectPath(string _fileName)
    {
        return string.Format("Common/Effect/{0}", _fileName);
    }
    #endregion

    #region Unit Transform Method
    public void SettingUnitPosition(Vector3 _posValue)
    {
        UnitLocalPosProperty = _posValue;
        ModelControllerProperty.SettingSortOrder();
    }
    public void AddedUnitPosition(Vector3 _addPosValue)
    {
        UnitLocalPosProperty += _addPosValue;
        ModelControllerProperty.SettingSortOrder();
    }
    protected void SettingParentsTransform(Transform _battleRoot)
    {
        transform.parent = _battleRoot;
    }
    public float GetDistancToScaningUnit(RenewalUnit _targetUnit)
    {
        float distanceValue = Vector3.Distance(UnitLocalPosProperty, _targetUnit.UnitLocalPosProperty);
        return distanceValue;
    }
    public virtual float GetMoveSpeedValue()
    {
        return Time.deltaTime * TimeScaleProperty * StatControllerProperty.GetStatTableMoveSpeed();
    }
    public Vector3 GetTargetDirection()
    {
        if (TargetUnitProperty == null)
            return Vector3.zero;

        //switch (UnitTypeProperty)
        //{
        //    case Enumerations.eUnitType.MeleeType:
                Vector3 targetPos = TargetUnitProperty.GetClosedAreaPos(this);
                return Vector3.Normalize(targetPos - UnitLocalPosProperty);
        //    default:
        //    case Enumerations.eUnitType.RangeType:
        //        return Vector3.Normalize(TargetUnitProperty.UnitLocalPosProperty - UnitLocalPosProperty);
        //}

        return Vector3.Normalize(TargetUnitProperty.UnitLocalPosProperty - UnitLocalPosProperty);
    }
    public void KnockBackUnit(Vector3 _knockBackDir, float _knockBackForce, float _knockBackDuration)
    {
        //유닛에게 넉백정보를 저장해둔 다음 changeFsm
    }
    #endregion

    #region StatController Method
    public float GetUnitBaseScale()
    {
        return ModelControllerProperty.GetBaseScale();
    }
    public void GetUnitDamagePower(RenewalUnit _targetUnit, long _skillPower, out long _hitDamage, out long _addDamege, out bool _isCritical, out bool _isPierce, out bool _isHit)
    {
        StatControllerProperty.ToDamagePower(_targetUnit, _skillPower, out _hitDamage, out _addDamege, out _isCritical, out _isPierce, out _isHit);
    }
    public void UnitAddSp(long _addedSpValue)
    {
        StatControllerProperty.AddSp(_addedSpValue);
    }
    public long GetUnitAbliityValue(eAbility_type _abilityType)
    {
        return StatControllerProperty.GetTotalAbilityValue(_abilityType);
    }
    public bool GetSkillReserve()
    {
        return StatControllerProperty.IsResreveSkill && CheckSkillUsePossible();
    }
    public void SettingSkillReserve()
    {
        StatControllerProperty.IsResreveSkill = true;
    }
    public void SettingSkillReserveFalse()
    {
        StatControllerProperty.IsResreveSkill = false;
    }
    public Vector3 GetMeleeSkillAtkRange()
    {
        return StatControllerProperty.MeleeSkillRange;
    }
    public Vector3 GetRangeSkillAtkRange()
    {
        return StatControllerProperty.RangeSkillRange;
    }
    public void SelectSkillAction()
    {
        List<RenewalUnit> targetUnitList = ScanTargetControllerProperty.GetScaningTargetList();
        TargetUnitProperty = StatControllerProperty.SelectSkillAndTargetUnit(targetUnitList);
        if (TargetUnitProperty == null)
        {
            ChangeFSM(Enumerations.eFSMState.State_Idle);
            return;
        }

        // -> 스킬 예약 상태에 따라 범위 설정
        if (GetSkillReserve())
            SettingAtkAreaSize(GetRangeSkillAtkRange());
        else
            SettingAtkAreaSize(GetMeleeSkillAtkRange());

        if (!CheckEnterUnitEqualTargetUnit())
            ChangeFSM(Enumerations.eFSMState.State_Move);
        else
        {
            // 여기서도 상태에 따라 범위 설정
            if (GetSkillReserve())
                ChangeFSM(Enumerations.eFSMState.State_ActiveSkill);
            else
                ChangeFSM(Enumerations.eFSMState.State_NormalSkill);
        }
    }

    public void HitUnit(RenewalUnit _atteckerUnit, int _skillHitSoundID, int _skillHitEffetID, int _skillChargePoint, long _skillPower)
    {
        StatControllerProperty.HitUnit(_atteckerUnit, _skillHitSoundID, _skillHitEffetID, _skillChargePoint, _skillPower);
    }
    public Table_skill GetUnitNormalSkillTable()
    {
        return StatControllerProperty.NormalAtkSkill;
    }
    public Table_skill GetUnitActiveSkillTable()
    {
        return StatControllerProperty.ActiveAtkSkill;
    }
    public virtual bool CheckSkillUsePossible()
    {
        return StatControllerProperty.CheckSkillUsePossible();
    }
    public virtual eCharacter_type GetMasterUnitType()
    {
        return eCharacter_type.NONE;
    }
    #endregion

    #region Skill Controller
    public void StartSkill(Enumerations.eSkillType _skillType)
    {
        SkillControllerProperty.StartSkill(_skillType);
    }
    public ProjectileObject RentProjectileObject()
    {
        return BattleModeProperty.RentProjectileObject();
    }
    public virtual void AddMasterUnitAutoSkillManager()
    {

    }
    #endregion

    #region ModelController Method
    public void SettingUnitAnimation(string _aniName, bool _isAniLoop = false, bool _isCheckSameAni = true)
    {
        ModelControllerProperty.SettingAnimation(_aniName, _isAniLoop, _isCheckSameAni);
    }
    public void SettingAniCompleteTrackEntry(Spine.AnimationState.TrackEntryDelegate _trackEntryComplate)
    {
        ModelControllerProperty.SettingTrackEntryComplete(_trackEntryComplate);
    }
    public void SettingAniCompleteTrackEntryEmpty()
    {
        ModelControllerProperty.SettingTrackEntryCompleteEmpty();
    }
    public void SettingAniEventTrackEntry(Spine.AnimationState.TrackEntryEventDelegate _trackEntryEvent)
    {
        ModelControllerProperty.SettingTrackEntryEvent(_trackEntryEvent);
    }
    public void SettingAniEventTrackEntryEmpty()
    {
        ModelControllerProperty.SettingTrackEntryEventEmpty();
    }
    public void SettingUnitSortOrder()
    {
        ModelControllerProperty.SettingSortOrder();
    }
    public int GetUnitSortOrder()
    {
        return ModelControllerProperty.GetSortOrder();
    }
    public Spine.Bone GetUnitHitEffectBone()
    {
        return ModelControllerProperty.GetHitEffectBone();
    }
    public virtual void ReverseModelXScale(Vector3 _dir)
    {
        bool filpX = _dir.x < 0;
        ModelControllerProperty.ReverseXScale(filpX);
    }
    public virtual void ReverseModelXScaleAtk()
    {
        bool filpX = TargetUnitProperty.UnitLocalPosProperty.x < UnitLocalPosProperty.x;
        ModelControllerProperty.ReverseXScale(filpX);
    }
    #endregion

    #region FSM Method
    public void ChangeFSM(Enumerations.eFSMState _afterState, float _delayTime = 0f)
    {
        FSMControllerProperty.ChangeFSMState(_afterState, _delayTime);
    }
    public void StopFSM()
    {
        FSMControllerProperty.StopFSM();
    }
    public Enumerations.eFSMState GetUnitCurrentState()
    {
        return FSMControllerProperty.GetCurrnetState();
    }
    #endregion      

    #region HitBoxController Method
    public string GetUnitTag()
    {
        return HitBoxControllerProperty.GetUnitTag();
    }
    #endregion

    #region ScanController Method
    public void StartTargetUnitScaning()
    {
        ScanTargetControllerProperty.StartScaning();
    }
    public void EndTargetUnitScaning()
    {
        ScanTargetControllerProperty.InitializeScanArea();
    }
    public List<RenewalUnit> GetTargetList()
    {
        return ScanTargetControllerProperty.GetScaningTargetList();
    }
    #endregion

    #region AtkAreaController Method
    public void StartAtkAreaController()
    {
        AtkAreaControllerProperty.StartAtkAreaController();
    }
    public void SettingAtkAreaSize(Vector3 _atkAreaSize)
    {
        AtkAreaControllerProperty.SettingAtkRange(_atkAreaSize);
    }
    public bool CheckEnterUnitEqualTargetUnit()
    {
        return AtkAreaControllerProperty.CheckEnterUnitEqualTargetUnit();
    }
    public string GetTargetHitBoxTag()
    {
        return AtkAreaControllerProperty.TargetHitBoxTagProperty;
    }
    public string GetAllyHitBoxTag()
    {
        return HitBoxControllerProperty.GetUnitTag();
    }
    #endregion

    #region AreaController Method
    public Vector3 GetClosedAreaPos(RenewalUnit _atkUnit)
    {
        if (AreaControllerCheckEnterArea(_atkUnit))
            return UnitLocalPosProperty;

        Vector3 leftArea = AreaControllerProperty.GetLeftAreaPos();
        Vector3 rightArea = AreaControllerProperty.GetRightAreaPos();

        float leftAreaToAtkUnit = Vector3.Distance(leftArea, _atkUnit.UnitLocalPosProperty);
        float rightAreaToAtkUnit = Vector3.Distance(rightArea, _atkUnit.UnitLocalPosProperty);

        if (leftAreaToAtkUnit < rightAreaToAtkUnit)
            return leftArea;
        else
            return rightArea;
    }
    public bool AreaControllerCheckEnterArea(RenewalUnit _atkUnit)
    {
        return AreaControllerProperty.CheckEnterAreaTargetUnit(_atkUnit);
    }
    #endregion

    #region RigidBody Method
    void InitializeRigidBody()
    {
        RigidBodyProperty.mass = DefaultMassValue;
    }
    public void SettingRigidBodyMassIdle()
    {
        InitializeRigidBody();
    }
    public void SettingRigidBodyMass(int _changeMassValue)
    {
        RigidBodyProperty.mass = _changeMassValue;
    }
    #endregion

    #region Collision Method
    //[SerializeField] Vector3 CollectionDirPos = Vector3.zero;
    private void OnCollisionEnter(Collision _collision)
    {
        if (_collision.gameObject.tag.Equals(PushColliderTag))
            ModelControllerProperty.SettingSortOrder();

        if (!GetUnitCurrentState().Equals(Enumerations.eFSMState.State_Move))
            return;

        RenewalUnit collisionEnterUnit = _collision.gameObject.GetComponentInParent<RenewalUnit>();
        if (collisionEnterUnit == null)
            return;


        Vector3 collisionDir = Vector3.Normalize(collisionEnterUnit.UnitLocalPosProperty - UnitLocalPosProperty);
        Vector3 normalVector = Vector3.zero;
        Vector3.OrthoNormalize(ref collisionDir, ref normalVector);
        //CollectionDirPos = Vector3.Project(collisionDir, normalVector);
        //CollectionDirPos = Vector3.Reflect(collisionDir, normalVector);
        Vector3 projectVector = Vector3.Project(collisionDir, normalVector);
        //Vector3 reflectVector = Vector3.Reflect(collisionDir, normalVector);
        RigidBodyProperty.AddForce(projectVector, ForceMode.Impulse);
    }
    private void OnCollisionExit(Collision _collision)
    {
        if (_collision.gameObject.tag.Equals(PushColliderTag))
            ModelControllerProperty.SettingSortOrder();

        //if (!GetUnitCurrentState().Equals(Enumerations.eFSMState.State_Move))
        //    return;
    }
    #endregion

    #region like Define Keyword
    public readonly string PushColliderTag = "PushCollider";

    public readonly string CharHitBoxTag = "CharTargetCollider";
    public readonly string MonsterHitBoxTag = "MonsterTargetCollider";

    public readonly string AtkEventTag = "hit";
    #endregion
}