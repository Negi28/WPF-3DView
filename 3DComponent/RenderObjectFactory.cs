using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK;

namespace PRC_Phatv_3DView
{
    class RenderObjectFactory
    {
        public static float[] CreateCoordinate()
        {
            float[] vertices =
            {
                // Position       //Color  
                0.0f, 0.0f, 0.0f, 1.0f, 0.0f, 0.0f,
                1.0f, 0.0f, 0.0f, 1.0f, 0.0f, 0.0f,
                0.0f, 0.0f, 0.0f, 0.0f, 1.0f, 0.0f,
                0.0f, 1.0f, 0.0f, 0.0f, 1.0f, 0.0f,
                0.0f, 0.0f, 0.0f, 0.0f, 0.0f, 1.0f,
                0.0f, 0.0f, 1.0f, 0.0f, 0.0f, 1.0f
            };
            return vertices;
        }
        public static float[] CreateCoordinate(float length)
        {
            float[] vertices =
            {
                // Position       //Color  
                0.0f, 0.0f, 0.0f, 1.0f, 0.0f, 0.0f,
                length, 0.0f, 0.0f, 1.0f, 0.0f, 0.0f,
                0.0f, 0.0f, 0.0f, 0.0f, 1.0f, 0.0f,
                0.0f, length, 0.0f, 0.0f, 1.0f, 0.0f,
                0.0f, 0.0f, 0.0f, 0.0f, 0.0f, 1.0f,
                0.0f, 0.0f, length, 0.0f, 0.0f, 1.0f
            };
            return vertices;
        }

        public static float[] CreateRectPrism(Vector3 vtMin, Vector3 vtMax, Vector3 color)
        {
            float[] vertices =
            {
                // Position                // color
                vtMin.X, vtMin.Y, vtMin.Z, color.X, color.Y, color.Z,
                vtMax.X, vtMin.Y, vtMin.Z, color.X, color.Y, color.Z,

                vtMax.X, vtMin.Y, vtMin.Z, color.X, color.Y, color.Z,
                vtMax.X, vtMin.Y, vtMax.Z, color.X, color.Y, color.Z,

                vtMax.X, vtMin.Y, vtMax.Z, color.X, color.Y, color.Z,
                vtMin.X, vtMin.Y, vtMax.Z, color.X, color.Y, color.Z,

                vtMin.X, vtMin.Y, vtMax.Z, color.X, color.Y, color.Z,
                vtMin.X, vtMin.Y, vtMin.Z, color.X, color.Y, color.Z,

                vtMin.X, vtMin.Y, vtMin.Z, color.X, color.Y, color.Z,
                vtMin.X, vtMax.Y, vtMin.Z, color.X, color.Y, color.Z,

                vtMax.X, vtMin.Y, vtMin.Z, color.X, color.Y, color.Z,
                vtMax.X, vtMax.Y, vtMin.Z, color.X, color.Y, color.Z,

                vtMax.X, vtMin.Y, vtMax.Z, color.X, color.Y, color.Z,
                vtMax.X, vtMax.Y, vtMax.Z, color.X, color.Y, color.Z,

                vtMin.X, vtMin.Y, vtMax.Z, color.X, color.Y, color.Z,
                vtMin.X, vtMax.Y, vtMax.Z, color.X, color.Y, color.Z,

                vtMax.X, vtMax.Y, vtMax.Z, color.X, color.Y, color.Z,
                vtMax.X, vtMax.Y, vtMin.Z, color.X, color.Y, color.Z,

                vtMax.X, vtMax.Y, vtMin.Z, color.X, color.Y, color.Z,
                vtMin.X, vtMax.Y, vtMin.Z, color.X, color.Y, color.Z,

                vtMin.X, vtMax.Y, vtMin.Z, color.X, color.Y, color.Z,
                vtMin.X, vtMax.Y, vtMax.Z, color.X, color.Y, color.Z,

                vtMin.X, vtMax.Y, vtMax.Z, color.X, color.Y, color.Z,
                vtMax.X, vtMax.Y, vtMax.Z, color.X, color.Y, color.Z
            };
            return vertices;
        }

        public static float[] CreateGrid(float fSize, Vector3 color)
        {
            List<float> data = new List<float>();

            float minVal = - (fSize / 2.0f);
            float maxVal = (fSize / 2.0f);
            float GridSize = fSize / 20.0f; //Size of Cell
            float numLine = 20;

            //Horizontal lines
            for (int i = 0; i <= numLine; i++)
            {
                //A
                data.Add(minVal);                           //X
                data.Add((i - numLine / 2.0f) * GridSize);  //Y
                data.Add(0.0f);                             //Z
                data.Add(color.X);
                data.Add(color.Y);
                data.Add(color.Z);

                //B
                data.Add(maxVal);
                data.Add((i - numLine / 2.0f) * GridSize);
                data.Add(0.0f);
                data.Add(color.X);
                data.Add(color.Y);
                data.Add(color.Z);
            }
            //0..20

            //21..41
            //Vertical lines
            for (int i = 0; i <= numLine; i++)
            {
                //A
                data.Add((i - numLine / 2.0f) * GridSize); //X
                data.Add(minVal);                           //Y
                data.Add(0.0f);                             //Z
                data.Add(color.X);
                data.Add(color.Y);
                data.Add(color.Z);

                //B
                data.Add((i - numLine / 2.0f) * GridSize);  //X
                data.Add(maxVal);                           //Y
                data.Add(0.0f);                             //Z
                data.Add(color.X);
                data.Add(color.Y);
                data.Add(color.Z);
            }

            return (data.ToArray());
        }
    }
}
