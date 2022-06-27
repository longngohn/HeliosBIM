using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;
using Autodesk.AutoCAD.Runtime;

namespace HeliosBIM.ModalDialogTest
{
    public class TextUtils
    {
        /// <summary>
        /// Create text with scale by user
        /// </summary>
        /// <param name="scale"></param>
        /// <param name="size"></param>
        /// <param name="content"></param>
        /// <param name="insertPoint3d"></param>
        public static void CreateTextWithScale(Document doc,int scale, int sizePage, string clayer, string content, Point3d insertPoint3d)
        {
            // Get the current document and database
            Document acDoc = doc;
            Database acCurDb = acDoc.Database;

            // Start a transaction
            using (Transaction acTrans = acCurDb.TransactionManager.StartTransaction())
            {
                // Open the Block table for read
                BlockTable acBlkTbl;
                acBlkTbl = acTrans.GetObject(acCurDb.BlockTableId,
                    OpenMode.ForRead) as BlockTable;

                // Open the Block table record Model space for write
                BlockTableRecord acBlkTblRec;
                acBlkTblRec = acTrans.GetObject(acBlkTbl[BlockTableRecord.ModelSpace],
                    OpenMode.ForWrite) as BlockTableRecord;

                // Create a single-line text object
                using (DBText acText = new DBText())
                {
                    acText.VerticalMode = TextVerticalMode.TextVerticalMid;
                    acText.HorizontalMode = TextHorizontalMode.TextCenter;
                    acText.Height = scale * sizePage;
                    acText.TextString = content;
                    acText.Layer = clayer;
                    acText.AlignmentPoint = insertPoint3d;
                    acBlkTblRec.AppendEntity(acText);
                    acTrans.AddNewlyCreatedDBObject(acText, true);

                }

                // Save the changes and dispose of the transaction
                acTrans.Commit();
            }
        }

        public static void CreateMText()
        {
            // Get the current document and database
            Document acDoc = Application.DocumentManager.MdiActiveDocument;
            Database acCurDb = acDoc.Database;

            // Start a transaction
            using (Transaction acTrans = acCurDb.TransactionManager.StartTransaction())
            {
                // Open the Block table for read
                BlockTable acBlkTbl;
                acBlkTbl = acTrans.GetObject(acCurDb.BlockTableId,
                    OpenMode.ForRead) as BlockTable;

                // Open the Block table record Model space for write
                BlockTableRecord acBlkTblRec;
                acBlkTblRec = acTrans.GetObject(acBlkTbl[BlockTableRecord.ModelSpace],
                    OpenMode.ForWrite) as BlockTableRecord;

                // Create a multiline text object
                using (MText acMText = new MText())
                {
                    acMText.Location = new Point3d(2, 2, 0);
                    acMText.Width = 4;
                    acMText.Contents = "This is a text string for the MText object.";
                    acMText.Attachment = AttachmentPoint.BottomCenter;
                    acBlkTblRec.AppendEntity(acMText);
                    acTrans.AddNewlyCreatedDBObject(acMText, true);
                }

                // Save the changes and dispose of the transaction
                acTrans.Commit();
            }
        }

    }
}
