using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Core.Scene.External
{
    public class StartButtonView : MonoBehaviour
    {
        [SerializeField] private Button m_Button;
        [SerializeField] private TMP_Text m_Text;

        public TMP_Text Text
        {
            get => m_Text;
            set => m_Text = value;
        }

        public Button Button
        {
            get => m_Button;
            set => m_Button = value;
        }
    }
}