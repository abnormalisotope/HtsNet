using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using HtsNet.Extensions;
using static HtsNet.HtsConst;

namespace HtsNet
{
    public class HtsVoice
    {
        public float HtsVoiceVersion { get; set; }
        public int SamplingFrequency { get; set; }
        public int FramePeriod { get; set; }
        public int NumStates { get; set; }
        public int NumStreams => StreamType.Count;
        private readonly List<HtsStreamType> Exclusions = new List<HtsStreamType> { HtsStreamType.DUR, HtsStreamType.PDUR, HtsStreamType.RC, HtsStreamType.RS };
        public List<HtsStreamType> StreamType
        {
            get
            {
                var list = new List<HtsStreamType>();
                foreach (var stream in Streams)
                {
                    if (Exclusions.Contains(stream.Type))
                        continue;
                    list.Add(stream.Type);
                }
                return list;
            }
        }
        public string FullContextFormat { get; set; }
        public float FullContextVersion { get; set; }
        public string GvOffContext { get; set; } = null;
        public string Comment { get; set; } = null;
        public Dictionary<string, string> GlobalExtraMetadata { get; set; } = new Dictionary<string, string>();
        public List<HtsStream> Streams { get; set; } = new List<HtsStream>();
        public Dictionary<string, string> StreamExtraMetadata { get; set; } = new Dictionary<string, string>();
        public Dictionary<string, byte[]> PositionExtraStream { get; set; } = new Dictionary<string, byte[]>();
        public HtsVoice(object htsvoice = null)
        {
            var type = htsvoice.GetType();
            switch (type.FullName)
            {
                case "System.String":
                    var bytes = File.ReadAllBytes((string)htsvoice);
                    Parse(bytes);
                    break;
                case "System.Byte[]":
                    Parse((byte[])htsvoice);
                    break;
            }
        }
        public byte[] Save()
        {
            var htsvoice = Serialize();
            return htsvoice;
        }
        public void SaveToPath(string path)
        {
            var htsvoice = Serialize();
            File.WriteAllBytes(path, htsvoice);
        }
        public void Load(byte[] htsvoice)
        {
            Parse(htsvoice);
        }
        public void LoadFromPath(string path)
        {
            if (File.Exists(path))
            {
                var htsvoice = File.ReadAllBytes(path);
                Parse(htsvoice);
            }
        }
        private byte[] Serialize()
        {
            string header;
            using (var sw = new StringWriter())
            {
                sw.NewLine = "\n";

                sw.WriteLine(LABEL_GLOBAL);
                sw.WriteLine($"{HTS_VOICE_VERSION}:{HtsVoiceVersion.ToString("F1")}");
                sw.WriteLine($"{SAMPLING_FREQUENCY}:{SamplingFrequency}");
                sw.WriteLine($"{FRAME_PERIOD}:{FramePeriod}");
                sw.WriteLine($"{NUM_STATES}:{NumStates}");
                sw.WriteLine($"{NUM_STREAMS}:{NumStreams}");
                sw.WriteLine($"{STREAM_TYPE}:{string.Join(",", StreamType.Select(x => x.ToString()))}");
                sw.WriteLine($"{FULLCONTEXT_FORMAT}:{FullContextFormat}");
                if (float.IsNaN(FullContextVersion))
                {
                    sw.WriteLine($"{FULLCONTEXT_VERSION}:");
                }
                else
                {
                    sw.WriteLine($"{FULLCONTEXT_VERSION}:{FullContextVersion.ToString("F1")}");
                }
                if (Streams.FindAll(x => x.UseGv == true).Count > 0)
                    sw.WriteLine($"{GV_OFF_CONTEXT}:{GvOffContext}");
                sw.WriteLine($"{COMMENT}:{Comment}");

                sw.WriteLine(LABEL_STREAM);
                foreach (var type in StreamType)
                {
                    var stream = Streams.FirstOrDefault(x => x.Type == type);
                    sw.WriteLine($"{VECTOR_LENGTH}[{type}]:{stream.Pdf.VectorLength}");
                }
                foreach (var type in StreamType)
                {
                    var stream = Streams.FirstOrDefault(x => x.Type == type);
                    sw.WriteLine($"{IS_MSD}[{type}]:{Convert.ToInt32(stream.Pdf.IsMsd)}");
                }
                foreach (var type in StreamType)
                {
                    var stream = Streams.FirstOrDefault(x => x.Type == type);
                    sw.WriteLine($"{NUM_WINDOWS}[{type}]:{stream.NumWindows}");
                }
                if (Streams.FindAll(x => x.UseGv == true).Count > 0)
                {
                    foreach (var type in StreamType)
                    {
                        var stream = Streams.FirstOrDefault(x => x.Type == type);
                        sw.WriteLine($"{USE_GV}[{type}]:{Convert.ToInt32(stream.UseGv)}");
                    }
                }
                if (Streams.FindAll(x => x.UseAvailableRange == true).Count > 0)
                {
                    foreach (var type in StreamType)
                    {
                        var stream = Streams.FirstOrDefault(x => x.Type == type);
                        sw.WriteLine($"{USE_AVAILABLE_RANGE}[{type}]:{Convert.ToInt32(stream.UseAvailableRange)}");
                    }
                }
                foreach (var type in StreamType)
                {
                    var stream = Streams.FirstOrDefault(x => x.Type == type);
                    sw.WriteLine($"{OPTION}[{type}]:{stream.Option}");
                }
                foreach (var extra in StreamExtraMetadata)
                {
                    sw.WriteLine($"{extra.Key}:{extra.Value}");
                }

                var positions = new Dictionary<string, string>();
                string position;
                int offset = 0, length = 0;

                var durStream = Streams.FirstOrDefault(x => x.Type == HtsStreamType.DUR);
                var pdurStream = Streams.FirstOrDefault(x => x.Type == HtsStreamType.PDUR);
                var rcStream = Streams.FirstOrDefault(x => x.Type == HtsStreamType.RC);
                var rsStream = Streams.FirstOrDefault(x => x.Type == HtsStreamType.RS);

                if (durStream != null)
                {
                    length = (durStream.Pdf.States.Length * 4) + (durStream.Pdf.Data.Length * 4);
                    position = $"{offset}-{offset + length - 1}";
                    offset += length;
                    positions.Add($"{DURATION_PDF}", position);
                    length = durStream.Tree.Length;
                    position = $"{offset}-{offset + length - 1}";
                    offset += length;
                    positions.Add($"{DURATION_TREE}", position);
                }

                if (pdurStream != null)
                {
                    length = (pdurStream.Pdf.States.Length * 4) + (pdurStream.Pdf.Data.Length * 4);
                    position = $"{offset}-{offset + length - 1}";
                    offset += length;
                    positions.Add($"{PDURATION_PDF}", position);
                    length = pdurStream.Tree.Length;
                    position = $"{offset}-{offset + length - 1}";
                    offset += length;
                    positions.Add($"{PDURATION_TREE}", position);
                }

                if (rcStream != null)
                {
                    length = (rcStream.Pdf.States.Length * 4) + (rcStream.Pdf.Data.Length * 4);
                    position = $"{offset}-{offset + length - 1}";
                    offset += length;
                    positions.Add($"{REALIGNMENT_C_PDF}", position);
                    length = rcStream.Tree.Length;
                    position = $"{offset}-{offset + length - 1}";
                    offset += length;
                    positions.Add($"{REALIGNMENT_C_TREE}", position);
                }

                if (rsStream != null)
                {
                    length = (rsStream.Pdf.States.Length * 4) + (rsStream.Pdf.Data.Length * 4);
                    position = $"{offset}-{offset + length - 1}";
                    offset += length;
                    positions.Add($"{REALIGNMENT_S_PDF}", position);
                    length = rsStream.Tree.Length;
                    position = $"{offset}-{offset + length - 1}";
                    offset += length;
                    positions.Add($"{REALIGNMENT_S_TREE}", position);
                }

                foreach (var stream in Streams)
                {
                    if (Exclusions.Contains(stream.Type))
                        continue;

                    var windowPositions = new List<string>();
                    foreach (var window in stream.Windows)
                    {
                        windowPositions.Add($"{offset}-{offset + window.Length - 1}");
                        offset += window.Length;
                    }
                    if (windowPositions.Count > 0)
                        positions.Add($"{STREAM_WIN}[{stream.Type}]", string.Join(",", windowPositions));
                }
                foreach (var stream in Streams)
                {
                    if (Exclusions.Contains(stream.Type))
                        continue;

                    length = (stream.Pdf.States.Length * 4) + (stream.Pdf.Data.Length * 4);
                    position = $"{offset}-{offset + length - 1}";
                    offset += length;
                    positions.Add($"{STREAM_PDF}[{stream.Type}]", string.Join(",", position));
                }
                foreach (var stream in Streams)
                {
                    if (Exclusions.Contains(stream.Type))
                        continue;

                    length = stream.Tree.Length;
                    position = $"{offset}-{offset + length - 1}";
                    offset += length;
                    positions.Add($"{STREAM_TREE}[{stream.Type}]", position);
                }
                foreach (var stream in Streams)
                {
                    if (stream.UseGv)
                    {
                        length = (stream.GvPdf.States.Length * 4) + (stream.GvPdf.Data.Length * 4);
                        position = $"{offset}-{offset + length - 1}";
                        offset += length;
                        positions.Add($"{GV_PDF}[{stream.Type}]", string.Join(",", position));
                    }
                }
                foreach (var stream in Streams)
                {
                    if (stream.UseGv)
                    {
                        length = stream.GvTree.Length;
                        position = $"{offset}-{offset + length - 1}";
                        offset += length;
                        positions.Add($"{GV_TREE}[{stream.Type}]", string.Join(",", position));
                    }
                }
                foreach (var stream in Streams)
                {
                    if (stream.UseAvailableRange)
                    {
                        length = (stream.AvailableRangePdf.States.Length * 4) + (stream.AvailableRangePdf.Data.Length * 4);
                        position = $"{offset}-{offset + length - 1}";
                        offset += length;
                        positions.Add($"{AVAILABLE_RANGE_PDF}[{stream.Type}]", string.Join(",", position));
                    }
                }

                sw.WriteLine(LABEL_POSITION);
                foreach (var kvp in positions)
                {
                    sw.WriteLine($"{kvp.Key}:{kvp.Value}");
                }

                sw.WriteLine(LABEL_DATA);

                header = sw.ToString();
            }

            using (var ms = new MemoryStream())
            using (var bw = new BinaryWriter(ms))
            {
                bw.Write(Encoding.ASCII.GetBytes(header));

                var durStream = Streams.FirstOrDefault(x => x.Type == HtsStreamType.DUR);
                var pdurStream = Streams.FirstOrDefault(x => x.Type == HtsStreamType.PDUR);
                var rcStream = Streams.FirstOrDefault(x => x.Type == HtsStreamType.RC);
                var rsStream = Streams.FirstOrDefault(x => x.Type == HtsStreamType.RS);

                if (durStream != null)
                {
                    durStream.Pdf.Write(bw);
                    bw.Write(durStream.Tree.ToByteArray());
                }

                if (pdurStream != null)
                {
                    pdurStream.Pdf.Write(bw);
                    bw.Write(pdurStream.Tree.ToByteArray());
                }

                if (rcStream != null)
                {
                    rcStream.Pdf.Write(bw);
                    bw.Write(rcStream.Tree.ToByteArray());
                }

                if (rsStream != null)
                {
                    rsStream.Pdf.Write(bw);
                    bw.Write(rsStream.Tree.ToByteArray());
                }

                foreach (var stream in Streams)
                {
                    if (Exclusions.Contains(stream.Type))
                        continue;

                    foreach (var window in stream.Windows)
                    {
                        bw.Write(window.ToByteArray());
                    }
                }

                foreach (var stream in Streams)
                {
                    if (Exclusions.Contains(stream.Type))
                        continue;

                    stream.Pdf.Write(bw);
                }

                foreach (var stream in Streams)
                {
                    if (Exclusions.Contains(stream.Type))
                        continue;

                    bw.Write(stream.Tree.ToByteArray());
                }

                foreach (var stream in Streams)
                {
                    if (Exclusions.Contains(stream.Type))
                        continue;

                    if (stream.UseGv)
                        stream.GvPdf.Write(bw);
                }

                foreach (var stream in Streams)
                {
                    if (Exclusions.Contains(stream.Type))
                        continue;

                    if (stream.UseGv)
                        bw.Write(stream.GvTree.ToByteArray());
                }

                foreach (var stream in Streams)
                {
                    if (Exclusions.Contains(stream.Type))
                        continue;

                    if (stream.UseAvailableRange)
                        stream.AvailableRangePdf.Write(bw);
                }

                return ms.ToArray();
            }
        }
        private void Parse(byte[] htsvoice)
        {
            if (Encoding.ASCII.GetString(htsvoice, 0, LABEL_GLOBAL.Length) != LABEL_GLOBAL)
                throw new Exception("Not an .htsvoice!");

            var globalSection = GetTextSection(htsvoice, LABEL_GLOBAL, LABEL_STREAM);
            var streamSection = GetTextSection(htsvoice, LABEL_STREAM, LABEL_POSITION);
            var positionSection = GetTextSection(htsvoice, LABEL_POSITION, LABEL_DATA);
            var dataSection = GetBinarySection(htsvoice, LABEL_DATA, null);

            foreach (var line in globalSection.Split('\n'))
            {
                var kvp = line.Split(':');
                if (kvp.Length < 2)
                    continue;

                switch (kvp[0])
                {
                    case HTS_VOICE_VERSION:
                        HtsVoiceVersion = float.Parse(kvp[1]);
                        break;
                    case SAMPLING_FREQUENCY:
                        SamplingFrequency = int.Parse(kvp[1]);
                        break;
                    case FRAME_PERIOD:
                        FramePeriod = int.Parse(kvp[1]);
                        break;
                    case NUM_STATES:
                        NumStates = int.Parse(kvp[1]);
                        break;
                    case NUM_STREAMS:
                        continue;
                    case STREAM_TYPE:
                        if (positionSection.Contains(DURATION_PDF))
                            Streams.Add(new HtsStream { Type = HtsStreamType.DUR });
                        if (positionSection.Contains(PDURATION_PDF))
                            Streams.Add(new HtsStream { Type = HtsStreamType.PDUR });
                        if (positionSection.Contains(REALIGNMENT_C_PDF))
                            Streams.Add(new HtsStream { Type = HtsStreamType.RC });
                        if (positionSection.Contains(REALIGNMENT_S_PDF))
                            Streams.Add(new HtsStream { Type = HtsStreamType.RS });
                        foreach (var type in kvp[1].Split(','))
                        {
                            var result = (HtsStreamType)Enum.Parse(typeof(HtsStreamType), type);
                            Streams.Add(new HtsStream { Type = result });
                        }
                        break;
                    case FULLCONTEXT_FORMAT:
                        FullContextFormat = kvp[1];
                        break;
                    case FULLCONTEXT_VERSION:
                        try
                        {
                            FullContextVersion = float.Parse(kvp[1]);
                        }
                        catch (Exception ex)
                        {
                            FullContextVersion = float.NaN;
                        }
                        break;
                    case GV_OFF_CONTEXT:
                        GvOffContext = kvp[1];
                        break;
                    case COMMENT:
                        Comment = kvp[1];
                        break;
                    default:
                        GlobalExtraMetadata.Add(kvp[0], kvp[1]);
                        break;
                }
            }

            foreach (var line in streamSection.Split('\n'))
            {
                var field = line.GetFieldWithStreamType();
                if (field.Length < 2)
                    continue;

                HtsStreamType streamType;
                Enum.TryParse(field[2], out streamType);
                var stream = Streams.FirstOrDefault(x => x.Type == streamType);

                switch (field[0])
                {
                    case VECTOR_LENGTH:
                        stream.Pdf.VectorLength = int.Parse(field[1]);
                        break;
                    case IS_MSD:
                        stream.Pdf.IsMsd = Convert.ToBoolean(int.Parse(field[1]));
                        break;
                    case NUM_WINDOWS:
                        continue;
                    case USE_GV:
                        stream.UseGv = Convert.ToBoolean(int.Parse(field[1]));
                        break;
                    case USE_AVAILABLE_RANGE:
                        stream.UseAvailableRange = Convert.ToBoolean(int.Parse(field[1]));
                        break;
                    case OPTION:
                        stream.Option = field[1];
                        break;
                    case USE_VOICE_MODIFICATION:
                        var kvp = line.Split(new char[] {':'}, 2);
                        if (!StreamExtraMetadata.ContainsKey(kvp[0]))
                            StreamExtraMetadata.Add(kvp[0], kvp[1]);
                        break;
                    default:
                        StreamExtraMetadata.Add(field[0], field[1]);
                        break;
                }
            }

            foreach (var line in positionSection.Split('\n'))
            {
                var field = line.GetFieldWithStreamType();
                if (field.Length < 2)
                    continue;

                HtsStreamType streamType;
                Enum.TryParse(field[2], out streamType);
                var stream = Streams.FirstOrDefault(x => x.Type == streamType);

                switch (field[0])
                {
                    case DURATION_PDF:
                        stream = Streams.FirstOrDefault(x => x.Type == HtsStreamType.DUR);
                        stream.Pdf.Read(dataSection.ReadFromPosition(field[1]), 1, 5, 1);
                        break;
                    case DURATION_TREE:
                        stream = Streams.FirstOrDefault(x => x.Type == HtsStreamType.DUR);
                        var binDurTree = dataSection.ReadFromPosition(field[1]);
                        stream.Tree = Encoding.ASCII.GetString(binDurTree);
                        break;
                    case PDURATION_PDF:
                        stream = Streams.FirstOrDefault(x => x.Type == HtsStreamType.PDUR);
                        stream.Pdf.Read(dataSection.ReadFromPosition(field[1]), 1, 1, 1);
                        break;
                    case PDURATION_TREE:
                        stream = Streams.FirstOrDefault(x => x.Type == HtsStreamType.PDUR);
                        var binPdurTree = dataSection.ReadFromPosition(field[1]);
                        stream.Tree = Encoding.ASCII.GetString(binPdurTree);
                        break;
                    case REALIGNMENT_C_PDF:
                        stream = Streams.FirstOrDefault(x => x.Type == HtsStreamType.RC);
                        stream.Pdf.Read(dataSection.ReadFromPosition(field[1]), 1, 1, 1);
                        break;
                    case REALIGNMENT_C_TREE:
                        stream = Streams.FirstOrDefault(x => x.Type == HtsStreamType.RC);
                        var binRcTree = dataSection.ReadFromPosition(field[1]);
                        stream.Tree = Encoding.ASCII.GetString(binRcTree);
                        break;
                    case REALIGNMENT_S_PDF:
                        stream = Streams.FirstOrDefault(x => x.Type == HtsStreamType.RS);
                        stream.Pdf.Read(dataSection.ReadFromPosition(field[1]), 1, 1, 1);
                        break;
                    case REALIGNMENT_S_TREE:
                        stream = Streams.FirstOrDefault(x => x.Type == HtsStreamType.RS);
                        var binRsTree = dataSection.ReadFromPosition(field[1]);
                        stream.Tree = Encoding.ASCII.GetString(binRsTree);
                        break;
                    case STREAM_WIN:
                        foreach (var pos in field[1].Split(','))
                        {
                            var binWin = dataSection.ReadFromPosition(pos);
                            stream.Windows.Add(Encoding.ASCII.GetString(binWin));
                        }
                        break;
                    case STREAM_PDF:
                        stream.Pdf.Read(dataSection.ReadFromPosition(field[1]), NumStates, stream.Pdf.VectorLength, stream.NumWindows);
                        break;
                    case STREAM_TREE:
                        var binTree = dataSection.ReadFromPosition(field[1]);
                        stream.Tree = Encoding.ASCII.GetString(binTree);
                        break;
                    case GV_PDF:
                        stream.GvPdf.Read(dataSection.ReadFromPosition(field[1]), 1, stream.Pdf.VectorLength, 1);
                        break;
                    case GV_TREE:
                        var binGvTree = dataSection.ReadFromPosition(field[1]);
                        stream.GvTree = Encoding.ASCII.GetString(binGvTree);
                        break;
                    case AVAILABLE_RANGE_PDF:
                        stream.AvailableRangePdf.Read(dataSection.ReadFromPosition(field[1]), 1, stream.Pdf.VectorLength, 1);
                        break;
                    default:
                        var kvp = line.Split(new char[] { ':' }, 2);
                        PositionExtraStream.Add(kvp[0], dataSection.ReadFromPosition(kvp[1]));
                        break;
                }
            }
        }
        private string GetTextSection(byte[] htsvoice, string currentLabel, string nextLabel)
        {
            var ini = Encoding.ASCII.GetString(htsvoice);
            var index = ini.IndexOf(currentLabel) + currentLabel.Length;
            int length;
            try { length = ini.IndexOf(nextLabel) - index; }
            catch { length = htsvoice.Length - index; }
            var section = ini.Substring(index, length).Trim('\n');
            return section;
        }
        private byte[] GetBinarySection(byte[] htsvoice, string currentLabel, string nextLabel)
        {
            var ini = Encoding.ASCII.GetString(htsvoice);
            var index = ini.IndexOf(currentLabel) + currentLabel.Length + 1;
            int length;
            try { length = ini.IndexOf(nextLabel) - index; }
            catch { length = htsvoice.Length - index; }
            var section = new byte[length];
            Buffer.BlockCopy(htsvoice, index, section, 0, length);
            return section;
        }
    }
}
