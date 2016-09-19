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
            var hexProfile = ProfileSection.GetPrismSection(4, 0.625f);
            var cylinderProfile = ProfileSection.GetCylinderSection(1.25f, 32);
            foreach (var point in hexProfile.Points)
            {
                Trace.WriteLine(string.Format("uv:{0:0.0} angle:{1:0.00} angle2:{2:0.00}", point.SideUV * 10, point.RadialUV, point.NormRadialUV));
                //Trace.WriteLine(string.Format("p:{0} n:{1} u:{2} at {3}", point.Position.ToString("G3"), point.NormRadialUV.ToString("G4"), point.SideUV, point.RadialUV));
            }

            //var mk3Profile = ProfileSection.GetMk3Section(3.75f);
            //var mk3Profile = ProfileSection.GetMk3Section(3.75f);
            //var pentagonProfile = ProfileSection.GetPrismSection(5, 1.25f);
            //Trace.WriteLine("mk3Profile:");
            //foreach (var point in mk3Profile.Points)
            //{
            //    Trace.WriteLine(string.Format("p:{0} n:{1} u:{2} at {3}", point.Position, point.NormRadialUV, point.SideUV, point.RadialUV));
            //}
            //var adapt1 = ProfileSection.CreateAdapter(hexProfile, cylinderProfile);
            //var adapt2 = ProfileSection.CreateAdapter(cylinderProfile, hexProfile);
            var baseMesh = MeshBuilder.CreateProceduralMesh(hexProfile, cylinderProfile, 2f, 3);

            //Trace.WriteLine("hexProfile:");
            //foreach (var point in adapt1.Points)
            //{
            //    Trace.WriteLine(string.Format("uv:{0:0.0} angle:{1:0.00} angle2:{2:0.00}", point.SideUV * 10, point.RadialUV, point.NormRadialUV));
            //    //Trace.WriteLine(string.Format("p:{0} n:{1} u:{2} at {3}", point.Position.ToString("G3"), point.NormRadialUV.ToString("G4"), point.SideUV, point.RadialUV));
            //}
            //Trace.WriteLine("cylinderProfile:");
            //foreach (var point in adapt2.Points)
            //{
            //    Trace.WriteLine(string.Format("uv:{0:0.0} angle:{1:0.00} angle2:{2:0.00}", point.SideUV * 10, point.RadialUV, point.NormRadialUV));
            //    //Trace.WriteLine(string.Format("p:{0} n:{1} u:{2} at {3}", point.Position.ToString("G3"), point.NormRadialUV.ToString("G4"), point.SideUV, point.RadialUV));
            //}

            //var baseMesh = MeshBuilder.CreateProceduralMesh(hexProfile, cylinderProfile, 2f, 3);

        }
    }
}
