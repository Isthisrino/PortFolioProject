using UnityEngine;

using Kagura.Library.Common.Util;

public class RenewalMonster : RenewalUnit
{
    private int MonsterLevelProperty;
    private Table_monster MonsterTableProperty;
    private Table_monster_resource MonsterResourceTableProperty;
    private Table_monster_model MonsterModelTableProperty;

    public void SettingMonster(Table_monster _monsterTable, int _monsterLevel, Transform _battleRoot, Vector3 _firstPosition, float _gameSpeed)
    {
        InitializeGroup();
        SettingMonsterDBTable(_monsterTable, _monsterLevel);
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
    protected override void InitializeGroup()
    {
        StopFSM();
        base.InitializeGroup();
    }
    void SettingMonsterDBTable(Table_monster _monsterTable, int _monsterLevel)
    {
        MonsterTableProperty = _monsterTable;
        MonsterResourceTableProperty = DataManager.Instance.GetTableData<Table_monster_resource>(MonsterTableProperty.resource_ID);
        MonsterModelTableProperty = DataManager.Instance.GetTableData<Table_monster_model>(MonsterResourceTableProperty.body_codename_ID);
        MonsterLevelProperty = _monsterLevel;
    }
    protected override void SettingStat()
    {
        RenewalMonsterStatController monsterStatController = StatControllerProperty as RenewalMonsterStatController;
        monsterStatController.SettingMonsterStatController(MonsterTableProperty, MonsterLevelProperty);
    }
    protected override void SettingSkill()
    {
        SkillControllerProperty.SettingSkillController(this, MonsterTableProperty.skill_01_ID, MonsterTableProperty.skill_02_ID);
    }
    protected override void SettingModel()
    {
        MonsterModelController monsterModelController = ModelControllerProperty as MonsterModelController;
        monsterModelController.SettingMonsterModel(MonsterTableProperty, MonsterResourceTableProperty, MonsterModelTableProperty);
    }
    protected override void SettingFSM()
    {
        MonsterFSMController monsterFSMController = FSMControllerProperty as MonsterFSMController;
        monsterFSMController.SettingMonsterFSMController(MonsterModelTableProperty);
    }
    protected override void SettingAtkArea()
    {
        AtkAreaControllerProperty.SettingAtkAreaController(CharHitBoxTag);
    }
    protected override void SettingHitBox()
    {
        HitBoxControllerProperty.SettingHitBoxController(MonsterHitBoxTag);
    }
    protected override void SettingArea()
    {
        AreaControllerProperty.SettingAreaController(CharHitBoxTag);
    }

    #region Unit Transform Method
    public override float GetMoveSpeedValue()
    {
        return StatControllerProperty.GetStatTableMoveSpeed() * TimeScaleProperty * Time.deltaTime;
    }
    #endregion

    #region ModelController Method
    public override void ReverseModelXScaleAtk()
    {
        bool filpX = TargetUnitProperty.UnitLocalPosProperty.x < UnitLocalPosProperty.x;
        ModelControllerProperty.ReverseXScale(filpX);
    }
    #endregion

    public void SettingMonsterDisappeared(Transform _monsterPoolingRoot)
    {
        transform.localPosition = new Vector3(3000, 2000);
        transform.parent = _monsterPoolingRoot;
        transform.localPosition = Vector3.zero;
    }
    public int GetMonsterIdx()
    {
        return MonsterTableProperty.index;
    }
    public override string GetAssetBundleName()
    {
        return MonsterAssetBundleTag;
    }
    public override string GetEffectPath(string _fileName)
    {
        return string.Format("{0}Effect/{1}.asset", MonsterPath, _fileName);
    }
    #region Unit GameControl Method
    public override void SettingMasterUnitDieState()
    {
        base.SettingMasterUnitDieState();
    }


    #endregion
    #region like Define Keyword
    readonly string MonsterAssetBundleTag = "monsters";
    readonly string MonsterPath = "Assets/AssetBundle/Monsters/";
    #endregion
}
