using System;
using System.Collections.Generic;
using System.IO;
using System.Windows.Documents;
using Twm.Core.Controllers;
using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;

namespace Twm.Core.Classes
{
    public class BuildLogger: Logger
    {
        public override  void Initialize(IEventSource eventSource)
        {
            Source = new List<string>();
            eventSource.ProjectStarted += eventSource_ProjectStarted;
            eventSource.TaskStarted += eventSource_TaskStarted;
            eventSource.MessageRaised += eventSource_MessageRaised;
            eventSource.WarningRaised += eventSource_WarningRaised;
            eventSource.ErrorRaised += eventSource_ErrorRaised;
            eventSource.ProjectFinished += eventSource_ProjectFinished;

            
        }

        public List<string> Source { get; private set; }
        

        void eventSource_ErrorRaised(object sender, BuildErrorEventArgs e)
        {
            // BuildErrorEventArgs adds LineNumber, ColumnNumber, File, amongst other parameters
            string line = String.Format(": ERROR {0}({1},{2}): ", e.File, e.LineNumber, e.ColumnNumber);
            WriteLineWithSenderAndMessage(line, e);
        }

        void eventSource_WarningRaised(object sender, BuildWarningEventArgs e)
        {
            // BuildWarningEventArgs adds LineNumber, ColumnNumber, File, amongst other parameters

            //Suppress now
            /*string line = String.Format(": Warning {0}({1},{2}): ", e.File, e.LineNumber, e.ColumnNumber);
            WriteLineWithSenderAndMessage(line, e);*/
        }

        void eventSource_MessageRaised(object sender, BuildMessageEventArgs e)
        {
            // BuildMessageEventArgs adds Importance to BuildEventArgs
            // Let's take account of the verbosity setting we've been passed in deciding whether to log the message
            if ((e.Importance == MessageImportance.High && IsVerbosityAtLeast(LoggerVerbosity.Minimal))
                || (e.Importance == MessageImportance.Normal && IsVerbosityAtLeast(LoggerVerbosity.Normal))
                || (e.Importance == MessageImportance.Low && IsVerbosityAtLeast(LoggerVerbosity.Detailed))
                )
            {
                WriteLineWithSenderAndMessage(String.Empty, e);
            }
        }

        void eventSource_TaskStarted(object sender, TaskStartedEventArgs e)
        {
            // TaskStartedEventArgs adds ProjectFile, TaskFile, TaskName
            // To keep this log clean, this logger will ignore these events.
        }

        void eventSource_ProjectStarted(object sender, ProjectStartedEventArgs e)
        {
            // ProjectStartedEventArgs adds ProjectFile, TargetNames
            // Just the regular message string is good enough here, so just display that.
            WriteLine(String.Empty, e);
            indent++;
        }

        void eventSource_ProjectFinished(object sender, ProjectFinishedEventArgs e)
        {
            // The regular message string is good enough here too.
            indent--;
            WriteLine(String.Empty, e);
        }

        /// <summary>
        /// Write a line to the log, adding the SenderName and Message
        /// (these parameters are on all MSBuild event argument objects)
        /// </summary>
        private void WriteLineWithSenderAndMessage(string line, BuildEventArgs e)
        {
            if (0 == String.Compare(e.SenderName, "MSBuild", true /*ignore case*/))
            {
                // Well, if the sender name is MSBuild, let's leave it out for prettiness
                WriteLine(line, e);
            }
            else
            {
                WriteLine(e.SenderName + ": " + line, e);
            }
        }

        /// <summary>
        /// Just write a line to the log
        /// </summary>
        private void WriteLine(string line, BuildEventArgs e)
        {
            var indentStr = "";
            for (int i = indent; i > 0; i--)
            {
                indentStr += "\t";
                
            }
            Source.Add(indentStr +line + e.Message);
            
        }

        private int indent;

    }
}