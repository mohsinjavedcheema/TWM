using System;
using System.Diagnostics;

namespace Twm.Extensions
{
    public static class Extensions
    {
        public static string ExceptionInfo(this Exception exception)
        {
            StackFrame stackFrame = (new StackTrace(exception, true)).GetFrame(0);
            return string.Format("At line {0} column {1} in {2}: {3} {4}{3}{5}  ",
                stackFrame.GetFileLineNumber(), stackFrame.GetFileColumnNumber(),
                stackFrame.GetMethod(), Environment.NewLine, stackFrame.GetFileName(),
                exception.Message);
        }
    }
}