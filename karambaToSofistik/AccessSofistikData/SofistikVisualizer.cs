using System;
using System.Collections.Generic;
using System.IO;

using Grasshopper.Kernel;
using Grasshopper.Kernel.Data;
using Grasshopper.Kernel.Types;

using Rhino.Geometry;

using Karamba.Models;
using Karamba.Elements;

namespace karambaToSofistik
{
    public class SofistikVisualizer : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the SofistikVisualizer class.
        /// </summary>
        public SofistikVisualizer()
          : base("Calculate Sofistik", "Calc Sof",
              "Calculation settings for Sofistik model",
              "Karamba3D", "Export")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddTextParameter("Sofistik", "Sofistik", "Sofistik", GH_ParamAccess.item);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.Register_StringParam("Beam Forces", "Beam Forces", "Beam forces extracted from Sofistik.", GH_ParamAccess.list);
            pManager.Register_StringParam("Status", "Status", "Status of connection to Sofistik Database (cbd). ");
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            string iSofiText = "";
            DA.GetData(0, ref iSofiText);


            GH_Document ghDoc = OnPingDocument();

            if (ghDoc != null)
            {
                string ghFilePath = @ghDoc.FilePath;
                string cdbFilePath = Path.GetDirectoryName(ghFilePath) + "\\" + Path.GetFileNameWithoutExtension(ghFilePath) + ".cdb";

                //Initialize database access
                karambaToSofistik.AccessSofistik.AccessSofData.Main(cdbFilePath);

                if (AccessSofistik.AccessSofData.SofBeamForces.Count != 0)
                {
                    DA.SetDataList(0, AccessSofistik.AccessSofData.SofBeamForces);
                    DA.SetData(1, AccessSofistik.AccessSofData._status.ToString());
                    return;
                }
                else
                {
                    AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "No beamforces found.");
                    DA.SetData(1, AccessSofistik.AccessSofData._status.ToString());

                }


            }


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
            get { return new Guid("f0232ba2-bdda-4930-885a-91ea273f524b"); }
        }
    }
}