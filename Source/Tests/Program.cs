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
            
            var cylinderProfile = ProfileSection.GetCylinderSection(1.25f);
            var mk2Profile = ProfileSection.GetMk2Section(1.5f);
            //foreach (var point in mk2Profile.Points)
            //{
            //    Trace.WriteLine(string.Format("{0} {1} at {2}", point.Position, point.Normal, point.NormalizedRadial));
            //}
            var mk3Profile = ProfileSection.GetMk3Section(3.75f);
            var hexProfile = ProfileSection.GetPrismSection(6, 1.25f);
            var pentagonProfile = ProfileSection.GetPrismSection(5, 1.25f);

            var adapt1 = ProfileSection.CreateAdapter(cylinderProfile, pentagonProfile);
            var adapt2 = ProfileSection.CreateAdapter(pentagonProfile, cylinderProfile);
            var baseMesh = MeshBuilder.CreateProceduralMesh(mk2Profile, mk3Profile, 2f, 3);

            //Trace.WriteLine("Hex profile:");
            foreach (var point in pentagonProfile.Points)
            {
                Trace.WriteLine(string.Format("p:{0} n:{1} u:{2:0.00} at {3}", point.Position, point.Normal, point.SideUV, point.NormalizedRadial));
            }
            //Trace.WriteLine("Cylinder profile:");
            //foreach (var point in mk3Profile.Points)
            //{
            //    Trace.WriteLine(string.Format("p:{0} n:{1} u:{2:0.00} at {3}", point.Position, point.Normal, point.SideUV, point.RadialAngle));
            //}
            Trace.WriteLine("Adapter (Cylinder-> Hex) profile:");
            foreach (var point in adapt1.Points)
            {
                Trace.WriteLine(string.Format("p:{0} n:{1} u:{2:0.00} at {3}", point.Position, point.Normal, point.SideUV, point.NormalizedRadial));
            }
            Trace.WriteLine("Adapter (Hex-> Cylinder) profile:");
            foreach (var point in adapt2.Points)
            {
                Trace.WriteLine(string.Format("p:{0} n:{1} u:{2:0.00} at {3}", point.Position, point.Normal, point.SideUV, point.NormalizedRadial));
            }
        }
    }
}
