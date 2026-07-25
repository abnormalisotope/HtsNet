using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace HtsNet
{
    public struct HtsSinglePdf
    {
        public float[] Means;
        public float[] Variances;
        public float MSD;
    }
    public class HtsPdf
    {
        public int VectorLength { get; set; } = 0;
        public bool IsMsd { get; set; } = false;
        public int[] States { get; set; } = new int[0];
        public float[] Data { get; set; } = new float[0];
        public float[][][] Means { get; set; } = new float[0][][];
        public float[][][] Variances { get; set; } = new float[0][][];
        public float[][] MSD { get; set; } = new float[0][];
        public void ResizePDF(int newDimSize, int numWindows)
        {
            for (int i = 0; i < States.Length; i++)
            {
                int diff = 0;
                for (int j = 0; j < States[i]; j++)
                {
                    var oldMean = Means[i][j];
                    var oldDimSize = oldMean.Length / numWindows;
                    var length = oldDimSize < newDimSize ? oldDimSize : newDimSize;
                    var newMeans = new List<float[]>();
                    for (int k = 0; k < numWindows; k++)
                    {
                        var newMeanTemp = new float[newDimSize];
                        Array.Copy(oldMean, oldDimSize * k, newMeanTemp, 0, length);
                        newMeans.Add(newMeanTemp);
                    }
                    var newMean = newMeans.SelectMany(x => x).ToArray();
                    Means[i][j] = newMean;
                    diff += newMean.Length - oldMean.Length;

                    var oldVar = Variances[i][j];
                    oldDimSize = oldVar.Length / numWindows;
                    var newVars = new List<float[]>();
                    for (int k = 0; k < numWindows; k++)
                    {
                        var newVarTemp = new float[newDimSize];
                        Array.Copy(oldVar, newVarTemp, oldDimSize);
                        newVars.Add(newVarTemp);
                    }
                    var newVar = newVars.SelectMany(x => x).ToArray();
                    Variances[i][j] = newVar;
                    diff += newVar.Length - oldVar.Length;

                    if (IsMsd)
                    {
                        var oldMsd = MSD[i];
                        oldDimSize = oldMsd.Length / numWindows;
                        var newMsd = new float[newDimSize];
                        Array.Copy(oldMsd, newMsd, oldDimSize);
                        MSD[i] = newMsd;
                        diff += newMsd.Length - oldMsd.Length;
                    }   
                }
                //States[i] += diff;
            }
            VectorLength = newDimSize;
        }
        public HtsSinglePdf GetSinglePdf(int state, int index)
        {
            float[] means = Means[state][index];
            float[] variances = Variances[state][index];
            float msd = float.NaN;
            if (IsMsd)
                msd = MSD[state][index];
            return new HtsSinglePdf
            {
                Means = means,
                Variances = variances,
                MSD = msd,
            };
        }
        public void SetSinglePdf(int state, int index, HtsSinglePdf pdf)
        {
            Means[state][index] = pdf.Means;
            Variances[state][index] = pdf.Variances;
            if (IsMsd)
                MSD[state][index] = pdf.MSD;
        }
        public void Read(byte[] bytes, int numStates, int vectorLength, int numWindows)
        {
            using (var ms = new MemoryStream(bytes))
            using (var br = new BinaryReader(ms))
            {
                States = new int[numStates];
                for (int i = 0; i < numStates; i++)
                {
                    States[i] = br.ReadInt32();
                }

                var numFloats = (br.BaseStream.Length / 4) - numStates;
                Data = new float[numFloats];
                for (int i = 0; i < numFloats; i++)
                {
                    Data[i] = br.ReadSingle();
                }
            }

            var dataOffset = 0;
            Means = new float[numStates][][];
            Variances = new float[numStates][][];

            if (IsMsd)
                MSD = new float[numStates][];

            for (int i = 0; i < numStates; i++)
            {
                var numPdfs = States[i];
                Means[i] = new float[numPdfs][];
                Variances[i] = new float[numPdfs][];

                if (IsMsd)
                    MSD[i] = new float[numPdfs];

                for (int j = 0; j < numPdfs; j++)
                {
                    var pdfLength = vectorLength * numWindows;

                    Means[i][j] = new float[pdfLength];
                    Variances[i][j] = new float[pdfLength];

                    for (int k = 0; k < pdfLength; k++)
                    {
                        Means[i][j][k] = Data[dataOffset + k];
                        Variances[i][j][k] = Data[dataOffset + pdfLength + k];
                    }
                    dataOffset += pdfLength * 2;

                    if (IsMsd)
                    {
                        MSD[i][j] = Data[dataOffset];
                        dataOffset++;
                    }
                }
            }
        }
        public int Count()
        {
            int num = 0;
            num += States.Length * 4;
            for (int i = 0; i < States.Length; i++)
            {
                num += (Means[i][0].Length * 4) * States[i];
            }
            for (int i = 0; i < States.Length; i++)
            {
                num += (Variances[i][0].Length * 4) * States[i];
            }
            if (IsMsd)
            {
                for (int i = 0; i < States.Length; i++)
                {
                    num += MSD[i].Length * 4;
                }
            }
            return num;
        }
        public void Write(BinaryWriter bw)
        {
            for (int i = 0; i < States.Length; i++)
            {
                bw.Write(States[i]);
            }
            for (int i = 0; i < States.Length; i++)
            {
                for (int j = 0; j < States[i]; j++)
                {
                    for (int k = 0; k < Means[i][j].Length; k++)
                    {
                        bw.Write(Means[i][j][k]);
                    }
                    for (int k = 0; k < Variances[i][j].Length; k++)
                    {
                        bw.Write(Variances[i][j][k]);
                    }
                    if (IsMsd)
                        bw.Write(MSD[i][j]);
                }
            }
        }
    }
}