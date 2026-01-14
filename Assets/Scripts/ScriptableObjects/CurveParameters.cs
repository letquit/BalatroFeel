using UnityEngine;

/// <summary>
/// 手部曲线参数配置脚本对象
/// 用于定义手部动画的位置和旋转曲线参数
/// </summary>
[CreateAssetMenu(fileName = "CurveParameters", menuName = "Hand Curve Parameters")]
public class CurveParameters : ScriptableObject
{
    /// <summary>
    /// 位置调整动画曲线
    /// 控制手部位置变化的动画曲线
    /// </summary>
    public AnimationCurve positioning;
    
    /// <summary>
    /// 位置影响系数
    /// 默认值为0.1f，控制位置曲线的影响程度
    /// </summary>
    public float positioningInfluence = .1f;
    
    /// <summary>
    /// 旋转动画曲线
    /// 控制手部旋转变化的动画曲线
    /// </summary>
    public AnimationCurve rotation;
    
    /// <summary>
    /// 旋转影响系数
    /// 默认值为10f，控制旋转曲线的影响程度
    /// </summary>
    public float rotationInfluence = 10f;
}