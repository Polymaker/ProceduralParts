using ProceduralParts.Geometry;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using KSPAPIExtensions;

namespace ProceduralParts
{
    public class ProceduralShapeExtruded : ProceduralAbstractSoRShape
    {

        #region Properties (fields)

        private string[] ShapeNames = new string[] { "Polygon", "Mk2", "Mk3" };

        [KSPField(isPersistant = true, guiActiveEditor = true, guiActive = false, guiName = "Profile"),
         UI_ChooseOption(scene = UI_Scene.Editor)]
        public string extrudeShape = "Polygon";
        private string oldExtrudeShape;

        [KSPField(isPersistant = true, guiActiveEditor = false, guiActive = false, guiName = "Sides", guiFormat = "0"),
         UI_FloatEdit(scene = UI_Scene.Editor)]
        public float polySides = 4f;
        private float oldPolySides;

        [KSPField(isPersistant = true, guiActiveEditor = true, guiActive = false, guiName = "Diameter", guiFormat = "F3", guiUnits = "m"),
         UI_FloatEdit(scene = UI_Scene.Editor, minValue = 0.25f, incrementLarge = 1.25f, incrementSmall = 0.25f, incrementSlide = 0.001f, sigFigs = 3, unit = "m", useSI = true)]
        public float diameter = 1.25f;
        protected float oldDiameter;

        [KSPField(isPersistant = true, guiActiveEditor = false, guiActive = false, guiName = "Mode:"),
         UI_Toggle(disabledText = "Circumscribed", enabledText = "Inscribed")]
        public bool isInscribed = false;
        private bool oldIsInscribed;
        
        [KSPField(isPersistant = true, guiActiveEditor = true, guiActive = false, guiName = "Length", guiFormat = "F3", guiUnits = "m"),
         UI_FloatEdit(scene = UI_Scene.Editor, incrementSlide = 0.001f, sigFigs = 3, unit = "m", useSI = true)]
        public float length = 1f;
        protected float oldLength;

        #endregion

        public override void OnStart(StartState state)
        {
            if (!HighLogic.LoadedSceneIsEditor)
                return;

            UI_ChooseOption shapeEdit = (UI_ChooseOption)Fields["extrudeShape"].uiControlEditor;
            shapeEdit.options = ShapeNames;
            UpdateTechConstraints();

            this.ReorderField(Fields["costDisplay"], 10);
        }

        #region Shape

        protected override void UpdateShape(bool force)
        {
            //DebugMeshNormals(SidesMesh, Color.green);

            if (!force &&
                oldDiameter == diameter &&
                oldLength == length &&
                oldPolySides == polySides &&
                oldIsInscribed == isInscribed &&
                oldExtrudeShape == extrudeShape)
                return;

            CheckEditors();
            CheckSnapMk2Diameter();

            Vector2 norm = new Vector2(1, 0);

            UpdateMeshNodesSizes(
                new CircleSection(diameter, -0.5f * length, 0f, norm),
                new CircleSection(diameter, 0.5f * length, 1f, norm)
                );
            try
            {
                var extrudeProfile = GetSideSection(extrudeShape, diameter, (int)polySides, isInscribed);

                RaiseChangeTextureScale("sides", PPart.SidesMaterial, new Vector2(extrudeProfile.Perimeter * 2f, length));

                var extrudeMesh = MeshBuilder.CreateProceduralMesh(extrudeProfile, length);

                Volume = extrudeMesh.Volume;

                WriteMeshes(
                    extrudeMesh.SidesMesh,
                    extrudeMesh.CapsMesh,
                    extrudeMesh.ColliderMesh
                    );
            }
            catch (Exception ex)
            {
                Debug.LogException(ex);
            }
            oldDiameter = diameter;
            oldLength = length;
            oldPolySides = polySides;
            oldIsInscribed = isInscribed;
            oldExtrudeShape = extrudeShape;

            UpdateInterops();
        }

        private ProfileSection GetSideSection(string shapeName, float diam, int sideCount, bool inscribed)
        {
            switch (shapeName)
            {
                default:
                //case "Cylinder":
                //    return ProfileSection.GetCylinderSection(diam);
                case "Polygon":
                    return ProfileSection.GetPrismSection(sideCount, GetPolygonOuterDiam(inscribed, diam, sideCount));
                case "Mk2":
                    return ProfileSection.GetMk2Section(diam);
                case "Mk3":
                    return ProfileSection.GetMk3Section(diam);
            }
        }

        private static float GetPolygonOuterDiam(bool inscribed, float diam, int sides)
        {
            if (!inscribed)
                return diam;
            float theta = (Mathf.PI * 2f) / (float)sides;
            return diam / Mathf.Cos(theta / 2f);
        }

        #endregion

        private void CheckSnapMk2Diameter()
        {
            if (extrudeShape != oldExtrudeShape &&
                    extrudeShape == "Mk2" &&
                    diameter == 1.25f)
            {
                diameter = 1.5f;
            }
        }
        private void CheckEditors()
        {
            Fields["polySides"].guiActiveEditor = Fields["isInscribed"].guiActiveEditor = extrudeShape == "Polygon";
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

            UI_FloatEdit topDiameterEdit = (UI_FloatEdit)Fields["diameter"].uiControlEditor;
            topDiameterEdit.incrementLarge = PPart.diameterLargeStep;
            topDiameterEdit.incrementSmall = PPart.diameterSmallStep;
            topDiameterEdit.maxValue = PPart.diameterMax;
            topDiameterEdit.minValue = PPart.diameterMin;
            diameter = Mathf.Clamp(diameter, topDiameterEdit.minValue, topDiameterEdit.maxValue);

            UI_FloatEdit topSidesEdit = (UI_FloatEdit)Fields["polySides"].uiControlEditor;
            topSidesEdit.maxValue = 12f;
            topSidesEdit.minValue = 3f;
            topSidesEdit.incrementLarge = 1f;
            topSidesEdit.incrementSmall = 0f;
            topSidesEdit.incrementSlide = 1f;
            polySides = Mathf.Clamp(polySides, topSidesEdit.minValue, topSidesEdit.maxValue);
            
        }


        public override void UpdateTFInterops()
        {
            try
            {
                ProceduralPart.tfInterface.InvokeMember("AddInteropValue", ProceduralPart.tfBindingFlags, null, null, new System.Object[] { this.part, "diam1", diameter, "ProceduralParts" });
                ProceduralPart.tfInterface.InvokeMember("AddInteropValue", ProceduralPart.tfBindingFlags, null, null, new System.Object[] { this.part, "diam2", diameter, "ProceduralParts" });
                ProceduralPart.tfInterface.InvokeMember("AddInteropValue", ProceduralPart.tfBindingFlags, null, null, new System.Object[] { this.part, "length", length, "ProceduralParts" });
            }
            catch (Exception ex)
            {
                Debug.LogException(ex);
            }
        }
    }
}
