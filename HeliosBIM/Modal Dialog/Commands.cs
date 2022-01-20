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


        }

        /// <summary>
        /// Command to show the dialog box
        /// </summary>
        [CommandMethod("CMD_MODAL")]
        public void ModalDialogCmd()
        {
            // creation of an instance of ModalDialog
            // with the document data (layers and default values)
            var layers = GetLayerNames(doc.Database);
            if (!layers.Contains(layer))
            {
                layer = (string)AcAp.GetSystemVariable("clayer");
            }
            using (var dialog = new ModalDialog(layers, layer, radius))
            {
                // shows the dialog box in modal mode
                // and acts according to the DialogResult value
                var dlgResult = AcAp.ShowModalDialog(dialog);
                if (dlgResult == System.Windows.Forms.DialogResult.OK)
                {
                    // fields update
                    layer = dialog.Layer;
                    radius = dialog.Radius;
                    // circle drawing
                    DrawCircle(radius, layer);
                }
            }
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
    }
}
