using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Game
{
    public class UIRemainTime : MonoBehaviour
    {
        public const string LZ_TIME_S = "LZ_TIME_SS";
        public const string LZ_TIME_MS = "LZ_TIME_MMSS";
        public const string LZ_TIME_HM = "LZ_TIME_HHMM";
        public const string LZ_TIME_DH = "LZ_TIME_DDHH";
        public const string LZ_TIME_WD = "LZ_TIME_WWDD";
        public const string LZ_TIME_MD = "LZ_TIME_MMDD";

        private enum OutputType
        {
            none,
            ss,
            mm_ss,
            hh_mm,
            dd_hh,
            ww_dd,
            MM_dd
        }

        public static long milliSec_s = 1000;
        public static long milliSec_m = milliSec_s * 60;
        public static long milliSec_h = milliSec_m * 60;
        public static long milliSec_d = milliSec_h * 24;
        public static long milliSec_w = milliSec_d * 7;
        public static long milliSec_M = milliSec_d * 30;

        public static string txt_Time_S = "{0}s";
        public static string txt_Time_MS = "{0}m {1}s";
        public static string txt_Time_HM = "{0}h {1}m";
        public static string txt_Time_DH = "{0}d {1}h";
        public static string txt_Time_WD = "{0}w {1}d";
        public static string txt_Time_MD = "{0}m {1}d";

        [SerializeField]
        private Text _uiText;
        [SerializeField]
        private long milliSec_EndTime;

        private OutputType _outputType = OutputType.none;
        private long prevTime;
        private long _const1 = -1;
        private long _const2 = -1;

        private System.Action callback_Zero;

        private void Awake()
        {
            if(_uiText == null)
                _uiText = this.GetOrAddComponent(typeof(Text)) as Text;
        }

        private void OnEnable()
        {
            _const1 = -1;
            _const2 = -1;

            if (UserData.TimeInfo == null)
                return;

            Refresh();
        }

        private void Update()
        {
            if (UserData.TimeInfo == null)
            {
                this.gameObject.SetActive(false);
                return;
            }

            Refresh();

            var currTime = UserData.TimeInfo.GetServerTimeMilliSec();

            if(callback_Zero != null)
            {
                bool isPrevNatural = milliSec_EndTime - prevTime > 0;
                bool isCurrNatural = milliSec_EndTime - UserData.TimeInfo.GetServerTimeMilliSec() > 0;

                if(isPrevNatural != isCurrNatural)
                    callback_Zero.Invoke();
            }

            prevTime = currTime;
        }

        public void SetEndTime(long milliSec)
        {
            if (UserData.TimeInfo == null)
                return;

            milliSec_EndTime = milliSec;
            prevTime = UserData.TimeInfo.GetServerTimeMilliSec();
            Refresh();
        }

        public void SetZeroCallback(System.Action callback, bool bAdd = false)
        {
            if(!bAdd)
                callback_Zero = null;

            callback_Zero = callback;
        }

        /// <summary>
        /// 주의!! : this.Invoke로 재귀호출
        /// </summary>
        /// <returns>다음 갱신 권장 시간</returns>
        private void Refresh()
        {
            if(_uiText == null)
                return;

            var milliSec_CurrTime = UserData.TimeInfo.GetServerTimeMilliSec();
            long milliSec_Gap = milliSec_EndTime - milliSec_CurrTime;

            bool isNegative = milliSec_Gap < 0;
            if(isNegative)
                milliSec_Gap *= -1;

            // 1분 미만
            if(milliSec_Gap < milliSec_m)
            {
                var sec = milliSec_Gap / milliSec_s;
                milliSec_Gap %= milliSec_s;

                if(_const2 != sec || _outputType != OutputType.ss)
                {
                    _const2 = sec;
                    _outputType = OutputType.ss;

                    _uiText.text = string.Format(txt_Time_S, sec);
                }
            }
            // 1시간 미만
            else if(milliSec_Gap < milliSec_h)
            {
                var min = milliSec_Gap / milliSec_m;
                milliSec_Gap %= milliSec_m;

                var sec = milliSec_Gap / milliSec_s;
                milliSec_Gap %= milliSec_s;

                if(_const1 != min || _const2 != sec || _outputType != OutputType.mm_ss)
                {
                    _const1 = min;
                    _const2 = sec;
                    _outputType = OutputType.mm_ss;

                    _uiText.text = string.Format(txt_Time_MS, min, sec);
                }
            }
            // 1일 미만
            else if(milliSec_Gap < milliSec_d)
            {
                var hour = milliSec_Gap / milliSec_h;
                milliSec_Gap %= milliSec_h;

                var min = milliSec_Gap / milliSec_m;
                milliSec_Gap %= milliSec_m;

                if(_const1 != hour || _const2 != min || _outputType != OutputType.hh_mm)
                {
                    _const1 = hour;
                    _const2 = min;
                    _outputType = OutputType.hh_mm;

                    _uiText.text = string.Format(txt_Time_HM, hour, min);
                }
            }
            // 1주 미만
            else if(milliSec_Gap < milliSec_w)
            {
                var day = milliSec_Gap / milliSec_d;
                milliSec_Gap %= milliSec_d;

                var hour = milliSec_Gap / milliSec_h;
                milliSec_Gap %= milliSec_h;

                if(_const1 != day || _const2 != hour || _outputType != OutputType.dd_hh)
                {
                    _const1 = day;
                    _const2 = hour;
                    _outputType = OutputType.dd_hh;

                    _uiText.text = string.Format(txt_Time_DH, day, hour);
                }
            }
            // 1달 미만
            else if(milliSec_Gap < milliSec_M)
            {
                var week = milliSec_Gap / milliSec_w;
                milliSec_Gap %= milliSec_w;

                var day = milliSec_Gap / milliSec_d;
                milliSec_Gap %= milliSec_d;

                if(_const1 != week || _const2 != day || _outputType != OutputType.ww_dd)
                {
                    _const1 = week;
                    _const2 = day;
                    _outputType = OutputType.ww_dd;

                    _uiText.text = string.Format(txt_Time_WD, week, day);
                }
            }
            // 한달 이상
            else
            {
                var month = milliSec_Gap / milliSec_M;
                milliSec_Gap %= milliSec_M;

                var day = milliSec_Gap / milliSec_d;
                milliSec_Gap %= milliSec_d;

                if(_const1 != month || _const2 != day || _outputType != OutputType.MM_dd)
                {
                    _const1 = month;
                    _const2 = day;
                    _outputType = OutputType.MM_dd;

                    _uiText.text = string.Format(txt_Time_MD, month, day);
                }
            }
        }

        public static string GetTimeTextFromMilliSec(long milliSec)
        {
            bool isNegative = milliSec < 0;
            if(isNegative)
                milliSec *= -1;

            // 1분 미만
            if(milliSec < milliSec_m)
            {
                var sec = milliSec / milliSec_s;
                milliSec %= milliSec_s;

                return string.Format(txt_Time_S, sec);
            }
            // 1시간 미만
            else if(milliSec < milliSec_h)
            {
                var min = milliSec / milliSec_m;
                milliSec %= milliSec_m;

                var sec = milliSec / milliSec_s;
                milliSec %= milliSec_s;

                return string.Format(txt_Time_MS, min, sec);
            }
            // 1일 미만
            else if(milliSec < milliSec_d)
            {
                var hour = milliSec / milliSec_h;
                milliSec %= milliSec_h;

                var min = milliSec / milliSec_m;
                milliSec %= milliSec_m;

                return string.Format(txt_Time_HM, hour, min);
            }
            // 1주 미만
            else if(milliSec < milliSec_w)
            {
                var day = milliSec / milliSec_d;
                milliSec %= milliSec_d;

                var hour = milliSec / milliSec_h;
                milliSec %= milliSec_h;

                return string.Format(txt_Time_DH, day, hour);
            }
            // 1달 미만
            else if(milliSec < milliSec_M)
            {
                var week = milliSec / milliSec_w;
                milliSec %= milliSec_w;

                var day = milliSec / milliSec_d;
                milliSec %= milliSec_d;

                return string.Format(txt_Time_WD, week, day);
            }
            // 한달 이상
            else
            {
                var month = milliSec / milliSec_M;
                milliSec %= milliSec_M;

                var day = milliSec / milliSec_d;
                milliSec %= milliSec_d;

                return string.Format(txt_Time_MD, month, day);
            }
        }
    }
}
