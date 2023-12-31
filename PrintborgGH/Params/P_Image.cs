﻿using Printborg.GH_Types;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Types;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Printborg;

namespace PrintborgGH.Params {
    public class P_Image : GH_Param<GH_Image> {
        public P_Image()
          : base("Image", "Img", "Raster Image (bitmap)", Labels.PluginName, Labels.Category_Param, GH_ParamAccess.item) { }

        #region properties
        public override GH_Exposure Exposure {
            get {
                return GH_Exposure.primary;
            }
        }
        protected override Bitmap Icon {
            get {
                Bitmap icon = new Bitmap(24, 24, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
                Graphics g = Graphics.FromImage(icon);
                g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;
                g.Clear(Color.Transparent);

                g.FillEllipse(Brushes.AliceBlue, 4, 4, 16, 16);
                g.FillEllipse(Brushes.White, 7, 7, 5, 5);
                g.DrawEllipse(Pens.Aqua, 3, 3, 18, 18);
                g.DrawEllipse(Pens.DarkTurquoise, 4, 4, 16, 16);
                g.Dispose();
                return icon;
            }
        }
        public override Guid ComponentGuid {
            get { return new Guid("{8539aef3-4614-4212-8e30-44f55e1ba848}"); }
        }

        public override string TypeName {
            get {
                return "Image";
            }
        }
        #endregion

        #region casting
        /// <summary>
        /// Since IGH_Goo is an interface rather than a class, we HAVE to override this method. 
        /// For IGH_Goo parameters it's usually good to return a blank GH_ObjectWrapper.
        /// </summary>
        protected override GH_Image InstantiateT() {
            //return new GH_ObjectWrapper();
            return new GH_Image();
        }

        /// <summary>
        /// Since our parameter is of type IGH_Goo, it will accept ALL data. 
        /// We need to remove everything now that is not, GH_Colour, GH_Curve or null.
        /// </summary>
        protected override void OnVolatileDataCollected() {
            for (int p = 0; p < m_data.PathCount; p++) {
                var branch = m_data.Branches[p];
                for (int i = 0; i < branch.Count; i++) {
                    IGH_Goo? goo = branch[i];

                    //if (goo.GetType() != typeof(GH_Classifier)) AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Input is not a Classifier.");

                    //We accept existing nulls.
                    if (goo == null) continue;

                    if (goo is GH_Image) { continue; }

                    ////We accept colours.
                    //if (goo is GH_Colour) continue;

                    ////At this point the data is something other than a colour or a curve,
                    ////to be nice to the user, let's try and convert the data into a curve, then into a colour.


                    GH_String? castString = null;
                    if (GH_Convert.ToGHString(goo, GH_Conversion.Both, ref castString)) {
                        var img = Printborg.Util.FromBase64String(castString.Value);
                        branch[i] = new GH_Image((Bitmap)img);
                        continue;
                    }



                    //GH_Curve castCurve = null;
                    //if (GH_Convert.ToGHCurve(goo, GH_Conversion.Both, ref castCurve)) {
                    //  //Yay, the data could be converted. Put the new curve back into our volatile data.
                    //  branch[i] = castCurve;
                    //  continue;
                    //}

                    //GH_Colour castColour = null;
                    //if (GH_Convert.ToGHColour(goo, GH_Conversion.Both, ref castColour)) {
                    //  //Yay, the data could be converted. Put the new colour back into our volatile data.
                    //  branch[i] = castColour;
                    //  continue;
                    //}

                    //Tough luck, the data is beyond repair. We'll set a runtime error and insert a null.
                    AddRuntimeMessage(GH_RuntimeMessageLevel.Error,
                      string.Format("Data of type {0} could not be converted into either a colour or a curve", goo.TypeName));
                    branch[i] = null;

                    //As a side-note, we are not using the CastTo methods here on goo. If goo is of some unknown 3rd party type
                    //which knows how to convert itself into a curve then this parameter will not work with that. 
                    //If you want to know how to do this, ask.
                    //    }
                    //  }
                }

            }
        }

    } // end P_Image
    #endregion
}// end namespace
