/// <summary>
/// 提供数值映射转换的扩展方法
/// </summary>
public static class ExtensionMethods
{
    /// <summary>
    /// 将一个范围内的数值重新映射到另一个范围内
    /// </summary>
    /// <param name="value">需要进行映射转换的原始数值</param>
    /// <param name="from1">原始数值的起始范围值</param>
    /// <param name="to1">原始数值的结束范围值</param>
    /// <param name="from2">目标映射范围的起始值</param>
    /// <param name="to2">目标映射范围的结束值</param>
    /// <returns>映射到新范围后的数值</returns>
    public static float Remap(this float value, float from1, float to1, float from2, float to2)
    {
        return (value - from1) / (to1 - from1) * (to2 - from2) + from2;
    }
}