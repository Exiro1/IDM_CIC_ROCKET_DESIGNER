namespace RocketDesigner
{
    partial class BatchGenerator
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
            this.batchNumber_Numeric = new System.Windows.Forms.NumericUpDown();
            this.param1_comboBox = new System.Windows.Forms.ComboBox();
            this.param1_Label = new System.Windows.Forms.Label();
            this.param2_Label = new System.Windows.Forms.Label();
            this.param2_comboBox = new System.Windows.Forms.ComboBox();
            this.batchNumber_Label = new System.Windows.Forms.Label();
            this.min1_Numeric = new System.Windows.Forms.NumericUpDown();
            this.max1_Numeric = new System.Windows.Forms.NumericUpDown();
            this.min2_Numeric = new System.Windows.Forms.NumericUpDown();
            this.max2_Numeric = new System.Windows.Forms.NumericUpDown();
            this.min1_label = new System.Windows.Forms.Label();
            this.min2_label = new System.Windows.Forms.Label();
            this.max2_label = new System.Windows.Forms.Label();
            this.max1_label = new System.Windows.Forms.Label();
            this.generate_button = new System.Windows.Forms.Button();
            this.info_label = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            ((System.ComponentModel.ISupportInitialize)(this.batchNumber_Numeric)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.min1_Numeric)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.max1_Numeric)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.min2_Numeric)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.max2_Numeric)).BeginInit();
            this.SuspendLayout();
            // 
            // batchNumber_Numeric
            // 
            this.batchNumber_Numeric.Location = new System.Drawing.Point(168, 115);
            this.batchNumber_Numeric.Name = "batchNumber_Numeric";
            this.batchNumber_Numeric.Size = new System.Drawing.Size(120, 20);
            this.batchNumber_Numeric.TabIndex = 0;
            // 
            // param1_comboBox
            // 
            this.param1_comboBox.FormattingEnabled = true;
            this.param1_comboBox.Location = new System.Drawing.Point(108, 21);
            this.param1_comboBox.Name = "param1_comboBox";
            this.param1_comboBox.Size = new System.Drawing.Size(121, 21);
            this.param1_comboBox.TabIndex = 1;
            // 
            // param1_Label
            // 
            this.param1_Label.AutoSize = true;
            this.param1_Label.Location = new System.Drawing.Point(15, 24);
            this.param1_Label.Name = "param1_Label";
            this.param1_Label.Size = new System.Drawing.Size(76, 13);
            this.param1_Label.TabIndex = 2;
            this.param1_Label.Text = "First parameter";
            // 
            // param2_Label
            // 
            this.param2_Label.AutoSize = true;
            this.param2_Label.Location = new System.Drawing.Point(15, 69);
            this.param2_Label.Name = "param2_Label";
            this.param2_Label.Size = new System.Drawing.Size(94, 13);
            this.param2_Label.TabIndex = 3;
            this.param2_Label.Text = "Second parameter";
            // 
            // param2_comboBox
            // 
            this.param2_comboBox.FormattingEnabled = true;
            this.param2_comboBox.Location = new System.Drawing.Point(108, 66);
            this.param2_comboBox.Name = "param2_comboBox";
            this.param2_comboBox.Size = new System.Drawing.Size(121, 21);
            this.param2_comboBox.TabIndex = 4;
            // 
            // batchNumber_Label
            // 
            this.batchNumber_Label.AutoSize = true;
            this.batchNumber_Label.Location = new System.Drawing.Point(15, 117);
            this.batchNumber_Label.Name = "batchNumber_Label";
            this.batchNumber_Label.Size = new System.Drawing.Size(147, 13);
            this.batchNumber_Label.TabIndex = 5;
            this.batchNumber_Label.Text = "number of profiles to generate";
            this.batchNumber_Label.Click += new System.EventHandler(this.label1_Click);
            // 
            // min1_Numeric
            // 
            this.min1_Numeric.DecimalPlaces = 3;
            this.min1_Numeric.Location = new System.Drawing.Point(305, 21);
            this.min1_Numeric.Name = "min1_Numeric";
            this.min1_Numeric.Size = new System.Drawing.Size(120, 20);
            this.min1_Numeric.TabIndex = 6;
            // 
            // max1_Numeric
            // 
            this.max1_Numeric.DecimalPlaces = 3;
            this.max1_Numeric.Location = new System.Drawing.Point(516, 21);
            this.max1_Numeric.Name = "max1_Numeric";
            this.max1_Numeric.Size = new System.Drawing.Size(120, 20);
            this.max1_Numeric.TabIndex = 7;
            // 
            // min2_Numeric
            // 
            this.min2_Numeric.DecimalPlaces = 3;
            this.min2_Numeric.Location = new System.Drawing.Point(305, 67);
            this.min2_Numeric.Name = "min2_Numeric";
            this.min2_Numeric.Size = new System.Drawing.Size(120, 20);
            this.min2_Numeric.TabIndex = 8;
            // 
            // max2_Numeric
            // 
            this.max2_Numeric.DecimalPlaces = 3;
            this.max2_Numeric.Location = new System.Drawing.Point(516, 67);
            this.max2_Numeric.Name = "max2_Numeric";
            this.max2_Numeric.Size = new System.Drawing.Size(120, 20);
            this.max2_Numeric.TabIndex = 9;
            // 
            // min1_label
            // 
            this.min1_label.AutoSize = true;
            this.min1_label.Location = new System.Drawing.Point(252, 24);
            this.min1_label.Name = "min1_label";
            this.min1_label.Size = new System.Drawing.Size(47, 13);
            this.min1_label.TabIndex = 10;
            this.min1_label.Text = "minimum";
            // 
            // min2_label
            // 
            this.min2_label.AutoSize = true;
            this.min2_label.Location = new System.Drawing.Point(252, 69);
            this.min2_label.Name = "min2_label";
            this.min2_label.Size = new System.Drawing.Size(47, 13);
            this.min2_label.TabIndex = 11;
            this.min2_label.Text = "minimum";
            // 
            // max2_label
            // 
            this.max2_label.AutoSize = true;
            this.max2_label.Location = new System.Drawing.Point(463, 69);
            this.max2_label.Name = "max2_label";
            this.max2_label.Size = new System.Drawing.Size(50, 13);
            this.max2_label.TabIndex = 12;
            this.max2_label.Text = "maximum";
            // 
            // max1_label
            // 
            this.max1_label.AutoSize = true;
            this.max1_label.Location = new System.Drawing.Point(463, 23);
            this.max1_label.Name = "max1_label";
            this.max1_label.Size = new System.Drawing.Size(50, 13);
            this.max1_label.TabIndex = 13;
            this.max1_label.Text = "maximum";
            // 
            // generate_button
            // 
            this.generate_button.Location = new System.Drawing.Point(215, 158);
            this.generate_button.Name = "generate_button";
            this.generate_button.Size = new System.Drawing.Size(331, 23);
            this.generate_button.TabIndex = 14;
            this.generate_button.Text = "Generate";
            this.generate_button.UseVisualStyleBackColor = true;
            this.generate_button.Click += new System.EventHandler(this.generate_button_Click);
            // 
            // info_label
            // 
            this.info_label.AutoSize = true;
            this.info_label.Location = new System.Drawing.Point(649, 23);
            this.info_label.Name = "info_label";
            this.info_label.Size = new System.Drawing.Size(88, 13);
            this.info_label.TabIndex = 15;
            this.info_label.Text = "(meters / degree)";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(649, 69);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(88, 13);
            this.label2.TabIndex = 17;
            this.label2.Text = "(meters / degree)";
            // 
            // BatchGenerator
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(748, 193);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.info_label);
            this.Controls.Add(this.generate_button);
            this.Controls.Add(this.max1_label);
            this.Controls.Add(this.max2_label);
            this.Controls.Add(this.min2_label);
            this.Controls.Add(this.min1_label);
            this.Controls.Add(this.max2_Numeric);
            this.Controls.Add(this.min2_Numeric);
            this.Controls.Add(this.max1_Numeric);
            this.Controls.Add(this.min1_Numeric);
            this.Controls.Add(this.batchNumber_Label);
            this.Controls.Add(this.param2_comboBox);
            this.Controls.Add(this.param2_Label);
            this.Controls.Add(this.param1_Label);
            this.Controls.Add(this.param1_comboBox);
            this.Controls.Add(this.batchNumber_Numeric);
            this.Name = "BatchGenerator";
            this.Text = "BatchGenerator";
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.BatchGenerator_FormClosed);
            this.Load += new System.EventHandler(this.BatchGenerator_Load);
            ((System.ComponentModel.ISupportInitialize)(this.batchNumber_Numeric)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.min1_Numeric)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.max1_Numeric)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.min2_Numeric)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.max2_Numeric)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.NumericUpDown batchNumber_Numeric;
        private System.Windows.Forms.ComboBox param1_comboBox;
        private System.Windows.Forms.Label param1_Label;
        private System.Windows.Forms.Label param2_Label;
        private System.Windows.Forms.ComboBox param2_comboBox;
        private System.Windows.Forms.Label batchNumber_Label;
        private System.Windows.Forms.NumericUpDown min1_Numeric;
        private System.Windows.Forms.NumericUpDown max1_Numeric;
        private System.Windows.Forms.NumericUpDown min2_Numeric;
        private System.Windows.Forms.NumericUpDown max2_Numeric;
        private System.Windows.Forms.Label min1_label;
        private System.Windows.Forms.Label min2_label;
        private System.Windows.Forms.Label max2_label;
        private System.Windows.Forms.Label max1_label;
        private System.Windows.Forms.Button generate_button;
        private System.Windows.Forms.Label info_label;
        private System.Windows.Forms.Label label2;
    }
}