using System;
using UnityEngine;

namespace Yamadev.YamaStream
{
    [AttributeUsage(AttributeTargets.Field)]
    public class TranslationKeyAttribute : PropertyAttribute
    {
        public string Key { get; }

        public TranslationKeyAttribute(string key)
        {
            Key = key;
        }
    }
}
