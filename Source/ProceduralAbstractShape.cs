using System;
using UnityEngine;
using System.Reflection;
using System.Linq;
using KSPAPIExtensions;
using System.Collections.Generic;

namespace ProceduralParts
{
	public enum PartVolumes
	{
		/// <summary>
		/// Tankage - the volume devoted to storage of fuel, life support resources, ect
		/// </summary>
		Tankage,
		/// <summary>
		/// The volume devoted to habitable space.
		/// </summary>
		Habitable,
	}


    public abstract class ProceduralAbstractShape : PartModule
    {
        public override void OnAwake()
        {
            base.OnAwake();
            //PartMessageService.Register(this);
            //this.RegisterOnUpdateEditor(OnUpdateEditor);
        }

        #region Config data
        [KSPField]
        public string displayName;

        [KSPField]
        public string techRequired;

        [KSPField]
        public string techObsolete;

        [KSPField]
        public string volumeName = PartVolumes.Tankage.ToString();


        #endregion

        #region balancing
        // this are additional info fields that can be used by other modules for balancing purposes. shape classes should not use them themself

        [KSPField]
        public float costMultiplier = 1.0f;

        [KSPField]
        public float massMultiplier = 1.0f;

        [KSPField]
        public float resourceMultiplier = 1.0f;
        
        /////////////////////////////////////
        
        #endregion

        #region Objects
        public ProceduralPart PPart
        {
            get { return _pPart ?? (_pPart = GetComponent<ProceduralPart>()); }
        }
        private ProceduralPart _pPart;

        public Mesh SidesMesh
        {
            get { return PPart.SidesMesh; }
        }

        public Mesh EndsMesh
        {
            get { return PPart.EndsMesh; }
        }
        #endregion

        #region Shape details

        public float Volume
        {
            get { return _volume; }
            protected set
            {
                // ReSharper disable once CompareOfFloatsByEqualityOperator
                if (value != _volume)
                {
                    _volume = value;
                    ChangeVolume(volumeName, value);
                    if (HighLogic.LoadedSceneIsEditor)
                        GameEvents.onEditorShipModified.Fire(EditorLogic.fetch.ship);
                }
            }
        }
        private float _volume;

        #endregion

        #region Events

        // Events. These will get bound up automatically

        //[PartMessageEvent]
        //public event PartVolumeChanged ChangeVolume;

		public void ChangeVolume(string volName, double newVolume)
		{
			var data = new BaseEventData (BaseEventData.Sender.USER);
			data.Set<string> ("volName", volName);
			data.Set<double> ("newTotalVolume", newVolume);
			part.SendEvent ("OnPartVolumeChanged", data, 0);
            
		}

        //[PartMessageEvent]
        //public event ChangeTextureScaleDelegate ChangeTextureScale;

        //[PartMessageEvent]
        //public event PartAttachNodeSizeChanged ChangeAttachNodeSize;

		public void ChangeAttachNodeSize(AttachNode node, float minDia, float area)
		{
			var data = new BaseEventData (BaseEventData.Sender.USER);
			data.Set<AttachNode> ("node", node);
			data.Set<float> ("minDia", minDia);
			data.Set<float> ("area", area);
			part.SendEvent ("OnPartAttachNodeSizeChanged", data, 0);
		}

        //[PartMessageEvent]
        //public event PartModelChanged ModelChanged;

		private void ModelChanged()
		{
			part.SendEvent ("OnPartModelChanged", null, 0);
		}

        //[PartMessageEvent]
        //public event PartColliderChanged ColliderChanged;

		private void ColliderChanged()
		{
			part.SendEvent ("OnPartColliderChanged", null, 0);
		}
		
        protected void RaiseChangeTextureScale(string meshName, Material material, Vector2 targetScale)
        {
            //ChangeTextureScale(meshName, material, targetScale);

			var data = new BaseEventData (BaseEventData.Sender.USER);
			data.Set<string> ("meshName", meshName);
			data.Set<Material> ("material", material);
			data.Set<Vector2> ("targetScale", targetScale);
			part.SendEvent ("OnChangeTextureScale", data, 0);

        }
        
        protected void RaiseChangeAttachNodeSize(AttachNode node, float minDia, float area)
        {
            ChangeAttachNodeSize(node, minDia, area);
        }

        protected void RaiseModelAndColliderChanged()
        {
            ModelChanged();
            ColliderChanged();
        }

        #endregion

        #region Callbacks

        private bool forceNextUpdate = true;

        public void ForceNextUpdate()
        {
            forceNextUpdate = true;
        }

        public override void OnSave(ConfigNode node)
        {
            // Force saved value for enabled to be true.
            node.SetValue("isEnabled", "True");
        }

        public override void OnUpdate()
        {
            OnUpdateEditor();
        }
        public void Update()
        {
            if (HighLogic.LoadedSceneIsEditor)
                OnUpdateEditor();
        }

        public void UpdateInterops()
        {
            if (HighLogic.LoadedSceneIsEditor || HighLogic.LoadedSceneIsFlight)
            {
                if(ProceduralPart.installedFAR)
                    part.SendMessage("GeometryPartModuleRebuildMeshData");

                _pPart.UpdateTFInterops();
            }
        }

        public abstract void UpdateTFInterops();

        public void OnUpdateEditor()
        {
            try
            {
                bool wasForce = forceNextUpdate;
                forceNextUpdate = false;

                UpdateShape(wasForce);

                if (wasForce)
                {
                    ChangeVolume(volumeName, Volume);
                    if (HighLogic.LoadedSceneIsEditor)
                        GameEvents.onEditorShipModified.Fire(EditorLogic.fetch.ship);
                }

                if (HighLogic.LoadedScene == GameScenes.LOADING)
                    FixEditorIconScale();
            }
            catch (Exception ex)
            {
                print(ex);
                enabled = false;
            }
        }

        private void FixEditorIconScale()
        {
            var meshBounds = CalculateBounds(part.partInfo.iconPrefab.gameObject);
            if (meshBounds.extents == Vector3.zero)
                meshBounds = PPart.SidesIconMesh.bounds;
            var maxSize = Mathf.Max(meshBounds.size.x, meshBounds.size.y, meshBounds.size.z);

            
            var oldIconScale = part.partInfo.iconScale;
            part.partInfo.iconScale = 1f / maxSize;

            var iconMainTrans = part.partInfo.iconPrefab.transform.GetChild(0).transform;
            var oldScale = iconMainTrans.localScale;
            float factor = (40f / maxSize) / 40f;
            iconMainTrans.localScale *= factor;
            Debug.Log(String.Format("Rescaling part '{0}' editor's icon: model from {1} to {2}, icon from {3} to {4}", 
                part.partInfo.name, 
                oldScale, iconMainTrans.localScale,
                oldIconScale, part.partInfo.iconScale));
            iconMainTrans.localPosition -= meshBounds.center;
        }


        //Code from PartIconFixer addon
        private static Bounds CalculateBounds(GameObject go)
        {
            var renderers = go.GetComponentsInChildren<Renderer>(true).ToList();

            if (renderers.Count == 0) return default(Bounds);

            var boundsList = new List<Bounds>();

            renderers.ForEach(r =>
            {
                // why wouldn't it be enabled? not necessarily a problem though

                if (r is SkinnedMeshRenderer)
                {
                    var smr = r as SkinnedMeshRenderer;

                    // the localBounds of the SkinnedMeshRenderer are initially large enough
                    // to accomodate all animation frames; they're likely to be far off for 
                    // parts that do a lot of animation-related movement (like solar panels expanding)
                    //
                    // We can get correct mesh bounds by baking the current animation into a mesh
                    // note: vertex positions in baked mesh are relative to smr.transform; any scaling
                    // is already baked in
                    Mesh mesh = new Mesh();
                    smr.BakeMesh(mesh);

                    // while the mesh bounds will now be correct, they don't consider orientation at all.
                    // If a long part is oriented along the wrong axis in world space, the bounds we'd get
                    // here could be very wrong. We need to come up with essentially the renderer bounds:
                    // a bounding box in world space that encompasses the mesh
                    Matrix4x4 m = Matrix4x4.TRS(smr.transform.position, smr.transform.rotation, Vector3.one
                        /* remember scale already factored in!*/);
                    var vertices = mesh.vertices;

                    Bounds smrBounds = new Bounds(m.MultiplyPoint3x4(vertices[0]), Vector3.zero);

                    for (int i = 1; i < vertices.Length; ++i)
                        smrBounds.Encapsulate(m.MultiplyPoint3x4(vertices[i]));

                    Destroy(mesh);

                    boundsList.Add(smrBounds);
                }
                else if (r is MeshRenderer) // note: there are ParticleRenderers, LineRenderers, and TrailRenderers
                {
                    r.gameObject.GetComponent<MeshFilter>().sharedMesh.RecalculateBounds();
                    boundsList.Add(r.bounds);
                }
            });


            Bounds bounds = boundsList[0];
            boundsList.Skip(1).ToList().ForEach(b => bounds.Encapsulate(b));

            return bounds;
        }

        /// <summary>
        /// Called to update the compShape.
        /// </summary>
        protected abstract void UpdateShape(bool force);

        #endregion

        #region Attachments

        /// <summary>
        /// Add object attached to the surface of this part.
        /// Base classes should proportionally move the location and orientation (rotation) as the part stretches.
        /// The return value will be passed back to removeTankAttachment when i's detached
        /// </summary>
        /// <param name="attach">Transform offset follower for the attachment</param>
        /// <param name="normalized">If true, the current offset of the attachment is in 'normalized' offset
        /// - where i would be in space on a unit length and diameter cylinder. This method will relocate the object.</param>
        /// <returns>Object used to track the attachment for Remove method</returns>
        public abstract object AddAttachment(TransformFollower attach, bool normalized);

        /// <summary>
        /// Remove object attached to the surface of this part.
        /// </summary>
        /// <param name="data">Data returned from child method</param>
        /// <param name="normalize">If true, the transform positon follower will be relocated to a 'normalized' 
        /// offset - where i would appear on a unit length and diameter cylinder</param>
        public abstract TransformFollower RemoveAttachment(object data, bool normalize);

        public class ShapeCoordinates
        {
            public enum RMode
            {
                OFFSET_FROM_SHAPE_CENTER,
                OFFSET_FROM_SHAPE_RADIUS,
                RELATIVE_TO_SHAPE_RADIUS
            }

            public enum YMode
            {
                OFFSET_FROM_SHAPE_CENTER,
                OFFSET_FROM_SHAPE_TOP,
                OFFSET_FROM_SHAPE_BOTTOM,
                RELATIVE_TO_SHAPE
            }

            public RMode RadiusMode = RMode.OFFSET_FROM_SHAPE_RADIUS;
            public YMode HeightMode = YMode.RELATIVE_TO_SHAPE;

            public float u;
            public float y;
            public float r;

            /// <summary>
            /// Initializes a new instance of the <see cref="ShapeCoordinates"/> class.
            /// </summary>
            public ShapeCoordinates()
            {
                u = 0f;
                y = 0f;
                r = 0f;
            }

            public ShapeCoordinates(RMode radiusMode, YMode heightMode, float u, float y, float r)
            {
                RadiusMode = radiusMode;
                HeightMode = heightMode;
                this.u = u;
                this.y = y;
                this.r = r;
            }

            public override string ToString()
            {
                return "(u: " + u + " y: " + y + " r: " + r + ") R: " +RadiusMode + "Y: " + HeightMode;
            }
        }

        public abstract void GetCylindricCoordinates(Vector3 position, ShapeCoordinates coords);

        public abstract Vector3 FromCylindricCoordinates(ShapeCoordinates coords);

        #endregion

        public float GetCurrentCostMult()
        {
            return costMultiplier;
        }

        public abstract void UpdateTechConstraints();

        protected void RefreshPartEditorWindow()
        {
            var window = FindObjectsOfType<UIPartActionWindow>().FirstOrDefault(w => w.part == part);
            if (window != null)
                window.displayDirty = true;
        }

        private static Material LineMaterial;

        protected GameObject DrawLine(Vector3 start, Vector3 end, Color color, float duration = 0.2f)
        {
            GameObject myLine = new GameObject();
            myLine.transform.parent = transform;
            //myLine.layer = 30;
            LineRenderer lr = myLine.AddComponent<LineRenderer>();
            lr.useWorldSpace = false;
            lr.transform.localPosition = Vector3.zero;
            lr.transform.localEulerAngles = Vector3.zero; 

            if(LineMaterial == null)
                LineMaterial = new Material(Shader.Find("Particles/Additive"));
            lr.material = LineMaterial;
            lr.SetColors(color, color);
            lr.SetWidth(0.01f, 0.01f);
            lr.SetVertexCount(2);
            lr.SetPosition(0, start);
            lr.SetPosition(1, end);
            if (duration > 0)
                GameObject.Destroy(myLine, duration);
            return myLine;
        }
    }
}