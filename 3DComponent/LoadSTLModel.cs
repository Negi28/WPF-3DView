using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK;

namespace PRC_Phatv_3DView
{
    class LoadSTLModel
    {
        public float _maxValue = 0.0f;
        public int FaceCount = 0;
        public int VertexCount = 0;
        public Vector3 vtMin;
        public Vector3 vtMax;
        public List<float> data = new List<float>();
        public LoadSTLModel(string path)
        {
            try
            {
                byte[] c = new byte[96];

                FileStream fileStream = new FileStream(path, FileMode.Open);
                fileStream.Seek(80, SeekOrigin.Begin);
                fileStream.Read(c, 0, 95);
                for (int i = 0; i < 95; ++i)
                {
                    if (((c[i] < ' ') || (c[i] > '~')) && (c[i] != 10) && (c[i] != 13) && (c[i] != '\t'))
                    {
                        //Check Binary file
                        break;
                    }
                }

                float[] minArr = new float[] { 1000, 1000, 1000 };
                float[] maxArr = new float[] { -1000, -1000, -1000 };

                byte[] buffer = new byte[4];
                fileStream.Seek(80, SeekOrigin.Begin);
                fileStream.Read(buffer, 0, 4);
                UInt32 Number = BitConverter.ToUInt32(buffer, 0);

                byte[] nb = new byte[3 * 4];
                float[] n = new float[3];
                byte[] vb = new byte[3 * 3 * 4];
                float[,] v = new float[3, 3];
                byte[] Skip = new byte[2];
                for (int count = 0; count < Number; ++count)
                {
                    fileStream.Read(nb, 0, 4 * 3);
                    n = BytesToFloats(nb);  //Normal vector
                    fileStream.Read(vb, 0, 4 * 3 * 3);
                    float[] vf = BytesToFloats(vb);
                    v = Float1DTo2D(vf);    //Vertex vector
                    fileStream.Read(Skip, 0, 2);
                    if (IsEqual(v[0, 0], v[1, 0]) && IsEqual(v[0, 1], v[1, 1]) && IsEqual(v[0, 2], v[1, 2])) { }
                    else if (IsEqual(v[0, 0], v[2, 0]) && IsEqual(v[0, 1], v[2, 1]) && IsEqual(v[0, 2], v[2, 2])) { }
                    else if (IsEqual(v[1, 0], v[2, 0]) && IsEqual(v[1, 1], v[2, 1]) && IsEqual(v[1, 2], v[2, 2])) { }
                    else
                    {

                        for (int i = 0; i < 3; i++)
                        {
                            //Vertex
                            for (int j = 0; j < 3; j++)
                            {
                                data.Add(v[i, j]);
                                if (_maxValue < Math.Abs(v[i, j])) _maxValue = Math.Abs(v[i, j]);
                            }
                            //Normal
                            for (int k = 0; k < 3; k++)
                            {
                                data.Add(n[k]);
                            }
                            VertexCount++;

                            if (v[i, 0] < minArr[0]) minArr[0] = v[i, 0];
                            if (v[i, 1] < minArr[1]) minArr[1] = v[i, 1];
                            if (v[i, 2] < minArr[2]) minArr[2] = v[i, 2];

                            if (v[i, 0] > maxArr[0]) maxArr[0] = v[i, 0];
                            if (v[i, 1] > maxArr[1]) maxArr[1] = v[i, 1];
                            if (v[i, 2] > maxArr[2]) maxArr[2] = v[i, 2];
                        }
                        FaceCount++;
                    }
                }//end for

                fileStream.Close();

                vtMin = new Vector3(minArr[0], minArr[1], minArr[2]);
                vtMax = new Vector3(maxArr[0], maxArr[1], maxArr[2]);

                Debug.WriteLine("Data Count : " + data.Count);
                Debug.WriteLine("X Range : " + minArr[0] + " -> " + maxArr[0]);
                Debug.WriteLine("Y Range : " + minArr[1] + " -> " + maxArr[1]);
                Debug.WriteLine("Z Range : " + minArr[2] + " -> " + maxArr[2]);
                Debug.WriteLine("_maxValue : " + _maxValue);
                Debug.WriteLine("VertexCount : " + VertexCount);
                Debug.WriteLine("FaceCount : " + FaceCount);

            }
            catch (Exception ex)
            {
                ErrorLog = ex.ToString();
            }
        }
        public string ErrorLog { get; set; }

        float[] BytesToFloats(byte[] bytes)
        {
            float[] floats = new float[bytes.Length / 4];

            for (int i = 0; i < bytes.Length / 4; i++)
                floats[i] = BitConverter.ToSingle(bytes, i * 4);

            return floats;
        }

        float[,] Float1DTo2D(float[] arr1)
        {
            int size = arr1.Length / 3;
            if (size != 3) return null;
            float[,] arr2 = new float[size, size];
            for (int i = 0; i < arr1.Length; i++)
            {
                int row = i / size;
                int col = i % size;
                arr2[row, col] = arr1[i];
            }
            return arr2;
        }

        byte[] FloatsToBytes(float[] floats)
        {
            byte[] bytes = new byte[floats.Length * 4];

            for (int i = 0; i < bytes.Length; i++)
            {
                Buffer.BlockCopy(BitConverter.GetBytes(floats[i]), 0, bytes, i * 4, 4);
            }
            return bytes;
        }

        bool IsEqual(float a, float b)
        {
            float c = a - b;
            if (c < 0.01 && (c > (-0.01))) return true;
            else return false;
        }
    }
}
