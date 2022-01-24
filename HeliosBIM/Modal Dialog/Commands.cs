using System;
using System.Collections.Generic;
using System.Windows.Forms;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Geometry;
using Autodesk.AutoCAD.Runtime;

using AcAp = Autodesk.AutoCAD.ApplicationServices.Application;

[assembly: CommandClass(typeof(HeliosBIM.ModalDialogTest.Commands))]

namespace HeliosBIM.ModalDialogTest
{
    public class Commands
    {
        // instance fields
        Document doc;  // active document
        double radius; // radius default value
        string layer;  // layer default value
        int textHeight; 
        int scale;
        int ColorOfWcadObject;

        /// <summary>
        /// Create a new instance of Commands.
        /// This constructor is run once per document
        /// at the first call of a 'CommandMethod' method.
        /// </summary>
        public Commands()
        {
            // private fields initialization (initial default values)
            doc = AcAp.DocumentManager.MdiActiveDocument;
            layer = (string)AcAp.GetSystemVariable("clayer");
            radius = 100.0;
            textHeight = 3;
            scale = 100;
            ColorOfWcadObject=252;
        }

     

        /// <summary>
        /// Draws a circle.
        /// </summary>
        /// <param name="radius">Circle radius.</param>
        /// <param name="layer">Circle center.</param>
        private void DrawCircle(double radius, string layer)
        {
            var db = doc.Database;
            var ed = doc.Editor;
            var ppr = ed.GetPoint("\nSpecify the center: ");
            if (ppr.Status == PromptStatus.OK)
            {
                using (var tr = db.TransactionManager.StartTransaction())
                {
                    var curSpace =
                        (BlockTableRecord)tr.GetObject(db.CurrentSpaceId, OpenMode.ForWrite);
                    using (var circle = new Circle(ppr.Value, Vector3d.ZAxis, radius))
                    {
                        circle.TransformBy(ed.CurrentUserCoordinateSystem);
                        circle.Layer = layer;
                        curSpace.AppendEntity(circle);
                        tr.AddNewlyCreatedDBObject(circle, true);
                    }
                    tr.Commit();
                }
            }
        }

        /// <summary>
        /// Gets the layer list.
        /// </summary>
        /// <param name="db">Database instance this method applies to.</param>
        /// <returns>Layer names list.</returns>
        private List<string> GetLayerNames(Database db)
        {
            var layers = new List<string>();
            using (var tr = db.TransactionManager.StartOpenCloseTransaction())
            {
                var layerTable = (LayerTable)tr.GetObject(db.LayerTableId, OpenMode.ForRead);
                foreach (ObjectId id in layerTable)
                {
                    var layer = (LayerTableRecord)tr.GetObject(id, OpenMode.ForRead);
                    layers.Add(layer.Name);
                }
            }
            return layers;
        }

        [CommandMethod("Zoning")]
        public void ZoningCmd()
        {
            // creation of an instance of ModalDialog
            // with the document data (layers and default values)
            var layers = GetLayerNames(doc.Database);
            if (!layers.Contains(layer))
            {
                layer = (string)AcAp.GetSystemVariable("clayer");
            }
            using (var dialog = new Zoning(layers, layer,textHeight,scale))
            {
                // shows the dialog box in modal mode
                // and acts according to the DialogResult value
                var dlgResult = AcAp.ShowModalDialog(dialog);
                if (dlgResult == DialogResult.OK)
                {
                    // fields update
                    layer = dialog.Layer;
                    textHeight = dialog.TextHeight;
                    scale = dialog.Scale;
                    // circle drawing
                    Zoning(layer, textHeight, scale);
                }
            }
        }
        /// <summary>
        /// Auto get area to text
        /// </summary>
        /// <param name="xlayer"></param>
        /// <param name="xtextHeight"></param>
        /// <param name="xscale"></param>
        public void Zoning(string xlayer, int xtextHeight, int xscale)
        {
            Editor ed = doc.Editor;
            Database db = doc.Database;
            //pick all hatch
            SelectionSet resultHatch = SelectionUtils.FilterForSingleEntity(ed, "HATCH");
            if (resultHatch == null) return;
            foreach (SelectedObject selectedObject in resultHatch)
            {
                using (Transaction tr = db.TransactionManager.StartTransaction())
                {
                    try
                    {
                        Entity ent = tr.GetObject(selectedObject.ObjectId, OpenMode.ForRead) as Entity;

                        if (ent != null && ent is Hatch)
                        {
                            Hatch hatch = ent as Hatch;

                            //Get position of text
                            Extents3d entGeoEx = ent.GeometricExtents;
                            var positionOfText = new Point3d((entGeoEx.MaxPoint.X + entGeoEx.MinPoint.X) / 2, (entGeoEx.MaxPoint.Y + entGeoEx.MinPoint.Y) / 2, 0);

                            //Get some information
                            double area = Math.Round(hatch.Area / 1000000, 0);
 
                            //Create text for zoning
                            TextUtils.CreateTextWithScale(doc,
                                xscale,
                                xtextHeight,
                                xlayer,
                                "ZONE",
                                new Point3d(positionOfText.X, positionOfText.Y + xscale * xtextHeight + xscale * xtextHeight / 3, 0));

                            TextUtils.CreateTextWithScale(doc,
                                xscale,
                                xtextHeight,
                                xlayer,
                                String.Concat(area, "m2"),
                                positionOfText);

                        }
                    }
                    catch (Autodesk.AutoCAD.Runtime.Exception e)
                    {
                        MessageBox.Show(e.ToString());
                    }
                    tr.Commit();
                }
            }
        }

        #region NETLOAD

        [CommandMethod("BTL")]
        public static void VeBeTongLot()
        {


            // get the editor object so we can carry out some input 
            Editor ed = AcAp.DocumentManager.MdiActiveDocument.Editor;
            Database db = ed.Document.Database;
            //Choise which Option we draw
            PromptKeywordOptions pptKeyOpts = new PromptKeywordOptions("")
            {
                Message = "\nEnter an option: "
            };
            pptKeyOpts.Keywords.Add("Foundation");
            pptKeyOpts.Keywords.Add("Slab");
            pptKeyOpts.Keywords.Default = "Foundation";

            PromptResult pptKeyRes = ed.GetKeywords(pptKeyOpts);

            //---------------------------------
            int input;
            do
            {
                input = 1;
                PromptPointResult pPtRes;
                PromptPointOptions pPtOpts = new PromptPointOptions("");
                db.Orthomode = true;
                // Prompt for the start point
                pPtOpts.Message = "\nEnter the start point of the foundation: ";
                pPtRes = ed.GetPoint(pPtOpts);
                Point3d ptStart = pPtRes.Value;
                if (pPtRes.Status == PromptStatus.Cancel) return;
                // Prompt for the start point
                pPtOpts.Message = "\nEnter the end point of the foundation: ";
                pPtOpts.UseBasePoint = true;
                pPtOpts.BasePoint = ptStart;
                pPtRes = ed.GetPoint(pPtOpts);
                Point3d ptEnd = pPtRes.Value;
                if (pPtRes.Status == PromptStatus.Cancel) return;
                using (Transaction tr = db.TransactionManager.StartTransaction())
                {
                    BlockTable acBlkTbl;
                    BlockTableRecord btr;
                    // Open Model space for write
                    acBlkTbl = tr.GetObject(db.BlockTableId, OpenMode.ForRead) as BlockTable;
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

            db.Orthomode = false;
        }

        [CommandMethod("VPP")]
        public static void VeVanPhuPhim()
        {

            // get the editor object so we can carry out some input 
            Editor ed = Autodesk.AutoCAD.ApplicationServices.Application.DocumentManager.MdiActiveDocument.Editor;
            Database db = ed.Document.Database;
            // first decide what type of entity we want to create 
            PromptKeywordOptions pptKeyOpts = new PromptKeywordOptions("")
            {
                Message = "\nEnter an option: "
            };
            pptKeyOpts.Keywords.Add("Beam");
            pptKeyOpts.Keywords.Add("Slab");
            pptKeyOpts.Keywords.Default = "Beam";

            PromptResult pptKeyRes = ed.GetKeywords(pptKeyOpts);
            PromptPointOptions pPtOpts = new PromptPointOptions("");
            PromptPointResult pPtRes;
            // Prompt for the start point
            int input;
            do
            {
                input = 1;
                pPtOpts.Message = "\nChon diem dau: ";
                pPtRes = ed.GetPoint(pPtOpts);
                Point3d ptStart = pPtRes.Value;
                pPtOpts.UseBasePoint = true;
                pPtOpts.BasePoint = ptStart;
                if (pPtRes.Status == PromptStatus.Cancel) return;
                // Prompt for the start point
                pPtOpts.Message = "\nChon diem cuoi: ";
                pPtRes = ed.GetPoint(pPtOpts);
                Point3d ptEnd = pPtRes.Value;
                if (pPtRes.Status == PromptStatus.Cancel) return;
                using (Transaction tr = db.TransactionManager.StartTransaction())
                {
                    BlockTable acBlkTbl;
                    BlockTableRecord btr;
                    // Open Model space for write
                    acBlkTbl = tr.GetObject(db.BlockTableId, OpenMode.ForRead) as BlockTable;
                    btr = tr.GetObject(acBlkTbl[BlockTableRecord.ModelSpace], OpenMode.ForWrite) as BlockTableRecord;
                    switch (pptKeyRes.StringResult)
                    {
                        case "Beam":
                            //Ve van thanh 1
                            using (Polyline acPoly = new Polyline())
                            {
                                acPoly.AddVertexAt(0, new Point2d(ptStart.X, ptStart.Y), 0, 0, 0);
                                acPoly.AddVertexAt(1, new Point2d(ptStart.X - 15, ptStart.Y), 0, 0, 0);
                                acPoly.AddVertexAt(2, new Point2d(ptStart.X - 15, ptEnd.Y), 0, 0, 0);
                                acPoly.AddVertexAt(3, new Point2d(ptStart.X, ptEnd.Y), 0, 0, 0);
                                acPoly.AddVertexAt(4, new Point2d(ptStart.X, ptStart.Y), 0, 0, 0);
                                acPoly.ColorIndex = 8;
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
                                    acHatch.PatternScale = 50;
                                    acHatch.SetHatchPattern(HatchPatternType.PreDefined, "ANSI31");
                                    acHatch.ColorIndex = 252;
                                    acHatch.Associative = true;
                                    acHatch.AppendLoop(HatchLoopTypes.Outermost, acObjIdColl);
                                    acHatch.EvaluateHatch(true);
                                }
                            }

                            //Ve van thanh 2
                            using (Polyline acPoly = new Polyline())
                            {
                                acPoly.AddVertexAt(0, new Point2d(ptEnd.X, ptEnd.Y), 0, 0, 0);
                                acPoly.AddVertexAt(1, new Point2d(ptEnd.X + 15, ptEnd.Y), 0, 0, 0);
                                acPoly.AddVertexAt(2, new Point2d(ptEnd.X + 15, ptStart.Y), 0, 0, 0);
                                acPoly.AddVertexAt(3, new Point2d(ptEnd.X, ptStart.Y), 0, 0, 0);
                                acPoly.AddVertexAt(4, new Point2d(ptEnd.X, ptEnd.Y), 0, 0, 0);
                                acPoly.ColorIndex = 8;
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
                                    acHatch.PatternScale = 50;
                                    acHatch.SetHatchPattern(HatchPatternType.PreDefined, "ANSI31");
                                    acHatch.ColorIndex = 252;
                                    acHatch.Associative = true;
                                    acHatch.AppendLoop(HatchLoopTypes.Outermost, acObjIdColl);
                                    acHatch.EvaluateHatch(true);

                                }
                            }

                            //Ve van day dam
                            using (Polyline acPoly = new Polyline())
                            {
                                acPoly.AddVertexAt(0, new Point2d(ptStart.X - 200, ptEnd.Y), 0, 0, 0);
                                acPoly.AddVertexAt(1, new Point2d(ptEnd.X + 200, ptEnd.Y), 0, 0, 0);
                                acPoly.AddVertexAt(2, new Point2d(ptEnd.X + 200, ptEnd.Y - 15), 0, 0, 0);
                                acPoly.AddVertexAt(3, new Point2d(ptStart.X - 200, ptEnd.Y - 15), 0, 0, 0);
                                acPoly.AddVertexAt(4, new Point2d(ptStart.X - 200, ptEnd.Y), 0, 0, 0);
                                acPoly.ColorIndex = 8;
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
                                    acHatch.PatternScale = 50;
                                    acHatch.SetHatchPattern(HatchPatternType.PreDefined, "ANSI31");
                                    acHatch.ColorIndex = 252;
                                    acHatch.Associative = true;
                                    acHatch.AppendLoop(HatchLoopTypes.Outermost, acObjIdColl);
                                    acHatch.EvaluateHatch(true);

                                }
                            }

                            pPtOpts.UseBasePoint = false;
                            break;

                        case "Slab":
                            using (Polyline acPoly = new Polyline())
                            {
                                acPoly.AddVertexAt(0, new Point2d(ptStart.X, ptStart.Y), 0, 0, 0);
                                acPoly.AddVertexAt(1, new Point2d(ptEnd.X, ptEnd.Y), 0, 0, 0);
                                acPoly.AddVertexAt(2, new Point2d(ptEnd.X, ptEnd.Y - 15), 0, 0, 0);
                                acPoly.AddVertexAt(3, new Point2d(ptStart.X, ptStart.Y - 15), 0, 0, 0);
                                acPoly.AddVertexAt(4, new Point2d(ptStart.X, ptStart.Y), 0, 0, 0);
                                acPoly.ColorIndex = 8;
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
                                    acHatch.PatternScale = 50;
                                    acHatch.SetHatchPattern(HatchPatternType.PreDefined, "ANSI31");
                                    acHatch.ColorIndex = 252;
                                    acHatch.Associative = true;
                                    acHatch.AppendLoop(HatchLoopTypes.Outermost, acObjIdColl);
                                    acHatch.EvaluateHatch(true);

                                }

                                ed.WriteMessage(acPoly.ObjectId.ToString());

                            }

                            pPtOpts.UseBasePoint = false;
                            break;

                    }

                    // Commit the changes and dispose of the transaction
                    tr.Commit();

                }
            } while (input == 1);
        }

        [CommandMethod("DTB")]
        public void ChangeBlockName()
        {
            Document mdiActiveDocument = AcAp.DocumentManager.MdiActiveDocument;
            Editor editor = mdiActiveDocument.Editor;
            Database workingDatabase = HostApplicationServices.WorkingDatabase;
            try
            {
                PromptEntityOptions promptEntityOptions = new PromptEntityOptions("\nSelect block reference");
                promptEntityOptions.SetRejectMessage("\nSelect only block reference");
                promptEntityOptions.AddAllowedClass(typeof(BlockReference), false);
                PromptEntityResult entity = editor.GetEntity(promptEntityOptions);
                using (Transaction transaction = workingDatabase.TransactionManager.StartTransaction())
                {
                    BlockReference blockReference = transaction.GetObject(entity.ObjectId, 0) as BlockReference;
                    bool isDynamicBlock = blockReference.IsDynamicBlock;
                    BlockTableRecord blockTableRecord;
                    if (isDynamicBlock)
                    {
                        blockTableRecord =
                            (transaction.GetObject(blockReference.DynamicBlockTableRecord, OpenMode.ForWrite) as
                                BlockTableRecord);
                    }
                    else
                    {
                        blockTableRecord =
                            (transaction.GetObject(blockReference.BlockTableRecord, OpenMode.ForWrite) as
                                BlockTableRecord);
                    }

                    bool flag = blockTableRecord != null;
                    if (flag)
                    {
                        editor.WriteMessage("Block name is : " + blockTableRecord.Name + "\n");
                        DateTime now = DateTime.Now;
                        string text = now.Year.ToString() + now.Month.ToString() + now.Day.ToString() +
                                      now.Hour.ToString();
                        PromptStringOptions promptStringOptions = new PromptStringOptions("\nNhap vao ten block moi: ")
                        {
                            AllowSpaces = true
                        };
                        PromptResult @string = editor.GetString(promptStringOptions);
                        string text2 = @string.StringResult.ToString();
                        blockTableRecord.Name = text + " " + text2;
                        editor.WriteMessage(string.Concat(new string[]
                        {
                            "Block new name is: ",
                            text,
                            " ",
                            text2,
                            "\n"
                        }));
                        transaction.Commit();
                    }
                }
            }
            catch (Autodesk.AutoCAD.Runtime.Exception ex)
            {
                //editor.WriteMessage(ex.ToString());
            }
        }

        [CommandMethod("CNC")]
        public void ChangeColorOfSelectedObject()
        {

            Editor ed = AcAp.DocumentManager.MdiActiveDocument.Editor;
            Database db = HostApplicationServices.WorkingDatabase;
            RXClass blockClass = RXClass.GetClass(typeof(BlockReference));

            using (Transaction tr = db.TransactionManager.StartTransaction())
            {
                try
                {
                    PromptSelectionOptions pptSelOpts = new PromptSelectionOptions();
                    pptSelOpts.MessageForAdding = "Chọn các đối tượng để thay đổi Color: ";
                    pptSelOpts.AllowDuplicates = false;


                    PromptSelectionResult ppSelRes = ed.GetSelection(pptSelOpts);
                    if (ppSelRes.Status == PromptStatus.OK)
                    {

                        PromptIntegerOptions pptIntOpts = new PromptIntegerOptions("Nhập mã màu mới (0-255): ")
                        {
                            AllowNegative = false,
                            AllowNone = true,
                            DefaultValue = ColorOfWcadObject,
                            LowerLimit = 0,
                            UpperLimit = 255
                        };

                        PromptIntegerResult pptIntRes = ed.GetInteger(pptIntOpts);
                        if (pptIntRes.Status == PromptStatus.Cancel) return;
                        ColorOfWcadObject = pptIntRes.Value;


                        SelectionSet selSet = ppSelRes.Value;
                        foreach (SelectedObject selObj in selSet)
                        {
                            if (selObj != null)
                            {
                                Entity ent = tr.GetObject(selObj.ObjectId, OpenMode.ForRead) as Entity;
                                if (ent.ObjectId.ObjectClass != blockClass)
                                {
                                    ent.UpgradeOpen();
                                    ent.ColorIndex = ColorOfWcadObject;
                                    ent.DowngradeOpen();
                                }
                                else
                                {
                                    BlockReference br = ent as BlockReference;
                                    ChangeNestedEntitiesToColor(br, (short)ColorOfWcadObject);
                                }
                            }
                        }
                    }

                }
                catch (Autodesk.AutoCAD.Runtime.Exception ex)
                {

                    ed.WriteMessage("Error encountered: " + ex);
                }

                ed.Regen();
                tr.Commit();
            }

        }


        private void ChangeNestedEntitiesToColor(BlockReference btrId, short colorIndex)

        {
            RXClass blockClass = RXClass.GetClass(typeof(BlockReference));

            Document doc = AcAp.DocumentManager.MdiActiveDocument;

            Database db = doc.Database;

            Editor ed = doc.Editor;

            using (Transaction tr = db.TransactionManager.StartTransaction())
            {

                BlockTableRecord btr = (BlockTableRecord)tr.GetObject(btrId.BlockTableRecord, OpenMode.ForRead);
                try
                {
                    foreach (ObjectId entId in btr)

                    {

                        Entity ent = tr.GetObject(entId, OpenMode.ForRead) as Entity;
                        if (ent != null)
                        {
                            if (ent.ObjectId.ObjectClass == blockClass)
                            {
                                BlockReference br = ent as BlockReference;

                                // Recurse for nested blocks
                                ChangeNestedEntitiesToColor(br, colorIndex);
                            }
                            else

                            {
                                if (ent.ColorIndex != colorIndex)
                                {
                                    // Entity is only open for read
                                    ent.UpgradeOpen();
                                    ent.ColorIndex = colorIndex;
                                    ent.DowngradeOpen();

                                }

                            }

                        }



                    }
                }
                catch (Autodesk.AutoCAD.Runtime.Exception ex)
                {

                    ed.WriteMessage("Error encountered: " + ex);
                }

                tr.Commit();
            }

        }

        [CommandMethod("CFC")]
        public void ConvertTextStyleOfAttribute()

        {

            Editor ed = AcAp.DocumentManager.MdiActiveDocument.Editor;

            Database db = HostApplicationServices.WorkingDatabase;

            Transaction tr = db.TransactionManager.StartTransaction();

            // Start the transaction
            try

            {

                // Build a filter list so that only

                // block references are selected

                TypedValue[] filList = new TypedValue[1]
                {
                    new TypedValue((int) DxfCode.Start, "INSERT")

                };

                SelectionFilter filter = new SelectionFilter(filList);

                PromptSelectionOptions opts = new PromptSelectionOptions();

                opts.MessageForAdding = "Select block references: ";

                PromptSelectionResult res = ed.GetSelection(opts, filter);


                // Do nothing if selection is unsuccessful

                if (res.Status != PromptStatus.OK)

                    return;

                SelectionSet selSet = res.Value;

                ObjectId[] idArray = selSet.GetObjectIds();

                foreach (ObjectId blkId in idArray)

                {

                    BlockReference blkRef = (BlockReference)tr.GetObject(blkId, OpenMode.ForRead);

                    BlockTableRecord btr = (BlockTableRecord)tr.GetObject(blkRef.BlockTableRecord, OpenMode.ForRead);

                    ed.WriteMessage(

                        "\nBlock: " + btr.Name

                    );

                    btr.Dispose();


                    AttributeCollection attCol = blkRef.AttributeCollection;

                    foreach (ObjectId attId in attCol)

                    {

                        AttributeReference attRef = (AttributeReference)tr.GetObject(attId, OpenMode.ForWrite);

                        //You need to change TextStyleId property instead, which means that if you know the text style name, 
                        //Look up the TextStyleTable of the drawing database for the TextStyleTableRecord's Id of that name.

                        var tstTable = (TextStyleTable)tr.GetObject(db.TextStyleTableId, OpenMode.ForRead);
                        var tstTableRecord =
                            (TextStyleTableRecord)tr.GetObject(tstTable["PHC-vntimeh.shx"], OpenMode.ForRead);
                        attRef.TextStyleId = tstTableRecord.Id;

                        ed.WriteMessage(attRef.TextStyleId.ToString());

                    }
                }

                tr.Commit();
            }

            catch (Autodesk.AutoCAD.Runtime.Exception ex)

            {

                ed.WriteMessage(("Exception: " + ex.Message));

            }

            finally

            {

                tr.Dispose();

            }

        }


        [CommandMethod("ListLayouts")]
        public void ListLayouts()
        {
            // Get the current document and database
            Document acDoc = AcAp.DocumentManager.MdiActiveDocument;
            Database acCurDb = acDoc.Database;

            // Get the layout dictionary of the current database
            using (Transaction acTrans = acCurDb.TransactionManager.StartTransaction())
            {
                DBDictionary lays =
                    acTrans.GetObject(acCurDb.LayoutDictionaryId,
                        OpenMode.ForRead) as DBDictionary;

                acDoc.Editor.WriteMessage("\nLayouts123:");

                // Step through and list each named layout and Model
                foreach (DBDictionaryEntry item in lays)
                {
                    if (item.Key == "Model") continue;

                    acDoc.Editor.WriteMessage("\n  " + item.Key);
                }

                // Abort the changes to the database
                acTrans.Abort();
            }
        }

        [CommandMethod("CMC")]

        public void TaoCaoDo()

        {
            doc = AcAp.DocumentManager.MdiActiveDocument;
            Editor ed = doc.Editor;
            Database db = HostApplicationServices.WorkingDatabase;

            PromptPointResult pPtRes;
            PromptPointOptions pPtOpts = new PromptPointOptions("");
            // Prompt for the start point
            pPtOpts.Message = "\nPick point to get cote 0.000: ";
            pPtRes = ed.GetPoint(pPtOpts);
            Point3d ptStart = pPtRes.Value;
            if (pPtRes.Status == PromptStatus.Cancel) return;
            // Prompt for the start point
            pPtOpts.Message = "\nPick point to get current cote: ";
            pPtOpts.UseBasePoint = true;
            pPtOpts.BasePoint = ptStart;
            pPtRes = ed.GetPoint(pPtOpts);
            Point3d ptEnd = pPtRes.Value;
            if (pPtRes.Status == PromptStatus.Cancel) return;

            double cote = ptEnd.Y - ptStart.Y;
            string result = String.Format("{0:F3}", Math.Round(cote / 1000, 3));
            if (cote > 0)
            {
                ed.WriteMessage(result);

                InsertingABlock("+" + result, ptEnd);
            }
            else
            {
                ed.WriteMessage(result);

                InsertingABlock(result, ptEnd);
            }

        }


        public void InsertingABlock(string CoteCaoDo, Point3d insertPoint)
        {
            // Get the current database and start a transaction
            Database acCurDb;
            acCurDb = AcAp.DocumentManager.MdiActiveDocument.Database;
            using (Transaction acTrans = acCurDb.TransactionManager.StartTransaction())
            {
                // Open the Block table for read
                BlockTable acBlkTbl;
                acBlkTbl = acTrans.GetObject(acCurDb.BlockTableId, OpenMode.ForRead) as BlockTable;

                ObjectId blkRecId = ObjectId.Null;
               
                if (!acBlkTbl.Has("cote cao do"))
                {
                    ImportBlocks();
                    blkRecId = acBlkTbl["cote cao do"];
                }
                else
                {
                    blkRecId = acBlkTbl["cote cao do"];
                }

                    //if (!acBlkTbl.Has("CircleBlock"))
                    //{
                    //    using (BlockTableRecord acBlkTblRec = new BlockTableRecord())
                    //    {
                    //        acBlkTblRec.Name = "CircleBlock";

                    //        // Set the insertion point for the block
                    //        acBlkTblRec.Origin = new Point3d(0, 0, 0);

                    //        // Add a circle to the block
                    //        using (Circle acCirc = new Circle())
                    //        {
                    //            acCirc.Center = new Point3d(0, 0, 0);
                    //            acCirc.Radius = 2;

                    //            acBlkTblRec.AppendEntity(acCirc);

                    //            acTrans.GetObject(acCurDb.BlockTableId, OpenMode.ForWrite);
                    //            acBlkTbl.Add(acBlkTblRec);
                    //            acTrans.AddNewlyCreatedDBObject(acBlkTblRec, true);
                    //        }

                    //        blkRecId = acBlkTblRec.Id;
                    //    }
                    //}
                    //else
                    //{

                    //}



                    // Insert the block into the current space
                    if (blkRecId != ObjectId.Null)
                {

                    //START TEST-----------------------------------

                    BlockTableRecord acBlkTblRec;
                    acBlkTblRec = acTrans.GetObject(blkRecId, OpenMode.ForRead) as BlockTableRecord;

                    using (BlockReference acBlkRef = new BlockReference(insertPoint, blkRecId))
                    {
                        BlockTableRecord acCurSpaceBlkTblRec;
                        acCurSpaceBlkTblRec = acTrans.GetObject(acCurDb.CurrentSpaceId, OpenMode.ForWrite) as BlockTableRecord;

                        acCurSpaceBlkTblRec.AppendEntity(acBlkRef);
                        acTrans.AddNewlyCreatedDBObject(acBlkRef, true);

                        // Verify block table record has attribute definitions associated with it
                        if (acBlkTblRec.HasAttributeDefinitions)
                        {
                            // Add attributes from the block table record
                            foreach (ObjectId objID in acBlkTblRec)
                            {
                                DBObject dbObj = acTrans.GetObject(objID, OpenMode.ForRead) as DBObject;

                                if (dbObj is AttributeDefinition)
                                {
                                    AttributeDefinition acAtt = dbObj as AttributeDefinition;

                                    if (!acAtt.Constant)
                                    {
                                        using (AttributeReference acAttRef = new AttributeReference())
                                        {
                                            acAttRef.SetAttributeFromBlock(acAtt, acBlkRef.BlockTransform);
                                            acAttRef.Position = acAtt.Position.TransformBy(acBlkRef.BlockTransform);

                                            acAttRef.TextString = CoteCaoDo;

                                            acBlkRef.AttributeCollection.AppendAttribute(acAttRef);
                                            acTrans.AddNewlyCreatedDBObject(acAttRef, true);
                                        }
                                    }

                                    // Change the attribute definition to be displayed as backwards
                                    acAtt.UpgradeOpen();
                                    acAtt.IsMirroredInX = true;
                                }
                            }
                        }

                    }
                    //END TEST-----------------------------------


                    //using (BlockReference acBlkRef = new BlockReference(new Point3d(0, 0, 0), blkRecId))
                    //{

                    //    BlockTableRecord acCurSpaceBlkTblRec;
                    //    acCurSpaceBlkTblRec = acTrans.GetObject(acCurDb.CurrentSpaceId, OpenMode.ForWrite) as BlockTableRecord;

                    //    acCurSpaceBlkTblRec.AppendEntity(acBlkRef);
                    //    acTrans.AddNewlyCreatedDBObject(acBlkRef, true);


                    //}
                }




                // Save the new object to the database
                acTrans.Commit();

                // Dispose of the transaction
            }
        }


        public void ImportBlocks()

        {
            DocumentCollection dm =
                AcAp.DocumentManager;

            Editor ed = dm.MdiActiveDocument.Editor;

            Database destDb = dm.MdiActiveDocument.Database;

            Database sourceDb = new Database(false, true);


            try

            {

                // Get name of DWG from which to copy blocks
                var fileName = (Environment.GetFolderPath(
                Environment.SpecialFolder.ApplicationData) + "\\Autodesk\\ApplicationPlugins\\HeliosBim.bundle\\Contents\\Resources\\PHC-ISO.dwg");
                ed.WriteMessage(fileName);

                string stringResult = fileName;

                //ed.GetString("\nEnter the name of the source drawing: ");
                //"C:\Users\Admin\AppData\Roaming\\Autodesk\\ApplicationPlugins\\wcadplugin.bundle\\Contents\\Resources\\PHC-ISO.dwg"
                // Read the DWG into a side database
                //C:\Users\Admin\AppData\Roaming\Contents\Resources\PHC-ISO.dwg

                sourceDb.ReadDwgFile(stringResult,

                                    System.IO.FileShare.Read,

                                    true,

                                    "");


                // Create a variable to store the list of block identifiers

                ObjectIdCollection blockIds = new ObjectIdCollection();


                Autodesk.AutoCAD.DatabaseServices.TransactionManager tm =

                  sourceDb.TransactionManager;


                using (Transaction myT = tm.StartTransaction())

                {

                    // Open the block table

                    BlockTable bt =

                        (BlockTable)tm.GetObject(sourceDb.BlockTableId,

                                                OpenMode.ForRead,

                                                false);


                    // Check each block in the block table

                    foreach (ObjectId btrId in bt)

                    {

                        BlockTableRecord btr =

                          (BlockTableRecord)tm.GetObject(btrId,

                                                        OpenMode.ForRead,

                                                        false);

                        // Only add named & non-layout blocks to the copy list

                        if (!btr.IsAnonymous && !btr.IsLayout)

                            blockIds.Add(btrId);

                        btr.Dispose();

                    }

                }

                // Copy blocks from source to destination database

                IdMapping mapping = new IdMapping();

                sourceDb.WblockCloneObjects(blockIds,

                                            destDb.BlockTableId,

                                            mapping,

                                            DuplicateRecordCloning.Replace,

                                            false);

                //ed.WriteMessage("\nCopied "

                //                + blockIds.Count.ToString()

                //                + " block definitions from "

                //                + stringResult

                //                + " to the current drawing.");

            }

            catch (Autodesk.AutoCAD.Runtime.Exception ex)

            {

                ed.WriteMessage("\nError during copy: " + ex.Message);

            }

            sourceDb.Dispose();

        }

        #endregion
    }
}
