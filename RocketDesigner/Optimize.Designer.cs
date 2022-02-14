namespace RocketDesigner
{
    partial class Optimize
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
            this.buttonOptimize = new System.Windows.Forms.Button();
            this.numericUpDownPop = new System.Windows.Forms.NumericUpDown();
            this.numericUpDownKeep = new System.Windows.Forms.NumericUpDown();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.numericUpDownEpoch = new System.Windows.Forms.NumericUpDown();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownPop)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownKeep)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownEpoch)).BeginInit();
            this.SuspendLayout();
            // 
            // buttonOptimize
            // 
            this.buttonOptimize.Location = new System.Drawing.Point(116, 101);
            this.buttonOptimize.Name = "buttonOptimize";
            this.buttonOptimize.Size = new System.Drawing.Size(75, 23);
            this.buttonOptimize.TabIndex = 0;
            this.buttonOptimize.Text = "Optimize";
            this.buttonOptimize.UseVisualStyleBackColor = true;
            this.buttonOptimize.Click += new System.EventHandler(this.buttonOptimize_Click);
            // 
            // numericUpDownPop
            // 
            this.numericUpDownPop.Location = new System.Drawing.Point(164, 12);
            this.numericUpDownPop.Name = "numericUpDownPop";
            this.numericUpDownPop.Size = new System.Drawing.Size(120, 20);
            this.numericUpDownPop.TabIndex = 1;
            // 
            // numericUpDownKeep
            // 
            this.numericUpDownKeep.Location = new System.Drawing.Point(164, 38);
            this.numericUpDownKeep.Name = "numericUpDownKeep";
            this.numericUpDownKeep.Size = new System.Drawing.Size(120, 20);
            this.numericUpDownKeep.TabIndex = 2;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(36, 14);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(112, 13);
            this.label1.TabIndex = 3;
            this.label1.Text = "Generation Population";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(36, 40);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(96, 13);
            this.label2.TabIndex = 4;
            this.label2.Text = "Population to keep";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(36, 66);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(97, 13);
            this.label3.TabIndex = 5;
            this.label3.Text = "Generation number";
            // 
            // numericUpDownEpoch
            // 
            this.numericUpDownEpoch.Location = new System.Drawing.Point(164, 64);
            this.numericUpDownEpoch.Name = "numericUpDownEpoch";
            this.numericUpDownEpoch.Size = new System.Drawing.Size(120, 20);
            this.numericUpDownEpoch.TabIndex = 6;
            // 
            // Optimize
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(315, 133);
            this.Controls.Add(this.numericUpDownEpoch);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.numericUpDownKeep);
            this.Controls.Add(this.numericUpDownPop);
            this.Controls.Add(this.buttonOptimize);
            this.Name = "Optimize";
            this.Text = "Optimize";
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownPop)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownKeep)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownEpoch)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button buttonOptimize;
        private System.Windows.Forms.NumericUpDown numericUpDownPop;
        private System.Windows.Forms.NumericUpDown numericUpDownKeep;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.NumericUpDown numericUpDownEpoch;
    }
}