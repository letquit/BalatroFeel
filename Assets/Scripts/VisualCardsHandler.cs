using System;
using UnityEngine;

/// <summary>
/// 视觉卡牌处理器，用于管理游戏中的视觉卡牌相关功能
/// </summary>
public class VisualCardsHandler : MonoBehaviour
{
    /// <summary>
    /// 当前视觉卡牌处理器的单例实例
    /// </summary>
    public static VisualCardsHandler instance;

    /// <summary>
    /// 在对象初始化时调用，设置单例实例
    /// </summary>
    private void Awake()
    {
        instance = this;
    }
}