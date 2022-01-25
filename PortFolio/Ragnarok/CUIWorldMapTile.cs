using UnityEngine;
using System.Collections.Generic;

public class CUIWorldMapTile : CPoolBase
{
    // [Inspector Field]
    [SerializeField] private RectTransform ins_rtraTile = null;

    [SerializeField] private Transform ins_traCenterRoot = null;
    [SerializeField] private Transform ins_traLeftTopRoot = null;
    [SerializeField] private Transform ins_traRightTopRoot = null;
    [SerializeField] private Transform ins_traIconGroupRoot = null;

    // Private Field
    private CPopupWorldMapUI _cPopUpWorldMapUI = null;

    private CWoldMapNode _cWorldNodeData;
    private CDataEAssetWorldNodeInfoData _cWorldNodeInfoData;
    private CFunc.OnVoidFuncCUIWoldMapTile _OnClickMapTileMethod = null;
    private CUIWorldMapTileIcon _cUIQstProgressIcon = null;

    private Vector2 _vecTileSize = new Vector2(128f, 128f);

    private int _nQuestID = CDataGameInfo.M_nDefaultValue;
    private bool _bIsNotViewWorldMap = false;

    private delegate bool OnBoolFuncWorldMapNode(CWoldMapNode cTargetWorldNode, bool bIsDungeon = false);

    // override Method
    public override void InitPool(EmPoolState emPoolState)
    {
        switch (emPoolState)
        {
            case EmPoolState.Reset:
            case EmPoolState.Destroy:
                DisposeField();
                break;
        }
    }

    // Setting Method
    public void InitMapTile(CWoldMapNode cWoldNodeData, CPopupWorldMapUI cPopUpWorldMapUI, CFunc.OnVoidFuncCUIWoldMapTile onClickTileDelegate)
    {
        _cPopUpWorldMapUI = cPopUpWorldMapUI;
        _cWorldNodeData = cWoldNodeData;
        _cWorldNodeInfoData = cWoldNodeData.m_cNodeInfoData;

        _OnClickMapTileMethod = onClickTileDelegate;

        InitTilePosition();
        InitTileIcon();
    }
    private void InitTilePosition()
    {
        ins_rtraTile.SetParent(_cPopUpWorldMapUI.GetTileRoot());
        ins_rtraTile.sizeDelta = _vecTileSize;
        ins_rtraTile.anchoredPosition = new Vector3((_cWorldNodeData.m_cNodeInfoData.m_nUIGridX - 1) * ins_rtraTile.sizeDelta.x, -(_cWorldNodeData.m_cNodeInfoData.m_nUIGridY - 1) * ins_rtraTile.sizeDelta.y);
    }
    private void InitTileIcon()
    {
        CPopupWorldMapUI.EmIconFilterType emIconFilterType = _cPopUpWorldMapUI.GetIconFilterType();
        bool bPlayerPoint = IsInitIconTemplete(IsInitPlayerPointTemplete);
        bool bWayPointIconOn = CUtil.IsCheckBitValue((int)emIconFilterType, (int)CPopupWorldMapUI.EmIconFilterType.WayPoint);

        if (bPlayerPoint)
        {
            if (bWayPointIconOn)
            {
                // 플레이어가 떠있는데 안쪽 웨이포인트가 떠야할 때 (ex. 플레이어가 프론테라에 있는데 지하 감옥 워프 웨이포인트가 떠야할 때)
                InitWayPointInnerDungeon();
            }
        }
        else
        {
            bool bTownIconOn = (emIconFilterType & CPopupWorldMapUI.EmIconFilterType.Town) != 0;
            if (bTownIconOn && InitTownIcon(emIconFilterType) == false)
            {
                if (bWayPointIconOn && InitWayPointIcon(emIconFilterType) == false)
                {
                    InitWayPointInnerDungeon();
                }
            }
        }
        // 1. 마을 + 일반 웨이포인트는 플레이어 포인트가 떠있을 때 뜨면 안됨 
        // 2. 플레이어 포인트가 떠있을 때 던전 웨이포인트 나와야 함 
        // 3. 플레이어 포인트가 없을 때, 마을 + 일반 웨이포인트가 있으면 던전 포인트는 뜨면 안됨 

        _cUIQstProgressIcon = null;
        IsInitIconTemplete(IsInitQstProgressTemplate);

        bool bIsQuestIconOn = CUtil.IsCheckBitValue((int)emIconFilterType, (int)CPopupWorldMapUI.EmIconFilterType.Quest);
        if (bIsQuestIconOn)
        {
            IsInitIconTemplete(IsInitQstTemplate);
        }

        bool bIsFieldBossIconOn = CUtil.IsCheckBitValue((int)emIconFilterType, (int)CPopupWorldMapUI.EmIconFilterType.FieldBoss);
        if (bIsFieldBossIconOn)
        {
            IsInitIconTemplete(IsInitBossIconTemplate, true, false);
        }

        bool bIsDungeonPortalIconOn = CUtil.IsCheckBitValue((int)emIconFilterType, (int)CPopupWorldMapUI.EmIconFilterType.DungeonPortal);
        if (bIsDungeonPortalIconOn)
        {
            InitDungeonPortalIcon();
        }

        bool bIsItemIconOn = CUtil.IsCheckBitValue((int)emIconFilterType, (int)CPopupWorldMapUI.EmIconFilterType.Item);
        if (bIsItemIconOn && _cPopUpWorldMapUI.GetSelectStuffID() > 0)
        {
            IsInitIconTemplete(IsInitItemTileIconTemplate);
        }

        bool bIsBookMarkIconOn = CUtil.IsCheckBitValue((int)emIconFilterType, (int)CPopupWorldMapUI.EmIconFilterType.BookMark);
        if (bIsBookMarkIconOn)
        {
            InitBookMarkIcon();
        }

        bool bIsFestaMarkIconOn = CUtil.IsCheckBitValue((int)emIconFilterType, (int)CPopupWorldMapUI.EmIconFilterType.FestaEvent);
        if (bIsFestaMarkIconOn)
        {
            InitDropFestaCoinIcon();
        }
    }
    private bool IsInitIconTemplete(OnBoolFuncWorldMapNode onBoolFunc, bool bInitDungeon = true, bool bInitIndun = true)
    {
        if (onBoolFunc(_cWorldNodeData))
        {
            return true;
        }
        if (bInitDungeon && IsInitInnerMap(_cWorldNodeInfoData.m_listInnerDungeon, onBoolFunc))
        {
            return true;
        }
        if (bInitIndun && IsInitInnerMap(_cWorldNodeInfoData.m_listInnerIndun, onBoolFunc))
        {
            return true;
        }
        return false;
    }
    private bool IsInitPlayerPointTemplete(CWoldMapNode cTargetWorldNode, bool bIsDungeon = false)
    {
        bool bPlayerPointNode = CDataManager.In.m_cDataEAssetMap.IsCurrentMap(cTargetWorldNode.m_nGridX, cTargetWorldNode.m_nGridY);
        if (bPlayerPointNode)
        {
            CUIWorldMapTileIcon cTileIcon = _cPopUpWorldMapUI.GetWorldMapTileIcon();
            cTileIcon.InitRole(CUIWorldMapTileIcon.EmWorldMapIconType.PlayerPoint, ins_traCenterRoot, -1);
            return true;
        }
        return false;
    }
    private bool IsInitQstTemplate(CWoldMapNode cTargetWorldNode, bool bIsDungeon = false)
    {
        if (cTargetWorldNode == null)
        {
            return false;
        }

        CDataEAssetWorldNodeInfoData cInfoData = cTargetWorldNode.m_cNodeInfoData;
        if (cInfoData != null)
        {
            int nQstIndex = cInfoData.GetWorldNodeHaveQuestAvailbleIndex();
            if (nQstIndex > 0)
            {
                CUIWorldMapTileIcon cTileIcon = _cPopUpWorldMapUI.GetWorldMapTileIcon();
                cTileIcon.InitRole(CUIWorldMapTileIcon.EmWorldMapIconType.Quest, ins_traIconGroupRoot, nQstIndex);
                return true;
            }
        }
        return false;
    }
    private bool IsInitInnerMap(List<CDataEAssetWorldNodeInfoData.SInnerMapKey> listInnerMap, OnBoolFuncWorldMapNode onBoolFunc)
    {
        if (listInnerMap == null || listInnerMap.Count <= 0)
        {
            return false;
        }
        for (int i = 0; i < listInnerMap.Count; ++i)
        {
            CWoldMapNode cDungeonNode = GetInnerWorldNode(listInnerMap[i]);
            if (onBoolFunc(cDungeonNode, true))
            {
                return true;
            }
        }
        return false;
    }
    private bool IsInitQstProgressTemplate(CWoldMapNode cTargetWorldNode, bool bIsDungeon = false)
    {
        if (cTargetWorldNode == null)
        {
            return false;
        }
        
        int nProgressQstID = CGameManager.In.ins_scrSQuestCt.GetMapOnGoingQuestID(cTargetWorldNode.MapName);
        if (nProgressQstID > CDataGameInfo.M_nDefaultValue)
        {
            CUIWorldMapTileIcon cTileIcon = _cPopUpWorldMapUI.GetWorldMapTileIcon();
            cTileIcon.InitRole(CUIWorldMapTileIcon.EmWorldMapIconType.ProgressQuest, ins_traCenterRoot, nProgressQstID);
            cTileIcon.InitQuestDungeon(bIsDungeon);
            _cUIQstProgressIcon = cTileIcon;
            _bIsNotViewWorldMap = bIsDungeon;
            _nQuestID = nProgressQstID;
            return true;
        }
        return false;
    }
    private void InitDungeonPortalIcon()
    {
        if (_cWorldNodeInfoData.m_bIsWorldNodeHaveDungeonPortal)
        {
            CUIWorldMapTileIcon cTileIcon = _cPopUpWorldMapUI.GetWorldMapTileIcon();
            cTileIcon.InitRole(CUIWorldMapTileIcon.EmWorldMapIconType.DungeonPortal, ins_traIconGroupRoot);
        }
    }
    private void InitDropFestaCoinIcon()
    {
        if (_cWorldNodeData.GetIsThisNodeDropCurrentFestaCoin())
        {
            CUIWorldMapTileIcon cTileIcon = _cPopUpWorldMapUI.GetWorldMapTileIcon();
            cTileIcon.InitRole(CUIWorldMapTileIcon.EmWorldMapIconType.FestaIcon, ins_traIconGroupRoot);
        }
    }
    private bool IsInitItemTileIconTemplate(CWoldMapNode cTargetWorldNode, bool bIsDungeon)
    {
        if (cTargetWorldNode.IsWorldNodeHavatargetItemID(_cPopUpWorldMapUI.GetSelectStuffID()))
        {
            CUIWorldMapTileIcon cTileIcon = _cPopUpWorldMapUI.GetWorldMapTileIcon();
            cTileIcon.InitRole(CUIWorldMapTileIcon.EmWorldMapIconType.Item, ins_traRightTopRoot);
            cTileIcon.InittemIconSprite(bIsDungeon);
            return true;
        }
        return false;
    }
    private bool IsInitBossIconTemplate(CWoldMapNode cTargetWorldNode, bool bIsDungeon)
    {
        if (cTargetWorldNode.m_cNodeInfoData.m_bIsHaveFieldBoss)
        {
            CUIWorldMapTileIcon cTileIcon = _cPopUpWorldMapUI.GetWorldMapTileIcon();
            cTileIcon.InitRole(CUIWorldMapTileIcon.EmWorldMapIconType.FieldBoss, ins_traIconGroupRoot);
            bool bIsFieldBossAlive = CGameManager.In.ins_scrMapCt.GetIsFieldBossAliveInMap(cTargetWorldNode.MapName);
            cTileIcon.InitFieldBossIconColor(bIsFieldBossAlive, bIsDungeon);
            return true;
        }
        return false;
    }
    private bool InitTownIcon(CPopupWorldMapUI.EmIconFilterType emIconFilterType)
    {
        bool bTownIconOn = (emIconFilterType & CPopupWorldMapUI.EmIconFilterType.Town) != 0;

        if (bTownIconOn == true)
        {
            if (_cWorldNodeInfoData.m_bIsTownNode)
            {
                if (CDataManager.In.m_cDataNetPlayerLogin.m_cDataRes.MapRecord == null || CDataManager.In.m_cDataNetPlayerLogin.m_cDataRes.MapRecord.Count <= 0)
                {
                    return false;
                }

                if (CGameManager.In.ins_scrMapCt.IsRecordedMap(_cWorldNodeData.Region, _cWorldNodeData.MapName))
                {
                    CUIWorldMapTileIcon cTileIcon = _cPopUpWorldMapUI.GetWorldMapTileIcon();
                    cTileIcon.InitRole(CUIWorldMapTileIcon.EmWorldMapIconType.WayPoint, ins_traCenterRoot);
                    return true;
                }
            }
        }
        return false;
    }
    private bool InitWayPointIcon(CPopupWorldMapUI.EmIconFilterType emIconFilterType)
    {
        if (IsMapHaveWayPoint())
        {
            CUIWorldMapTileIcon cTileIcon = _cPopUpWorldMapUI.GetWorldMapTileIcon();
            cTileIcon.InitRole(CUIWorldMapTileIcon.EmWorldMapIconType.WayPoint, ins_traCenterRoot);
            return true;
        }
        return false;
    }
    private void InitWayPointInnerDungeon()
    {
        if (IsMapDungeonHaveWayPoint())
        {
            CUIWorldMapTileIcon cTileIcon = _cPopUpWorldMapUI.GetWorldMapTileIcon();
            cTileIcon.InitRole(CUIWorldMapTileIcon.EmWorldMapIconType.WayPoint, ins_traCenterRoot);
        }
    }
    private void InitBookMarkIcon()
    {
        bool bIsWorldNodeInBookMark = CGameManager.In.ins_scrMapCt.IsMapInBookMark(_cWorldNodeData.MapName);
        if (bIsWorldNodeInBookMark)
        {
            CUIWorldMapTileIcon cTileIcon = _cPopUpWorldMapUI.GetWorldMapTileIcon();
            cTileIcon.InitRole(CUIWorldMapTileIcon.EmWorldMapIconType.BookMark, ins_traLeftTopRoot);
        }
    }
    private CWoldMapNode GetInnerWorldNode(CDataEAssetWorldNodeInfoData.SInnerMapKey sInnerMapKey)
    {
        return CDataManager.In.m_cDataEAssetWorld.GetWoldMapNode(sInnerMapKey.m_emEmWorld, sInnerMapKey.m_nGridX, sInnerMapKey.m_nGridY);
    }
    // Update Method
    public void UpdateMapTile()
    {
        InitTileIcon();
    }
    public void MoveWorldMapScrollPosThisTile()
    {
        float fDuration = _cPopUpWorldMapUI.GetScrollMoveSpeed();
        _cPopUpWorldMapUI.StartWorldMapScrollPosTween(this, fDuration);
    }

    // DisPose Method
    private void DisposeField()
    {
        _cPopUpWorldMapUI = null;
        _cWorldNodeData = null;
        _cWorldNodeInfoData = null;
        _OnClickMapTileMethod = null;
        _cUIQstProgressIcon = null;

        _nQuestID = CDataGameInfo.M_nDefaultValue;
        _bIsNotViewWorldMap = false;
    }

    // OnClick Method
    public void OnClickMapIcon()
    {
        if (_OnClickMapTileMethod != null)
        {
            CSoundManager.In.PlayOneShotsEffect(CSoundManager.EmEffect.BtnClick);
            _OnClickMapTileMethod?.Invoke(this);
        }
    }


    // Get / Set Method
    private bool IsMapHaveWayPoint()
    {
        if (_cWorldNodeInfoData.m_bIsHaveWayPoint && CGameManager.In.ins_scrMapCt.IsRecordedMap(_cWorldNodeData.Region, _cWorldNodeData.MapName))
        {
            return true;
        }
        return false;
    }
    private bool IsMapDungeonHaveWayPoint()
    {
        if (CDataManager.In.m_cDataNetPlayerLogin.m_cDataRes.MapRecord == null || CDataManager.In.m_cDataNetPlayerLogin.m_cDataRes.MapRecord.Count <= 0)
        {
            return false;
        }

        if (_cWorldNodeData.m_cNodeInfoData.m_listInnerDungeon == null || _cWorldNodeData.m_cNodeInfoData.m_listInnerDungeon.Count <= 0)
        {
            return false;
        }
        List<CDataEAssetWorldNodeInfoData.SInnerMapKey> listInnerDungeon = _cWorldNodeData.m_cNodeInfoData.m_listInnerDungeon;
        for (int i = 0; i < listInnerDungeon.Count; ++i)
        {
            CWoldMapNode cDungeonNode = CDataManager.In.m_cDataEAssetWorld.GetWoldMapNode(listInnerDungeon[i].m_emEmWorld, listInnerDungeon[i].m_nGridX, listInnerDungeon[i].m_nGridY);
            if (CGameManager.In.ins_scrMapCt.IsRecordedMap(cDungeonNode.Region, cDungeonNode.MapName))
            {
                return true;
            }
        }
        return false;
    }
    public string GetMapName()
    {
        return _cWorldNodeData.MapName;
    }
    public int GetGridX()
    {
        return _cWorldNodeData.m_nGridX;
    }
    public int GetGridY()
    {
        return _cWorldNodeData.m_nGridY;
    }
    public int GetSelectStuffID()
    {
        return _cPopUpWorldMapUI.GetSelectStuffID();
    }
    public List<CDataEAssetWorldNodeInfoData.SInnerMapKey> GetListNodeInnerDungeon()
    {
        if (_cWorldNodeData.m_cNodeInfoData != null && _cWorldNodeData.m_cNodeInfoData.m_listInnerDungeon != null && _cWorldNodeData.m_cNodeInfoData.m_listInnerDungeon.Count > 0)
        {
            return _cWorldNodeData.m_cNodeInfoData.m_listInnerDungeon;
        }
        return null;
    }
    public List<CDataEAssetWorldNodeInfoData.SInnerMapKey> GetListNodeInnerIndun()
    {
        if (_cWorldNodeData.m_cNodeInfoData != null && _cWorldNodeData.m_cNodeInfoData.m_listInnerIndun != null && _cWorldNodeData.m_cNodeInfoData.m_listInnerIndun.Count > 0)
        {
            return _cWorldNodeData.m_cNodeInfoData.m_listInnerIndun;
        }
        return null;
    }
    public void InitTweenQstProgressIcon(bool bActiveTween, int nQuestID = -1, bool bIsNotViewWorldMap = false)
    {
        if (_cUIQstProgressIcon != null)
        {
            _cUIQstProgressIcon.RunTweenPos(bActiveTween);
            _cUIQstProgressIcon.InitQuestDestinationColor(bActiveTween ? nQuestID : _nQuestID);
            _cUIQstProgressIcon.InitQuestDungeon(bActiveTween ? bIsNotViewWorldMap : _bIsNotViewWorldMap);
        }
    }
}
