using System;
using System.Drawing;
using Grasshopper.Kernel;

namespace karambaToSofistik
{
    public class karambaToSofistikInfo : GH_AssemblyInfo
    {
        public override string Name
        {
            get
            {
                return "KarambaToSofistik";
            }
        }
        public override Bitmap Icon
        {
            get
            {
                //Return a 24x24 pixel bitmap to represent this GHA library.
                return null;
            }
        }
        public override string Description
        {
            get
            {
                //Return a short string describing the purpose of this GHA library.
                return "Export possibilities from Karamba to Sofistik.";
            }
        }
        public override Guid Id
        {
            get
            {
                return new Guid("16ad233f-2fc6-456a-ac27-cac2e214acec");
            }
        }

        public override string AuthorName
        {
            get
            {
                //Return a string identifying you or your company.
                return "Simon Lut";
            }
        }
        public override string AuthorContact
        {
            get
            {
                //Return a string representing your preferred contact details.
                return "simonj.lut@gmail.com";
            }
        }
    }
}
