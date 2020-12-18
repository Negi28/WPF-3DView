using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK;
using Presto.PRC.Types;

namespace PRC_Phatv_3DView
{
    public class VisualObject
    {
        public List<STLRenderObject> stlListModels;
        public Vector3 currPosition;
        public Vector3 prePosition;
        public Vector3 targetPosition;
        public bool bEnableMove = false;

        public VisualObject(STLRenderObject stbModel)
        {
            stlListModels = new List<STLRenderObject>();
            stlListModels.Add(stbModel);
        }

        public VisualObject()
        {
            stlListModels = new List<STLRenderObject>();
        }

        public void Update(Vector3 speed)
        {
            currPosition += speed;
            foreach (var renderObject in stlListModels)
            {
                renderObject.UpdateTranslation(Matrix4.CreateTranslation(speed));
            }
        }

        public void RealLMMC_Update(Vector3 curPositon )
        {
            Vector3 speed = curPositon - prePosition;
            if (speed != new Vector3(0.0f, 0.0f, 0.0f))
            {
                prePosition = curPositon;
                foreach (var renderObject in stlListModels)
                {
                    renderObject.UpdateTranslation(Matrix4.CreateTranslation(speed));
                }
            }
        }

    }
}
