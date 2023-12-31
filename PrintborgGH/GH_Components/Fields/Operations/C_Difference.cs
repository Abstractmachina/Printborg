﻿using System;
using System.Collections.Generic;
using Grasshopper.Kernel;
using Rhino.Geometry;
using SpatialSlur.SlurField;

namespace PrintborgGH.GH_Components.Fields.Operations
{
    public class C_Difference : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the component class.
        /// </summary>
        public C_Difference()
          : base("Difference", "Dif",
              "Perform a boolean difference on two fields.",
              Labels.PluginName, Labels.Category_Fields)
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_InputParamManager pManager)
        {
            pManager.AddGenericParameter("Field 0", "F0", "First Field for Difference Operation", GH_ParamAccess.item);
            pManager.AddGenericParameter("Field 1", "F1", "Second Field for Difference Operation", GH_ParamAccess.item);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_OutputParamManager pManager)
        {
            pManager.AddGenericParameter("Difference Result", "F", "Difference field", GH_ParamAccess.item);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            IField3d<double> f0 = null;
            IField3d<double> f1 = null;

            if (!DA.GetData(0, ref f0)) return;
            if (!DA.GetData(1, ref f1)) return;


            DA.SetData(0, FuncField3d.CreateDifference(f1, f0));
        }

        public override GH_Exposure Exposure
        {
            get { return GH_Exposure.secondary; }
        }

        /// <summary>
        /// Provides an Icon for the component.
        /// </summary>
        protected override System.Drawing.Bitmap Icon
        {
            get
            {
                //You can add image files to your project resources and access them like this:
                //return Resources.difference;
                //return Properties.Resources.difference;
                return null;
            }
        }

        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("303e16a8-6770-49fb-8f04-79cd4cbed3bb"); }
        }
    }
}