using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.AutoCAD.Runtime;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;
using Autodesk.AutoCAD.Colors;
using System.Windows.Forms;
using Autodesk.AutoCAD.GraphicsInterface;
using Autodesk.Private.InfoCenterLib;
using AcAp = Autodesk.AutoCAD.ApplicationServices.Application;
using Polyline = Autodesk.AutoCAD.DatabaseServices.Polyline;



namespace HeliosBIM
{
    public class FirstCommandViewModel : ViewModelBase
    {
        private Document Doc;
        private Editor Ed;
        private Database Db;
        public FirstCommandViewModel(Document doc)
        {
            Doc= doc;
            Ed = Doc.Editor;
             Db = Doc.Database;
        }
        public void MasterMethod()
        {

            Ed.WriteMessage("JUST DO IT!");
            MessageBox.Show("Da hoan thanh test!");
        }

        public void VeBeTongLot()
        {
            // get the editor object so we can carry out some input 
            
            //Choise which Option we draw
            PromptKeywordOptions pptKeyOpts = new PromptKeywordOptions("")
            {
                Message = "\nEnter an option: "
            };
            pptKeyOpts.Keywords.Add("Foundation");
            pptKeyOpts.Keywords.Add("Slab");
            pptKeyOpts.Keywords.Default = "Foundation";

            PromptResult pptKeyRes = Ed.GetKeywords(pptKeyOpts);

            //---------------------------------
            int input;
            do
            {
                input = 1;
                PromptPointResult pPtRes;
                PromptPointOptions pPtOpts = new PromptPointOptions("");
                Db.Orthomode = true;
                // Prompt for the start point
                pPtOpts.Message = "\nEnter the start point of the foundation: ";
                pPtRes = Ed.GetPoint(pPtOpts);
                Point3d ptStart = pPtRes.Value;
                if (pPtRes.Status == PromptStatus.Cancel) return;
                // Prompt for the start point
                pPtOpts.Message = "\nEnter the end point of the foundation: ";
                pPtOpts.UseBasePoint = true;
                pPtOpts.BasePoint = ptStart;
                pPtRes = Ed.GetPoint(pPtOpts);
                Point3d ptEnd = pPtRes.Value;
                if (pPtRes.Status == PromptStatus.Cancel) return;
                using (Transaction tr = Db.TransactionManager.StartTransaction())
                {
                    BlockTable acBlkTbl;
                    BlockTableRecord btr;
                    // Open Model space for write
                    acBlkTbl = tr.GetObject(Db.BlockTableId, OpenMode.ForRead) as BlockTable;
                    btr = tr.GetObject(acBlkTbl[BlockTableRecord.ModelSpace], OpenMode.ForWrite) as BlockTableRecord;
                    // Define the new rectangle
                    Polyline2d pLine2d = new Polyline2d();
                    using (Polyline acPoly = new Polyline())
                    {
                        switch (pptKeyRes.StringResult)
                        {
                            case "Foundation":
                                int toaDo = 100;
                                acPoly.AddVertexAt(0, new Point2d(ptStart.X - toaDo, ptStart.Y), 0, 0, 0);
                                acPoly.AddVertexAt(1, new Point2d(ptEnd.X + toaDo, ptEnd.Y), 0, 0, 0);
                                acPoly.AddVertexAt(2, new Point2d(ptEnd.X + toaDo, ptEnd.Y - 100), 0, 0, 0);
                                acPoly.AddVertexAt(3, new Point2d(ptStart.X - toaDo, ptStart.Y - 100), 0, 0, 0);
                                acPoly.AddVertexAt(4, new Point2d(ptStart.X - toaDo, ptStart.Y), 0, 0, 0);
                                break;
                            case "Slab":
                                acPoly.AddVertexAt(0, new Point2d(ptStart.X, ptStart.Y), 0, 0, 0);
                                acPoly.AddVertexAt(1, new Point2d(ptEnd.X, ptEnd.Y), 0, 0, 0);
                                acPoly.AddVertexAt(2, new Point2d(ptEnd.X, ptEnd.Y - 100), 0, 0, 0);
                                acPoly.AddVertexAt(3, new Point2d(ptStart.X, ptStart.Y - 100), 0, 0, 0);
                                acPoly.AddVertexAt(4, new Point2d(ptStart.X, ptStart.Y), 0, 0, 0);
                                break;
                        }

                        acPoly.ColorIndex = 8;
                        acPoly.Closed = true;
                        // Add the new object to the block table record and the transaction
                        btr.AppendEntity(acPoly);
                        tr.AddNewlyCreatedDBObject(acPoly, true);
                        using (Hatch acHatch = new Hatch())
                        {
                            btr.AppendEntity(acHatch);
                            tr.AddNewlyCreatedDBObject(acHatch, true);
                            ObjectIdCollection acObjIdColl = new ObjectIdCollection
                            {
                                acPoly.ObjectId
                            };
                            // Set the properties of the hatch object
                            // Associative must be set after the hatch object is appended to the 
                            // block table record and before AppendLoop
                            acHatch.PatternScale = 10;
                            acHatch.SetHatchPattern(HatchPatternType.PreDefined, "AR-CONC");
                            acHatch.ColorIndex = 252;
                            acHatch.Associative = true;
                            acHatch.AppendLoop(HatchLoopTypes.Outermost, acObjIdColl);
                            acHatch.EvaluateHatch(true);

                        }
                    }

                    // Commit the changes and dispose of the transaction
                    tr.Commit();

                }
            } while (input == 1);

            Db.Orthomode = false;
        }

    }
}
