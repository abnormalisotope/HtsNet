using System.Collections.Generic;
using System.Text;
using HtsNet;

namespace HtsTool
{
    internal class Program
    {
        static void Main(string[] args)
        {
            var mikuPath = "C:\\Users\\User\\Downloads\\Synth\\HTS\\htsvoice\\dl\\miku-type-d.htsvoice";
            var mikuModel = new HtsVoice(mikuPath);
            var mikuDur = mikuModel.Streams.Find(x => x.Type == HtsStreamType.DUR);
            var mikuMcp = mikuModel.Streams.Find(x => x.Type == HtsStreamType.MCP);

            var sasaraPath = "C:\\Users\\User\\Downloads\\Synth\\HTS\\htsvoice\\f801_normal_svss.htsvoice";
            var sasaraModel = new HtsVoice(sasaraPath);
            var sasaraDur = sasaraModel.Streams.Find(x => x.Type == HtsStreamType.DUR);
            var sasaraRc = sasaraModel.Streams.Find(x => x.Type == HtsStreamType.RC);
            var sasaraRs = sasaraModel.Streams.Find(x => x.Type == HtsStreamType.RS);
            var sasaraMgc = sasaraModel.Streams.Find(x => x.Type == HtsStreamType.MGC);
            var sasaraLf0 = sasaraModel.Streams.Find(x => x.Type == HtsStreamType.LF0);
            var sasaraBap = sasaraModel.Streams.Find(x => x.Type == HtsStreamType.BAP);
            var sasaraVib = sasaraModel.Streams.Find(x => x.Type == HtsStreamType.VIB);

            mikuMcp.Type = HtsStreamType.MGC;
            mikuMcp.Pdf.ResizePDF(sasaraMgc.Pdf.VectorLength, mikuMcp.NumWindows);
            mikuMcp.Option = "GAMMA=0,LN_GAIN=1,ALPHA=0.55";
            mikuMcp.GvPdf = new HtsPdf();
            mikuMcp.GvTree = string.Empty;
            mikuMcp.UseGv = false;
            mikuMcp.AvailableRangePdf = sasaraMgc.AvailableRangePdf;
            mikuMcp.UseAvailableRange = true;

            var newStreams = new List<HtsStream>()
            {
                mikuDur,
                sasaraRc,
                sasaraRs,
                mikuMcp,
                sasaraLf0,
                sasaraBap,
                sasaraVib
            };

            mikuModel.FullContextFormat = "HTS_SVSS";
            mikuModel.Comment = "ly TSVOICE miku-type-d X sasara_normal ver20141119_mod";
            mikuModel.Streams = newStreams;
            mikuModel.StreamExtraMetadata = sasaraModel.StreamExtraMetadata;

            mikuModel.SaveToPath(mikuPath.Replace(".htsvoice", "-50d-cevio.htsvoice"));
        }
        public static string ExportParameters(float[][][] mean, float[][][] variance, float[][] msd, int numWindows)
        {
            numWindows = numWindows == 0 ? 1 : numWindows;
            var sb = new StringBuilder();

            sb.AppendLine($"Number of States: {mean.Length}");
            for (int i = 0; i < mean.Length; i++)
            {
                sb.AppendLine($"State {i + 2}");
                sb.AppendLine($"    Number of PDFs: {mean[i].Length}");
                for (int j = 0; j < mean[i].Length; j++)
                {
                    sb.AppendLine($"    PDF {j + 1}");
                    sb.AppendLine($"        Vector Length: {mean[i][j].Length / numWindows}");
                    for (int k = 0; k < mean[i][j].Length; k++)
                    {
                        sb.AppendLine($"        Mean {mean[i][j][k]}");
                    }
                    for (int k = 0; k < variance[i][j].Length; k++)
                    {
                        sb.AppendLine($"        Variance {variance[i][j][k]}");
                    }
                    if (msd != null)
                    {
                        sb.AppendLine($"        MSD {msd[i][j]}");
                    }
                }
            }

            return sb.ToString();
        }
    }
}
