using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 处理卡片视觉效果的着色器控制脚本
/// 负责随机设置卡片版本并根据父对象旋转更新着色器参数
/// </summary>
public class ShaderCode : MonoBehaviour
{

    Image image;
    Material m;
    CardVisual visual;

    // Start is called before the first frame update
    void Start()
    {
        image = GetComponent<Image>();
        m = new Material(image.material);
        image.material = m;
        visual = GetComponentInParent<CardVisual>();

        string[] editions = new string[4];
        editions[0] = "REGULAR";
        editions[1] = "POLYCHROME";
        editions[2] = "REGULAR";
        editions[3] = "NEGATIVE";

        // 禁用当前所有启用的关键字
        for (int i = 0; i < image.material.enabledKeywords.Length; i++)
        {
            image.material.DisableKeyword(image.material.enabledKeywords[i]);
        }
        // 随机启用一个版本关键字
        image.material.EnableKeyword("_EDITION_" + editions[Random.Range(0, editions.Length)]);
    }

    // Update is called once per frame
    void Update()
    {

        // 获取当前旋转作为四元数
        Quaternion currentRotation = transform.parent.localRotation;

        // 将四元数转换为欧拉角
        Vector3 eulerAngles = currentRotation.eulerAngles;

        // 获取X轴角度
        float xAngle = eulerAngles.x;
        float yAngle = eulerAngles.y;

        // 确保X轴角度保持在-90到90度范围内
        xAngle = ClampAngle(xAngle, -90f, 90f);
        yAngle = ClampAngle(yAngle, -90f, 90);

        // 设置着色器中的旋转向量参数
        m.SetVector("_Rotation", new Vector2(ExtensionMethods.Remap(xAngle,-20,20,-.5f,.5f), ExtensionMethods.Remap(yAngle, -20, 20, -.5f, .5f)));

    }

    /// <summary>
    /// 将角度限制在最小值和最大值之间
    /// </summary>
    /// <param name="angle">要限制的角度值</param>
    /// <param name="min">最小角度限制</param>
    /// <param name="max">最大角度限制</param>
    /// <returns>限制后的角度值</returns>
    float ClampAngle(float angle, float min, float max)
    {
        if (angle < -180f)
            angle += 360f;
        if (angle > 180f)
            angle -= 360f;
        return Mathf.Clamp(angle, min, max);
    }
}
