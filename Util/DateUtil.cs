using System;
using System.Globalization;

namespace DataSynchronizor.Util
{
    public static class DateUtil
    {
        public static readonly DateTimeFormatInfo DateTimeFormat = new DateTimeFormatInfo
        {
            ShortDatePattern = "yyyy/MM/dd",
            ShortTimePattern = "HH:mm:ss"
        };

        public static DateTime ToDateTime(string str)
        {
            return DateTime.Parse(str, DateTimeFormat);
        }

        public static string ToStr(DateTime dt)
        {
            return dt.ToString(DateTimeFormat);
        }
    }
}