using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using UnityEngine;

namespace ProceduralParts.Geometry
{
    [DebuggerDisplay("Pos = {Pos} UV = {Uv}")]
    public class Vertex
    {
        public Vector3 Pos { get; set; }
        public Vector3 Norm { get; set; }
        public Vector4 Tan { get; set; }
        public Vector2 Uv { get; set; }

        public Vertex()
        {
            Pos = Vector3.zero;
            Norm = Vector3.zero;
            Tan = Vector4.zero;
            Uv = Vector2.zero;
        }

        public Vertex(Vector3 pos, Vector3 norm, Vector4 tan, Vector2 uv)
        {
            Pos = pos;
            Norm = norm;
            Tan = tan;
            Uv = uv;
        }

        public override string ToString()
        {
            var sb = new StringBuilder();
            sb.AppendLine("Pos = " + Pos);
            sb.AppendLine("Norm = " + Norm);
            sb.AppendLine("Tan = " + Tan);
            sb.AppendLine("Uv = " + Uv);
            return sb.ToString();
        }
    }
}
