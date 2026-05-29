using System;
using System.Collections.Generic;
using UnityEngine;

namespace _Project.Scripts.Core.ResKit.Address
{
    public class AddressHelper
    {
        // 约定式前缀
        public static string UI(string name)     => $"UI/{name}";
        public static string Prefab(string name) => $"Prefabs/{name}";
        public static string Scene(string name)  => $"Scenes/{name}";
        public static string Audio(string name)  => $"Audio/{name}";
        public static string Config(string name) => $"Configs/{name}";
    }
}