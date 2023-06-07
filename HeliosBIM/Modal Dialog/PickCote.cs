using System.Collections.Generic;
using System.Windows.Forms;

namespace HeliosBIM.Modal_Dialog
{
    public partial class PickCote : Form
    {
        public PickCote()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Gets scale of text
        /// </summary>
        public int Scale => int.Parse(txtScale.Text);
        /// <summary>
        /// Gets the selected item in the ComboBox control.
        /// </summary>
        public string Layer => (string)cbxLayer.SelectedItem;


        /// <summary>
        /// Create instance of Zoning
        /// </summary>
        /// <param name="layers"></param>
        /// <param name="clayer"></param>
        /// <param name="textHeight"></param>
        /// <param name="scale"></param>
        public PickCote(List<string> layers, string clayer, int textHeight, int scale)
        {
            InitializeComponent();

            // assigning the DialogResult values to the buttons
            btnCancel.DialogResult = DialogResult.Cancel;
            btnOK.DialogResult = DialogResult.OK;

            // binding the layer collection to the ComboBox control
            cbxLayer.DataSource = layers;
            cbxLayer.SelectedItem = clayer;

            txtScale.Text = scale.ToString();

        }
    }
}
