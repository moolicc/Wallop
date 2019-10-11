using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WallApp.Services;

namespace WallApp.Scripting
{
    [Service]
    public class ErrorHandler
    {
        [ServiceReference]
        private UI.TrayIcon _trayIcon;
        [ServiceReference]
        private AppState _state;
        //LOGGER
        //[ServiceReference]
        //private UI.TrayIcon _trayIcon;


        private Dictionary<int, Dictionary<int, Exception>> _exceptions;

        public ErrorHandler()
        {
            _exceptions = new Dictionary<int, Dictionary<int, Exception>>();
        }

        public int AddError(int layerId, Exception exception, string altMessage = "")
        {
            if(!_exceptions.TryGetValue(layerId, out var layerExceptions))
            {
                layerExceptions = new Dictionary<int, Exception>();
                _exceptions.Add(layerId, layerExceptions);
            }

            int hash = HashException(exception, layerId);
            layerExceptions.Add(hash, exception);

            string message = altMessage;
            if(altMessage.IsNull())
            {
                altMessage = exception.Message;
            }
            _trayIcon.SetLayerError(layerId, exception, message);

            return hash;
        }

        public void RemoveError(int layerId, int errorId)
        {
            Exception exception = null;
            if (_exceptions.TryGetValue(layerId, out var layerExceptions))
            {
                if(layerExceptions.TryGetValue(errorId, out exception))
                {
                    layerExceptions.Remove(errorId);
                }
            }

            if(_trayIcon.LastException == exception)
            {
                var errors = _exceptions.Where(i => i.Value.Count > 0);
                if (errors.Any())
                {
                    var nextException = errors.First().Value.FirstOrDefault();
                    if(nextException.Value != null)
                    {
                        _trayIcon.SetLayerError(errors.First().Key, nextException.Value, nextException.Value.Message);
                    }
                }
                _trayIcon.RemoveLayerError();
            }
        }

        public void ClearErrors(int layerId)
        {
            if (_exceptions.TryGetValue(layerId, out var layerExceptions))
            {
                while (layerExceptions.Count > 0)
                {
                    RemoveError(layerId, layerExceptions.ElementAt(0).Key);
                }
            }
        }

        private int HashException(Exception exception, int layerId)
        {
            unchecked
            {
                return layerId.GetHashCode() * 31 + exception.GetHashCode();
            }
        }

    }
}
