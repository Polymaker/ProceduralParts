using ProceduralParts.Geometry;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tests
{
    class Program
    {
        static void Main(string[] args)
        {
            
            var testAngle = Angle.FromDegrees(352).IsBetween(Angle.FromDegrees(331), Angle.FromDegrees(28));
            var cylinderProfile = ProfileSection.GetCylinderSection(1.25f, 64);
            var mk2Profile = ProfileSection.GetMk2Section(1.5f);
            var mk3Profile = ProfileSection.GetMk3Section(3.75f);
            var prismProfile = ProfileSection.GetPrismSection(8, 0.625f);
            //var adapt1 = ProfileSection.CreateAdapter(cylinderProfile, prismProfile);
            //var adapt2 = ProfileSection.CreateAdapter(prismProfile, cylinderProfile);
            var baseMesh = MeshBuilder.CreateAdapterSides(cylinderProfile, prismProfile, 2f);
            //var capsMesh = MeshBuilder.CreateCaps(mk3Profile, mk2Profile, 2f);
            //var colMesh = MeshBuilder.MergeMeshes(baseMesh, capsMesh);

 
            //foreach (var point in adapt1.Points)
            //{
            //    Trace.WriteLine(string.Format("{0} {1} at {2}", point.Position, point.Normal, point.RadialAngle));
            //}
            //Trace.WriteLine("");
            //foreach (var point in adapt2.Points)
            //{
            //    Trace.WriteLine(string.Format("{0} {1} at {2}", point.Position, point.Normal, point.RadialAngle));
            //}
        }
    }
}
