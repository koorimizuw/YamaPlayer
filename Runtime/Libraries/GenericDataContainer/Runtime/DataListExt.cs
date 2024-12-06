using VRC.SDK3.Data;
using Koyashiro.GenericDataContainer.Internal;

namespace Koyashiro.GenericDataContainer
{
    public static class DataListExt
    {
        public static int Capacity<T>(this DataList<T> list)
        {
            var dataList = (DataList)(object)(list);
            return dataList.Capacity;
        }

        public static int Count<T>(this DataList<T> list)
        {
            var dataList = (DataList)(object)(list);
            return dataList.Count;
        }

        public static void Add<T>(this DataList<T> list, T item)
        {
            var dataList = (DataList)(object)(list);
            var token = DataTokenUtil.NewDataToken(item);
            dataList.Add(token);
        }

        public static void AddRange<T>(this DataList<T> list, T[] collection)
        {
            foreach (var item in collection)
            {
                list.Add(item);
            }
        }

        public static void AddRange<T>(this DataList<T> list, DataList<T> collection)
        {
            var dataList = (DataList)(object)(list);
            var tokens = (DataList)(object)collection;
            dataList.AddRange(tokens);
        }

        public static void BinarySearch<T>(this DataList<T> list, T item)
        {
            var dataList = (DataList)(object)(list);
            var token = DataTokenUtil.NewDataToken(item);
            dataList.BinarySearch(token);
        }

        public static void BinarySearch<T>(this DataList<T> list, int index, int count, T item)
        {
            var dataList = (DataList)(object)(list);
            var token = DataTokenUtil.NewDataToken(item);
            dataList.BinarySearch(index, count, token);
        }

        public static void Clear<T>(this DataList<T> list)
        {
            var dataList = (DataList)(object)(list);
            dataList.Clear();
        }

        public static bool Contains<T>(this DataList<T> list, T item)
        {
            var dataList = (DataList)(object)(list);
            var token = DataTokenUtil.NewDataToken(item);
            return dataList.Contains(token);
        }

        public static DataList<T> DeepClone<T>(this DataList<T> list)
        {
            var dataList = (DataList)(object)(list);
            return (DataList<T>)(object)dataList.DeepClone();
        }

        public static DataList<T> GetRange<T>(this DataList<T> list, int index, int count)
        {
            var dataList = (DataList)(object)(list);
            return (DataList<T>)(object)dataList.GetRange(index, count);
        }

        public static int IndexOf<T>(this DataList<T> list, T item)
        {
            var dataList = (DataList)(object)(list);
            var token = DataTokenUtil.NewDataToken(item);
            return dataList.IndexOf(token);
        }

        public static int IndexOf<T>(this DataList<T> list, T item, int index)
        {
            var dataList = (DataList)(object)(list);
            var token = DataTokenUtil.NewDataToken(item);
            return dataList.IndexOf(token, index);
        }

        public static int IndexOf<T>(this DataList<T> list, T item, int index, int count)
        {
            var dataList = (DataList)(object)(list);
            var token = DataTokenUtil.NewDataToken(item);
            return dataList.IndexOf(token, index, count);
        }

        public static void Insert<T>(this DataList<T> list, int index, T item)
        {
            var dataList = (DataList)(object)(list);
            var token = DataTokenUtil.NewDataToken(item);
            dataList.Insert(index, token);
        }

        public static void InsertRange<T>(this DataList<T> list, int index, T[] collection)
        {
            for (var i = index; i < collection.Length; i++)
            {
                list.Insert(i, collection[i]);
            }
        }

        public static void InsertRange<T>(this DataList<T> list, int index, DataList<T> collection)
        {
            var dataList = (DataList)(object)(list);
            var tokens = (DataList)(object)collection;
            dataList.InsertRange(index, tokens);
        }

        public static int LastIndexOf<T>(this DataList<T> list, T item)
        {
            var dataList = (DataList)(object)(list);
            var token = DataTokenUtil.NewDataToken(item);
            return dataList.LastIndexOf(token);
        }

        public static int LastIndexOf<T>(this DataList<T> list, T item, int index)
        {
            var dataList = (DataList)(object)(list);
            var token = DataTokenUtil.NewDataToken(item);
            return dataList.LastIndexOf(token, index);
        }

        public static int LastIndexOf<T>(this DataList<T> list, T item, int index, int count)
        {
            var dataList = (DataList)(object)(list);
            var token = DataTokenUtil.NewDataToken(item);
            return dataList.LastIndexOf(token, index, count);
        }

        public static bool Remove<T>(this DataList<T> list, T item)
        {
            var dataList = (DataList)(object)(list);
            var token = DataTokenUtil.NewDataToken(item);
            return dataList.Remove(token);
        }

        public static bool RemoveAll<T>(this DataList<T> list, T item)
        {
            var dataList = (DataList)(object)(list);
            var token = DataTokenUtil.NewDataToken(item);
            return dataList.RemoveAll(token);
        }

        public static void RemoveAt<T>(this DataList<T> list, int index)
        {
            var dataList = (DataList)(object)(list);
            dataList.RemoveAt(index);
        }

        public static void RemoveRange<T>(this DataList<T> list, int index, int count)
        {
            var dataList = (DataList)(object)(list);
            dataList.RemoveRange(index, count);
        }

        public static void Reverse<T>(this DataList<T> list)
        {
            var dataList = (DataList)(object)(list);
            dataList.Reverse();
        }

        public static void Reverse<T>(this DataList<T> list, int index, int count)
        {
            var dataList = (DataList)(object)(list);
            dataList.Reverse(index, count);
        }

        public static void SetValue<T>(this DataList<T> list, int index, T item)
        {
            var dataList = (DataList)(object)(list);
            var token = DataTokenUtil.NewDataToken(item);
            dataList.SetValue(index, token);
        }

        public static DataList<T> ShallowClone<T>(this DataList<T> list)
        {
            var dataList = (DataList)(object)(list);
            return (DataList<T>)(object)dataList.ShallowClone();
        }

        public static void Sort<T>(this DataList<T> list)
        {
            var dataList = (DataList)(object)(list);
            dataList.Sort();
        }

        public static void Sort<T>(this DataList<T> list, int index, int count)
        {
            var dataList = (DataList)(object)(list);
            dataList.Sort(index, count);
        }

        public static T[] ToArray<T>(this DataList<T> list)
        {
            var dataList = (DataList)(object)(list);
            var length = dataList.Count;
            var array = new T[length];
            for (var i = 0; i < length; i++)
            {
                var token = dataList[i];
                switch (token.TokenType)
                {
                    case TokenType.Reference:
                        array[i] = (T)token.Reference;
                        break;
                    default:
                        array[i] = (T)(object)token;
                        break;
                }
            }
            return array;
        }

        public static object[] ToObjectArray<T>(this DataList<T> list)
        {
            var dataList = (DataList)(object)(list);
            var length = dataList.Count;
            var array = new object[length];
            for (var i = 0; i < length; i++)
            {
                var token = dataList[i];
                switch (token.TokenType)
                {
                    case TokenType.Reference:
                        array[i] = (T)token.Reference;
                        break;
                    default:
                        array[i] = (T)(object)token;
                        break;
                }
            }
            return array;
        }

        public static void TrimExcess<T>(this DataList<T> list)
        {
            var dataList = (DataList)(object)(list);
            dataList.TrimExcess();
        }

        public static bool TryGetValue<T>(this DataList<T> list, int index, out T value)
        {
            var dataList = (DataList)(object)(list);
            if (!dataList.TryGetValue(index, out var token))
            {
                value = default;
                return false;
            }

            switch (token.TokenType)
            {
                case TokenType.Reference:
                    value = (T)token.Reference;
                    break;
                default:
                    value = (T)(object)token;
                    break;
            }

            return true;
        }

        public static T GetValue<T>(this DataList<T> list, int index)
        {
            var dataList = (DataList)(object)(list);

            var token = dataList[index];
            switch (token.TokenType)
            {
                case TokenType.Reference:
                    return (T)token.Reference;
                default:
                    return (T)(object)token;
            }
        }
    }
}
