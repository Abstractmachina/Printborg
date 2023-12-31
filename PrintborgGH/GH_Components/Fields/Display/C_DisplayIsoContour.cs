﻿using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Grasshopper.Kernel;
using Rhino.Geometry;
using SpatialSlur.SlurField;

namespace PrintborgGH.GH_Components.Fields.Display
{
    public class C_DisplayIsoContour : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the C_DisplayIsoContour class.
        /// </summary>
        public C_DisplayIsoContour()
          : base("Iso Contour", "IsoConMulti",
              "Draws Iso contour of a 2D field.",
              Labels.PluginName, Labels.Category_Fields)
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_InputParamManager pManager)
        {
            pManager.AddMeshParameter("Mesh", "M", "Base mesh.", GH_ParamAccess.item);
            pManager.AddGenericParameter("Field", "F", "Field.", GH_ParamAccess.item);
            pManager.AddNumberParameter("Tolerance", "T", "Weld Tolerance.", GH_ParamAccess.item, 1d);
            pManager.AddNumberParameter("Iso Value", "I", "Iso at which to draw curve.", GH_ParamAccess.item, 0d);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_OutputParamManager pManager)
        {
            pManager.AddCurveParameter("Isocontour", "C", "Isocontour of field at 0.", GH_ParamAccess.list);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            double tolerance = 1d;
            Mesh mesh = null;
            IField3d<double> f = null;
            double iso = 0d;

            if (!DA.GetData(0, ref mesh)) return;
            if (!DA.GetData(1, ref f)) return;
            if (!DA.GetData(2, ref tolerance)) return;
            if (!DA.GetData(3, ref iso)) return;

            var lines = new List<LineCurve>();

            Parallel.For(0, mesh.Faces.Count, i =>
            {
                Point3d p0 = mesh.Vertices[mesh.Faces[i].A];
                Point3d p1 = mesh.Vertices[mesh.Faces[i].B];
                Point3d p2 = mesh.Vertices[mesh.Faces[i].C];

                double t0 = f.ValueAt(mesh.Vertices[mesh.Faces[i].A]);
                double t1 = f.ValueAt(mesh.Vertices[mesh.Faces[i].B]);
                double t2 = f.ValueAt(mesh.Vertices[mesh.Faces[i].C]);

                int mask = 0;
                if (t0 >= iso) { mask |= 1; }
                if (t1 >= iso) { mask |= 2; }
                if (t2 >= iso) { mask |= 4; }

                switch (mask)
                {
                    case 1:
                        lines.Add(DrawLine(p0, p1, p2, Normalize(t0, t1, iso), Normalize(t0, t2, iso)));
                        break;
                    case 2:
                        lines.Add(DrawLine(p1, p2, p0, Normalize(t1, t2, iso), Normalize(t1, t0, iso)));
                        break;
                    case 3:
                        lines.Add(DrawLine(p2, p0, p1, Normalize(t2, t0, iso), Normalize(t2, t1, iso)));
                        break;
                    case 4:
                        lines.Add(DrawLine(p2, p0, p1, Normalize(t2, t0, iso), Normalize(t2, t1, iso)));
                        break;
                    case 5:
                        lines.Add(DrawLine(p1, p2, p0, Normalize(t1, t2, iso), Normalize(t1, t0, iso)));
                        break;
                    case 6:
                        lines.Add(DrawLine(p0, p1, p2, Normalize(t0, t1, iso), Normalize(t0, t2, iso)));
                        break;
                }
            });

            DA.SetDataList(0, Curve.JoinCurves(lines, tolerance));
        }

        private LineCurve DrawLine(Point3d p0, Point3d p1, Point3d p2, double t01, double t02)
        {
            Point3d p01 = Lerp(p0, p1, t01);
            Point3d p02 = Lerp(p0, p2, t02);

            return new LineCurve(p01, p02);
        }

        Point3d Lerp(Point3d p0, Point3d p1, double t)
        {
            return p0 + (p1 - p0) * t;
        }

        double Normalize(double t0, double t1, double t)
        {
            return (t - t0) / (t1 - t0);
        }

        /// <summary>
        /// Provides an Icon for the component.
        /// </summary>
        protected override System.Drawing.Bitmap Icon
        {
            get
            {
                //You can add image files to your project resources and access them like this:
                // return Resources.IconForThisComponent;
                return null;
            }
        }

        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("827E12FE-FA9A-4B2F-BFDD-86E8A4692878"); }
        }
    }
}