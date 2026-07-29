using System.Collections.Generic;
using System.Text;
using HtsNet;

namespace HtsTool
{
    internal class Program
    {
        static void Main(string[] args)
        {
            var modelsFolder = "C:\\Users\\User\\Downloads\\Synth\\HTS\\htsvoice";

            var genericName = "nitech_jp_atr503_m001";
            var genericPath = $"{modelsFolder}\\{genericName}.htsvoice";
            var genericModel = new HtsVoice(genericPath);
            var genericDur = genericModel.Streams.Find(x => x.Type == HtsStreamType.DUR);
            var genericMcp = genericModel.Streams.Find(x => x.Type == HtsStreamType.MCP);
            var genericLf0 = genericModel.Streams.Find(x => x.Type == HtsStreamType.LF0);
            //can be skipped altogether
            var genericLpf = genericModel.Streams.Find(x => x.Type == HtsStreamType.LPF);

            var proprietaryName = "f801_normal_tts";
            var proprietaryPath = $"{modelsFolder}\\{proprietaryName}.htsvoice";
            var proprietaryModel = new HtsVoice(proprietaryPath);
            var proprietaryDur = proprietaryModel.Streams.Find(x => x.Type == HtsStreamType.DUR);
            var proprietaryMgc = proprietaryModel.Streams.Find(x => x.Type == HtsStreamType.MGC);
            var proprietaryLf0 = proprietaryModel.Streams.Find(x => x.Type == HtsStreamType.LF0);
            var proprietaryBap = proprietaryModel.Streams.Find(x => x.Type == HtsStreamType.BAP);
            //talk only
            var proprietaryPDur = proprietaryModel.Streams.Find(x => x.Type == HtsStreamType.PDUR);
            //song only
            var proprietaryRc = proprietaryModel.Streams.Find(x => x.Type == HtsStreamType.RC);
            var proprietaryRs = proprietaryModel.Streams.Find(x => x.Type == HtsStreamType.RS);
            var proprietaryVib = proprietaryModel.Streams.Find(x => x.Type == HtsStreamType.VIB);

            //resizing mgc and cleaning it up for proprietary editor
            //note: no conversions are being done
            genericMcp.Type = HtsStreamType.MGC;
            genericMcp.Pdf.ResizePDF(proprietaryMgc.Pdf.VectorLength, genericMcp.NumWindows);
            genericMcp.Option = proprietaryMgc.Option;
            genericMcp.GvPdf = new HtsPdf();
            genericMcp.GvTree = string.Empty;
            genericMcp.UseGv = false;
            genericMcp.AvailableRangePdf = proprietaryMgc.AvailableRangePdf;
            genericMcp.UseAvailableRange = true;

            var newStreams = new List<HtsStream>();
            if (proprietaryName.Contains("tts"))
            {
                //you may use HTS_TTS for other languages
                genericModel.FullContextFormat = "HTS_TTS_JPN";
                newStreams.Add(genericDur);
                newStreams.Add(proprietaryPDur);
                newStreams.Add(genericMcp);
                newStreams.Add(genericLf0);
                newStreams.Add(proprietaryBap);
            }
            else
            {
                //for both jpn and eng
                genericModel.FullContextFormat = "HTS_SVSS";
                //for more accurate timing, may deteriorate quality
                //newStreams.Add(proprietaryDur);
                newStreams.Add(genericDur);
                newStreams.Add(proprietaryRc);
                newStreams.Add(proprietaryRs);
                newStreams.Add(genericMcp);
                newStreams.Add(proprietaryLf0);
                newStreams.Add(proprietaryBap);
                newStreams.Add(proprietaryVib);
            }

            genericModel.Comment = $"ly TSVOICE {genericName} X {proprietaryName}";
            genericModel.Streams = newStreams;
            genericModel.StreamExtraMetadata = proprietaryModel.StreamExtraMetadata;

            genericModel.SaveToPath(genericPath.Replace(".htsvoice", "-50d-p.htsvoice"));

            //please note hybrid models only work on <=6.0
            //while talk models can be directly imported into >=6.1
            //song models may require some modification in the questions
        }
        public static string ExportParameters(HtsPdf pdf, int numWindows)
        {
            numWindows = numWindows == 0 ? 1 : numWindows;
            var sb = new StringBuilder();

            sb.AppendLine($"Number of States: {pdf.Means.Length}");
            for (int i = 0; i < pdf.Means.Length; i++)
            {
                sb.AppendLine($"State {i + 2}");
                sb.AppendLine($"    Number of PDFs: {pdf.Means[i].Length}");
                for (int j = 0; j < pdf.Means[i].Length; j++)
                {
                    sb.AppendLine($"    PDF {j + 1}");
                    sb.AppendLine($"        Vector Length: {pdf.Means[i][j].Length / numWindows}");
                    for (int k = 0; k < pdf.Means[i][j].Length; k++)
                    {
                        sb.AppendLine($"        Mean {pdf.Means[i][j][k]}");
                    }
                    for (int k = 0; k < pdf.Variances[i][j].Length; k++)
                    {
                        sb.AppendLine($"        Variance {pdf.Variances[i][j][k]}");
                    }
                    if (pdf.IsMsd)
                    {
                        sb.AppendLine($"        MSD {pdf.MSD[i][j]}");
                    }
                }
            }

            return sb.ToString();
        }
    }
}
