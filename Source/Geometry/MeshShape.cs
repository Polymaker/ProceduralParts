using KSPAPIExtensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace ProceduralParts.Geometry
{
    public class MeshShape
    {
        // Fields...
        private MeshLayer[] _Sections;

        public MeshLayer[] Sections
        {
            get { return _Sections; }
        }

        public int PointCount
        {
            get { return Sections[0].PointCount; }
        }

        public int SubDivCount
        {
            get { return Sections.Length; }
        }

        public MeshLayer Top
        {
            get { return Sections[0]; }
        }

        public MeshLayer Bottom
        {
            get { return Sections[Sections.Length - 1]; }
        }

        public float Length
        {
            get { return Top.PosY - Bottom.PosY; }
        }

        internal MeshBuilder.ShapeParams Parameters;

        public MeshShape(IEnumerable<MeshLayer> sections)
        {
            _Sections = sections.OrderByDescending(s => s.PosY).ToArray();
            for (int i = 0; i < Sections.Length; i++)
            {
                Sections[i].Mesh = this;
                Sections[i].Index = i;
            }

            for (int i = 0; i < Sections.Length; i++)
                Sections[i].Init();
        }

        public MeshShape(params MeshLayer[] sections)
        {
            _Sections = sections.OrderByDescending(s => s.PosY).ToArray();
            for (int i = 0; i < Sections.Length; i++)
            {
                Sections[i].Mesh = this;
                Sections[i].Index = i;
            }

            for (int i = 0; i < Sections.Length; i++)
                Sections[i].Init();
        }
    }
}
