using System;
using System.Collections.Generic;

namespace Game
{
    public class NetTimeInfo
    {
        private DateTime _serverStartTime;
        private DateTime _serverTime;
        private DateTime _deviceTime;

        private long _serverStartTimeMilliSec = -1;
        private long _serverTimeMilliSec;
        private long _deviceTimeMilliSec;

        public void SetServerTime(long serverTimeMillis)
        {
            _serverTimeMilliSec = serverTimeMillis;
            _deviceTimeMilliSec = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
            _serverTime = DateTimeOffset.FromUnixTimeMilliseconds(serverTimeMillis).DateTime;
            _deviceTime = DateTime.UtcNow;

            if (_serverStartTimeMilliSec < 0)
            {
                _serverStartTime = _serverTime;
                _serverStartTimeMilliSec = _serverTimeMilliSec;
            }
        }

        /// <summary>
        /// UTC Time
        /// </summary>
        public DateTime GetServerTime()
        {
            var gap = DateTime.UtcNow - _deviceTime;
            return _serverTime + gap;
        }

        /// <summary>
        /// Local Time
        /// </summary>
        public DateTime GetServerTimeToLocal()
        {
            return GetServerTime().ToLocalTime();
        }

        /// <summary>
        /// 서버 갱신 기준 시간 체크용
        /// </summary>
        /// <returns></returns>
        public DateTime GetServerTimeForRefreshContents()
        {
            return GetServerTime().AddHours(9);
        }

        public long GetServerTimeMilliSec()
        {
            var gap = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() - _deviceTimeMilliSec;
            return _serverTimeMilliSec + gap;
        }

        public DateTime GetServerStartTime()
        {
            return _serverStartTime;
        }

        public long GetServerStartTimeMilliSec()
        {
            return _serverStartTimeMilliSec;
        }

        public DateTime ConvertServerTimeToDeviceLocalTime(DateTime serverDateTime)
        {
            var gap = serverDateTime - GetServerTime();
            return DateTime.Now + gap;
        }
    }
}