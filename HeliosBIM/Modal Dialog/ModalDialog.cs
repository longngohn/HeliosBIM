using System.Collections.Generic;
using System.Windows.Forms;

using Autodesk.AutoCAD.EditorInput;

using AcAp = Autodesk.AutoCAD.ApplicationServices.Application;

namespace HeliosBIM.ModalDialogTest
{
    public partial class ModalDialog : Form
    {
        double radius;

        /// <summary>
        /// Gets the radius value.
        /// </summary>
        public double Radius => radius;

        /// <summary>
        /// Gets the selected item in the ComboBox control.
        /// </summary>
        public string Layer => (string)cbxLayer.SelectedItem;

        /// <summary>
        /// Creates a new instance of ModalDialog.
        /// </summary>
        /// <param name="layers">Collection of layers to be bound to the ComboBox control.</param>
        /// <param name="clayer">Default layer name.</param>
        /// <param name="radius">Default radius.</param>
        public ModalDialog(List<string> layers, string clayer, double radius)
        {
            InitializeComponent();

            // assigning the DialogResult values to the buttons
            btnCancel.DialogResult = DialogResult.Cancel;
            btnOk.DialogResult = DialogResult.OK;

            // binding the layer collection to the ComboBox control
            cbxLayer.DataSource = layers;
            cbxLayer.SelectedItem = clayer;

            // radius default value
            txtRadius.Text = radius.ToString();
        }

        /// <summary>
        /// Handles the 'TextChanged' event of the 'Radius' TextBox.
        /// </summary>
        /// <param name="sender">Event source.</param>
        /// <param name="e">Event data.</param>
        private void txtRadius_TextChanged(object sender, System.EventArgs e)
        {
            // OK button is 'disable' if the text does not represent a valid number
            // the radius field is updated accordingly
            btnOk.Enabled = double.TryParse(txtRadius.Text, out radius);
        }

        /// <summary>
        /// Handles the 'Click' event of the 'Radius' button.
        /// </summary>
        /// <param name="sender">Event source.</param>
        /// <param name="e">Event data.</param>
        private void btnRadius_Click(object sender, System.EventArgs e)
        {
            var ed = AcAp.DocumentManager.MdiActiveDocument.Editor;
            var opts = new PromptDistanceOptions("\nSpecify the radius: ");
            opts.AllowNegative = false;
            opts.AllowZero = false;
            // AutoCAD automatically hides the dialog box to let the user specify the radius
            var pdr = ed.GetDistance(opts);
            if (pdr.Status == PromptStatus.OK)
                txtRadius.Text = pdr.Value.ToString();
        }

       
    }
}