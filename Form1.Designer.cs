namespace GraphingCalculator
{
    partial class Form1
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
            this.txtFormula = new System.Windows.Forms.TextBox();
            this.btnPlot = new System.Windows.Forms.Button();
            this.pictureBox = new System.Windows.Forms.PictureBox();
            this.panelButtons = new System.Windows.Forms.Panel();
            this.saveFileDialog1 = new System.Windows.Forms.SaveFileDialog();
            this.SaveGraph = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox)).BeginInit();
            this.SuspendLayout();
            // 
            // txtFormula
            // 
            this.txtFormula.BackColor = System.Drawing.Color.YellowGreen;
            this.txtFormula.Font = new System.Drawing.Font("Microsoft Sans Serif", 15F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.txtFormula.Location = new System.Drawing.Point(10, 10);
            this.txtFormula.Name = "txtFormula";
            this.txtFormula.Size = new System.Drawing.Size(446, 30);
            this.txtFormula.TabIndex = 0;
            this.txtFormula.Text = "2+2*2";
            // 
            // btnPlot
            // 
            this.btnPlot.BackColor = System.Drawing.Color.Chocolate;
            this.btnPlot.Font = new System.Drawing.Font("Microsoft Sans Serif", 13F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.btnPlot.Location = new System.Drawing.Point(462, 10);
            this.btnPlot.Name = "btnPlot";
            this.btnPlot.Size = new System.Drawing.Size(128, 30);
            this.btnPlot.TabIndex = 1;
            this.btnPlot.Text = "Нарисовать";
            this.btnPlot.UseVisualStyleBackColor = false;
            this.btnPlot.Click += new System.EventHandler(this.btnPlot_Click);
            // 
            // pictureBox
            // 
            this.pictureBox.BackColor = System.Drawing.Color.White;
            this.pictureBox.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.pictureBox.Location = new System.Drawing.Point(5, 46);
            this.pictureBox.Name = "pictureBox";
            this.pictureBox.Size = new System.Drawing.Size(708, 460);
            this.pictureBox.TabIndex = 2;
            this.pictureBox.TabStop = false;
            // 
            // panelButtons
            // 
            this.panelButtons.AutoScroll = true;
            this.panelButtons.Location = new System.Drawing.Point(5, 512);
            this.panelButtons.Name = "panelButtons";
            this.panelButtons.Size = new System.Drawing.Size(708, 127);
            this.panelButtons.TabIndex = 3;
            // 
            // SaveGraph
            // 
            this.SaveGraph.BackColor = System.Drawing.Color.Chocolate;
            this.SaveGraph.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.SaveGraph.Location = new System.Drawing.Point(596, 10);
            this.SaveGraph.Name = "SaveGraph";
            this.SaveGraph.Size = new System.Drawing.Size(117, 30);
            this.SaveGraph.TabIndex = 5;
            this.SaveGraph.Text = "Сохранить";
            this.SaveGraph.UseVisualStyleBackColor = false;
            this.SaveGraph.Click += new System.EventHandler(this.SaveGraph_Click);
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(715, 641);
            this.Controls.Add(this.SaveGraph);
            this.Controls.Add(this.panelButtons);
            this.Controls.Add(this.pictureBox);
            this.Controls.Add(this.btnPlot);
            this.Controls.Add(this.txtFormula);
            this.Name = "Form1";
            this.Text = "Создание графиков";
            this.Load += new System.EventHandler(this.Form1_Load);
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TextBox txtFormula;
        private System.Windows.Forms.Button btnPlot;
        private System.Windows.Forms.PictureBox pictureBox;
        private System.Windows.Forms.Panel panelButtons;
        private System.Windows.Forms.SaveFileDialog saveFileDialog1;
        private System.Windows.Forms.Button SaveGraph;
    }
}