using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using Autodesk.AutoCAD.EditorInput;
using AcAp = Autodesk.AutoCAD.ApplicationServices.Application;

namespace HeliosBIM.Modal_Dialog_XAML
{
    /// <summary>
    /// Interaction logic for ModalWpfDialog.xaml
    /// </summary>
    public partial class ModalWpfDialog : Window
    {
        // private field
        double radius;

        /// <summary>
        /// Gets the selected layer name.
        /// </summary>
        public string Layer => (string)cbxLayer.SelectedItem;

        /// <summary>
        /// Gets the radius.
        /// </summary>
        public double Radius => radius;

        /// <summary>
        /// Creates a new instance of ModalWpfDialog.
        /// </summary>
        /// <param name="layers">Layer names collection.</param>
        /// <param name="layer">Default layer name.</param>
        /// <param name="radius">Default radius</param>

        public ModalWpfDialog(List<string> layers, string layer, double radius)
        {
            InitializeComponent();
            this.radius = radius;
            cbxLayer.ItemsSource = layers;
            cbxLayer.SelectedItem = layer;
            txtRadius.Text = radius.ToString();

            InitializeComponent();

            /// <summary>
            /// Handles the 'Click' event of the 'Radius' button.
            /// </summary>
            /// <param name="sender">Event source.</param>
            /// <param name="e">Event data.</param>
            
        }

        private void btnRadius_Click(object sender, RoutedEventArgs e)
        {
            // prompt the user to specify distance
            var ed = AcAp.DocumentManager.MdiActiveDocument.Editor;
            var opts = new PromptDistanceOptions("\nSpecify the radius: ");
            opts.AllowNegative = false;
            opts.AllowZero = false;
            var pdr = ed.GetDistance(opts);
            if (pdr.Status == PromptStatus.OK)
            {
                txtRadius.Text = pdr.Value.ToString();
            }
        }

        /// <summary>
        /// Handles the 'Click' event of the 'OK' button.
        /// </summary>
        /// <param name="sender">Event source.</param>
        /// <param name="e">Event data.</param>
        private void btnOK_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
        }

        /// <summary>
        /// Handles the 'TextChanged' event ot the 'Radius' TextBox.
        /// </summary>
        /// <param name="sender">Event source.</param>
        /// <param name="e">Event data.</param>
        private void txtRadius_TextChanged(object sender, TextChangedEventArgs e)
        {
            btnOK.IsEnabled = double.TryParse(txtRadius.Text, out radius);
        }
    }
}
