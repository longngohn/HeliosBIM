using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;


namespace HeliosBIM.ModalDialogTest
{
    public class SelectionUtils
    {
        public static SelectionSet FilterForSingleEntity(Editor ed, string graphicObject)
        {
            // Get the current document editor

            SelectionSet acSSet = null;
            // Create a TypedValue array to define the filter criteria
            TypedValue[] acTypValAr = new TypedValue[1];

            acTypValAr.SetValue(new TypedValue((int)DxfCode.Start, graphicObject), 0);

            // Assign the filter criteria to a SelectionFilter object
            SelectionFilter acSelFtr = new SelectionFilter(acTypValAr);

            // Request for objects to be selected in the drawing area
            PromptSelectionResult acSSPrompt = null;
            acSSPrompt = ed.GetSelection(acSelFtr);


            // If the prompt status is OK, objects were selected
            if (acSSPrompt.Status == PromptStatus.OK)
            {
                acSSet = acSSPrompt.Value;

                Application.ShowAlertDialog("Number of objects selected: " +
                                            acSSet.Count.ToString());
                return acSSet;
            }
            return acSSet;
        }
    }
}