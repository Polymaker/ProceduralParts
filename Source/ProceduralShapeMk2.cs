using KSPAPIExtensions;
using ProceduralParts.Geometry;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace ProceduralParts
{
    public class ProceduralShapeMk2 : ProceduralAbstractSoRShape
    {
        #region Properties (fields)

        [KSPField(isPersistant = true, guiActiveEditor = true, guiActive = false, guiName = "Diameter", guiFormat = "F3", guiUnits = "m"),
         UI_FloatEdit(scene = UI_Scene.Editor, incrementSlide = 0.001f, sigFigs = 3, unit = "m", useSI = true)]
        public float diameter = 1.25f;
        private float oldDiameter;

        [KSPField(isPersistant = true, guiActiveEditor = true, guiActive = false, guiName = "Length", guiFormat = "F3", guiUnits = "m"),
         UI_FloatEdit(scene = UI_Scene.Editor, incrementSlide = 0.001f, sigFigs = 3, unit = "m", useSI = true)]
        public float length = 1f;
        private float oldLength;

        #endregion

        private float CalcVolume()
        {
            return 1f;
            //float theta = (Mathf.PI * 2f) / (float)sides;
            //float radius = diameter / 2f;

            //float tHeight = isInscribed ? radius : radius * Mathf.Cos(theta / 2f);
            //float tBase = 2f * tHeight * Mathf.Tan(theta / 2f);

            //return ((tHeight * tBase / 2f) * sides) * length;
        }

        protected override void UpdateShape(bool force)
        {
            if (!force &&
                oldDiameter == diameter &&
                oldLength == length)
                return;

            Volume = CalcVolume();

            var mk2Profile = ProfileSection.GetMk2Section(diameter);

            RaiseChangeTextureScale("sides", PPart.SidesMaterial, new Vector2(mk2Profile.Perimeter * 2f, length));

            Vector2 norm = new Vector2(1, 0);
            UpdateMeshNodesSizes(
                new ProfilePoint(diameter, -0.5f * length, 0f, norm),
                new ProfilePoint(diameter, 0.5f * length, 1f, norm)
                );

            WriteMeshes(
                MeshBuilder.ExtrudeSides(mk2Profile, length),
                //new UncheckedMesh(0,0),
                MeshBuilder.CreateCaps(mk2Profile, length),
                //new UncheckedMesh(0,0)
                MeshBuilder.CreateCollider(mk2Profile, length)
                );

            oldDiameter = diameter;
            oldLength = length;

            UpdateInterops();
        }

        public override void OnStart(StartState state)
        {
            UpdateTechConstraints();
        }

        public override void UpdateTechConstraints()
        {
            if (!HighLogic.LoadedSceneIsEditor)
                return;

            if (PPart.lengthMin == PPart.lengthMax)
                Fields["length"].guiActiveEditor = false;
            else
            {
                UI_FloatEdit lengthEdit = (UI_FloatEdit)Fields["length"].uiControlEditor;
                lengthEdit.maxValue = PPart.lengthMax;
                lengthEdit.minValue = PPart.lengthMin;
                lengthEdit.incrementLarge = PPart.lengthLargeStep;
                lengthEdit.incrementSmall = PPart.lengthSmallStep;
                length = Mathf.Clamp(length, PPart.lengthMin, PPart.lengthMax);
            }

            if (PPart.diameterMin == PPart.diameterMax)
                Fields["diameter"].guiActiveEditor = false;
            else
            {
                UI_FloatEdit diameterEdit = (UI_FloatEdit)Fields["diameter"].uiControlEditor;
                if (null != diameterEdit)
                {
                    diameterEdit.maxValue = PPart.diameterMax;
                    diameterEdit.minValue = PPart.diameterMin;
                    diameterEdit.incrementLarge = PPart.diameterLargeStep;
                    diameterEdit.incrementSmall = PPart.diameterSmallStep;
                    diameter = Mathf.Clamp(diameter, PPart.diameterMin, PPart.diameterMax);
                }
                else
                    Debug.LogError("*PP* could not find field 'diameter'");
            }
        }


        public override void UpdateTFInterops()
        {
            ProceduralPart.tfInterface.InvokeMember("AddInteropValue", ProceduralPart.tfBindingFlags, null, null, new System.Object[] { this.part, "diam1", diameter, "ProceduralParts" });
            ProceduralPart.tfInterface.InvokeMember("AddInteropValue", ProceduralPart.tfBindingFlags, null, null, new System.Object[] { this.part, "length", length, "ProceduralParts" });
        }

    }
}
