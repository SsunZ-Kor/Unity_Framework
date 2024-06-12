using UnityEngine;
using System.IO;
using System;
using UnityEngine.Events;
using System.Net.Sockets;
using System.Net;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Game
{
    public static class Utils
    {
        public static DateTime UNIX_EPOCH_TIME = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);

        public static Action Subscribe(this UnityEvent onEvent, UnityAction onAddEvent)
        {
            if (onEvent == null)
                throw new ArgumentNullException("onEventNull");

            onEvent.RemoveListener(onAddEvent);
            onEvent.AddListener(onAddEvent);
            return () => onEvent.RemoveListener(onAddEvent);
        }

        public static Action Subscribe<T>(this UnityEvent<T> onEvent, UnityAction<T> onAddEvent)
        {
            if (onEvent == null)
                throw new ArgumentNullException("onEventNull");

            onEvent.RemoveListener(onAddEvent);
            onEvent.AddListener(onAddEvent);
            return () => onEvent.RemoveListener(onAddEvent);
        }

        public static Color32 HexToColor(this string hex)
        {
            const System.Globalization.NumberStyles HexNumber = System.Globalization.NumberStyles.HexNumber;

            if (hex.Length < 6)
                return Color.magenta;

            hex = hex.Replace("#", "");
            byte r = byte.Parse(hex.Substring(0, 2), HexNumber);
            byte g = byte.Parse(hex.Substring(2, 2), HexNumber);
            byte b = byte.Parse(hex.Substring(4, 2), HexNumber);
            byte a = 0xFF;
            if (hex.Length == 8)
                a = byte.Parse(hex.Substring(6, 2), HexNumber);

            return new Color32(r, g, b, a);
        }

        public static DateTime GetDateTimeLocal(this long lTimeMilliseconds)
        {
            return GetDateTimeUTC(lTimeMilliseconds).ToLocalTime();
        }

        public static DateTime GetDateTimeUTC(this long lTimeMilliseconds)
        {
            return DateTimeOffset.FromUnixTimeMilliseconds(lTimeMilliseconds).DateTime;
        }



        public static long ToUnixEpochTime(this DateTime dateTime)
        {
            return (long)dateTime.ToUniversalTime().Subtract(UNIX_EPOCH_TIME).TotalSeconds;
        }

        public static DateTime FromUnixEpochTimeUtc(double milliSeconds)
        {
            return UNIX_EPOCH_TIME.AddMilliseconds(milliSeconds);
        }

        public static DateTime FromUnixEpochTimeLocal(double milliSeconds)
        {
            return FromUnixEpochTimeUtc(milliSeconds).ToLocalTime();
        }


        public static void SafeAddAnimClip(this Animation anim, AnimationClip animClip)
        {
            if (animClip == null || anim == null || anim.GetClip(animClip.name) != null)
                return;
            
            anim.AddClip(animClip, animClip.name);
        }

        public static string GetLocalAddressIPv4()
        {
            if (TryGetLocalAddressIPv4(out var ipAddress))
                return ipAddress;
            return string.Empty;
        }

        public static string GetLocalAddressIPv6()
        {
            if (TryGetLocalAddressIPv6(out var ipAddress))
                return ipAddress;
            return string.Empty;
        }

        public static bool TryGetLocalAddressIPv4(out string ipAddress)
        {
            return TryGetLocalAddress(AddressFamily.InterNetwork, out ipAddress);
        }

        public static bool TryGetLocalAddressIPv6(out string ipAddress)
        {
            return TryGetLocalAddress(AddressFamily.InterNetworkV6, out ipAddress);
        }

        public static bool TryGetLocalAddress(AddressFamily addressFamily, out string address)
        {
            foreach (var addr in Dns.GetHostAddresses(Dns.GetHostName()))
            {
                if (addr.AddressFamily == addressFamily)
                {
                    address = addr.ToString();
                    return true;
                }
            }

            address = string.Empty;
            return false;
        }

#if UNITY_EDITOR
        public static void CreateAsset<T>() where T : ScriptableObject
        {
            T asset = ScriptableObject.CreateInstance<T>();

            string path = AssetDatabase.GetAssetPath(Selection.activeObject);
            if (path == "")
            {
                path = "Assets";
            }
            else if (Path.GetExtension(path) != "")
            {
                path = path.Replace(Path.GetFileName(AssetDatabase.GetAssetPath(Selection.activeObject)), "");
            }

            string assetPathAndName = AssetDatabase.GenerateUniqueAssetPath(path + "/New " + typeof(T).ToString() + ".asset");

            AssetDatabase.CreateAsset(asset, assetPathAndName);

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            EditorUtility.FocusProjectWindow();
            Selection.activeObject = asset;
        }
#endif
    }
}