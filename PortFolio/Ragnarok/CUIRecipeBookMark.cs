using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

using frame8.Logic.Misc.Visual.UI.ScrollRectItemsAdapter;
using frame8.ScrollRectItemsAdapter.Util;

using DG.Tweening;

public class CUIRecipeBookMark : SRIA<CUIRecipeBookMarkParams, CUIRecipeBookMarkHolder>
{
    [Header("[타이틀 텍스트]")]
    [SerializeField] private Text ins_txtTitle = null;

    [Header("[트윈 움직임을 위한 RectTransform]")]
    [SerializeField] private RectTransform ins_rtraObj = null;
    [SerializeField] private CanvasGroup ins_canObj = null;

    [Header("[레시피가 존재할 때, 안할 때 처리를 위한 inspector]")]
    [SerializeField] private ScrollRect ins_scrollRect = null;
    [SerializeField] private GameObject ins_objRecipeCellScroll = null;
    [SerializeField] private CUIEmptyData ins_cUIEmptyData = null;

    [SerializeField] private GameObject ins_objMiniBackground = null;
    [SerializeField] private GameObject ins_objBigBackground = null;

    private bool _bClickLock = false;

    private float _fUIOpenPosX = 0f;
    private float _fUIClosePosX = 100f;
    private float _fTweenDuration = 0.25f;

    private int _nSelectStuffID = -1;

    private CFunc.OnVoidFuncInt _callbackClickStuffIcon = null;
    private CFunc.OnVoidFunc _onCallBackRemoveRecipe = null;

    //protected override void Awake()
    //{
    //    SetTextInfo();

    //    //InitUIPosition();
    //}
    protected override void Start()
    {
        base.Start();
        gameObject.SetActive(false);
    }

    #region [SRIA Contorl Method]
    protected override CUIRecipeBookMarkHolder CreateViewsHolder(int nItemIndex)
    {
        CUIRecipeBookMarkHolder instance = new CUIRecipeBookMarkHolder();
        instance.Init(_Params.itemPrefab, nItemIndex);
        instance.m_cUIRecipeBookMark = this;
        return instance;
    }
    protected override void UpdateViewsHolder(CUIRecipeBookMarkHolder newOrRecycled)
    {
        int nRecipeIdx = _Params.Data[newOrRecycled.ItemIndex].m_nRecipeID;
        newOrRecycled.SetView(nRecipeIdx);
    }
    private void OnDataRetrieved()
    {
        ResetItems(_Params.Data.Count);
        InitCellScrollNEmptyDataActive();
    }
    public IEnumerator CorRemoveParamsData(int nRemoveRecipeID)
    {
        if (_Params != null && _Params.Data != null)
        {
            for (int i = 0; i < _Params.Data.Count; ++i)
            {
                if (_Params.Data[i].m_nRecipeID == nRemoveRecipeID)
                {
                    _Params.Data.Remove(_Params.Data[i]);
                    break;
                }
            }
        }
        yield return CYieldInstructionCache.WaitForEndOfFrame;
        OnDataRetrieved();
        CToastMessageManager.In.ShowToastMsgAtKey(CDataGameInfo.M_strKeyRecipeBookMarkToastRemove, 2f);
        _onCallBackRemoveRecipe?.Invoke();
        _bClickLock = false;
    }
    #endregion

    private void SetTextInfo()
    {
        CLanguageManager.In.SetText(CLanguageManager.EmKind.GameInfo, ins_txtTitle, CDataGameInfo.M_strKeyRecipeBookMarkTitle);
    }
    private void InitUIPosition()
    {
        ins_rtraObj.localPosition = Vector3.right * _fUIClosePosX;
    }

    private void StartTweenWhenOpen(CFunc.OnVoidFunc closeCallBack = null)
    {
        InitUIPosition();
        ins_rtraObj.DOLocalMoveX(_fUIOpenPosX, _fTweenDuration).OnComplete(() =>
        {
            _bClickLock = false;
            closeCallBack?.Invoke();
        });
        ins_canObj.DOFade(1f, _fTweenDuration);
    }
    private void StartTweenWhenClose(float fDuration, CFunc.OnVoidFunc closeCallBack = null)
    {
        _fTweenDuration = fDuration;
        ins_rtraObj.DOLocalMoveX(_fUIClosePosX, _fTweenDuration).OnComplete(() =>
        {
            gameObject.SetActive(false);
            _bClickLock = false;
            closeCallBack?.Invoke();
        });
        ins_canObj.DOFade(0f, _fTweenDuration);
    }
    private void SetClickStuffIconCallBack(CFunc.OnVoidFuncInt callBackStuffIcon)
    {
        _callbackClickStuffIcon = callBackStuffIcon;
    }
    private void SetRemoveRecipe(CFunc.OnVoidFunc onCallBackRemoveRecipe)
    {
        _onCallBackRemoveRecipe = onCallBackRemoveRecipe;
    }
    private void InitSRIAData()
    {
        List<int> listRecipeBookMark = CGameManager.In.ins_scrRecipeBookMarkCt.GetlistRecipeBookMark();
        if (listRecipeBookMark != null)
        {
            for (int i = 0; i < listRecipeBookMark.Count; ++i)
            {
                int nRecipeID = listRecipeBookMark[i];
                bool bIsRecipeMakeable = CDataManager.In.m_cDataAssetCraftData.IsCraftMakeable(nRecipeID);
                _Params.Data.Add(new CRecipeBookMarkModel(nRecipeID, bIsRecipeMakeable));
            }
            _Params.Data.Sort((aBookMarkModel, bBookMarkModel) =>
            {
                if (aBookMarkModel.m_bIsRecipeMakeAble && !bBookMarkModel.m_bIsRecipeMakeAble)
                {
                    return -1;
                }
                else if (!aBookMarkModel.m_bIsRecipeMakeAble && bBookMarkModel.m_bIsRecipeMakeAble)
                {
                    return 1;
                }
                else
                {
                    return 0;
                }
            });
        }
    }
    private void InitCellScrollNEmptyDataActive()
    {
        bool bIsRecipeScrollOn = _Params.Data.Count > 0;

        ins_objRecipeCellScroll.SetActive(bIsRecipeScrollOn);
        ins_cUIEmptyData.SetData(CDataGameInfo.M_strKeyRecipeBookMarkToastEmpty, !bIsRecipeScrollOn);
        ins_cUIEmptyData.gameObject.SetActive(!bIsRecipeScrollOn);

        bool bIsBigBackGroundOn = _Params.Data.Count > 1;
        //ins_scrollRect.vertical = bIsBigBackGroundOn;
        ins_objBigBackground.SetActive(bIsBigBackGroundOn);
        ins_objMiniBackground.SetActive(!bIsBigBackGroundOn);
    }
    public void SetCUIRecipeBookMark(Transform traParents, CFunc.OnVoidFuncInt onCallBackStuffIcon, CFunc.OnVoidFunc onCallBackRemoveRecipe, int nSelectStufID = -1)
    {
        ins_rtraObj.SetParent(traParents);
        ins_rtraObj.localScale = Vector3.one;
        _nSelectStuffID = nSelectStufID;

        SetClickStuffIconCallBack(onCallBackStuffIcon);

        SetRemoveRecipe(onCallBackRemoveRecipe);
    }
    public void SerCUiRecipcParents(Transform traParents)
    {
        ins_rtraObj.SetParent(traParents);
        ins_rtraObj.localScale = Vector3.one;
    }
    public void UpdateRecipeBookMark(int nSelectStuffID)
    {
        if (gameObject.activeInHierarchy)
        {
            StartCoroutine(CorUpdateRecipeBookMark(nSelectStuffID));
        }
    }
    private IEnumerator CorUpdateRecipeBookMark(int nSelectStuffID)
    {
        yield return CYieldInstructionCache.WaitForEndOfFrame;
        SetSelectStuffID(nSelectStuffID);
        OnDataRetrieved();
    }

    private IEnumerator CorRemoveRecipeBooMark(CUIRecipeBookMarkCell cContentsCell)
    {
        int nRecipeID = cContentsCell.GetRecipeID();
        // _bClickLock은 CorRemoveParamsData 안에 
        yield return StartCoroutine(CGameManager.In.ins_scrRecipeBookMarkCt.CorSendRefreshRecipeBookMarkData(nRecipeID, CorRemoveParamsData));
    }

    public IEnumerator CorOpenUIRecipeBookMark(CFunc.OnVoidFunc onCallBackOpenUI)
    {
        _Params.Data.Clear();
        yield return null;

        SetTextInfo();
        InitSRIAData();
        OnDataRetrieved();
        gameObject.SetActive(true);
        SetNormalizedPosition(1f);

        StartTweenWhenOpen(onCallBackOpenUI);
    }
    public void CloseUIRecipeBookMark(float fDuration = 0.2f, CFunc.OnVoidFunc closeCallBack = null)
    {
        if (_bClickLock)
        {
            return;
        }
        _bClickLock = true;
        StartTweenWhenClose(fDuration, closeCallBack);
    }

    public void OnClickStuffItem(int nStuffID)
    {
        _callbackClickStuffIcon?.Invoke(nStuffID);
    }
    /// <summary>
    /// CUIRecipeCell 상단 빼기 버튼 클릭시 전달됨
    /// </summary>
    public void RemoveRecipeBookMark(CUIRecipeBookMarkCell cContentsCell)
    {
        if (_bClickLock)
        {
            return;
        }
        _bClickLock = true;
        StartCoroutine(CorRemoveRecipeBooMark(cContentsCell));
    }

    public bool IsStuffCallBackNotEmpty()
    {
        return _callbackClickStuffIcon != null;
    }
    private void SetSelectStuffID(int nSelectStuffID)
    {
        if (_nSelectStuffID == nSelectStuffID)
        {
            _nSelectStuffID = CDataGameInfo.M_nDefaultValue;
        }
        else
        {
            _nSelectStuffID = nSelectStuffID;
        }
    }
    public int GetSelectStuffID()
    {
        return _nSelectStuffID;
    }

}
[System.Serializable]
public class CUIRecipeBookMarkParams : BaseParamsWithPrefabAndData<CRecipeBookMarkModel>
{

}
public class CRecipeBookMarkModel
{
    public CRecipeBookMarkModel(int nRecipeID, bool bIsRecipeMakeable)
    {
        m_nRecipeID = nRecipeID;
        m_bIsRecipeMakeAble = bIsRecipeMakeable;
    }
    public int m_nRecipeID = CDataGameInfo.M_nDefaultValue;
    public bool m_bIsRecipeMakeAble = false;

}
public class CUIRecipeBookMarkHolder : BaseItemViewsHolder
{
    public CUIRecipeBookMark m_cUIRecipeBookMark = null;
    private CUIRecipeBookMarkCell _cUIRecipeBookMarkCell = null;
    public override void CollectViews()
    {
        base.CollectViews();
        _cUIRecipeBookMarkCell = root.GetComponent<CUIRecipeBookMarkCell>();
    }

    public void SetView(int nRecipeID)
    {
        _cUIRecipeBookMarkCell.SetView(nRecipeID, m_cUIRecipeBookMark, m_cUIRecipeBookMark.OnClickStuffItem);
    }
}
