using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Geometry;
using Autodesk.AutoCAD.Runtime;
using System.Windows.Forms;
using Autodesk.AutoCAD.GraphicsInterface;
using AcAp = Autodesk.AutoCAD.ApplicationServices.Core.Application;
using Newtonsoft.Json;


namespace HeliosBIM
{
    public class Commands : IExtensionApplication
    {
        #region InitAndTerm

        public void Initialize()
        {
            Document doc = AcAp.DocumentManager.MdiActiveDocument;
            Editor ed = doc.Editor;
            AcAp.MainWindow.Text = "Welcome to HeliosBIM";

            AcAp.ShowAlertDialog("Reload Addin Helios Test");

            //AcAp.ShowAlertDialog("\nLệnh tắt các tính năng:" +
            //                     "\nBTL: Vẽ BLT cho móng và nền" +
            //                     "\nVPP: Vẽ VPP cho dầm và sàn" +
            //                     "\nDTB: Đổi tên Block" +
            //                     "\nCNC: Đổi màu đối tượng"
            //);
        }

        public void Terminate()
        {

        }

        #endregion

        // instance fields
        Document doc;  // active document
        double radius; // radius default value
        string layer;  // layer default value

        /// <summary>
        /// Creates a new instance of Commands.
        /// This constructor is run once per document
        /// at the first call of a 'CommandMethod' method.
        /// </summary>
        public Commands()
        {
            // private fields initialization (initial default values)
            doc = AcAp.DocumentManager.MdiActiveDocument;
            radius = 10.0;
            layer = (string)AcAp.GetSystemVariable("clayer");
        }

        /// <summary>
        /// Command to draw the circle.
        /// </summary>
        [CommandMethod("CMD_CIRCLE")]
        public void DrawCircleCmd()
        {
            Database db = doc.Database;
            Editor ed = doc.Editor;

            // choose of the layer
            List<string> layers = GetLayerNames(db);
            if (!layers.Contains(layer))
            {
                layer = (string)AcAp.GetSystemVariable("clayer");
            }
            PromptStringOptions strOptions = new PromptStringOptions("\nLayer name: ");
            strOptions.DefaultValue = layer;
            strOptions.UseDefaultValue = true;
            PromptResult strResult = ed.GetString(strOptions);
            if (strResult.Status != PromptStatus.OK)
                return;
            if (!layers.Contains(strResult.StringResult))
            {
                ed.WriteMessage(
                  $"\nNone layer named '{strResult.StringResult}' in the layer table.");
                return;
            }
            layer = strResult.StringResult;

            // specification of the radius
            PromptDistanceOptions distOptions = new PromptDistanceOptions("\nSpecify the radius: ");
            distOptions.DefaultValue = radius;
            distOptions.UseDefaultValue = true;
            PromptDoubleResult distResult = ed.GetDistance(distOptions);
            if (distResult.Status != PromptStatus.OK)
                return;
            radius = distResult.Value;

            // specification of the center
            PromptPointResult ppr = ed.GetPoint("\nSpecify the center: ");
            if (ppr.Status == PromptStatus.OK)
            {
                // drawing of the circle in the current space
                using (Transaction tr = db.TransactionManager.StartTransaction())
                {
                    BlockTableRecord curSpace =
                      (BlockTableRecord)tr.GetObject(db.CurrentSpaceId, OpenMode.ForWrite);
                    using (Circle circle = new Circle(ppr.Value, Vector3d.ZAxis, radius))
                    {
                        circle.TransformBy(ed.CurrentUserCoordinateSystem);
                        circle.Layer = strResult.StringResult;
                        curSpace.AppendEntity(circle);
                        tr.AddNewlyCreatedDBObject(circle, true);
                    }
                    tr.Commit();
                }
            }
        }

        /// <summary>
        /// Command to test NewtonJson
        /// </summary>
        [CommandMethod("CMD_CreateJSON")]
        public void CreatJSON()
        {
            var aPollo = new CAT("Apollo", 10, "2kg", 10000);

            string jSonApollo = JsonConvert.SerializeObject(aPollo);

            Database db = doc.Database;
            Editor ed = doc.Editor;

            MessageBox.Show(jSonApollo);

            var anDrew = new CAT();

            JsonConvert.PopulateObject(jSonApollo,anDrew);

            MessageBox.Show("\nName of Andrew: " + anDrew.Name
            + "\nColor ID of Andrew: " + anDrew.Color
            + "\nWeight of Andrew: " + anDrew.Weight
            + "\nPrice of Andrew: " + anDrew.Price
            );
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

        [CommandMethod("HeliosCommand")]
        public void HeliosCommand()
        {
            Document doc = AcAp.DocumentManager.MdiActiveDocument;
            Editor ed = doc.Editor;
            Database db = doc.Database;

            FirstCommandViewModel viewModel = new FirstCommandViewModel(doc);

            FirstCommandWindow window = new FirstCommandWindow(viewModel);

            window.ShowDialog();

        }
    }
}
