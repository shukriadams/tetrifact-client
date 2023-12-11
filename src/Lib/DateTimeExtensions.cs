﻿using System;

namespace TetrifactClient
{
    public static class DateTimeExtensions
    {
        public static string Ago(this DateTime? beforeUtc)
        {
            if (beforeUtc == null)
                return string.Empty;

            return _agoLocalTime(beforeUtc.Value);
        }

        public static string ToHumanString(this TimeSpan ts, bool shorten = false)
        {
            return _ago(ts, shorten);
        }

        public static string ToHumanString(this TimeSpan? ts, bool shorten = false)
        {
            if (ts == null)
                return string.Empty;
            return _ago(ts.Value, shorten);
        }

        public static string Ago(this DateTime beforeUtc, bool shorten = false)
        {
            return _agoLocalTime(beforeUtc, shorten);
        }

        private static string _agoLocalTime(DateTime beforeUtc, bool shorten = false)
        {
            return _ago(DateTime.Now.ToLocalTime() - beforeUtc.ToLocalTime(), shorten);
        }

        private static string _ago(TimeSpan ts, bool shorten)
        {
            int count = 0;
            string unit = string.Empty;
            string pluralMod = string.Empty;
            if (ts.TotalDays > 364)
            {
                count = (int)Math.Round(ts.TotalDays / 364, 0);
                unit = shorten ? "y" : "year";
                pluralMod = shorten ? string.Empty : "s";
                return $"{count} {unit}" + (count == 1 ? "" : pluralMod);
            }

            if (ts.TotalHours >= 24)
            {
                count = (int)Math.Round(ts.TotalDays, 0);
                unit = shorten ? "d" : "day";
                pluralMod = shorten ? string.Empty : "s";
                return $"{count} {unit}" + (count == 1 ? "" : pluralMod);
            }

            if (ts.TotalMinutes >= 60)
            {
                count = (int)Math.Round(ts.TotalHours, 0);
                unit = shorten ? "h" : "hour";
                pluralMod = shorten ? string.Empty : "s";
                return $"{count} {unit}" + (count == 1 ? "" : pluralMod);
            }

            if (ts.TotalSeconds >= 60)
            {
                count = (int)Math.Round(ts.TotalMinutes, 0);
                unit = shorten ? "m" : "minute";
                pluralMod = shorten ? string.Empty : "s";
                return $"{count} {unit}" + (count == 1 ? "" : pluralMod);
            }

            count = (int)Math.Round(ts.TotalSeconds, 0);
        
            // set floor at 1 second
            if (count < 1)
                count = 1;

            unit = shorten ? "s" : "second";
            pluralMod = shorten ? string.Empty : "s";
            return $"{count} {unit}" + (count == 1 ? "" : pluralMod);
        }

        /// <summary>
        /// Generates a human-friendly date in in local time. Assumes input is UTC.
        /// </summary>
        /// <param name="dateUtc"></param>
        /// <param name="shorten"></param>
        /// <returns></returns>
        public static string ToHumanString(this DateTime dateUtc, bool shorten = true)
        {
            DateTime date = dateUtc.ToLocalTime();
            string format = "yy-MM-dd HH:mm";

            // for date great than a day since, we don't care about time
            if (shorten && (DateTime.Now.ToLocalTime() - date).TotalHours > 24)
                format = "yy-MM-dd";

            string shortened = date.ToString(format)
                .Replace(".", ":"); // .net in its infinite stupidity ignores the ":" in the format string and forces fullstops, replace those

            return shortened;
        }

        /// <summary>
        /// Converts datetime to a format that can be written to filename
        /// </summary>
        /// <param name="date"></param>
        /// <returns></returns>
        public static string ToFSString(this DateTime date) 
        {
            return date.ToString("yyMMdd_HHmmss");
        }

        public static string ToHumanString(this DateTime? dateUtc)
        {
            if (!dateUtc.HasValue)
                return string.Empty;

            return ToHumanString(dateUtc.Value);
        }
    }
}