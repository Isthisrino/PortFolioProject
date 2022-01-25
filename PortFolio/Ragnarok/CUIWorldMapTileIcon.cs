using UnityEngine;
using DG.Tweening;
public class CUIWorldMapTileIcon : CPoolBase
{
    public enum EmWorldMapIconType
    {
        NONE = 0,
        Town,
        WayPoint,
        DungeonPortal,
        FieldBoss,
        Item,
        Quest,
        BookMark,
        PlayerPoint,
        FestaIcon,
        DungeonItem,
        ProgressQuest,
        DungeonProgress,
        Size,
    }

    //[SerializeField] private Transform ins_traParents;
    [SerializeField] private Sprite[] ins_sprGroup = null;

    [SerializeField] private GameObject ins_objSpriteIcon = null;
    [SerializeField] private UnityEngine.UI.Image ins_ImgIcon = null;

    [SerializeField] private GameObject ins_objPlayerPointIcon = null;

    [SerializeField] private Transform ins_traSpriteIcon = null;
    [SerializeField] private CUISimpleTween ins_TweenPos = null;

    private EmWorldMapIconType _emMyIconType = EmWorldMapIconType.NONE;
    private int _nQuestID = CDataGameInfo.M_nDefaultValue;

    public override void InitPool(EmPoolState emPoolState)
    {
        switch (emPoolState)
        {
            case EmPoolState.Reset:
            case EmPoolState.Destroy:
                //m_traThis.SetParent(ins_traParents);
                _nQuestID = CDataGameInfo.M_nDefaultValue;
                m_traThis.localPosition = Vector3.zero;
                m_traThis.localRotation = Quaternion.identity;
                _emMyIconType = EmWorldMapIconType.NONE;
                ins_ImgIcon.color = CTextColorManager.In.GetDefaultColor();
                break;
        }
    }
    public CUIWorldMapTileIcon InitRole(EmWorldMapIconType emMapIconType, Transform traParent, int nQuestID = -1)
    {
        _emMyIconType = emMapIconType;
        _nQuestID = nQuestID;
        if (emMapIconType != EmWorldMapIconType.PlayerPoint)
        {
            ins_ImgIcon.sprite = ins_sprGroup[(int)_emMyIconType - 1];
            // KHM 210114
            //ins_ImgIcon.SetNativeSize();
            ins_objSpriteIcon.SetActive(true);
            ins_objPlayerPointIcon.SetActive(false);

            if (_nQuestID > 0)
            {
                InitQuestDestinationColor(nQuestID);
            }
        }
        else
        {
            ins_objSpriteIcon.SetActive(false);
            ins_objPlayerPointIcon.SetActive(true);
        }
        m_traThis.SetParent(traParent);
        m_traThis.localScale = Vector3.one;
        m_traThis.localPosition = Vector3.zero;
        ins_traSpriteIcon.localPosition = Vector3.zero;

        ins_TweenPos.enabled = emMapIconType == EmWorldMapIconType.WayPoint;
        return this;
    }
    public void InitQuestDestinationColor(int nQuestID)
    {
        ins_ImgIcon.color = CDataManager.In.m_cDataAssetQuest.GetQuestMarkTypeColor(nQuestID);
    }
    public void InitQuestDungeon(bool bIsDungeon)
    {
        if (bIsDungeon)
        {
            ins_ImgIcon.sprite = ins_sprGroup[(int)EmWorldMapIconType.DungeonProgress - 1];
        }
        else
        {
            ins_ImgIcon.sprite = ins_sprGroup[(int)EmWorldMapIconType.ProgressQuest - 1];
        }
    }
    public void InitFieldBossIconColor(bool bIsFieldBossAlive, bool bIsDungeon)
    {
        if (bIsFieldBossAlive)
        {
            ins_ImgIcon.color = bIsDungeon ? CTextColorManager.In.GetColor(CTextColorManager.ColorName.pink) : CTextColorManager.In.GetDefaultColor();
        }
        else
        {
            ins_ImgIcon.color = CTextColorManager.In.GetColor(CTextColorManager.ColorName.Grey);
        }
    }
    public void InittemIconSprite(bool bIsDungeonDropItem)
    {
        EmWorldMapIconType emItemIconType = EmWorldMapIconType.Item;
        if (bIsDungeonDropItem)
        {
            emItemIconType = EmWorldMapIconType.DungeonItem;
        }
        ins_ImgIcon.sprite = ins_sprGroup[(int)emItemIconType - 1];
    }
    public void RunTween(CPopupWorldMapUI.EmIconFilterType emFilterType)
    {
        m_traThis.DOKill();
        bool bIsTweenRunIcon = false;
        switch (emFilterType)
        {
            case CPopupWorldMapUI.EmIconFilterType.Item:
                if (_emMyIconType == EmWorldMapIconType.Item || _emMyIconType == EmWorldMapIconType.DungeonItem)
                {
                    bIsTweenRunIcon = true;
                }
                break;
            case CPopupWorldMapUI.EmIconFilterType.FieldBoss:
                if (_emMyIconType == EmWorldMapIconType.FieldBoss)
                {
                    bIsTweenRunIcon = true;
                }
                break;
            case CPopupWorldMapUI.EmIconFilterType.Quest:
                if (_emMyIconType == EmWorldMapIconType.Quest
                    || _emMyIconType == EmWorldMapIconType.ProgressQuest)
                {
                    bIsTweenRunIcon = true;
                }
                break;
            case CPopupWorldMapUI.EmIconFilterType.FestaEvent:
                if (_emMyIconType == EmWorldMapIconType.FestaIcon)
                {
                    bIsTweenRunIcon = true;
                }
                break;
            case CPopupWorldMapUI.EmIconFilterType.DungeonPortal:
                if (_emMyIconType == EmWorldMapIconType.DungeonPortal)
                {
                    bIsTweenRunIcon = true;
                }
                break;
        }
        if (bIsTweenRunIcon)
        {
            float fDegree = 15f;
            m_traThis.DOLocalRotate(Vector3.forward * fDegree, 0.15f, RotateMode.LocalAxisAdd).onComplete = () =>
            {
                m_traThis.DOLocalRotate(Vector3.back * fDegree * 2, 0.15f, RotateMode.LocalAxisAdd).onComplete = () =>
                {
                    m_traThis.DOLocalRotate(Vector3.forward * fDegree, 0.15f, RotateMode.LocalAxisAdd);
                };
            };
        }
        else
        {
            m_traThis.localRotation = Quaternion.identity;
        }
    }
    public void RunTweenPos(bool bIsTweenActive)
    {
        if (bIsTweenActive)
        {
            ins_TweenPos.enabled = true;
        }
        else
        {
            ins_traSpriteIcon.localPosition = Vector3.zero;
            ins_TweenPos.enabled = false;
        }
    }
}
