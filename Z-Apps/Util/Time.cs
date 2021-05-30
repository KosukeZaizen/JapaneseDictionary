using System;

namespace Z_Apps.Util
{
    public class Time
    {
        public static DateTime GetJapaneseDateTime()
        {
#if DEBUG
            return DateTime.Now;
# else
            return DateTime.Now.AddHours(9);
# endif
        }
    }
}