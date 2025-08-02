using UnityEngine;
using System.Collections.Generic;
using UnityEngine.Serialization;

public class HoleMaskView : MonoBehaviour
{
    [SerializeField] private Transform m_MaskTransform;
    [SerializeField] private Material m_MaskedMaterial;

    public Transform GetMaskTransform()
    {
        return m_MaskTransform;
    }

    public Material GetMaskedMaterial()
    {
        return m_MaskedMaterial;
    }
}