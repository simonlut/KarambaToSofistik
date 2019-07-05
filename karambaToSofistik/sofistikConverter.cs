using Karamba.Elements;
using Karamba.Exporters;
using Karamba.Loads;
using Karamba.Materials;
using Karamba.Models;
using Karamba.Nodes;
using Karamba.Supports;
using Karamba.CrossSections;

using Grasshopper.Kernel;
using Rhino.Geometry;

using System;
using System.Text;
using System.Collections.Generic;
using System.Data;


namespace karambaToSofistik
{
    public class 
        sofistikConverter
    {
        //Beam properties
        public int beamDiv;
        private string beamId;
        private int grpInd = 1;

        //Stringbuilders
        int _instruct_count = 1;
        private StringBuilder _product = new StringBuilder();
        private StringBuilder _log = new StringBuilder();

        //Dictionaries
        protected Dictionary<int, int> _node_inst = new Dictionary<int, int>();
        private Dictionary<Guid, uint> _mat_inst = new Dictionary<Guid, uint>();
        private Dictionary<Guid, int> _crosec_inst = new Dictionary<Guid, int>();
        protected Dictionary<int, int> _loadcase_inst = new Dictionary<int, int>();
        private Dictionary<int, int> _elem_inst = new Dictionary<int, int>();
        private Dictionary<int, string> _support_inst = new Dictionary<int, string>();
        private Dictionary<int, string> _joint_inst = new Dictionary<int, string>();

        //Loadtables
        private DataTable _gloads_table = new DataTable();
        private DataTable _ploads_table = new DataTable();
        private DataTable _eloads_table = new DataTable();
        private DataTable _mloads_table = new DataTable();

        //Exceptions
        public List<string> _errors = new List<string>();
        public List<string> _warnings = new List<string>();


        protected int addInstruction(string instr)
        {
            _product.AppendLine(instr);
            _instruct_count++;
            return _instruct_count;
        }

        //Aqua definition
        public void initAQUA()
        {
            _log.Append("\n\nInitiate Aqua...");

            addInstruction("+PROG AQUA urs:1 $Materials and cross-sections");
            addInstruction("HEAD Material and cross-section definitions");
            addInstruction("UNIT 5 $SI - Units");
            addInstruction("NORM 'DIN' '1045-1' $German norm\n");

        }

        //Sofimsh A definition
        public void initSofimsha()
        {
            _log.Append("\n\nInitiate SofimshA...");

            addInstruction("\n+PROG SOFIMSHA urs:2\nHEAD Elements\nUNIT 5\nSYST TYPE 3D GDIR NEGZ GDIV -1000\n");

        }

        //Sofiload a definition
        public void initSofiload()
        {
            _log.Append("\n\nInitiate Sofiload...");

            gloadTable();
            ploadTable();
            eloadTable();

            addInstruction("\n+PROG SOFILOAD\nHEAD LOADING\nECHO LOAD");
        }

        //ASE a definition
        public void initASE()
        {
            _log.Append("\n\nInititate ASE...");
            addInstruction("\n+PROG ASE\nHEAD CALCULATIONS\nLC ALL");
        }


        //Add Material
        public void convertMaterial(FemMaterial material, int materialIndex)
        {
            int value = addInstruction("MAT TITL "+ material.name + " NO " + materialIndex + " E " + material.E() / 1000.0 + " G " + material.G3() / 1000.0 + " GAM " + material.gamma() + " ALFA " + material.alphaT());
            _mat_inst.Add(material.guid, (uint)materialIndex);           
        }

        //Add Cross-sections with corresponding materials
        public void convertCroSec(Karamba.CrossSections.CroSec crosec, int crosNumber, Dictionary<Guid, int> crosecDict)
        {
            _crosec_inst = crosecDict;

            double diameter, thickness, height,
                      upperWidth, lowerWidth, upperThick, lowerThick,
                      sWallThick, webThick, filletRadius;

            int count = _crosec_inst.Count;
            uint materialId = 0;
            string crosecName;

            if (crosec is CroSec_Beam && _mat_inst.TryGetValue(crosec.material.guid, out uint value))
            {
                materialId = value;
                switch (crosec.shape())
                {
                    case "V":
                        height = Math.Round((double)crosec.dims[0] * 1000, 3);
                        upperWidth = Math.Round((double)crosec.dims[2] * 1000, 3);
                        upperThick = Math.Round((double)crosec.dims[3] * 1000, 3);
                        lowerWidth = Math.Round((double)crosec.dims[4] * 1000, 3);
                        lowerThick = Math.Round((double)crosec.dims[5] * 1000, 3);
                        filletRadius = Math.Round((double)crosec.dims[6] * 1000, 3);
                        if (crosec.name == "") { crosecName = crosec.family; }
                        else { crosecName = crosec.name; }

                        addInstruction("SECT " + crosNumber + " MNO " + materialId
                               + "\nPLAT NO 1 YB " + (-upperWidth / 2)
                                          + " ZB " + height
                                          + " YE " + (upperWidth / 2)
                                          + " ZE " + height
                                          + " T 10"
                               + "\nNO 2 YB " + (upperWidth / 2)
                                     + " ZB " + height
                                     + " YE " + (upperWidth / 2)
                                     + " ZE " + 0
                                     + " T 10"
                               + "\nNO 3 YB " + (upperWidth / 2)
                                     + " ZB " + 0
                                     + " YE " + (-upperWidth / 2)
                                     + " ZE " + 0
                                     + " T 10"
                               + "\nNO 4 YB " + (-upperWidth / 2)
                                     + " ZB " + 0
                                     + " YE " + (-upperWidth / 2)
                                     + " ZE " + height
                                     + " T 10");
                        break;

                    case "I":

                        height = Math.Round((double)crosec.dims[0] * 1000, 3);
                        webThick = Math.Round((double)crosec.dims[1] * 1000, 3);
                        upperWidth = Math.Round((double)crosec.dims[2] * 1000, 3);
                        upperThick = Math.Round((double)crosec.dims[3] * 1000, 3);
                        lowerWidth = Math.Round((double)crosec.dims[4] * 1000, 3);
                        lowerThick = Math.Round((double)crosec.dims[5] * 1000, 3);
                        filletRadius = Math.Round((double)crosec.dims[6] * 1000, 3);
                        if (crosec.name == "") { crosecName = crosec.family; }
                        else { crosecName = crosec.name; }


                        addInstruction("\nSECT " + crosNumber + " MNO " + materialId + " TITL " + crosecName
                               + "\nPLAT NO 1 YB " + (-upperWidth / 2)
                                    + " ZB " + (height - upperThick / 2)
                                    + " YE " + 0
                                    + " ZE " + (height - upperThick / 2)
                                    + " T " + upperThick
                               + "\nNO 2 YB " + 0
                                     + " ZB " + (height - upperThick / 2)
                                     + " YE " + (upperWidth / 2)
                                     + " ZE " + (height - upperThick / 2)
                                     + " T " + upperThick
                               + "\nNO 3 YB " + 0
                                     + " ZB " + (height - upperThick / 2)
                                     + " YE " + 0
                                     + " ZE " + (lowerThick / 2)
                                     + " T " + webThick
                               + "\nNO 4 YB " + (-lowerWidth / 2)
                                     + " ZB " + (lowerThick / 2)
                                     + " YE " + 0
                                     + " ZE " + (lowerThick / 2)
                                     + " T " + lowerThick
                               + "\nNO 5 YB " + 0
                                     + " ZB " + (lowerThick / 2)
                                     + " YE " + (lowerWidth / 2)
                                     + " ZE " + (lowerThick / 2)
                                     + " T " + lowerThick);
                         break;

                    case "O":             

                        diameter = Math.Round((double)crosec.dims[0] * 1000, 3);
                        thickness = Math.Round((double)crosec.dims[1] * 1000, 3);
                        if(crosec.name == "") { crosecName = crosec.family; }
                        else{ crosecName = crosec.name; }

                        addInstruction("\nSCIT NO " + crosNumber + " MNO " + materialId + " D " + diameter + " T " + thickness + " TITL " + crosecName);
                        break;

                    case "[]":
                        height = Math.Round((double)crosec.dims[0] * 1000, 3);
                        upperWidth = Math.Round((double)crosec.dims[2] * 1000, 3);
                        upperThick = Math.Round((double)crosec.dims[3] * 1000, 3);
                        lowerWidth = Math.Round((double)crosec.dims[4] * 1000, 3);
                        lowerThick = Math.Round((double)crosec.dims[5] * 1000, 3);
                        if (crosec.name == "") { crosecName = crosec.family; }
                        else { crosecName = crosec.name; }

                        addInstruction("\nSREC NO " + crosNumber + " MNO " + materialId + " H " + height + " HO " + Math.Max(lowerThick, upperThick) + " B " + lowerWidth + " BO " + upperWidth + " TITL " + crosecName);
                        break;
                    


                    default:
                        _log.Append("Cross-section of type: " + crosec.shape() + "not (yet) supported.");
                        _warnings.Add("Cross-section of type: " + crosec.shape() + "not (yet) supported.");
                        return;
                }

            }

            else
            {
                _log.Append("Cross-section of type: " + crosec.GetType() + " not yet supported.");
                _warnings.Add("Cross-section of type: " + crosec.GetType() + " not yet supported.");
            }
        }

        //Add Nodes
        public void addNode(Node node)
        {
            int nodeId = node.ind + 1; //Sofistik start at 1
            _node_inst.Add(node.ind, nodeId);
        }

        //Convert Supports
        public void convertSupport(Support support)
        {
            if(!_node_inst.ContainsKey(support.node_ind))
            {
                _log.Append("Error exporting, node not found");
                _errors.Add("Error exporting, node not found");
            }

            else
            {
                List<bool> conditions = support._condition;
                string nodeSupport = "";

                if (conditions.Contains(true)) { nodeSupport += " FIX "; }
                if (conditions[0]){ nodeSupport += "PX"; }
                if (conditions[1]) { nodeSupport += "PY"; }
                if (conditions[2]) { nodeSupport += "PZ"; }
                if (conditions[3]) { nodeSupport += "MX"; }
                if (conditions[4]) { nodeSupport += "MY"; }
                if (conditions[5]) { nodeSupport += "MZ"; }

                if (nodeSupport != "")
                {
                    _support_inst.Add(support.node_ind, nodeSupport);

                }

            }


        }

        // Convert Nodes
        public void convertNode(Node node)
        {
            double x = Math.Round(node.pos.X, 3);
            double y = Math.Round(node.pos.Y, 3);
            double z = Math.Round(node.pos.Z, 3);
            int nodeId = node.ind + 1; //Sofistik start at 1

            string nodeInstr = "NODE NO " + nodeId + " X " + x + " Y " + y + " Z " + z;

            if (_support_inst.ContainsKey(node.ind))
            {
                bool sup = _support_inst.TryGetValue(node.ind, out string nodeSupport);
                if (sup) { nodeInstr += " " + nodeSupport; }
                else {
                    _log.Append(" Error exporting supports. Support " + node.ind + " not found");
                    _warnings.Add(" Error exporting supports. Support " + node.ind + " not found");
                }
                
            }
            
            int instruction = addInstruction(nodeInstr);

        }


        //Convert elements (just beams for now)
        public void convertElem(ModelElement elem, Model karambaModel)
        {
            if (elem is ModelBeam)
            {
                addBeam(elem as ModelBeam);
            }
            else if (elem is ModelShell)
            {
                _log.Append("The element with type shell is not (yet) supported.");
                _errors.Add("The element with type shell is not (yet) supported.");
            }
            else
            {
                _log.Append("The element with type " + elem.GetType() + " is not (yet) supported.");
                _errors.Add("The element with type " + elem.GetType() + " is not (yet) supported.");
            }
        }

        protected void addBeam(ModelBeam beam) //TODO - Add beamgroups
        {
            int beamNO = beam.ind + 1;
            bool beamStart = _node_inst.TryGetValue(beam.node_inds[0], out int startNode);
            bool beamEnd = _node_inst.TryGetValue(beam.node_inds[1], out int endNode);
            bool beamCros = _crosec_inst.TryGetValue(beam.crosec.guid, out int beamCrossection);
            string beamJoint = "";
                        

            if (beam.joint != null)
            {
                double?[] _jointConditions = beam.joint.c;
                string _ahin = "";
                string _ehin = "";

                for (int i = 0; i < 6; i++)
                {
                    if (_jointConditions[i].HasValue)
                    {
                        _ahin = " AHIN ";
                    }
                }
                if( _ahin != "")
                {
                    if (_jointConditions[0].HasValue) { _ahin += "N"; }
                    if (_jointConditions[1].HasValue) { _ahin += "VY"; }
                    if (_jointConditions[2].HasValue) { _ahin += "VZ"; }
                    if (_jointConditions[3].HasValue) { _ahin += "MT"; }
                    if (_jointConditions[4].HasValue) { _ahin += "MY"; }
                    if (_jointConditions[5].HasValue) { _ahin += "MZ"; }
                }

                for (int i = 6; i < 12; i++)
                {
                    if (_jointConditions[i].HasValue)
                    {
                        _ehin = " EHIN ";
                    }
                }
                if (_ehin != "")
                {
                    if (_jointConditions[6].HasValue) { _ehin += "N"; }
                    if (_jointConditions[7].HasValue) { _ehin += "VY"; }
                    if (_jointConditions[8].HasValue) { _ehin += "VZ"; }
                    if (_jointConditions[9].HasValue) { _ehin += "MT"; }
                    if (_jointConditions[10].HasValue) { _ehin += "MY"; }
                    if (_jointConditions[11].HasValue) { _ehin += "MZ"; }
                }

                if(_ahin != "" || _ehin != "")
                {
                    beamJoint = _ahin + _ehin;
                    _joint_inst.Add(beam.ind, beamJoint);

                }

            }

            

            if (beam.id != "")
            {
                string beamName = beam.id;
                if (beamName != beamId)
                {
                    addInstruction("\nGRP " + grpInd + " TITL " + "\"" + beamName + "\"");
                    beamId = beamName;
                    grpInd++;
                }
            }

            if (beamStart && beamEnd && beamCros)
            {
                _elem_inst.Add(beam.ind, beamNO);
                if(beamDiv < 2)
                {
                    addInstruction("BEAM NO " + beamNO + " NA " + startNode + " NE " + endNode + " NCS " + beamCrossection + beamJoint);
                }
                else
                {
                    addInstruction("BEAM NO " + beamNO + "0 NA " + startNode + " NE " + endNode + " NCS " + beamCrossection + " DIV " + beamDiv + " NM 0 " + beamJoint );
                }
            }
            else
            {
                _log.Append("Error: Node or cross-section not found. Node Start: " + beamStart + "\nNode End: " + beamEnd + "\nBeam Crossection " + beamCros);
                _warnings.Add("Node or cross-section not found. Node Start: " + beamStart + "Node End: " + beamEnd + "Beam Crossection " + beamCros);
            }
        }

        private void gloadTable()
        {
            _gloads_table.Columns.Add("Type", typeof(string));
            _gloads_table.Columns.Add("Gravity_Force", typeof(double));
            _gloads_table.Columns.Add("Loadcase", typeof(int));
        }

        private void ploadTable()
        {
            _ploads_table.Columns.Add("Type", typeof(string));
            _ploads_table.Columns.Add("NodeInd", typeof(int));
            _ploads_table.Columns.Add("Force", typeof(Vector3d));
            _ploads_table.Columns.Add("Moment", typeof(Vector3d));
            _ploads_table.Columns.Add("Loadcase", typeof(int));
        }

        private void eloadTable()
        {
            _eloads_table.Columns.Add("Type", typeof(string));
            _eloads_table.Columns.Add("BeamIds", typeof(List<string>));
            _eloads_table.Columns.Add("LineLoad", typeof(UniformlyDistLoad));
            _eloads_table.Columns.Add("StrainLoad", typeof(StrainLoad));
            _eloads_table.Columns.Add("TempLoad", typeof(TemperatureLoad));
            _eloads_table.Columns.Add("Imperfection", typeof(Imperfection));
            _eloads_table.Columns.Add("Loadcase", typeof(int));

        }

        public void addGload(GravityLoad gload, Model karambaModel)
        {
            _gloads_table.Rows.Add("Gravity", (double)gload.force.Z, gload.loadcase);
        }


    public void addPload(PointLoad load, Model karambaModel)
        {
            _ploads_table.Rows.Add("Point",load.node_ind, load.force, load.moment, load.loadcase);

        }

        public void addEload(ElementLoad eload)
        {
            UniformlyDistLoad line = eload as UniformlyDistLoad;
            StrainLoad strain = eload as StrainLoad;
            TemperatureLoad temp = eload as TemperatureLoad;
            Imperfection imperfection = eload as Imperfection;

            if (line != null)
            {
                _eloads_table.Rows.Add("Line", eload.beamIds, line, strain, temp, imperfection, eload.loadcase);

            }
            if (strain != null)
            {
                _eloads_table.Rows.Add("Strain", eload.beamIds, line, strain, temp, imperfection, eload.loadcase);

            }
            if (temp != null)
            {
                _eloads_table.Rows.Add("Temp", eload.beamIds, line, strain, temp, imperfection, eload.loadcase);

            }
            if (imperfection != null)
            {
                _eloads_table.Rows.Add("Imperfection", eload.beamIds, line, strain, temp, imperfection, eload.loadcase);

            }
        }

        public void convertLoads()
        {
        DataTable dt = new DataTable();

        dt.Merge(_ploads_table);
        dt.Merge(_eloads_table);
        dt.Merge(_mloads_table);
        dt.Merge(_gloads_table);


        DataView sortedToLC = new DataView(dt);
        sortedToLC.Sort = "Loadcase";
        DataTable loads = sortedToLC.ToTable();
        DataTable lcIndexTable = sortedToLC.ToTable(true, "Loadcase");

        List<int> lcIndexes = new List<int>();
        foreach (DataRow row in lcIndexTable.Rows)
        {
            lcIndexes.Add((int) row["Loadcase"]);
            if (lcIndexes.Contains(0)){
                    _log.Append("Make sure that loadcase > 0. Sofistik starts at 1.");
                    _warnings.Add("Make sure that loadcase > 0. Sofistik starts at 1.");
                }
        }

        foreach (int lc in lcIndexes)
        {
            string initLC = "\nLC NO " + lc;
            addInstruction(initLC);
            foreach (DataRow row in loads.Rows)
            {   
                if ((int)row["Loadcase"] == lc)
                {
                        switch ((string)row["Type"]) {

                            case "Gravity":
                                double gforce = (double)row["Gravity_Force"];
                                _product.Replace(initLC, "\nLC NO " + lc + " DLZ " + gforce + " TITL 'Self-weight'");
                                break;

                            case "Point":
                                bool nodeValid = _node_inst.TryGetValue((int)row["NodeInd"], out int nodeId);
                                Vector3d force = (Vector3d)row["Force"];
                                if (nodeValid)
                                {
                                    addInstruction("NODE NO " + nodeId + " TYPE PP P1 " + Math.Round(force.X, 3) + " P2 " + Math.Round(force.Y, 3) + " P3 " + Math.Round(force.Z, 3));
                                }
                                else
                                {
                                    _warnings.Add("Node not found for point load");
                                }
                                break;

                            case "Line":
                                List<int> elems = new List<int>();
                                UniformlyDistLoad distLoad = (UniformlyDistLoad)row["LineLoad"];
                                string load_type = "";
                                int orientation = (int)distLoad.q_orient;
                                foreach (string beamId in (List<string>)row["BeamIds"])
                                {
                                    Int32.TryParse(beamId, out int j);
                                    bool elemValid =_elem_inst.TryGetValue(j, out int elem);

                                    if (elemValid)
                                    {
                                        if (orientation == 0)
                                        {
                                            load_type = "PX,PY,PZ";
                                        }
                                        else if (orientation == 2)
                                        {
                                            load_type = "PXP,PYP,PZP";
                                        }
                                        else
                                        {
                                            load_type = "PXX,PYY,PZZ";
                                        }

                                        addInstruction("BEAM " + beamId + " TYPE " + load_type + " PA " + Math.Round(distLoad.Load.X, 3) + "," + Math.Round(distLoad.Load.Y, 3) + "," + Math.Round(distLoad.Load.Z, 3));
                                    }
                                    else
                                    {
                                        _warnings.Add("Beam Ids for line-loads not valid");
                                    }
                                }
                                break;

                        case "Strain":
                                _log.Append("Strain loads are not supported (yet).");
                                _warnings.Add("Strain loads are not supported (yet).");
                                break;

                        case "Temp":
                            _log.Append("Temperature loads are not supported (yet).");
                            _warnings.Add("Temperature loads are not supported (yet).");
                            break;

                    }

                }
            }
        }
        }


        public void endSection()
        {
            _product.AppendLine("\nEND\n");
        }

        public StringBuilder getProduct()
        {
            return _product;
        }

        public StringBuilder getLog()
        {
            return _log;
        }

        public void setProduct(string product)
        {
            addInstruction(product);
        }

        public void setLog(string log)
        {
            _log.Append(log);
        }


    }
}
