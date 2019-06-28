using Karamba.Elements;
using Karamba.Exporters;
using Karamba.Loads;
using Karamba.Materials;
using Karamba.Models;
using Karamba.Nodes;
using Karamba.Supports;
using System;
using System.Collections.Generic;

namespace karambaToSofistik
{
    class ExportDirector
    {
        public void ConstructExport(Model m, sofistikConverter sc)
        {
            //******************************************************************************************************************************************************
            // AQUA
            //******************************************************************************************************************************************************

            //Initiate aqua - (material and cross-sections)
            sc.initAQUA();

            //Init Materials
            int matNum = 1; //material id's
            foreach (FemMaterial material in m.materials)
            {
                sc.convertMaterial(material, matNum++);
            }
            sc.setLog("\n" + matNum + " materials added.");

            //Init Cross-sections based on element cross-sections (just I shape for now)
            int crosNum = 1; // Cross-section ID's
            Dictionary<Guid, int> _crosecDict = new Dictionary<Guid, int>();
            foreach (ModelElement elem in m.elems)
            {
                if (!_crosecDict.ContainsKey(elem.crosec.guid))
                {
                    sc.convertCroSec(elem.crosec, crosNum, _crosecDict);
                    _crosecDict.Add(elem.crosec.guid, crosNum++);
                }


            }
            sc.setLog("\n" + crosNum + " cross-sections added.");

            //End section
            sc.endSection();
            sc.setLog("\nAqua converted.");


            //******************************************************************************************************************************************************
            // SOFIMSH A
            //******************************************************************************************************************************************************


            //Init Sofimsha (generation of nodes and beams)
            sc.initSofimsha();

            //Add Nodes
            foreach (Node node in m.nodes)
            {
                sc.addNode(node);
            }
            sc.setLog("\n" + m.nodes.Count + " nodes added.");

            //Define which nodes get supports and support conditions
            foreach (Support support in m.supports)
            {
                sc.convertSupport(support);
            }
            sc.setLog("\n" + m.supports.Count + " supports added.");

            //Convert nodes to and add support conditions
            foreach (Node node in m.nodes)
            {
                sc.convertNode(node);
            }
            sc.setLog("\nSupports added to corresponding nodes.");

            //Convert beam elements (just ModelBeam for now)

            sc.setProduct("\n");
            foreach (ModelElement beam in m.elems)
            {
                sc.convertElem(beam, m);
            }
            sc.setLog("\n" + m.elems.Count + " elements converted.");


            sc.endSection();
            sc.setLog("\nSofimshA converted.");


            //******************************************************************************************************************************************************
            // SOFILOAD
            //******************************************************************************************************************************************************

            sc.initSofiload();

            // Add gravity loads
            foreach(GravityLoad gload in m.gravities.Values)
            {
                sc.addGload(gload, m);
            }
            sc.setLog("\n" + m.gravities.Values.Count + " gravity loads added.");

            // Add point loads
            foreach (PointLoad pload in m.ploads)
            {
                sc.addPload(pload, m);
            }
            sc.setLog("\n" + m.ploads.Count + " point loads added.");


            // Add element loads
            foreach (ElementLoad eload in m.eloads)
            {
                sc.addEload(eload);
            }
            sc.setLog("\n" + m.eloads.Count + " element loads added.");


            sc.convertLoads();
            sc.setLog("\nLoads converted to Sofistik format.");


            sc.endSection();
            sc.setLog("\nSofiload converted.");


            //******************************************************************************************************************************************************
            // ASE
            //******************************************************************************************************************************************************

            sc.initASE();
            sc.endSection();
            sc.setLog("\nASE converted.");

        }

    }
}
