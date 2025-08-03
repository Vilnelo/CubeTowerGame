using UnityEngine;

namespace Core.TrashHole.External
{
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
}