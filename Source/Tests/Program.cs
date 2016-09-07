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
            var cylinderProfile = ProfileSection.GetCylinderSection(1.25f, 64);
            var prismProfile = ProfileSection.GetPrismSection(6, 0.625f);
            var baseMesh = MeshBuilder.CreateAdapterSides(cylinderProfile, prismProfile, 2f);
            var capsMesh = MeshBuilder.CreateCaps(cylinderProfile, prismProfile, 2f);
            var colMesh = MeshBuilder.MergeMeshes(baseMesh, capsMesh);
            var test = capsMesh.triangles.Where(t => t > capsMesh.nVrt);
            //var adapterProfile = ProfileSection.CreateAdapter(prismProfile, cylinderProfile);
            //foreach (var point in adapterProfile.Points)
            //{
            //    Trace.WriteLine(string.Format("{0} {1} at {2}", point.Position, point.Normal, point.NormalizedRadial));
            //}
        }
    }
}
