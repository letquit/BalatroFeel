using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using DG.Tweening;
using System.Linq;

/// <summary>
/// 水平卡片持有器，用于管理可拖拽的卡片布局和交互
/// </summary>
public class HorizontalCardHolder : MonoBehaviour
{

    [SerializeField] private Card selectedCard;
    [SerializeReference] private Card hoveredCard;

    [SerializeField] private GameObject slotPrefab;
    private RectTransform rect;

    [Header("Spawn Settings")]
    [SerializeField] private int cardsToSpawn = 7;
    public List<Card> cards;

    bool isCrossing = false;
    [SerializeField] private bool tweenCardReturn = true;

    /// <summary>
    /// 初始化卡片持有器，创建指定数量的卡片槽位并设置事件监听
    /// </summary>
    void Start()
    {
        // 创建指定数量的卡片槽位
        for (int i = 0; i < cardsToSpawn; i++)
        {
            Instantiate(slotPrefab, transform);
        }

        rect = GetComponent<RectTransform>();
        cards = GetComponentsInChildren<Card>().ToList();

        int cardCount = 0;

        // 为每张卡片设置事件监听器
        foreach (Card card in cards)
        {
            card.PointerEnterEvent.AddListener(CardPointerEnter);
            card.PointerExitEvent.AddListener(CardPointerExit);
            card.BeginDragEvent.AddListener(BeginDrag);
            card.EndDragEvent.AddListener(EndDrag);
            card.name = cardCount.ToString();
            cardCount++;
        }

        StartCoroutine(Frame());

        // 延迟一帧后更新所有卡片的索引显示
        IEnumerator Frame()
        {
            yield return new WaitForSecondsRealtime(.1f);
            for (int i = 0; i < cards.Count; i++)
            {
                if (cards[i].cardVisual != null)
                    cards[i].cardVisual.UpdateIndex(transform.childCount);
            }
        }
    }

    /// <summary>
    /// 开始拖拽时设置选中的卡片
    /// </summary>
    /// <param name="card">开始拖拽的卡片</param>
    private void BeginDrag(Card card)
    {
        selectedCard = card;
    }


    /// <summary>
    /// 结束拖拽时将选中卡片移回原位置并重置状态
    /// </summary>
    /// <param name="card">结束拖拽的卡片</param>
    void EndDrag(Card card)
    {
        if (selectedCard == null)
            return;

        selectedCard.transform.DOLocalMove(selectedCard.selected ? new Vector3(0,selectedCard.selectionOffset,0) : Vector3.zero, tweenCardReturn ? .15f : 0).SetEase(Ease.OutBack);

        rect.sizeDelta += Vector2.right;
        rect.sizeDelta -= Vector2.right;

        selectedCard = null;

    }

    /// <summary>
    /// 鼠标进入卡片区域时设置悬停卡片
    /// </summary>
    /// <param name="card">鼠标进入的卡片</param>
    void CardPointerEnter(Card card)
    {
        hoveredCard = card;
    }

    /// <summary>
    /// 鼠标离开卡片区域时清除悬停卡片
    /// </summary>
    /// <param name="card">鼠标离开的卡片</param>
    void CardPointerExit(Card card)
    {
        hoveredCard = null;
    }

    /// <summary>
    /// 更新方法，处理键盘输入、右键点击和卡片交换逻辑
    /// </summary>
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Delete))
        {
            if (hoveredCard != null)
            {
                Destroy(hoveredCard.transform.parent.gameObject);
                cards.Remove(hoveredCard);

            }
        }

        if (Input.GetMouseButtonDown(1))
        {
            foreach (Card card in cards)
            {
                card.Deselect();
            }
        }

        if (selectedCard == null)
            return;

        if (isCrossing)
            return;

        // 检测选中卡片与其它卡片的位置关系，进行交换操作
        for (int i = 0; i < cards.Count; i++)
        {

            if (selectedCard.transform.position.x > cards[i].transform.position.x)
            {
                if (selectedCard.ParentIndex() < cards[i].ParentIndex())
                {
                    Swap(i);
                    break;
                }
            }

            if (selectedCard.transform.position.x < cards[i].transform.position.x)
            {
                if (selectedCard.ParentIndex() > cards[i].ParentIndex())
                {
                    Swap(i);
                    break;
                }
            }
        }
    }

    /// <summary>
    /// 交换选中卡片与指定索引卡片的位置
    /// </summary>
    /// <param name="index">要交换的卡片在列表中的索引</param>
    void Swap(int index)
    {
        isCrossing = true;

        Transform focusedParent = selectedCard.transform.parent;
        Transform crossedParent = cards[index].transform.parent;

        cards[index].transform.SetParent(focusedParent);
        cards[index].transform.localPosition = cards[index].selected ? new Vector3(0, cards[index].selectionOffset, 0) : Vector3.zero;
        selectedCard.transform.SetParent(crossedParent);

        isCrossing = false;

        if (cards[index].cardVisual == null)
            return;

        bool swapIsRight = cards[index].ParentIndex() > selectedCard.ParentIndex();
        cards[index].cardVisual.Swap(swapIsRight ? -1 : 1);

        // 更新所有卡片的视觉索引
        foreach (Card card in cards)
        {
            card.cardVisual.UpdateIndex(transform.childCount);
        }
    }
}