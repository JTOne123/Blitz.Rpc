using System;
using System.Reflection;

namespace Blitz.Rpc.Client.BaseClasses
{
    public static class RpcMethodInfoExtension
    {
        public static RpcMethodInfo ToRpcMethodInfo(this MethodInfo info, Type masterType)
        {
            return new RpcMethodInfo
            {
                Name = info.Name,
                MasterType = masterType,
                DefinedIn = info.DeclaringType,
                ServiceId = info.DeclaringType.FullName,
                PackageName = info.DeclaringType.Assembly.GetName().Name,
                ReturnType = info.ReturnType,
                ParamType = info.GetParameters().Length == 0 ? null : info.GetParameters()[0].ParameterType,
                Major = info.DeclaringType.Assembly.GetName().Version.Major,
                Minor = info.DeclaringType.Assembly.GetName().Version.Minor
            };
        }
    }
}