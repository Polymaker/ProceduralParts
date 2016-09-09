using ProceduralParts.Geometry;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Tests
{
    class Program
    {
        static void Main(string[] args)
        {
            
            var testAngle = Angle.FromDegrees(352).IsBetween(Angle.FromDegrees(331), Angle.FromDegrees(28));
            var cylinderProfile = ProfileSection.GetCylinderSection(1.25f, 12);
            var mk2Profile = ProfileSection.GetMk2Section(1.5f);
            foreach (var point in mk2Profile.Points)
            {
                Trace.WriteLine(string.Format("{0} {1} at {2}", point.Position, point.Normal, point.NormalizedRadial));
            }
            var mk3Profile = ProfileSection.GetMk3Section(3.75f);
            var prismProfile = ProfileSection.GetPrismSection(6, 1.25f);
            var adapt1 = ProfileSection.CreateAdapter(mk2Profile, cylinderProfile);
            //var adapt2 = ProfileSection.CreateAdapter(prismProfile, cylinderProfile);
            var baseMesh = MeshBuilder.CreateProceduralMesh(cylinderProfile, prismProfile, 2f, 3);
            //var capsMesh = MeshBuilder.CreateCaps(mk3Profile, mk2Profile, 2f);
            //var colMesh = MeshBuilder.MergeMeshes(baseMesh, capsMesh);

            //for (int i = 0; i < baseMesh.SidesMesh.nVrt; i++ )
            //{
            //    Trace.WriteLine(string.Format("p:{0} n:{1} uv:{2}", baseMesh.SidesMesh.verticies[i], baseMesh.SidesMesh.normals[i], baseMesh.SidesMesh.uv[i]));
            //}
            foreach (var point in adapt1.Points)
            {
                Trace.WriteLine(string.Format("{0} {1} at {2}", point.Position, point.Normal, point.RadialAngle));
            }
            //Trace.WriteLine("");
            //foreach (var point in adapt2.Points)
            //{
            //    Trace.WriteLine(string.Format("{0} {1} at {2}", point.Position, point.Normal, point.RadialAngle));
            //}
        }
    }
}
