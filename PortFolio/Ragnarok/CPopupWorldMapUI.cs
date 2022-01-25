using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

using DG.Tweening;

public class CPopupWorldMapUI : CBasePopup
{
    // Enum Group
    public enum EmUIOpenType
    {
        OpenByMiniMap,
        OpenByCafra,
        Size,
    }
    public enum EmIconFilterType   // 동일하게 쓰는게 좋을거 같은데
    {
        NONE = 0,
        Town = 1,
        WayPoint = 2,
        DungeonPortal = 4,
        FieldBoss = 8,
        Item = 16,
        Quest = 32,
        BookMark = 64,
        PlayerPoint = 128,
        FestaEvent = 256,
    }


    // Inspector Field
    [Header("CPopupWorldMapUI inspector")]
    [SerializeField] private Text ins_TextWorldMapTitle = null;
    [SerializeField] private Transform ins_traTileRoot = null;
    [SerializeField] private Transform ins_traTilePoolRoot = null;
    [SerializeField] private Transform ins_traWorldMapIconPoolRoot = null;
    [SerializeField] private RectTransform ins_rtraViewPort = null;
    [SerializeField] private RectTransform ins_rtraContent = null;
    [SerializeField] private RectTransform ins_rtraMapImage = null;

    [SerializeField] private CUIAnimTrigger ins_scrPopupAnim = null;

    [Header("레시피 UI용 Root")]
    [SerializeField] private Transform ins_traRecipeUIRoot = null;
    [SerializeField] private CUIRecipeBookMarkBtn ins_cuiRecipeBookMarkBtn = null;

    [Header("Filter Buttons")]
    [SerializeField] private Toggle ins_toggleFestaFilterButton = null;
    [SerializeField] private Toggle ins_toggleFieldBossFilterButton = null;
    [SerializeField] private Toggle ins_toggleQuestFilterButton = null;
    [SerializeField] private Toggle ins_toggleDungeonPortalFilterButton = null;

    [Space(10)]
    [SerializeField] private Text ins_textFestaFilterButton = null;
    [SerializeField] private Text ins_textFieldBossFilterButton = null;
    [SerializeField] private Text ins_textQuestFilterButton = null;
    [SerializeField] private Text ins_textDungeonPortalFilterButton = null;

    [Header("Zoom In Out Value")]
    [SerializeField] private Vector2 ins_vecZoomInSizeDelta;
    [SerializeField] private Vector2 ins_vecZoomInLocalPos;
    [SerializeField] private Vector2 ins_vecZoomOutSizeDelta;

    [SerializeField] private Text ins_textAllMapBtn = null;

    [Header("Select Stuff Icon")]
    [SerializeField] private CUIRecipeBookMarkStuffItemCell ins_cuiSelectStuffItemCell = null;
    [SerializeField] private RectTransform ins_rtraSelectStuffItemCell = null;
    [SerializeField] private CanvasGroup ins_canvasSelectStuffItemCell = null;

    [Header("Quest View Scroll")]
    [SerializeField] private CUIWorldMapQuestView ins_cuiWorldMapQuestView = null;
    [SerializeField] private RectTransform ins_rtraQuestListView = null;
    [SerializeField] private CanvasGroup ins_canvasQuestListView = null;
    [SerializeField] private GameObject ins_objQuestListBtnSelect = null;

    // Private field
    private EmWold _emWorldType = EmWold.Size;
    private EmIconFilterType _emIconFilterType = EmIconFilterType.NONE;
    private EmIconFilterType _emLastFilterType = EmIconFilterType.NONE;

    private bool _bRecipeUIOpened = false;
    private bool _bQuestListOpened = false;
    private int _nSelectStuffID = CDataGameInfo.M_nDefaultValue;
    private bool _bZoomIn = true;
    private bool _bIsInit = false;

    private CDataEAssetWorld _cDataEAssetWorld = null;

    private ScriptPool<CUIWorldMapTile> _cPoolBaseWorldMapTile = null;
    private ScriptPool<CUIWorldMapTileIcon> _cPoolBaseWorldMapIcon = null;

    private CFunc.OnVoidFuncCUIWoldMapTile _onClickMapTileDelagate = null;

    // const Field 
    private const float _fTweenTime = 0.4f;
    private const float _fTweenMoveTime = 0.25f;
    private const int _nTweenMovePosX = -230;
    private const int _nTweenDefaultPosX = 0;

    private const int _nQuestTweenMovePosX = -92;
    private const int _nQuestTweenDefaultMovePosX = 270;

    private const string _strFilterTypeKey = "strTileFilterKey";
    private const string _strSelectStuffIDKey = "strSelectStuffIDKey";
    private const string _strSampleRecipeKey = "strSampleRecipeKey";

    private const float _fRevisionValue = 64f; // = tilesize 128 / 2

    private float _fScrollTime = 0.2f;
    // overriding Method

    public override IEnumerator CorEndPopup()
    {
        if (_bRecipeUIOpened == true)
        {
            CloseRecipeBookMark();
        }
        if (_bQuestListOpened)
        {
            CloseQuestList();
        }

        m_bClickLock = true;

        yield return CStaticCoroutineManager.In.StartCoroutine(ins_scrPopupAnim.CorOnReverseActivated());

        gameObject.SetActive(false);
        CUIManager.In.CloseBaseInit(this);
        m_bClickLock = false;
        _callBack?.Invoke(EmOnClickPopup.Ok);
    }

    public override IEnumerator CorInitPopup(CFunc.OnVoidFuncEmOnClick callBack = null)
    {
        m_bClickLock = true;
        _bIsInit = true;

        InitDataEAssetWorld();
        InitWorldMapTilePool();
        InitTileIconPool();

        InitemPlayerPrefFilterType();
        InitFilterButton();
        InitSelectStuffIconCell();
        InitRecipeBookMarkBtn();
        InitQuestBtn();

        InitSampleRecipeBookMark();

        EmWold emSettingWorldType = GetWorldType();
        bool bIsChnageWorld = _emWorldType != emSettingWorldType;
        if (bIsChnageWorld)
        {
            ClearWorldMapTilePool();
            ClearTileIconPool();
            SetEmWorldType(emSettingWorldType);
            InitCUIWorldMapTile();
        }
        else
        {
            UpdateMapTile();
        }

        int nCurGridX = CDataManager.In.m_cDataEAssetMap.data.m_nWoldDataX;
        int nCurGridY = CDataManager.In.m_cDataEAssetMap.data.m_nWoldDataY;
        StartWorldMapScrollPosTween(nCurGridX, nCurGridY);

        SetTextInfo();
        _callBack = callBack;
        gameObject.SetActive(true);
        yield return StartCoroutine(ins_scrPopupAnim.CorOnActivated());

        ins_cuiWorldMapQuestView.UpdateData();

        m_bClickLock = false;

        _bIsInit = false;
    }
    public override void OnScreenChange(bool bLandscape) { }
    public override void RemoveTextInfo() { }
    public override void SetTextInfo()
    {
        SetTextInfoZoomInOut();

        CLanguageManager.In.SetText(CLanguageManager.EmKind.GameInfo, ins_TextWorldMapTitle, CDataGameInfo.M_strKeyWorldMapText);
        CLanguageManager.In.SetText(CLanguageManager.EmKind.GameInfo, ins_textFestaFilterButton, CDataGameInfo.M_strKeyWorldMapFilterFesta);
        CLanguageManager.In.SetText(CLanguageManager.EmKind.GameInfo, ins_textFieldBossFilterButton, CDataGameInfo.M_strKeyWorldMapFilterBoss);
        CLanguageManager.In.SetText(CLanguageManager.EmKind.GameInfo, ins_textQuestFilterButton, CDataGameInfo.M_strKeyWorldMapFilterQuest);
        CLanguageManager.In.SetText(CLanguageManager.EmKind.GameInfo, ins_textDungeonPortalFilterButton, CDataGameInfo.M_strKeyWorldMapFilterDungeon);
    }
    private void SetTextInfoZoomInOut()
    {
        CLanguageManager.In.SetText(CLanguageManager.EmKind.GameInfo, ins_textAllMapBtn, _bZoomIn ? CDataGameInfo.M_strKeyWorldMapViewAllMap : CDataGameInfo.M_strKeyWorldMapViewZoomMap);
    }

    // Install Mathod
    public void SetData(EmIconFilterType emFilterType, CFunc.OnVoidFuncCUIWoldMapTile onClickTileDelegate = null)
    {
        _emIconFilterType = emFilterType;
        _onClickMapTileDelagate = onClickTileDelegate;
    }

    // Initialize Method
    private void InitDataEAssetWorld()
    {
        if (_cDataEAssetWorld == null)
        {
            _cDataEAssetWorld = CDataManager.In.m_cDataEAssetWorld;
        }
    }
    private void InitWorldMapTilePool()
    {
        if (_cPoolBaseWorldMapTile == null)
        {
            _cPoolBaseWorldMapTile = new ScriptPool<CUIWorldMapTile>(EmPoolType.Size, CAssetManager.In.m_preWorldMapTile, 30, ins_traTilePoolRoot);
        }
    }
    private void InitTileIconPool()
    {
        if (_cPoolBaseWorldMapIcon == null)
        {
            _cPoolBaseWorldMapIcon = new ScriptPool<CUIWorldMapTileIcon>(EmPoolType.Size, CAssetManager.In.m_preWorldMapTileIcon, 20, ins_traWorldMapIconPoolRoot);
        }
    }

    // Setting Method
    private void SetEmFilterType(EmIconFilterType emFilterType)
    {
        _emIconFilterType |= emFilterType;
    }
    private void InitemPlayerPrefFilterType()
    {
        EmIconFilterType emPlayerPrefFilterType = (EmIconFilterType)PlayerPrefs.GetInt(_strFilterTypeKey, (int)EmIconFilterType.NONE);
        SetEmFilterType(emPlayerPrefFilterType);
        _emLastFilterType = EmIconFilterType.NONE;
    }
    private void SetEmWorldType(EmWold emWorldType)
    {
        _emWorldType = emWorldType;
    }
    private void InitFilterButton()
    {
        bool bIsFestaFilterOn = CFestaManager.In.IsRemainFestaEvent() ? CUtil.IsCheckBitValue((int)_emIconFilterType, (int)EmIconFilterType.FestaEvent) : false;
        ins_toggleFestaFilterButton.isOn = bIsFestaFilterOn;

        ins_toggleFieldBossFilterButton.isOn = CUtil.IsCheckBitValue((int)_emIconFilterType, (int)EmIconFilterType.FieldBoss);
        ins_toggleQuestFilterButton.isOn = CUtil.IsCheckBitValue((int)_emIconFilterType, (int)EmIconFilterType.Quest);
        ins_toggleDungeonPortalFilterButton.isOn = CUtil.IsCheckBitValue((int)_emIconFilterType, (int)EmIconFilterType.DungeonPortal);
    }
    private void InitRecipeBookMarkBtn()
    {
        ins_cuiRecipeBookMarkBtn.InitReciepeBookMarkBtn(OnClickRecipeBookMarkButton);
    }
    private void InitQuestBtn()
    {
        ins_objQuestListBtnSelect.SetActive(false);
    }
    private void InitSelectStuffIconCell()
    {
        _nSelectStuffID = PlayerPrefs.GetInt(_strSelectStuffIDKey, CDataGameInfo.M_nDefaultValue);

        if (_nSelectStuffID > 0)
        {
            ins_cuiSelectStuffItemCell.SetDisplayItemStuffCell(_nSelectStuffID, CDataGameInfo.M_nDefaultValue, OnClickSelectStuffItemIcon);
            SelectStuffIconFadeOn();
        }
        else
        {
            if (ins_canvasSelectStuffItemCell.gameObject.activeSelf)
            {
                SelectStuffIconFadeOff();
            }
        }
    }
    private void SelectStuffIconFadeOn()
    {
        if (_nSelectStuffID > 0)
        {
            ins_cuiSelectStuffItemCell.gameObject.SetActive(true);
            ins_canvasSelectStuffItemCell.DOFade(1f, _fTweenTime);
        }
    }
    private void SelectStuffIconFadeOff()
    {
        if (ins_cuiSelectStuffItemCell.gameObject.activeSelf)
        {
            ins_canvasSelectStuffItemCell.DOFade(0f, _fTweenTime).onComplete = () =>
            {
                ins_cuiSelectStuffItemCell.gameObject.SetActive(false);
            };
        }
    }
    private void InitCUIWorldMapTile()
    {
        CWoldMapNode[,] cWoldMapNodeData = _cDataEAssetWorld.GetWoldMapGrid(_emWorldType);

        for (int nGridY = 0; nGridY < cWoldMapNodeData.GetLength(1); ++nGridY)
        {
            for (int nGridX = 0; nGridX < cWoldMapNodeData.GetLength(0); ++nGridX)
            {
                CWoldMapNode worldMapNode = cWoldMapNodeData[nGridX, nGridY];

                if (!IsWorldMapNodeDataNormality(worldMapNode))
                {
                    continue;
                }

                CUIWorldMapTile cUIWorldMapTile = _cPoolBaseWorldMapTile.Spawn(Vector3.zero, Quaternion.identity);
                cUIWorldMapTile.InitMapTile(worldMapNode, this, OnCallBackOnClickMapTile);
            }
        }
    }
    private bool IsWorldMapNodeDataNormality(CWoldMapNode cWorldMapNode)  // 월드맵 데이터가 정상인가?
    {
        if (CDataGameInfo.M_strDown1.Equals(cWorldMapNode.MapName) ||
                cWorldMapNode.m_cNodeInfoData == null ||
                cWorldMapNode.m_cNodeInfoData.m_bIsNotViewWorldMapUI ||
                cWorldMapNode.m_cNodeInfoData.m_nUIGridX < 0 ||
                cWorldMapNode.m_cNodeInfoData.m_nUIGridY < 0)
        {
            return false;
        }
        return true;
    }
    public void StartWorldMapScrollPosTween(int nGridX, int nGridY, float fDuration = 0f)
    {
        CUIWorldMapTile cUiCurrentWorldMapTile = GetWorldMapTile(nGridX, nGridY);
        StartWorldMapScrollPosTween(cUiCurrentWorldMapTile, fDuration);
    }
    public CUIWorldMapTile GetWorldMapTile(int nGridX, int nGridY)
    {
        for (int i = 0; i < _cPoolBaseWorldMapTile.m_listScrtieUseing.Count; ++i)
        {
            CUIWorldMapTile cUiCurrentWorldMapTile = _cPoolBaseWorldMapTile.m_listScrtieUseing[i];
            if ((cUiCurrentWorldMapTile.GetGridX().Equals(nGridX) && cUiCurrentWorldMapTile.GetGridY().Equals(nGridY))
              || IsInnerMapSameGrid(cUiCurrentWorldMapTile, nGridX, nGridY))
            {
                return cUiCurrentWorldMapTile;
            }
        }
        return null;
    }
    private bool IsInnerMapSameGrid(CUIWorldMapTile cuiWorldMapTile, int nGridX, int nGridY)
    {
        List<CDataEAssetWorldNodeInfoData.SInnerMapKey> listDungeon = cuiWorldMapTile.GetListNodeInnerDungeon();
        if (listDungeon != null)
        {
            for (int i = 0; i < listDungeon.Count; ++i)
            {
                if (listDungeon[i].m_nGridX.Equals(nGridX) && listDungeon[i].m_nGridY.Equals(nGridY))
                {
                    return true;
                }
            }
        }
        List<CDataEAssetWorldNodeInfoData.SInnerMapKey> listIndun = cuiWorldMapTile.GetListNodeInnerIndun();
        if (listIndun != null)
        {
            for (int i = 0; i < listIndun.Count; ++i)
            {
                if (listIndun[i].m_nGridX.Equals(nGridX) && listIndun[i].m_nGridY.Equals(nGridY))
                {
                    return true;
                }
            }
        }
        return false;
    }
    public void StartWorldMapScrollPosTween(CUIWorldMapTile cTargetWorldMapTile, float fTweenDuration)
    {
        if (cTargetWorldMapTile != null)
        {
            // 스크롤 노말라이즈 포지션으로는 원하는 결과가 안나와서 트윈포스로 움직임
            ins_rtraContent.DOAnchorPos(new Vector2(-(cTargetWorldMapTile.m_traThis.localPosition.x + _fRevisionValue), (-cTargetWorldMapTile.m_traThis.localPosition.y + _fRevisionValue - ins_rtraMapImage.anchoredPosition.y)), fTweenDuration);
        }
        else
        {
            ins_rtraContent.localPosition = Vector3.zero;
        }
    }
    // Update Method
    public void UpdateMapTile(bool bRunTween = true)
    {
        ClearTileIconPool();
        for (int i = 0; i < _cPoolBaseWorldMapTile.m_listScrtieUseing.Count; ++i)
        {
            _cPoolBaseWorldMapTile.m_listScrtieUseing[i].UpdateMapTile();
        }

        if (bRunTween)
        {
            for (int i = 0; i < _cPoolBaseWorldMapIcon.m_listScrtieUseing.Count; ++i)
            {
                _cPoolBaseWorldMapIcon.m_listScrtieUseing[i].RunTween(_emLastFilterType);
            }
        }
    }
    private void UpdateFilter(EmIconFilterType emUpdateFilterType)
    {
        _emIconFilterType ^= emUpdateFilterType;
        _emLastFilterType = CUtil.IsCheckBitValue((int)_emIconFilterType, (int)emUpdateFilterType) ? emUpdateFilterType : EmIconFilterType.NONE;
        SaveSelectFilterPlayerPref();
        UpdateMapTile();
    }
    // Control / Logic Method
    private void SaveSelectFilterPlayerPref()
    {
        PlayerPrefs.SetInt(_strFilterTypeKey, (int)GetEmIconFilterTypeForSave());
    }
    private void SaveSelectStuffItemIDPlayerPref()
    {
        PlayerPrefs.SetInt(_strSelectStuffIDKey, _nSelectStuffID);
    }


    // OnClick Method
    public void OnClickRecipeBookMarkButton()
    {
        if (m_bClickLock)
        {
            return;
        }
        m_bClickLock = true;

        if (_bQuestListOpened)
        {
            CloseQuestList();
        }

        CSoundManager.In.PlayOneShotsEffect(CSoundManager.EmEffect.BtnClick);

        // 이건 좀 다르게 처리해야 할 듯
        if (_bRecipeUIOpened == false)
        {
            OpenRecipceBookMark(OnCallBackClickLockFalse);
        }
        else
        {
            CloseRecipeBookMark(OnCallBackClickLockFalse);
        }
    }
    private void OpenRecipceBookMark(CFunc.OnVoidFunc callBackCloseUI = null)
    {
        _bRecipeUIOpened = true;
        ins_cuiRecipeBookMarkBtn.SetSelectOn(_bRecipeUIOpened);

        ins_rtraSelectStuffItemCell.DOLocalMoveX(_nTweenMovePosX, _fTweenMoveTime);

        CStaticCoroutineManager.In.StartCoroutine(CUIManager.In.m_cUICoreSceneHelp.CorOpenRecipeBookMark(ins_traRecipeUIRoot,
            callBackCloseUI,
            OnCallBackOnClickRecipeBookMarkStuffItem,
            InitRecipeBookMarkBtn,
            _nSelectStuffID));
    }
    private void CloseRecipeBookMark(CFunc.OnVoidFunc callBackCloseUI = null)
    {
        _bRecipeUIOpened = false;
        ins_rtraSelectStuffItemCell.DOLocalMoveX(_nTweenDefaultPosX, _fTweenMoveTime);
        ins_cuiRecipeBookMarkBtn.SetSelectOn(_bRecipeUIOpened);
        CUIManager.In.m_cUICoreSceneHelp.CloseRecipeBookMark(callBackCloseUI);
    }
    public void OnClickFestaFilterButton()
    {
        // => 여기서 현재 페스타 기간이 아닌 경우 
        if (_bIsInit)
        {
            return;
        }

        if (!CFestaManager.In.IsRemainFestaEvent())
        {
            _bIsInit = true;
            CSoundManager.In.PlayOneShotsEffect(CSoundManager.EmEffect.BtnClick);

            CToastMessageManager.In.ShowToastMsgAtKey(CDataGameInfo.M_strKeyToastFestaEventFinish, 2f);
            ins_toggleFestaFilterButton.isOn = false;
            _bIsInit = false;
            if (CUtil.IsCheckBitValue((int)_emIconFilterType, (int)EmIconFilterType.FestaEvent))
            {
                UpdateFilter(EmIconFilterType.FestaEvent);
            }
            return;
        }

        if (CUtil.IsCheckBitValue((int)_emIconFilterType, (int)EmIconFilterType.FestaEvent) == false)
        {
            List<CFestaManager.CFestaInfo> listRemainFesta = CFestaManager.In.GetListRemainFesta();
            if (listRemainFesta == null)
            {
                return;
            }

            string strFestaRemainText = CLanguageManager.In.GetText(CLanguageManager.EmKind.GameInfo, CDataGameInfo.M_strKeyToastFestaEventRemainRegion);
            System.Text.StringBuilder strBFestaRegion = new System.Text.StringBuilder();
            for (int i = 0; i < listRemainFesta.Count; ++i)
            {
                strBFestaRegion.Append(CLanguageManager.In.GetText(CLanguageManager.EmKind.GameInfo, listRemainFesta[i].emFestaRegion.ToString()));
                if (i < listRemainFesta.Count - 1)
                {
                    strBFestaRegion.Append(", ");
                }
            }
            CToastMessageManager.In.ShowToastMsg(string.Format(strFestaRemainText, strBFestaRegion.ToString()), 2f);
            strBFestaRegion.Remove(0, strBFestaRegion.Length);
        }

        OnClickFilterToggle((int)EmIconFilterType.FestaEvent);
    }
    public void OnClickFilterToggle(int nToogleIndex)
    {
        if (_bIsInit)
        {
            return;
        }
        if (m_bClickLock)
        {
            return;
        }
        m_bClickLock = true;

        CSoundManager.In.PlayOneShotsEffect(CSoundManager.EmEffect.BtnClick);

        UpdateFilter((EmIconFilterType)nToogleIndex);

        m_bClickLock = false;
    }
    public void OnClickCloseButton()
    {
        if (m_bClickLock)
        {
            return;
        }
        CSoundManager.In.PlayOneShotsEffect(CSoundManager.EmEffect.BtnClick);

        CStaticCoroutineManager.In.StartCoroutine(CorEndPopup());
    }
    public void OnClickViewAllMapButton()
    {
        if (m_bClickLock)
        {
            return;
        }
        m_bClickLock = true;
        CSoundManager.In.PlayOneShotsEffect(CSoundManager.EmEffect.BtnClick);

        if (_bZoomIn)
        {
            float fScale = ins_rtraViewPort.rect.width / ins_rtraContent.rect.width;
            ins_rtraContent.sizeDelta = ins_vecZoomOutSizeDelta;
            ins_rtraContent.localScale = new Vector3(fScale, fScale, 1f);
            ins_rtraMapImage.anchoredPosition = Vector2.zero;
            _bZoomIn = false;
        }
        else
        {
            ins_rtraContent.sizeDelta = ins_vecZoomInSizeDelta;
            ins_rtraMapImage.anchoredPosition = ins_vecZoomInLocalPos;
            ins_rtraContent.localScale = Vector3.one;
            _bZoomIn = true;
        }
        ins_textAllMapBtn.DOFade(0f, 0.2f).onComplete = () =>
        {
            SetTextInfoZoomInOut();
            ins_textAllMapBtn.DOFade(1, 0.2f).onComplete = OnCallBackClickLockFalse;
        };
    }
    public void OnClickSelectStuffItemIcon(int nStuffItemID)
    {
        if (_nSelectStuffID <= 0)
        {
            return;
        }
        CSoundManager.In.PlayOneShotsEffect(CSoundManager.EmEffect.BtnClick);
        CUIManager.In.m_cUICoreSceneHelp.UpdateRecipeBookMark(_nSelectStuffID);
        _nSelectStuffID = CDataGameInfo.M_nDefaultValue;
        _emLastFilterType = EmIconFilterType.NONE;
        SaveSelectStuffItemIDPlayerPref();
        InitSelectStuffIconCell();
        UpdateMapTile();
    }
    public void OnClickQuestListButton()
    {
        if (m_bClickLock)
        {
            return;
        }
        m_bClickLock = true;

        if (_bRecipeUIOpened)
        {
            CloseRecipeBookMark();
        }
        CSoundManager.In.PlayOneShotsEffect(CSoundManager.EmEffect.BtnClick);

        if (_bQuestListOpened == false)
        {
            SelectStuffIconFadeOff();

            _bQuestListOpened = true;
            ins_objQuestListBtnSelect.SetActive(true);
            ins_rtraQuestListView.DOAnchorPosX(_nQuestTweenMovePosX, _fTweenMoveTime);
            ins_canvasQuestListView.DOFade(1, _fTweenMoveTime).onComplete = OnCallBackClickLockFalse;
        }
        else
        {
            CloseQuestList(OnCallBackClickLockFalse);
        }
    }
    private void CloseQuestList(CFunc.OnVoidFunc onCallbackClose = null)
    {
        //StartSelectStuffIconTween(true);
        SelectStuffIconFadeOn();

        _bQuestListOpened = false;
        StopQstProgressIconTween();
        ins_cuiWorldMapQuestView.CloseQuestDetailInfo();
        WorldMapQuestViewStopNaviEff();
        ins_objQuestListBtnSelect.SetActive(false);
        ins_rtraQuestListView.DOAnchorPosX(_nQuestTweenDefaultMovePosX, _fTweenMoveTime);
        ins_canvasQuestListView.DOFade(0, _fTweenMoveTime).onComplete = () =>
        {
            onCallbackClose?.Invoke();
        };
    }

    // OnCallBack Method
    private void OnCallBackClickLockFalse()
    {
        m_bClickLock = false;
    }
    private void OnCallBackOnClickRecipeBookMarkStuffItem(int nStuffID)
    {
        if (_nSelectStuffID == nStuffID)
        {
            _nSelectStuffID = CDataGameInfo.M_nDefaultValue;
            _emLastFilterType = EmIconFilterType.NONE;
        }
        else
        {
            _nSelectStuffID = nStuffID;
            _emLastFilterType = EmIconFilterType.Item;
        }
        CUIManager.In.m_cUICoreSceneHelp.UpdateRecipeBookMark(_nSelectStuffID);
        SaveSelectStuffItemIDPlayerPref();
        InitSelectStuffIconCell();
        UpdateMapTile();
    }
    private void OnCallBackOnClickMapTile(CUIWorldMapTile cUIWorldMapTile)
    {
        if (m_bClickLock)
        {
            return;
        }
        m_bClickLock = true;

        if (_bRecipeUIOpened)
        {
            CloseRecipeBookMark();
        }
        if (_bQuestListOpened)
        {
            CloseQuestList();
        }

        _onClickMapTileDelagate?.Invoke(cUIWorldMapTile);
        m_bClickLock = false;
    }

    // Get / Set Method
    private EmWold GetWorldType()
    {
        if (_cDataEAssetWorld.m_emPlayerInWold > EmWold.Midgard) // 추후 미드 가르드 제외 생겼을 경우 변경 필요
        {
            return EmWold.Midgard;
        }
        return _cDataEAssetWorld.m_emPlayerInWold;
    }
    public EmIconFilterType GetIconFilterType()
    {
        return _emIconFilterType;
    }
    public CUIWorldMapTileIcon GetWorldMapTileIcon()
    {
        CUIWorldMapTileIcon poolIcon = _cPoolBaseWorldMapIcon.Spawn(Vector3.zero, Quaternion.identity);
        return poolIcon;
    }
    public Transform GetTileRoot()
    {
        return ins_traTileRoot;
    }
    public int GetSelectStuffID()
    {
        return _nSelectStuffID;
    }
    private EmIconFilterType GetEmIconFilterTypeForSave()
    {
        EmIconFilterType emGetIconFilterType = (_emIconFilterType & EmIconFilterType.FieldBoss) | (_emIconFilterType & EmIconFilterType.Quest) | (_emIconFilterType & EmIconFilterType.DungeonPortal) | (_emIconFilterType & EmIconFilterType.FestaEvent);
        return emGetIconFilterType;
    }
    // Clear Method
    private void ClearWorldMapTilePool()
    {
        _cPoolBaseWorldMapTile.Reset();
    }
    private void ClearTileIconPool()
    {
        _cPoolBaseWorldMapIcon.Reset();
    }

    public void WorldMapQuestViewStopNaviEff()
    {
        ins_cuiWorldMapQuestView.StopNaviEff();
    }
    public void StopQstProgressIconTween()
    {
        for (int i = 0; i < _cPoolBaseWorldMapTile.m_listScrtieUseing.Count; ++i)
        {
            _cPoolBaseWorldMapTile.m_listScrtieUseing[i].InitTweenQstProgressIcon(false);
        }
    }
    public void InitQstProgressIcon(int nGridX, int nGridY, int nQuestID, bool bIsNotViewWorldMap)
    {
        CUIWorldMapTile cCurTile = GetWorldMapTile(nGridX, nGridY);
        cCurTile.InitTweenQstProgressIcon(true, nQuestID, bIsNotViewWorldMap);
    }
    private void InitSampleRecipeBookMark() // For User Tutorial
    {
        int nRacipeSampleOn = PlayerPrefs.GetInt(_strSampleRecipeKey, CDataGameInfo.M_nDefaultValue);
        if (nRacipeSampleOn == CDataGameInfo.M_nDefaultValue
            && _nSelectStuffID == CDataGameInfo.M_nDefaultValue
            && !CGameManager.In.ins_scrRecipeBookMarkCt.IsRecipeBookEmpty())
        {
            List<int> listRecipeBookMark = CGameManager.In.ins_scrRecipeBookMarkCt.GetlistRecipeBookMark();
            for (int i = 0; i < listRecipeBookMark.Count; ++i)
            {
                int nRecipeID = listRecipeBookMark[i];
                CEnc.Int[] nStuffIDs = CDataManager.In.m_cDataAssetCraftData.GetDataByIntArray(nRecipeID, CDataAssetCraftData.EmData.StuffID);
                if (nStuffIDs != null && nStuffIDs.Length > 0)
                {
                    _nSelectStuffID = nStuffIDs[0].V;
                    SaveSelectStuffItemIDPlayerPref();

                    PlayerPrefs.SetInt(_strSampleRecipeKey, 1);

                    OpenRecipceBookMark();
                    InitSelectStuffIconCell();
                    break;
                }
            }
        }
    }
    public float GetScrollMoveSpeed()
    {
        return _fScrollTime;
    }
}
