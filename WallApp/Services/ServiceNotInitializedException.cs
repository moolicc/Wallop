using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WallApp.Services
{

    [Serializable]
    public class ServiceNotInitializedException : Exception
    {
        public ServiceNotInitializedException() : base()
        {
        }

        public ServiceNotInitializedException(string service)
            : base($"Service not initialized. Service name: {service}")
        {
        }

        public ServiceNotInitializedException(string message, string service)
            : base($"{message} Service name: {service}")
        {
        }

        public ServiceNotInitializedException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}
