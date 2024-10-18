using System.Linq;
using System.Reflection;

namespace Twm.Core.Helpers
{
    public static class CloneHelper
    {
        
            public static void CopyPropertiesTo<T>(this T source, T dest)
            {
                var plist = from prop in typeof(T).GetProperties() where prop.CanRead && prop.CanWrite select prop;

                foreach (PropertyInfo prop in plist)
                {
                    prop.SetValue(dest, prop.GetValue(source, null), null);
                }
            }


            public static T CloneTo<T>(this T source, T dest)
            {
                var plist = from prop in typeof(T).GetProperties() where prop.CanRead && prop.CanWrite select prop;

                foreach (PropertyInfo prop in plist)
                {
                    prop.SetValue(dest, prop.GetValue(source, null), null);
                }

                return dest;
            }


    }
}