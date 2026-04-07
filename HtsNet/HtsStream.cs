using System.Collections.Generic;

namespace HtsNet
{
    public enum HtsStreamType
    {
        /// <summary>
        /// DURation, Always 1 state
        /// </summary>		
        DUR,
        /// <summary>
        /// P?? DURation, Always 1 state
        /// </summary>		
        PDUR,
        /// <summary>
        /// Realignment C, Always 1 state
        /// </summary>
        RC,
        /// <summary>
        /// Realignment S, Always 1 state
        /// </summary>
        RS,
        /// <summary>
        ///	Mel CePstral
        /// </summary>
        MCP,
        /// <summary>
        ///	Mel Generalized Cepstral
        /// </summary>
        MGC,
        /// <summary>
        /// Log F0
        /// </summary>
        LF0,
        /// <summary>
        /// Low-Pass Filter
        /// </summary>
        LPF,
        /// <summary>
        /// Band-APeriodicity
        /// </summary>
        BAP,
        /// <summary>
        /// VIBrato
        /// </summary>		
        VIB
    }
    public class HtsStream
    {
        public HtsStreamType Type { get; set; }
        /// <summary>
        /// Text format
        /// </summary>
        public List<string> Windows { get; set; } = new List<string>();
        public int NumWindows => Windows.Count;
        /// <summary>
        /// Binary format
        /// </summary>
        public HtsPdf Pdf { get; set; } = new HtsPdf();
        /// <summary>
        /// Text format
        /// </summary>
        public string Tree { get; set; }
        public bool UseGv { get; set; } = false;
        public HtsPdf GvPdf { get; set; } = new HtsPdf();
        public string GvTree { get; set; } = string.Empty;
        public bool UseAvailableRange { get; set; } = false;
        public HtsPdf AvailableRangePdf { get; set; } = new HtsPdf();
        public Dictionary<string, float> Options { get; set; } = new Dictionary<string, float>();
        public string Option
        {
            get
            {
                List<string> options = new List<string>();
                foreach (var option in Options)
                {
                    options.Add($"{option.Key}={option.Value}");
                }
                return string.Join(",", options);
            }
            set
            {
                if (string.IsNullOrWhiteSpace(value))
                {
                    Options.Clear();
                    return;
                }

                foreach (var pair in value.Split(','))
                {
                    var kvp = pair.Split('=');
                    Options.Add(kvp[0], float.Parse(kvp[1]));
                }
            }
        }
    }
}
