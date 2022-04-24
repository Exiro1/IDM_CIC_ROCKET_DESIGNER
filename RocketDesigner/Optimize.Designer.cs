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
            this.numericUpDownMs = new System.Windows.Forms.NumericUpDown();
            this.label4 = new System.Windows.Forms.Label();
            this.numericSweep = new System.Windows.Forms.NumericUpDown();
            this.label5 = new System.Windows.Forms.Label();
            this.numericSpan = new System.Windows.Forms.NumericUpDown();
            this.label6 = new System.Windows.Forms.Label();
            this.label7 = new System.Windows.Forms.Label();
            this.label8 = new System.Windows.Forms.Label();
            this.numericTip = new System.Windows.Forms.NumericUpDown();
            this.numericChord = new System.Windows.Forms.NumericUpDown();
            this.numericPos = new System.Windows.Forms.NumericUpDown();
            this.label9 = new System.Windows.Forms.Label();
            this.numericTh = new System.Windows.Forms.NumericUpDown();
            this.label10 = new System.Windows.Forms.Label();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownPop)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownKeep)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownEpoch)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownMs)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericSweep)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericSpan)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericTip)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericChord)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericPos)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericTh)).BeginInit();
            this.SuspendLayout();
            // 
            // buttonOptimize
            // 
            this.buttonOptimize.Location = new System.Drawing.Point(39, 136);
            this.buttonOptimize.Name = "buttonOptimize";
            this.buttonOptimize.Size = new System.Drawing.Size(245, 23);
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
            // numericUpDownMs
            // 
            this.numericUpDownMs.DecimalPlaces = 2;
            this.numericUpDownMs.Location = new System.Drawing.Point(164, 90);
            this.numericUpDownMs.Minimum = new decimal(new int[] {
            100,
            0,
            0,
            -2147483648});
            this.numericUpDownMs.Name = "numericUpDownMs";
            this.numericUpDownMs.Size = new System.Drawing.Size(120, 20);
            this.numericUpDownMs.TabIndex = 8;
            this.numericUpDownMs.Value = new decimal(new int[] {
            1,
            0,
            0,
            0});
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(36, 92);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(111, 13);
            this.label4.TabIndex = 7;
            this.label4.Text = "Min Static margin (cal)";
            // 
            // numericSweep
            // 
            this.numericSweep.DecimalPlaces = 2;
            this.numericSweep.Location = new System.Drawing.Point(480, 92);
            this.numericSweep.Minimum = new decimal(new int[] {
            100,
            0,
            0,
            -2147483648});
            this.numericSweep.Name = "numericSweep";
            this.numericSweep.Size = new System.Drawing.Size(120, 20);
            this.numericSweep.TabIndex = 16;
            this.numericSweep.Value = new decimal(new int[] {
            5,
            0,
            0,
            0});
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(352, 94);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(105, 13);
            this.label5.TabIndex = 15;
            this.label5.Text = "Deviation Sweep (%)";
            // 
            // numericSpan
            // 
            this.numericSpan.DecimalPlaces = 2;
            this.numericSpan.Location = new System.Drawing.Point(480, 66);
            this.numericSpan.Name = "numericSpan";
            this.numericSpan.Size = new System.Drawing.Size(120, 20);
            this.numericSpan.TabIndex = 14;
            this.numericSpan.Value = new decimal(new int[] {
            5,
            0,
            0,
            0});
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(352, 68);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(97, 13);
            this.label6.TabIndex = 13;
            this.label6.Text = "Deviation Span (%)";
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Location = new System.Drawing.Point(352, 42);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(114, 13);
            this.label7.TabIndex = 12;
            this.label7.Text = "Deviation Tipchord (%)";
            // 
            // label8
            // 
            this.label8.AutoSize = true;
            this.label8.Location = new System.Drawing.Point(352, 16);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(100, 13);
            this.label8.TabIndex = 11;
            this.label8.Text = "Deviation Chord (%)";
            // 
            // numericTip
            // 
            this.numericTip.DecimalPlaces = 2;
            this.numericTip.Location = new System.Drawing.Point(480, 40);
            this.numericTip.Name = "numericTip";
            this.numericTip.Size = new System.Drawing.Size(120, 20);
            this.numericTip.TabIndex = 10;
            this.numericTip.Value = new decimal(new int[] {
            5,
            0,
            0,
            0});
            // 
            // numericChord
            // 
            this.numericChord.DecimalPlaces = 2;
            this.numericChord.Location = new System.Drawing.Point(480, 14);
            this.numericChord.Name = "numericChord";
            this.numericChord.Size = new System.Drawing.Size(120, 20);
            this.numericChord.TabIndex = 9;
            this.numericChord.Value = new decimal(new int[] {
            5,
            0,
            0,
            0});
            // 
            // numericPos
            // 
            this.numericPos.DecimalPlaces = 2;
            this.numericPos.Location = new System.Drawing.Point(480, 118);
            this.numericPos.Minimum = new decimal(new int[] {
            100,
            0,
            0,
            -2147483648});
            this.numericPos.Name = "numericPos";
            this.numericPos.Size = new System.Drawing.Size(120, 20);
            this.numericPos.TabIndex = 18;
            // 
            // label9
            // 
            this.label9.AutoSize = true;
            this.label9.Location = new System.Drawing.Point(352, 120);
            this.label9.Name = "label9";
            this.label9.Size = new System.Drawing.Size(109, 13);
            this.label9.TabIndex = 17;
            this.label9.Text = "Deviation Position (%)";
            // 
            // numericTh
            // 
            this.numericTh.DecimalPlaces = 2;
            this.numericTh.Location = new System.Drawing.Point(480, 144);
            this.numericTh.Minimum = new decimal(new int[] {
            100,
            0,
            0,
            -2147483648});
            this.numericTh.Name = "numericTh";
            this.numericTh.Size = new System.Drawing.Size(120, 20);
            this.numericTh.TabIndex = 20;
            // 
            // label10
            // 
            this.label10.AutoSize = true;
            this.label10.Location = new System.Drawing.Point(352, 146);
            this.label10.Name = "label10";
            this.label10.Size = new System.Drawing.Size(121, 13);
            this.label10.TabIndex = 19;
            this.label10.Text = "Deviation Thickness (%)";
            // 
            // Optimize
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(664, 174);
            this.Controls.Add(this.numericTh);
            this.Controls.Add(this.label10);
            this.Controls.Add(this.numericPos);
            this.Controls.Add(this.label9);
            this.Controls.Add(this.numericSweep);
            this.Controls.Add(this.label5);
            this.Controls.Add(this.numericSpan);
            this.Controls.Add(this.label6);
            this.Controls.Add(this.label7);
            this.Controls.Add(this.label8);
            this.Controls.Add(this.numericTip);
            this.Controls.Add(this.numericChord);
            this.Controls.Add(this.numericUpDownMs);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.numericUpDownEpoch);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.numericUpDownKeep);
            this.Controls.Add(this.numericUpDownPop);
            this.Controls.Add(this.buttonOptimize);
            this.Name = "Optimize";
            this.Text = "Optimize";
            this.Load += new System.EventHandler(this.Optimize_Load);
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownPop)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownKeep)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownEpoch)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownMs)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericSweep)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericSpan)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericTip)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericChord)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericPos)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericTh)).EndInit();
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
        private System.Windows.Forms.NumericUpDown numericUpDownMs;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.NumericUpDown numericSweep;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.NumericUpDown numericSpan;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.Label label8;
        private System.Windows.Forms.NumericUpDown numericTip;
        private System.Windows.Forms.NumericUpDown numericChord;
        private System.Windows.Forms.NumericUpDown numericPos;
        private System.Windows.Forms.Label label9;
        private System.Windows.Forms.NumericUpDown numericTh;
        private System.Windows.Forms.Label label10;
    }
}