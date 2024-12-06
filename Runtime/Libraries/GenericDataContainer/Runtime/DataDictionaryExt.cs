using VRC.SDK3.Data;
using Koyashiro.GenericDataContainer.Internal;

namespace Koyashiro.GenericDataContainer
{
    public static class DataDictionaryExt
    {
        public static int Count<TKey, TValue>(this DataDictionary<TKey, TValue> dictionary)
        {
            var dataDictionary = (DataDictionary)(object)(dictionary);
            return dataDictionary.Count;
        }

        public static void Add<TKey, TValue>(this DataDictionary<TKey, TValue> dictionary, TKey key, TValue value)
        {
            var dataDictionary = (DataDictionary)(object)(dictionary);
            var keyToken = DataTokenUtil.NewDataToken(key);
            var valueToken = DataTokenUtil.NewDataToken(value);
            dataDictionary.Add(keyToken, valueToken);
        }

        public static void Clear<TKey, TValue>(this DataDictionary<TKey, TValue> dictionary)
        {
            var dataDictionary = (DataDictionary)(object)(dictionary);
            dataDictionary.Clear();
        }

        public static bool ContainsKey<TKey, TValue>(this DataDictionary<TKey, TValue> dictionary, TKey key)
        {
            var dataDictionary = (DataDictionary)(object)(dictionary);
            var keyToken = DataTokenUtil.NewDataToken(key);
            return dataDictionary.ContainsKey(keyToken);
        }

        public static bool ContainsValue<TKey, TValue>(this DataDictionary<TKey, TValue> dictionary, TValue value)
        {
            var dataDictionary = (DataDictionary)(object)(dictionary);
            var keyValue = DataTokenUtil.NewDataToken(value);
            return dataDictionary.ContainsValue(keyValue);
        }

        public static DataDictionary<TKey, TValue> DeepClohne<TKey, TValue>(this DataDictionary<TKey, TValue> dictionary)
        {
            var dataDictionary = (DataDictionary)(object)(dictionary);
            return (DataDictionary<TKey, TValue>)(object)dataDictionary.DeepClone();
        }

        public static DataList<TKey> GetKeys<TKey, TValue>(this DataDictionary<TKey, TValue> dictionary)
        {
            var dataDictionary = (DataDictionary)(object)(dictionary);
            return (DataList<TKey>)(object)dataDictionary.GetKeys();
        }

        public static DataList<TValue> GetValues<TKey, TValue>(this DataDictionary<TKey, TValue> dictionary)
        {
            var dataDictionary = (DataDictionary)(object)(dictionary);
            return (DataList<TValue>)(object)dataDictionary.GetValues();
        }

        public static bool Remove<TKey, TValue>(this DataDictionary<TKey, TValue> dictionary, TKey key)
        {
            var dataDictionary = (DataDictionary)(object)(dictionary);
            var keyToken = DataTokenUtil.NewDataToken(key);
            return dataDictionary.Remove(keyToken);
        }

        public static bool Remove<TKey, TValue>(this DataDictionary<TKey, TValue> dictionary, TKey key, out TValue value)
        {
            var dataDictionary = (DataDictionary)(object)(dictionary);
            var keyToken = DataTokenUtil.NewDataToken(key);
            var result = dataDictionary.Remove(keyToken, out var valueToken);
            switch (valueToken.TokenType)
            {
                case TokenType.Reference:
                    value = (TValue)valueToken.Reference;
                    break;
                default:
                    value = (TValue)(object)valueToken;
                    break;
            }
            return result;
        }

        public static void SetValue<TKey, TValue>(this DataDictionary<TKey, TValue> dictionary, TKey key, TValue value)
        {
            var dataDictionary = (DataDictionary)(object)(dictionary);
            var keyToken = DataTokenUtil.NewDataToken(key);
            var keyValue = DataTokenUtil.NewDataToken(value);
            dataDictionary.SetValue(keyToken, keyValue);
        }

        public static DataDictionary<TKey, TValue> ShallowClone<TKey, TValue>(this DataDictionary<TKey, TValue> dictionary)
        {
            var dataDictionary = (DataDictionary)(object)(dictionary);
            return (DataDictionary<TKey, TValue>)(object)dataDictionary.ShallowClone();
        }

        public static bool TryGetValue<TKey, TValue>(this DataDictionary<TKey, TValue> dictionary, TKey key, out TValue value)
        {
            var dataDictionary = (DataDictionary)(object)(dictionary);

            var keyToken = DataTokenUtil.NewDataToken(key);
            if (!dataDictionary.TryGetValue(keyToken, out var valueToken))
            {
                value = default;
                return false;
            }

            switch (valueToken.TokenType)
            {
                case TokenType.Reference:
                    value = (TValue)valueToken.Reference;
                    break;
                default:
                    value = (TValue)(object)valueToken;
                    break;
            }

            return true;
        }

        public static TValue GetValue<TKey, TValue>(this DataDictionary<TKey, TValue> dictionary, TKey key)
        {
            var dataDictionary = (DataDictionary)(object)(dictionary);

            var keyToken = DataTokenUtil.NewDataToken(key);

            var valueToken = dataDictionary[keyToken];
            switch (valueToken.TokenType)
            {
                case TokenType.Reference:
                    return (TValue)valueToken.Reference;
                default:
                    return (TValue)(object)valueToken;
            }
        }
    }
}
