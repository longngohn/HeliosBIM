﻿
namespace HeliosBIM.Modal_Dialog
{
    partial class PickCote
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.cbxLayer = new System.Windows.Forms.ComboBox();
            this.txtScale = new System.Windows.Forms.TextBox();
            this.label3 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.btnCancel = new System.Windows.Forms.Button();
            this.btnOK = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // cbxLayer
            // 
            this.cbxLayer.FormattingEnabled = true;
            this.cbxLayer.Location = new System.Drawing.Point(107, 46);
            this.cbxLayer.Margin = new System.Windows.Forms.Padding(2);
            this.cbxLayer.Name = "cbxLayer";
            this.cbxLayer.Size = new System.Drawing.Size(120, 21);
            this.cbxLayer.TabIndex = 15;
            // 
            // txtScale
            // 
            this.txtScale.Location = new System.Drawing.Point(107, 11);
            this.txtScale.Margin = new System.Windows.Forms.Padding(2);
            this.txtScale.Name = "txtScale";
            this.txtScale.Size = new System.Drawing.Size(120, 20);
            this.txtScale.TabIndex = 14;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(32, 49);
            this.label3.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(36, 13);
            this.label3.TabIndex = 12;
            this.label3.Text = "Layer:";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(32, 18);
            this.label2.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(37, 13);
            this.label2.TabIndex = 11;
            this.label2.Text = "Scale:";
            // 
            // btnCancel
            // 
            this.btnCancel.Location = new System.Drawing.Point(145, 85);
            this.btnCancel.Margin = new System.Windows.Forms.Padding(2);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new System.Drawing.Size(82, 34);
            this.btnCancel.TabIndex = 9;
            this.btnCancel.Text = "Cancel";
            this.btnCancel.UseVisualStyleBackColor = true;
            // 
            // btnOK
            // 
            this.btnOK.Location = new System.Drawing.Point(35, 85);
            this.btnOK.Margin = new System.Windows.Forms.Padding(2);
            this.btnOK.Name = "btnOK";
            this.btnOK.Size = new System.Drawing.Size(86, 34);
            this.btnOK.TabIndex = 8;
            this.btnOK.Text = "Pick point";
            this.btnOK.UseVisualStyleBackColor = true;
            // 
            // PickToaDo
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(258, 143);
            this.Controls.Add(this.cbxLayer);
            this.Controls.Add(this.txtScale);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.btnCancel);
            this.Controls.Add(this.btnOK);
            this.Name = "PickToaDo";
            this.Text = "Pick Cao Do";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.ComboBox cbxLayer;
        private System.Windows.Forms.TextBox txtScale;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Button btnCancel;
        private System.Windows.Forms.Button btnOK;
    }
}