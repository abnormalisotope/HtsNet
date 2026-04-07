using System.IO;
using System.Text;
using HtsNet;

namespace HtsTool
{
    internal class Program
    {
        static void Main(string[] args)
        {
            var folder = "C:\\Users\\joaop\\Downloads\\hts\\htsvoice";
            var name = "nitech_jp_song070_f001";
            var model = new HtsVoice($"{folder}\\{name}.htsvoice");
            var mgc = model.Streams.Find(x => x.Type == HtsStreamType.MGC);
            var parameters = ExportParameters(mgc.Pdf.Means, mgc.Pdf.Variances, mgc.Pdf.MSD, mgc.NumWindows);
            File.WriteAllText(Path.Combine(folder, name + $"_{mgc.Type.ToString().ToLower()}_pdf.txt"), parameters);
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
