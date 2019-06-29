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
    public class SofistikExportOptions : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the SofistikExportOptions class.
        /// </summary>
        public SofistikExportOptions()
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
            pManager.AddTextParameter("Sofistik", "Sofistik", "Text output from sofistik converter", GH_ParamAccess.item);
            pManager.AddBooleanParameter("Calculate", "Calculate", "Calculate structure directly in Sofistik", GH_ParamAccess.item);
            pManager.AddBooleanParameter("Open Teddy", "Open Teddy", "Open the teddy file that was just created", GH_ParamAccess.item, false);
            pManager.AddGenericParameter("Solver Options", "Solver Option", "Choose the order of the solver and the settings", GH_ParamAccess.item);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.Register_StringParam("Sofistik", "Sofistik", "Text output from sofistik converter", GH_ParamAccess.item);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            string iSofText = "";
            bool iCalculate = false;
            bool iTeddy = false;
            

            DA.GetData(0, ref iSofText);
            DA.GetData(1, ref iCalculate);
            DA.GetData(2, ref iTeddy);
            GH_Document ghDoc = OnPingDocument();

            if (ghDoc != null)
            {
                string ghFilePath = @ghDoc.FilePath;
                string datFilePath = Path.GetDirectoryName(ghFilePath) + "\\" + Path.GetFileNameWithoutExtension(ghFilePath) + ".dat";

            
            bool datExists = File.Exists(datFilePath);
            bool ghModified = ghDoc.IsModified;


            if (iCalculate && ghDoc.IsFilePathDefined == false)
                {
                    System.Windows.Forms.MessageBox.Show("Please save grasshopper file in an empty folder.");
                     return;
                }

            if (iCalculate == true && datExists == true)
                {

                    //Write .dat file
                    string oSofText = iSofText;
                    File.WriteAllText(@datFilePath, oSofText);

                    //Init Calculations
                    CalculateSof(datFilePath);

                    return;

                }

            else if (iCalculate && ghDoc.IsFilePathDefined && datExists == false)
                {
                    System.Windows.Forms.MessageBox.Show("Sofistik files will be generated in current grasshopper folder.");

                    //Write .dat file
                    string oSofText = iSofText;
                    File.WriteAllText(@datFilePath, oSofText);

                    //Init Calculations
                    CalculateSof(datFilePath);
                    return;
                }

            if ( iTeddy && datExists == true)
                {
                    System.Diagnostics.Process.Start(datFilePath);
                }
            else if (iTeddy && datExists == false)
                {
                    System.Windows.Forms.MessageBox.Show(".dat File not yet generated. Press calculate.");

                }

            }
        }

        void CalculateSof(string path)
        {
            string targetPath = Path.GetFullPath(@path);
            System.Diagnostics.Process process = new System.Diagnostics.Process();
            System.Diagnostics.ProcessStartInfo startInfo = new System.Diagnostics.ProcessStartInfo();
            startInfo.FileName = "cmd.exe";
            startInfo.Arguments = "/c cd C:/Program Files/SOFiSTiK/2018/SOFiSTiK 2018/ & sps -B \"" + targetPath + "\"";

            process.StartInfo = startInfo;
            process.Start();
        }

        /// <summary>
        /// Provides an Icon for the component.
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
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("94ff2423-0a7d-4a3e-aec6-0a06d6220257"); }
        }
    }
}