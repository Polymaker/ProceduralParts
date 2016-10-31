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
         UI_Toggle(scene = UI_Scene.Editor, disabledText = "Circumscribed", enabledText = "Inscribed")]
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

        [KSPField(isPersistant = false, guiName = "Advanced options", guiActive = false, guiActiveEditor = true),
         UI_Toggle(scene = UI_Scene.Editor, enabledText="On",disabledText="Off")]
        public bool showAdvancedOptions = false;

        [KSPField(isPersistant = true, guiName = "Rotation", guiActive = false, guiActiveEditor = false, guiFormat = "F3", guiUnits = "°"),
         UI_FloatEdit(scene = UI_Scene.Editor, minValue = -180f, maxValue = 180f, incrementLarge = 45f, incrementSmall = 22.5f, incrementSlide = 0.5f, sigFigs = 2, unit = "°")]
        public float rotationOffset = 0;
        private float oldRotationOffset;

        [KSPField(isPersistant = true, guiName = "Slant", guiActive = false, guiActiveEditor = false, guiFormat = "F2"),
         UI_FloatEdit(scene = UI_Scene.Editor, minValue = -1f, maxValue = 1f, incrementLarge = 1f, incrementSmall = 0.25f, incrementSlide = 0.01f, sigFigs = 2)]
        public float slantOffset = 0;
        private float oldSlantOffset;

        [KSPField(isPersistant = true, guiActiveEditor = false, guiActive = false, guiName = "Curve"), UI_ChooseOption(scene = UI_Scene.Editor)]
        public string curveShape;
        private string oldCurveShape;

        #endregion

        public override void OnStart(StartState state)
        {
            if (!HighLogic.LoadedSceneIsEditor)
                return;
            
            UI_ChooseOption shapeEdit = (UI_ChooseOption)Fields["topShape"].uiControlEditor;
            shapeEdit.options = ShapeNames;
            shapeEdit = (UI_ChooseOption)Fields["bottomShape"].uiControlEditor;
            shapeEdit.options = ShapeNames;
            UI_ChooseOption selectedShapeEdit = (UI_ChooseOption)Fields["curveShape"].uiControlEditor;
            selectedShapeEdit.options = (from p in ProceduralShapeBezierCone.shapePresets select p.name).ToArray();
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

        private MeshShape lastShape = null;

        protected override void UpdateShape(bool force)
        {
            CheckEditors();

            if (!force && 
                oldTopDiameter == topDiameter && 
                oldBottomDiameter == bottomDiameter &&
                oldLength == length &&
                oldBottomIsInscribed == bottomIsInscribed &&
                oldBottomPolySides == bottomPolySides &&
                oldBottomShape == bottomShape &&
                oldTopIsInscribed == topIsInscribed &&
                oldTopPolySides == topPolySides &&
                oldTopShape == topShape &&
                oldRotationOffset == rotationOffset &&
                oldSlantOffset == slantOffset)
                return;
            
            CheckSnapMk2Diameter();

            var topSection = GetSideSection(topShape, topDiameter, (int)topPolySides, topIsInscribed);
            var bottomSection = GetSideSection(bottomShape, bottomDiameter, (int)bottomPolySides, bottomIsInscribed);

            

            var shapeParams = new MeshBuilder.ShapeParams() { Slant = slantOffset };
            if (topShape != "Cylinder")
                shapeParams.OffsetRotation = Angle.FromDegrees(rotationOffset);

            var partMesh = MeshBuilder.CreateProceduralMesh(topSection, bottomSection, length, shapeParams);
            
            if (partMesh != null)
            {
                lastShape = partMesh.Shape;

                Volume = partMesh.Volume;
                
                Vector2 norm = new Vector2(length, (bottomDiameter - topDiameter) / 2f).normalized;

                UpdateMeshNodesSizes(
                    new CircleSection(bottomDiameter, -0.5f * length, 0f, norm),
                    new CircleSection(topDiameter, 0.5f * length, 1f, norm, slantOffset: partMesh.Shape.Top.Offset)
                    );

                var texHorUV = Mathf.Max(topSection.Perimeter, bottomSection.Perimeter) * 2f;
                var texVerUV = Mathf.Sqrt(Mathf.Pow(Mathf.Max(Mathf.Abs(topSection.Size.magnitude - bottomSection.Size.magnitude), 1f), 2) * (length * length));

                RaiseChangeTextureScale("sides", PPart.SidesMaterial, new Vector2(texHorUV, texVerUV));

                WriteMeshes(
                    partMesh.SidesMesh,
                    partMesh.CapsMesh,
                    partMesh.ColliderMesh
                    );
            }

            
            //DebugMeshNormals(SidesMesh, Color.red);
            //DebugMeshTangents(SidesMesh, Color.blue);

            oldTopDiameter = topDiameter;
            oldBottomDiameter = bottomDiameter;
            oldLength = length;
            oldBottomIsInscribed = bottomIsInscribed;
            oldBottomPolySides = bottomPolySides;
            oldBottomShape = bottomShape;
            oldTopIsInscribed = topIsInscribed;
            oldTopPolySides = topPolySides;
            oldTopShape = topShape;
            oldRotationOffset = rotationOffset;
            oldSlantOffset = slantOffset;
            //RefreshPartEditorWindow(); //updates the tank resources' amounts but breaks dragging on sliders

            UpdateInterops();
        }

        public override Vector3 FromCylindricCoordinates(ProceduralAbstractShape.ShapeCoordinates coords)
        {
            try
            {
                if (lastShape != null)
                {
                    if (coords.HeightMode == ShapeCoordinates.YMode.OFFSET_FROM_SHAPE_CENTER && coords.RadiusMode == ShapeCoordinates.RMode.OFFSET_FROM_SHAPE_CENTER)
                    {
                        MeshLayer coordLayer = null;
                        if (coords.y == lastShape.Top.PosY)
                            coordLayer = lastShape.Top;
                        else if (coords.y == lastShape.Bottom.PosY)
                            coordLayer = lastShape.Bottom;
                        else
                        {
                            foreach (var layer in lastShape.Sections)
                            {
                                if (layer.Next == null)
                                    break;

                                if (coords.y >= Mathf.Min(layer.PosY, layer.Next.PosY) && coords.y < Mathf.Max(layer.PosY, layer.Next.PosY))
                                {
                                    float t = Mathf.InverseLerp(Mathf.Min(layer.PosY, layer.Next.PosY), Mathf.Max(layer.PosY, layer.Next.PosY), coords.y);
                                    if (float.IsNaN(t))
                                        t = 0f;
                                    var p2 = layer.InterpolateByUV(coords.u);
                                    var p1 = layer.Next.InterpolateByUV(coords.u);
                                    var finalPt = Vector3.Slerp(p1, p2, t);
                                    return new Vector3(finalPt.x, coords.y, finalPt.y);
                                }
                            }
                        }
                        if (coordLayer != null)
                        {
                            var finalPt = coordLayer.InterpolateByUV(coords.u);
                            return new Vector3(finalPt.x, coords.y, finalPt.y);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.LogWarning("[ProceduralParts] FromCylindricCoordinates");
                Debug.LogException(ex);
            }
            return base.FromCylindricCoordinates(coords);
        }

        private void CheckSnapMk2Diameter()
        {

            if (oldBottomShape != bottomShape &&
                    bottomShape == "Mk2" &&
                    bottomDiameter == 1.25f)
            {
                bottomDiameter = 1.5f;
                RefreshPartEditorWindow();
            }
            
            if (oldTopShape != topShape &&
                topShape == "Mk2" &&
                topDiameter == 1.25f)
            {
                topDiameter = 1.5f;
                RefreshPartEditorWindow();
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

            Fields["rotationOffset"].guiActiveEditor = topShape != "Cylinder" && showAdvancedOptions;
            Fields["slantOffset"].guiActiveEditor = showAdvancedOptions;
            Fields["curveShape"].guiActiveEditor = showAdvancedOptions;
        }

        private ContourProfile GetSideSection(string shapeName, float diam, int sideCount, bool inscribed)
        {
            switch (shapeName)
            {
                default:
                case "Cylinder":
                    return ContourProfile.GetCylinderSection(diam);
                case "Polygon":
                    return ContourProfile.GetPrismSection(sideCount, GetPolygonOuterDiam(inscribed, diam, sideCount));
                case "Mk2":
                    return ContourProfile.GetMk2Section(diam);
                case "Mk3":
                    return ContourProfile.GetMk3Section(diam);
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
