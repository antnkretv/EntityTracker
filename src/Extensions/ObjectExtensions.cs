using System;
using System.Linq;
using System.Reflection;

namespace EntityTracker.Extensions
{
    internal static class ObjectExtensions
    {
        public static void SetTrackId<T>(this T obj, object trackId)
            where T : class, new() =>
            GetTrackPropertyInfo(obj)
                .GetSetMethod()
                .Invoke(obj, new[] { trackId });

        public static object GetTrackId<T>(this T obj)
            where T : class =>
            GetTrackPropertyInfo(obj)
                .GetGetMethod()
                .Invoke(obj, null);

        public static void CopyFiledsTo<T>(this T src, T dst)
            where T : class, new()
        {
            var type = typeof(T);

            do
            {
                type.GetFields(BindingFlags.Instance | BindingFlags.NonPublic)
                    .ToList()
                    .ForEach(field => field.SetValue(dst, field.GetValue(src)));

                type = type.BaseType;
            }
            while (type != null);
        }

        private static PropertyInfo GetTrackPropertyInfo(object obj) =>
            obj.GetType().GetProperty(Global.TrackPropertyName)
                ?? throw new ArgumentException("Type is not trackable", nameof(obj));
    }
}
