using UnityEngine;
using VRC.SDK3.Data;
using UdonSharp;

namespace Yamadev.YamaStream.Libraries.GenericDataContainer
{
    [AddComponentMenu("")]
    public class DataDictionary<TKey, TValue> : UdonSharpBehaviour
    {
        public static DataDictionary<TKey, TValue> New()
        {
            return (DataDictionary<TKey, TValue>)(object)new DataDictionary();
        }
    }
}
