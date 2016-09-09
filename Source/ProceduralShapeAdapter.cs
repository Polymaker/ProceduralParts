using KSPAPIExtensions;
using ProceduralParts.Geometry;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace ProceduralParts
{
    public class ProceduralShapeAdapter : ProceduralAbstractSoRShape
    {

        #region Properties (fields)

        private string[] ShapeNames = new string[] { "Cylinder", "Polygon", "Mk2", "Mk3" };

        [KSPField(guiName = "Top", guiActiveEditor = true, guiActive = false, isPersistant = false)]
        private string topLabel;

        [KSPField(isPersistant = true, guiActiveEditor = true, guiActive = false, guiName = "Shape"),
         UI_ChooseOption(scene = UI_Scene.Editor)]
        public string topShape;
        private string oldTopShape;

        [KSPField(isPersistant = true, guiActiveEditor = false, guiActive = false, guiName = "Sides", guiFormat = "0"),
         UI_FloatEdit(scene = UI_Scene.Editor)]
        public float topPolySides = 4f;
        private float oldTopPolySides;

        [KSPField(isPersistant = true, guiActiveEditor = true, guiActive = false, guiName = "Diameter", guiFormat = "F3", guiUnits = "m"),
         UI_FloatEdit(scene = UI_Scene.Editor, minValue = 0.25f, incrementLarge = 1.25f, incrementSmall = 0.25f, incrementSlide = 0.001f, sigFigs = 3, unit = "m", useSI = true)]
        public float topDiameter = 1.25f;
        protected float oldTopDiameter;

        [KSPField(isPersistant = true, guiActiveEditor = false, guiActive = false, guiName = "Mode:"),
         UI_Toggle(disabledText = "Circumscribed", enabledText = "Inscribed")]
        public bool topIsInscribed = false;
        private bool oldTopIsInscribed;

        [KSPField(guiName = "Bottom", guiActiveEditor = true, guiActive = false, isPersistant = false)]
        private string bottomLabel;

        [KSPField(isPersistant = true, guiActiveEditor = true, guiActive = false, guiName = "Shape"),
         UI_ChooseOption(scene = UI_Scene.Editor)]
        public string bottomShape;
        private string oldBottomShape;

        [KSPField(isPersistant = true, guiActiveEditor = false, guiActive = false, guiName = "Sides", guiFormat = "0"),
         UI_FloatEdit(scene = UI_Scene.Editor)]
        public float bottomPolySides = 4f;
        private float oldBottomPolySides;

        [KSPField(isPersistant = true, guiActiveEditor = true, guiActive = false, guiName = "Diameter", guiFormat = "F3", guiUnits = "m"),
         UI_FloatEdit(scene = UI_Scene.Editor, incrementSlide = 0.001f, sigFigs = 3, unit = "m", useSI = true)]
        public float bottomDiameter = 1.25f;
        protected float oldBottomDiameter;

        [KSPField(isPersistant = true, guiActiveEditor = false, guiActive = false, guiName = "Mode:"),
         UI_Toggle(disabledText = "Circumscribed", enabledText = "Inscribed")]
        public bool bottomIsInscribed = false;
        private bool oldBottomIsInscribed;

        [KSPField(isPersistant = true, guiActiveEditor = true, guiActive = false, guiName = "Length", guiFormat = "F3", guiUnits = "m"),
         UI_FloatEdit(scene = UI_Scene.Editor, incrementSlide = 0.001f, sigFigs = 3, unit = "m", useSI = true)]
        public float length = 1f;
        protected float oldLength;


        #endregion

        public override void OnStart(StartState state)
        {
            if (!HighLogic.LoadedSceneIsEditor)
                return;
            
            UI_ChooseOption shapeEdit = (UI_ChooseOption)Fields["topShape"].uiControlEditor;
            shapeEdit.options = ShapeNames;
            shapeEdit = (UI_ChooseOption)Fields["bottomShape"].uiControlEditor;
            shapeEdit.options = ShapeNames;
            UpdateTechConstraints();
        }


        public override void UpdateTFInterops()
        {
            try
            {
                ProceduralPart.tfInterface.InvokeMember("AddInteropValue", ProceduralPart.tfBindingFlags, null, null, new System.Object[] { this.part, "diam1", topDiameter, "ProceduralParts" });
                ProceduralPart.tfInterface.InvokeMember("AddInteropValue", ProceduralPart.tfBindingFlags, null, null, new System.Object[] { this.part, "diam2", bottomDiameter, "ProceduralParts" });
                ProceduralPart.tfInterface.InvokeMember("AddInteropValue", ProceduralPart.tfBindingFlags, null, null, new System.Object[] { this.part, "length", length, "ProceduralParts" });
            }
            catch (Exception ex)
            {
                Debug.LogException(ex);
            }
        }

        protected override void UpdateShape(bool force)
        {
            DebugMeshNormals(SidesMesh, Color.red);
            DebugMeshTangents(SidesMesh, Color.blue);

            if (!force && 
                oldTopDiameter == topDiameter && 
                oldBottomDiameter == bottomDiameter &&
                oldLength == length &&
                oldBottomIsInscribed == bottomIsInscribed &&
                oldBottomPolySides == bottomPolySides &&
                oldBottomShape == bottomShape &&
                oldTopIsInscribed == topIsInscribed &&
                oldTopPolySides == topPolySides &&
                oldTopShape == topShape)
                return;

            CheckEditors();
            CheckSnapMk2Diameter();

            Vector2 norm = new Vector2(length, (bottomDiameter - topDiameter) / 2f).normalized;

            UpdateMeshNodesSizes(
                new CircleSection(bottomDiameter, -0.5f * length, 0f, norm),
                new CircleSection(topDiameter, 0.5f * length, 1f, norm)
                );

            var topSection = GetSideSection(topShape, topDiameter, (int)topPolySides, topIsInscribed);
            var bottomSection = GetSideSection(bottomShape, bottomDiameter, (int)bottomPolySides, bottomIsInscribed);

            var partMesh = MeshBuilder.CreateProceduralMesh(topSection, bottomSection, length, 3);
            Volume = partMesh.Volume;

            WriteMeshes(
                partMesh.SidesMesh,
                partMesh.CapsMesh,
                partMesh.ColliderMesh
                );

            oldTopDiameter = topDiameter;
            oldBottomDiameter = bottomDiameter;
            oldLength = length;
            oldBottomIsInscribed = bottomIsInscribed;
            oldBottomPolySides = bottomPolySides;
            oldBottomShape = bottomShape;
            oldTopIsInscribed = topIsInscribed;
            oldTopPolySides = topPolySides;
            oldTopShape = topShape;

            UpdateInterops();
        }

        private void CheckSnapMk2Diameter()
        {
            if (oldBottomShape != bottomShape &&
                    bottomShape == "Mk2" &&
                    bottomDiameter == 1.25f)
            {
                bottomDiameter = 1.5f;
            }

            if (oldTopShape != topShape &&
                topShape == "Mk2" &&
                topDiameter == 1.25f)
            {
                topDiameter = 1.5f;
            }
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

            //Top Editors
            UI_FloatEdit topDiameterEdit = (UI_FloatEdit)Fields["topDiameter"].uiControlEditor;
            topDiameterEdit.incrementLarge = PPart.diameterLargeStep;
            topDiameterEdit.incrementSmall = PPart.diameterSmallStep;
            topDiameterEdit.maxValue = PPart.diameterMax;
            topDiameterEdit.minValue = PPart.diameterMin;
            topDiameter = Mathf.Clamp(topDiameter, topDiameterEdit.minValue, topDiameterEdit.maxValue);

            UI_FloatEdit topSidesEdit = (UI_FloatEdit)Fields["topPolySides"].uiControlEditor;
            topSidesEdit.maxValue = 12f;
            topSidesEdit.minValue = 3f;
            topSidesEdit.incrementLarge = 1f;
            topSidesEdit.incrementSmall = 0f;
            topSidesEdit.incrementSlide = 1f;
            topPolySides = Mathf.Clamp(topPolySides, topSidesEdit.minValue, topSidesEdit.maxValue);

            //Bottom Editors
            UI_FloatEdit bottomDiameterEdit = (UI_FloatEdit)Fields["bottomDiameter"].uiControlEditor;
            bottomDiameterEdit.incrementLarge = PPart.diameterLargeStep;
            bottomDiameterEdit.incrementSmall = PPart.diameterSmallStep;
            bottomDiameterEdit.maxValue = PPart.diameterMax;
            bottomDiameterEdit.minValue = PPart.diameterMin;
            bottomDiameter = Mathf.Clamp(bottomDiameter, bottomDiameterEdit.minValue, bottomDiameterEdit.maxValue);

            UI_FloatEdit bottomSidesEdit = (UI_FloatEdit)Fields["bottomPolySides"].uiControlEditor;
            bottomSidesEdit.maxValue = 12f;
            bottomSidesEdit.minValue = 3f;
            bottomSidesEdit.incrementLarge = 1f;
            bottomSidesEdit.incrementSmall = 0f;
            bottomSidesEdit.incrementSlide = 1f;
            topPolySides = Mathf.Clamp(topPolySides, bottomSidesEdit.minValue, bottomSidesEdit.maxValue);

        }

        private void CheckEditors()
        {
            Fields["topPolySides"].guiActiveEditor = Fields["topIsInscribed"].guiActiveEditor = topShape == "Polygon";
            Fields["bottomPolySides"].guiActiveEditor = Fields["bottomIsInscribed"].guiActiveEditor = bottomShape == "Polygon";
        }

        private ProfileSection GetSideSection(string shapeName, float diam, int sideCount, bool inscribed)
        {
            switch (shapeName)
            {
                default:
                case "Cylinder":
                    return ProfileSection.GetCylinderSection(diam);
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
    }
}
