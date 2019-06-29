using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices;       //importing DLLS

namespace karambaToSofistik.AccessSofistik
{

    public static class AccessSofData
    {

        public static List<string> SofBeamForces = new List<string>();
        public static List<double> SofBeamN = new List<double>();
        public static List<double> SofBeamVy = new List<double>();
        public static List<double> SofBeamVz = new List<double>();
        public static List<double> SofBeamMy = new List<double>();
        public static List<double> SofBeamMz = new List<double>();
        public static StringBuilder _status = new StringBuilder();


        // In this example 64bit dlls are used (Visual Studio Platform 64bit)

        // sof_cdb_init
        [DllImport("cdb_w_edu50_x64.dll", CallingConvention = CallingConvention.Cdecl)]
        // [DllImport("cdb_w33_x64.lib")]
        public static extern int sof_cdb_init(
            string name_,
            int initType_
        );

        // sof_cdb_close
        [DllImport("cdb_w_edu50_x64.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern void sof_cdb_close(
            int index_);

        // sof_cdb_status
        [DllImport("cdb_w_edu50_x64.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern int sof_cdb_status(
            int index_);

        // sof_cdb_flush
        [DllImport("cdb_w_edu50_x64.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern int sof_cdb_flush(
            int index_);

        // sof_cdb_flush
        [DllImport("cdb_w_edu50_x64.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern int sof_cdb_free(
            int kwh_,
            int kwl_);

        // sof_cdb_flush
        [DllImport("cdb_w_edu50_x64.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern void sof_cdb_kenq_ex(
            int index,
            ref int kwh_,
            ref int kwl_,
            int request_);

        // sof_cdb_get
        [DllImport("cdb_w_edu50_x64.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern int sof_cdb_get(
            int index_,
            int kwh_,
            int kwl_,
            ref cs_beam_for data_,
            ref int recLen_,
            int pos);

        public static unsafe void Main(string ghpath)
        {
            SofBeamForces.Clear();
            SofBeamN.Clear();
            SofBeamMy.Clear();
            SofBeamMz.Clear();
            SofBeamVy.Clear();
            SofBeamVz.Clear();
            _status.Clear();

            int index = 0;
            int status = 0;
            int datalen;


            string directory1 = @"C:\Program Files\SOFiSTiK\2018\SOFiSTiK 2018\interfaces\64bit";
            string cdbPath = @ghpath;

            // Get the path
            string path = Environment.GetEnvironmentVariable("path");

            // Set the new path environment variable + SOFiSTiK dlls path
            string envPath = directory1 + ";" + path;

            // Set the path variable (to read the data from CDB)
            System.Environment.SetEnvironmentVariable("path", envPath);
            // connect to CDB
            index = sof_cdb_init(cdbPath, 99);
            // check if sof_cdb_flush is working
            status = AccessSofData.sof_cdb_status(index);
            if (status == 3)
            {
                _status.Append("Database connected.");


            cs_beam_for getBeamForces = new karambaToSofistik.AccessSofistik.cs_beam_for();
            datalen = Marshal.SizeOf(typeof(karambaToSofistik.AccessSofistik.cs_beam_for));

            int pos = 1;

            while (sof_cdb_get(index, 102, 1, ref getBeamForces, ref datalen, pos) < 2)
            {
                if (sof_cdb_get(index, 102, 1, ref getBeamForces, ref datalen, pos) == 0)
                {
                    if (getBeamForces.m_nr == 0)
                        {
                            SofBeamForces.Add("Superpositioned Maximum Beam Forces"
                            + "\n\nN: " + Math.Round(getBeamForces.m_n, 3).ToString() + " kN "
                            + "\nMy: " + Math.Round(getBeamForces.m_my, 3).ToString() + " kNm "
                            + "\nMz: " + Math.Round(getBeamForces.m_mz, 3).ToString() + " kNm "
                            + "\nVy: " + Math.Round(getBeamForces.m_vy, 3).ToString() + " kN "
                            + "\nVz: " + Math.Round(getBeamForces.m_vz, 3).ToString() + " kN \n");
                        }
                    else
                    {
                        SofBeamForces.Add("Beam: " + (getBeamForces.m_nr - 1).ToString() //Convert beam number back to Karamba
                        + "\n\nN: " + Math.Round(getBeamForces.m_n, 3).ToString() + " kN "
                        + "\nMy: " + Math.Round(getBeamForces.m_my, 3).ToString() + " kNm "
                        + "\nMz: " + Math.Round(getBeamForces.m_mz, 3).ToString() + " kNm "
                        + "\nVy: " + Math.Round(getBeamForces.m_vy, 3).ToString() + " kN "
                        + "\nVz: " + Math.Round(getBeamForces.m_vz, 3).ToString() + " kN \n");

                            SofBeamN.Add(getBeamForces.m_n);
                            SofBeamVy.Add(getBeamForces.m_vy);
                            SofBeamVz.Add(getBeamForces.m_vz);
                            SofBeamMy.Add(getBeamForces.m_my);
                            SofBeamMz.Add(getBeamForces.m_mz);
                    }



                }

                datalen = Marshal.SizeOf(typeof(karambaToSofistik.AccessSofistik.cs_beam_for));

            }
                _status.Append("\nBeam forces extracted.");


            }
            else if (status == 4)
            {
                _status.Append("\nCalculating... Database is locked.");
            }
            else
            {
                _status.Append("\nDatabase not connected...");

            }




            // use sof_cdb_flush() and sof_cdb_close()
            sof_cdb_flush(index);
            sof_cdb_close(0);   // close the CDB

            System.Environment.SetEnvironmentVariable("path", null); //Delete environment variables

            _status.Append("\nDatabase closed.");

            // Output the status after closing the CDB
            ////Console.WriteLine();
            ////if (sof_cdb_status(index) == 0)
            ////{
            ////    Console.WriteLine("CDB Status = 0, CDB closed succesfully");
            ////}
            ////else
            ////{
            ////    Console.WriteLine("CDB Status <> 0, the CDB doesn't closed successfully");
            ////}

            //Console.Write("Press any key to close the application...");
            //Console.ReadKey();
        }
    }
}