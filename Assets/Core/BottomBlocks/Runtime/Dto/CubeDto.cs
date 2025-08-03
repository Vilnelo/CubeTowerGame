using System;
using Newtonsoft.Json;
using UnityEngine;

namespace Core.BottomBlocks.Runtime.Dto
{
    [Serializable]
    [JsonObject(MemberSerialization.Fields)]
    public class CubeDto
    {
        [SerializeField, JsonProperty("id")] private int m_Id;

        [SerializeField, JsonProperty("color_name")]
        private string m_ColorName;

        [SerializeField, JsonProperty("sprite_name")]
        private string m_SpriteName;

        public int Id => m_Id;
        public string ColorName => m_ColorName;
        public string SpriteName => m_SpriteName;
    }
}