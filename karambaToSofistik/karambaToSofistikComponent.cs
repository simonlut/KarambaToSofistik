using System;
using System.Collections.Generic;
using System.IO;

using Grasshopper.Kernel;
using Grasshopper.Kernel.Data;
using Grasshopper.Kernel.Types;

using Rhino.Geometry;

using Karamba.Models;
using Karamba.Elements;

// In order to load the result of this wizard, you will also need to
// add the output bin/ folder of this project to the list of loaded
// folder in Grasshopper.
// You can use the _GrasshopperDeveloperSettings Rhino command for that.

namespace karambaToSofistik
{

    public class karambaToSofistikComponent : GH_Component
    {

        public karambaToSofistikComponent()
          : base("karambaToSofistik", "KarSof",
              "Export simple beam structures to Sofistik and visualize basic results in Grasshopper.",
              "Karamba3D", "Export")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddParameter(new Param_Model(), "Model", "Model", "Karamba Model", GH_ParamAccess.item);
            pManager.AddIntegerParameter("Beam Division", "Beam Division", "Amount of times beams are split for FEA.", GH_ParamAccess.item, 5);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.Register_StringParam("Sofistik", "Sofistik", "text output that can be copied to a teddy .dat file.");
            pManager.Register_StringParam("Log", "Log", "Log of conversion process.");
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object can be used to retrieve data from input parameters and 
        /// to store data in output parameters.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            sofistikConverter sofistikConverter = new sofistikConverter();
            Model iKarambaModel = null;
            int iBeamDiv = 1;

            DA.GetData(0, ref iKarambaModel);
            DA.GetData(1, ref iBeamDiv);

            if (iBeamDiv > 0)
            {
                sofistikConverter.beamDiv = iBeamDiv;

            }
            else
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, "Beam division should have a positive integer.");
            }

            Karamba.Models.Model karambaModel = (Karamba.Models.Model) iKarambaModel;

            if (karambaModel == null)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, ("The input is not of type model"));
                return;

            }

            //Clone Karamba model -> otherwise data changes in other components
            karambaModel = (Karamba.Models.Model) karambaModel.Clone();

            //Initialize exportsequence
            new ExportDirector().ConstructExport(karambaModel, sofistikConverter);

            if (sofistikConverter._errors.Count != 0 )
            {
                foreach (string error in sofistikConverter._errors)
                {
                    AddRuntimeMessage(GH_RuntimeMessageLevel.Error, error);
                }
                DA.SetData(1, new GH_String(sofistikConverter.getLog().ToString()));
                return;
            }
            else if (sofistikConverter._warnings.Count != 0)
            {
                foreach (string warning in sofistikConverter._warnings)
                {
                    AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, warning);
                }

            }
            
            DA.SetData(0, new GH_String(sofistikConverter.getProduct().ToString()));
            DA.SetData(1, new GH_String(sofistikConverter.getLog().ToString()));
        }

        /// <summary>
        /// Provides an Icon for every component that will be visible in the User Interface.
        /// Icons need to be 24x24 pixels.
        /// </summary>
        protected override System.Drawing.Bitmap Icon
        {
            //get { return Resource.Icon; }
            get
            {
                return Properties.Resources.icon;
            }
        }

        /// <summary>
        /// Each component must have a unique Guid to identify it. 
        /// It is vital this Guid doesn't change otherwise old ghx files 
        /// that use the old ID will partially fail during loading.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("522952dc-99b3-4c66-b25c-6dd8806d3d94"); }
        }
    }
}
