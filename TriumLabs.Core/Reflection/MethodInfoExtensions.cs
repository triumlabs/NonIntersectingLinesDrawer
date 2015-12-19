using System.Reflection;
using System.Runtime.CompilerServices;

namespace TriumLabs.Core.Reflection
{
    /// <summary>
    /// 
    /// </summary>
    public static class MethodInfoExtensions
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="methodInfo"></param>
        /// <returns></returns>
        public static bool IsAutoProperty(this MethodInfo methodInfo)
        {
            var isAutoProperty = methodInfo != null && methodInfo.IsDefined(typeof(CompilerGeneratedAttribute), false);
            return isAutoProperty;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="methodInfo"></param>
        /// <returns></returns>
        public static bool IsEmptyMethod(this MethodInfo methodInfo)
        {
            var methodBody = methodInfo.GetMethodBody();
            return methodBody != null && methodBody.GetILAsByteArray().Length <= 2;
        }
    }
}
