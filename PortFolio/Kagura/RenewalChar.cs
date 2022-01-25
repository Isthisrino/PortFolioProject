using UnityEngine;

using Kagura.Library.Common.Data;
using Kagura.Library.Common.Util;

public class RenewalChar : RenewalUnit
{
    public UserDBCharacter MasterCharDBProperty { get; private set; }
    public Table_character_base MasterCharBaseTableProperty { get; private set; }
    private Table_character_ani_resource MasterCharAniTableProperty;


    protected override void InitializeGroup()
    {
        StopFSM();
        base.InitializeGroup();
    }
    #region setting Method
    public void SettingCharacter(UserDBCharacter _charDB, Transform _battleRoot, Vector3 _firstPosition, float _gameSpeed)
    {
        InitializeGroup();

        SettingCharDBTable(_charDB);
        SettingStat();
        SettingSkill();
        SettingModel();
        SettingFSM();
        SettingAtkArea();
        SettingHitBox();
        SettingArea();
        SettingParentsTransform(_battleRoot);
        SettingUnitPosition(_firstPosition);
        SettingGameSpeed(_gameSpeed);
        SettingActiveOnUnit();

        ChangeFSM(Enumerations.eFSMState.State_Appear);
    }
    void SettingCharDBTable(UserDBCharacter _charDB)
    {
        MasterCharDBProperty = _charDB;
        MasterCharBaseTableProperty = DataManager.Instance.GetTableCharacter(MasterCharDBProperty);
        MasterCharAniTableProperty = DataManager.Instance.GetTableData<Table_character_ani_resource>(MasterCharBaseTableProperty.body_codename_ID);
    }

    protected override void SettingStat()
    {
        RenewalCharStatController charStatController = StatControllerProperty as RenewalCharStatController;
        charStatController.SettingCharStatController(MasterCharDBProperty, MasterCharBaseTableProperty);
    }
    protected override void SettingSkill()
    {
        SkillControllerProperty.SettingSkillController(this, MasterCharBaseTableProperty.base_attack_skill_ID, MasterCharBaseTableProperty.active_attack_skill_ID);
    }

    protected override void SettingModel()
    {
        CharModelController charModelController = ModelControllerProperty as CharModelController;
        charModelController.SettingCharModel(MasterCharBaseTableProperty, MasterCharAniTableProperty);
    }

    protected override void SettingFSM()
    {
        CharFSMController charFSMController = FSMControllerProperty as CharFSMController;
        charFSMController.SettingCharFSMController(MasterCharAniTableProperty);
    }

    protected override void SettingAtkArea()
    {
        AtkAreaControllerProperty.SettingAtkAreaController(MonsterHitBoxTag);
        AtkAreaControllerProperty.StartAtkAreaController();
    }
    protected override void SettingHitBox()
    {
        HitBoxControllerProperty.SettingHitBoxController(CharHitBoxTag);
    }
    protected override void SettingArea()
    {
        AreaControllerProperty.SettingAreaController(MonsterHitBoxTag);
    }
    RenewalCharSkillObj CharSkillObj;
    public void SettingSkillUI(RenewalCharSkillObj _skillObj)
    {
        CharSkillObj = _skillObj;
    }
    #endregion

    #region Unit GameControl Method
    public override void SettingMasterUnitDieState()
    {
        base.SettingMasterUnitDieState();
        if (CharSkillObj != null)
            CharSkillObj.SettingMasterUnitDieState();
    }
    public void UpdateSpUI(float _spPer = 0f)
    {
        if (CharSkillObj != null)
            CharSkillObj.SettingSpPer(_spPer);
    }
    #endregion

    #region Skill Control
    public override void AddMasterUnitAutoSkillManager()
    {
        BattleModeProperty.AddCharUnitAutoSkillManager(this);
    }
    public void AddMasterUnitMenualSkillManager()
    {
        BattleModeProperty.AddCharUnitMenualSkillManager(this);
    }
    #endregion        

    #region ease Method
    public override string GetPackageName()
    {
        return MasterCharBaseTableProperty.package_name;
    }
    public override string GetEffectPath(string _fileName)
    {
        return string.Format(EffectPath, _fileName);
    }

    #endregion

    #region like Define Keyword
    readonly string EffectPath = "Assets/AssetBundle/Monsters/Effect/{0}.asset";
    #endregion
}
