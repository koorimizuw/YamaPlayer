using UnityEngine;
using VRC.SDK3.Data;
using UdonSharp;
using Yamadev.YamaStream.Libraries.Internal;

namespace Yamadev.YamaStream.Libraries.GenericDataContainer
{
    [AddComponentMenu("")]
    public class DataList<T> : UdonSharpBehaviour
    {
        public static DataList<T> New()
        {
            return (DataList<T>)(object)new DataList();
        }

        public static DataList<T> New(params T[] array)
        {
            var tokens = DataTokenUtil.NewDataTokens(array);
            return (DataList<T>)(object)new DataList(tokens);
        }
    }
}
