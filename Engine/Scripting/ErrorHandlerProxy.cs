using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WallApp.Scripting
{
    public class ErrorHandlerProxy
    {
        private ErrorHandler _backingHandler;
        private int _boundLayer;
        private List<Error> _errors;

        public ErrorHandlerProxy(int boundLayer)
        {
            _backingHandler = Services.ServiceProvider.GetService<ErrorHandler>();
            _boundLayer = boundLayer;
            _errors = new List<Error>();
        }

        public void SetError(Exception error, object tag)
        {
            SetError(error, error.Message, tag);
        }

        public void SetError(Exception error, string message, object tag)
        {
            int errorId = _backingHandler.AddError(_boundLayer, error, message);
            _errors.Add(new Error(errorId, tag));
        }

        public void RemoveError(object tag)
        {
            _errors.RemoveAll(e => e.Tag == tag);
        }

        public void ClearErrors()
        {
            _backingHandler.ClearErrors(_boundLayer);
        }

        private struct Error
        {
            public int ErrorId { get; private set; }
            public object Tag { get; private set; }

            public Error(int errorId, object tag)
            {
                ErrorId = errorId;
                Tag = tag;
            }
        }
    }
}
