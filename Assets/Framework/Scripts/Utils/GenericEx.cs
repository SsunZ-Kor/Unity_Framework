using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Game
{
    public static class GenericEx
    {
        public static TValue GetOrCreate<TKey, TValue>(this IDictionary<TKey, TValue> dict, TKey key) where TValue : new()
        {
            TValue val;

            if (!dict.TryGetValue(key, out val))
            {
                val = new TValue();
                dict.Add(key, val);
            }

            return val;
        }

        public static TValue GetOrNull<TKey, TValue>(this IDictionary<TKey, TValue> dict, TKey key) where TValue : class
        {
            if (dict == null)
                return null;

            TValue val;
            dict.TryGetValue(key, out val);
            return val;
        }
        public static void SetOrCreate<TKey, TValue>(this IDictionary<TKey, TValue> dict, TKey key, TValue val) where TValue : new()
        {
            if (!dict.TryGetValue(key, out val))
            {
                val = new TValue();
                dict.Add(key, val);
            }
            else
            {
                dict[key] = val;
            }
        }

        public static void AddOrRefresh<TKey, TValue>(this IDictionary<TKey, TValue> dict, TKey key, TValue val)
        {
            if (dict.ContainsKey(key))
                dict[key] = val;
            else
                dict.Add(key, val);
        }

        public static void AddOrIgnore<TKey, TValue>(this IDictionary<TKey, TValue> dict, TKey key, TValue val)
        {
            if (dict.ContainsKey(key))
                return;

            dict.Add(key, val);
        }

        public static void ForEach<T>(this T[] array, System.Action<T> action)
        {
            if (array == null || action == null)
                return;

            for (int i = 0; i < array.Length; ++i)
            {
                action.Invoke(array[i]);
            }
        }

        public static void ForEachAllNode<T>(this LinkedList<T> llist, System.Action<LinkedListNode<T>> action)
        {
            var node = llist.First;
            if (node == null)
                return;

            while (node != null)
            {
                var currNode = node;
                node = node.Next;

                action.Invoke(currNode);
            }
        }

        public static void RemoveSelf<T>(this LinkedListNode<T> llistNode)
        {
            if (llistNode == null || llistNode.List == null)
                return;

            llistNode.List.Remove(llistNode);
        }

        public static void TrimNullIndex<T>(this List<T> list) where T : class
        {
            if (list == null)
                return;

            for (int i = list.Count - 1; i >= 0; --i)
            {
                if (list[i] == null)
                    list.RemoveAt(i);
            }

            list.TrimExcess();
        }

        public static bool CheckIndex<T>(this List<T> list, int index)
        {
            if (list == null)
                return false;

            return 0 <= index && index < list.Count;
        }

        public static bool CheckIndex(this System.Array array, int index)
        {
            if (array == null)
                return false;

            return 0 <= index && index < array.Length;
        }
        public static T[] SubArray<T>(this T[] data, int index, int length)
        {
            T[] result = new T[length];
            System.Array.Copy(data, index, result, 0, length);
            return result;
        }

        public static void Resize<TValue>(this List<TValue> list, int count) where TValue : new()
        {
            var countGap = list.Count - count;
            if (countGap > 0)
            {
                while (countGap-- > 0)
                    list.RemoveAt(list.Count - 1);
            }
            else if (countGap < 0)
            {
                while (countGap++ < 0)
                    list.Add(new TValue());
            }
        }
    }
}
