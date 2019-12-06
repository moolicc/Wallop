using System;

namespace WallApp.Services
{
    [AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = true)]
    sealed class ServiceAttribute : Attribute
    {
        public string Key { get; set; }
    }
}
