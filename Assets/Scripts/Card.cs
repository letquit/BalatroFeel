using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using System.Collections.Generic;
using System.Collections;
using UnityEngine.UI;

/// <summary>
/// 卡牌组件类，处理卡牌的拖拽、选择、悬停等交互功能
/// 实现了Unity的多种事件接口以支持鼠标和触摸交互
/// </summary>
public class Card : MonoBehaviour, IDragHandler, IBeginDragHandler, IEndDragHandler, IPointerEnterHandler, IPointerExitHandler, IPointerUpHandler, IPointerDownHandler
{
    private Canvas canvas;
    private Image imageComponent;
    [SerializeField] private bool instantiateVisual = true;
    private VisualCardsHandler visualHandler;
    private Vector3 offset;

    [Header("Movement")]
    [SerializeField] private float moveSpeedLimit = 50;

    [Header("Selection")]
    public bool selected;
    public float selectionOffset = 50;
    private float pointerDownTime;
    private float pointerUpTime;

    [Header("Visual")]
    [SerializeField] private GameObject cardVisualPrefab;
    [HideInInspector] public CardVisual cardVisual;

    [Header("States")]
    public bool isHovering;
    public bool isDragging;
    [HideInInspector] public bool wasDragged;

    [Header("Events")]
    [HideInInspector] public UnityEvent<Card> PointerEnterEvent;
    [HideInInspector] public UnityEvent<Card> PointerExitEvent;
    [HideInInspector] public UnityEvent<Card, bool> PointerUpEvent;
    [HideInInspector] public UnityEvent<Card> PointerDownEvent;
    [HideInInspector] public UnityEvent<Card> BeginDragEvent;
    [HideInInspector] public UnityEvent<Card> EndDragEvent;
    [HideInInspector] public UnityEvent<Card, bool> SelectEvent;

    /// <summary>
    /// 初始化卡牌组件，在启动时设置画布引用并创建视觉组件
    /// </summary>
    void Start()
    {
        canvas = GetComponentInParent<Canvas>();
        imageComponent = GetComponent<Image>();

        if (!instantiateVisual)
            return;

        visualHandler = FindObjectOfType<VisualCardsHandler>();
        cardVisual = Instantiate(cardVisualPrefab, visualHandler ? visualHandler.transform : canvas.transform).GetComponent<CardVisual>();
        cardVisual.Initialize(this);
    }

    /// <summary>
    /// 更新卡牌位置，处理拖拽移动和屏幕边界限制
    /// </summary>
    void Update()
    {
        ClampPosition();

        // 处理拖拽时的位置更新
        if (isDragging)
        {
            Vector2 targetPosition = Camera.main.ScreenToWorldPoint(Input.mousePosition) - offset;
            Vector2 direction = (targetPosition - (Vector2)transform.position).normalized;
            Vector2 velocity = direction * Mathf.Min(moveSpeedLimit, Vector2.Distance(transform.position, targetPosition) / Time.deltaTime);
            transform.Translate(velocity * Time.deltaTime);
        }
    }

    /// <summary>
    /// 将卡牌位置限制在屏幕边界内
    /// </summary>
    void ClampPosition()
    {
        Vector2 screenBounds = Camera.main.ScreenToWorldPoint(new Vector3(Screen.width, Screen.height, Camera.main.transform.position.z));
        Vector3 clampedPosition = transform.position;
        clampedPosition.x = Mathf.Clamp(clampedPosition.x, -screenBounds.x, screenBounds.x);
        clampedPosition.y = Mathf.Clamp(clampedPosition.y, -screenBounds.y, screenBounds.y);
        transform.position = new Vector3(clampedPosition.x, clampedPosition.y, 0);
    }

    /// <summary>
    /// 开始拖拽事件处理，记录偏移量并禁用射线检测
    /// </summary>
    /// <param name="eventData">指针事件数据</param>
    public void OnBeginDrag(PointerEventData eventData)
    {
        BeginDragEvent.Invoke(this);
        Vector2 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        offset = mousePosition - (Vector2)transform.position;
        isDragging = true;
        canvas.GetComponent<GraphicRaycaster>().enabled = false;
        imageComponent.raycastTarget = false;

        wasDragged = true;
    }

    /// <summary>
    /// 拖拽过程中的事件处理（当前为空实现）
    /// </summary>
    /// <param name="eventData">指针事件数据</param>
    public void OnDrag(PointerEventData eventData)
    {
    }

    /// <summary>
    /// 结束拖拽事件处理，重新启用射线检测并重置拖拽状态
    /// </summary>
    /// <param name="eventData">指针事件数据</param>
    public void OnEndDrag(PointerEventData eventData)
    {
        EndDragEvent.Invoke(this);
        isDragging = false;
        canvas.GetComponent<GraphicRaycaster>().enabled = true;
        imageComponent.raycastTarget = true;

        StartCoroutine(FrameWait());

        IEnumerator FrameWait()
        {
            yield return new WaitForEndOfFrame();
            wasDragged = false;
        }
    }

    /// <summary>
    /// 鼠标进入卡牌区域时的事件处理
    /// </summary>
    /// <param name="eventData">指针事件数据</param>
    public void OnPointerEnter(PointerEventData eventData)
    {
        PointerEnterEvent.Invoke(this);
        isHovering = true;
    }

    /// <summary>
    /// 鼠标离开卡牌区域时的事件处理
    /// </summary>
    /// <param name="eventData">指针事件数据</param>
    public void OnPointerExit(PointerEventData eventData)
    {
        PointerExitEvent.Invoke(this);
        isHovering = false;
    }


    /// <summary>
    /// 鼠标按下事件处理，记录按下时间
    /// </summary>
    /// <param name="eventData">指针事件数据</param>
    public void OnPointerDown(PointerEventData eventData)
    {
        if (eventData.button != PointerEventData.InputButton.Left)
            return;

        PointerDownEvent.Invoke(this);
        pointerDownTime = Time.time;
    }

    /// <summary>
    /// 鼠标释放事件处理，判断点击类型并处理选择逻辑
    /// </summary>
    /// <param name="eventData">指针事件数据</param>
    public void OnPointerUp(PointerEventData eventData)
    {
        if (eventData.button != PointerEventData.InputButton.Left)
            return;

        pointerUpTime = Time.time;

        PointerUpEvent.Invoke(this, pointerUpTime - pointerDownTime > .2f);

        // 如果按住时间超过0.2秒或已被拖拽过，则不处理选择
        if (pointerUpTime - pointerDownTime > .2f)
            return;

        if (wasDragged)
            return;

        selected = !selected;
        SelectEvent.Invoke(this, selected);

        // 根据选择状态调整卡牌位置
        if (selected)
            transform.localPosition += (cardVisual.transform.up * selectionOffset);
        else
            transform.localPosition = Vector3.zero;
    }

    /// <summary>
    /// 取消卡牌选择状态，将卡牌位置重置
    /// </summary>
    public void Deselect()
    {
        if (selected)
        {
            selected = false;
            if (selected)
                transform.localPosition += (cardVisual.transform.up * 50);
            else
                transform.localPosition = Vector3.zero;
        }
    }


    /// <summary>
    /// 获取父对象的子对象数量（如果父对象标签为"Slot"）
    /// </summary>
    /// <returns>父对象的子对象数量减1，否则返回0</returns>
    public int SiblingAmount()
    {
        return transform.parent.CompareTag("Slot") ? transform.parent.parent.childCount - 1 : 0;
    }

    /// <summary>
    /// 获取当前对象在父对象中的兄弟索引（如果父对象标签为"Slot"）
    /// </summary>
    /// <returns>兄弟索引，否则返回0</returns>
    public int ParentIndex()
    {
        return transform.parent.CompareTag("Slot") ? transform.parent.GetSiblingIndex() : 0;
    }

    /// <summary>
    /// 计算当前卡牌在父容器中的归一化位置
    /// </summary>
    /// <returns>归一化位置值（0-1之间），否则返回0</returns>
    public float NormalizedPosition()
    {
        return transform.parent.CompareTag("Slot") ? ExtensionMethods.Remap((float)ParentIndex(), 0, (float)(transform.parent.parent.childCount - 1), 0, 1) : 0;
    }

    /// <summary>
    /// 销毁卡牌对象时清理相关资源
    /// </summary>
    private void OnDestroy()
    {
        if(cardVisual != null)
            Destroy(cardVisual.gameObject);
    }
}