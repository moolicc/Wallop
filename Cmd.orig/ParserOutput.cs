using System;
using System.Collections.Generic;
using System.Text;

namespace Wallop.Cmd
{
    public struct ParserOutput
    {
        public bool Success => !Failed;
        public bool Failed => BadInput || CommandNotFound || InvocationError;

        public bool Empty { get; private set; }

        public bool BadInput { get; private set; }
        public bool CommandNotFound { get; private set; }
        public bool InvocationError { get; private set; }

        public string Message { get; private set; }


        public void MatchSuccess(Action Success = null)
        {
            if(this.Success)
            {
                Success?.Invoke();
            }
        }

        public void MatchFailed(Action<string> Failed = null)
        {
            if(this.Failed)
            {
                Failed?.Invoke(Message);
            }
        }

        public void Match(Action Success = null, Action<string> BadInput = null, Action<string> CommandNotFound = null,
            Action<string> InvocationError = null)
        {
            if(this.Success)
            {
                Success?.Invoke();
            }
            if(this.BadInput)
            {
                BadInput?.Invoke(Message);
            }
            if (this.CommandNotFound)
            {
                CommandNotFound?.Invoke(Message);
            }
            if (this.InvocationError)
            {
                InvocationError?.Invoke(Message);
            }
        }


        public static ParserOutput Succeeded => new ParserOutput()
        {
            Empty = false,
            BadInput = false,
            CommandNotFound = false,
            InvocationError = false,
            Message = "",
        };

        public static ParserOutput SucceededOnEmpty => new ParserOutput()
        {
            Empty = true,
            BadInput = false,
            CommandNotFound = false,
            InvocationError = false,
            Message = "",
        };


        public static ParserOutput FailedOnBadInput => new ParserOutput()
        {
            Empty = false,
            BadInput = true,
            CommandNotFound = false,
            InvocationError = false,
            Message = "Invalid input",
        };

        public static ParserOutput FailedOnNoCommand(string command)
        {
            return new ParserOutput()
            {
                Empty = false,
                BadInput = false,
                CommandNotFound = true,
                InvocationError = false,
                Message = $"'{command}' command not found",
            };
        }

        public static ParserOutput FailedOnInvocationError(string message)
        {
            return new ParserOutput()
            {
                Empty = false,
                BadInput = false,
                CommandNotFound = false,
                InvocationError = true,
                Message = $"Command invocation failed: {message}",
            };
        }
    }
}
