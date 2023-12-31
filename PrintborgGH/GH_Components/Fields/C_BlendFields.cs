﻿using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Grasshopper.Kernel;
using Rhino.Geometry;
using SpatialSlur.SlurField;
using SpatialSlur.SlurRhino;

namespace PrintborgGH.Components.Fields
{
    public class C_BlendFields : GH_Component
    {
        private List<string> debug = new List<string>();

        /// <summary>
        /// Initializes a new instance of the C_BlendFields class.
        /// </summary>
        public C_BlendFields()
          : base("Blend Fields Multi-threaded", "FBlendMult",
              "Blend two fields at specified parameters t - multi threaded",
              Labels.PluginName, Labels.Category_Fields)
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddMeshParameter("Base Mesh", "M", "Base Mesh.", GH_ParamAccess.item);
            pManager.AddGenericParameter("Field 0", "F0", "First Field.", GH_ParamAccess.item);
            pManager.AddGenericParameter("Field 1", "F1", "Second Field.", GH_ParamAccess.item);
            pManager.AddNumberParameter("Blend Parameters", "t", "Parameters at which blend is sampled", GH_ParamAccess.list);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddTextParameter("debug", "D", ".", GH_ParamAccess.list);
            pManager.AddGenericParameter("Blend Fields", "F", "blend fields.", GH_ParamAccess.list);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            Mesh mesh = null;
            IField3d<double> f0 = null;
            IField3d<double> f1 = null;
            List<double> t = new List<double>();

            if (!DA.GetData(0, ref mesh)) return;
            if (!DA.GetData(1, ref f0)) return;
            if (!DA.GetData(2, ref f1)) return;
            if (!DA.GetDataList(3, t)) return;

            var hem = mesh.ToHeMesh();

            List<MeshField3d<double>> blendfields = new List<MeshField3d<double>>();

            for (int j = 0; j < t.Count; j++) {
                Point3d[] pts = mesh.Vertices.ToPoint3dArray();
                double[] results = new double[pts.Length];

                debug.Add(pts.Length.ToString());

                Parallel.For(0, pts.Length, i => {
                    results[i] = SpatialSlur.SlurCore.SlurMath.Lerp(f0.ValueAt(pts[i]), f1.ValueAt(pts[i]), t[j]);
                });

                var field = Util.SpatialSlur.CreateMeshField(mesh, results);

                blendfields.Add(field);

            }

            DA.SetDataList(0, debug);
            DA.SetDataList(1, blendfields);

        }

        /// <summary>
        /// Provides an Icon for the component.
        /// </summary>
        protected override System.Drawing.Bitmap Icon
        {
            get {
                //return Resources.AMField;
                return null;
            }
        }

        public override GH_Exposure Exposure {
            get { return GH_Exposure.primary; }
        }

        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("080814AD-0F49-42BF-8107-6E22BBF1BB14"); }
        }
    }
}