using System;
using Plugins.Sirenix.Odin_Inspector.Modules;
using UnityEngine;

namespace _Scripts.Game.UI
{
    [Serializable]
    public class EncounterTypeToIconDictionary : UnitySerializedDictionary<EncounterType, Sprite>
    {
    }
}