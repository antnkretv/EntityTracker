#if !DEBUG
    #define ENABLE_LOG_TRACKER
#endif

using EntityTracker.Extensions;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace EntityTracker
{
    public static class Tracker
    {
        public static IEnumerable<T> AsTrackable<T>(IEnumerable<T> enumerable)
            where T : class, new()
        {
            var list = enumerable.ToList();
            for (int i = 0; i < list.Count; i++)
            {
                T refference = list[i];
                BeginTrack(ref refference);

                yield return refference;
            }
        }

        public static void BeginTrack<T>(ref T obj, Guid? trackId = null)
            where T : class, new()
        {
            if (obj is IEnumerable)
            {
                throw new Exception("This method can't process enumarated types");
            }

#if ENABLE_LOG_TRACKER
            MakeTrackable(ref obj);
            obj.SetTrackId(trackId);
#endif
        }

        public static void LinkTo<T1, T2>(T1 obj1, ref T2 obj2)
            where T1 : class
            where T2 : class, new()
        {
            _ = obj2 ?? throw new ArgumentNullException(nameof(obj2));

            var trackId = (Guid)obj1.GetTrackId();
            BeginTrack(ref obj2, trackId);
        }

        private static void MakeTrackable<T>(ref T obj)
            where T : class, new()
        {
            var sourceType = typeof(T);
            if (sourceType.IsTrackable())
            {
                throw new ArgumentException("The object already tracked", nameof(obj));
            }

            var trackableType = sourceType.AsTrackable();
            var trackableObj = (T)Activator.CreateInstance(trackableType);
            obj.CopyFiledsTo(trackableObj);

            obj = trackableObj;
        }
    }
}