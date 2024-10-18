namespace Twm.Core.DataCalc
{
    public class ScriptOptions
    {
        /// <summary>
        /// Allow script object show it`s plots
        /// </summary>
        public bool ShowPlots { get; set; }

        /// <summary>
        /// Allow script object show it`s additional panes
        /// </summary>
        public bool ShowPanes { get; set; }

        public ScriptOptions()
        {
            ShowPlots = true;
            ShowPanes = true;
        }
    }
}