using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using UnityEngine;

namespace Core.BottomBlocks.Runtime.Dto
{
    [Serializable]
    [JsonObject(MemberSerialization.Fields)]
    public class CubesDto
    {
        [SerializeField, JsonProperty("cubes")] private List<CubeDto> m_Cubes;

        public List<CubeDto> Cubes => m_Cubes;
    }
}