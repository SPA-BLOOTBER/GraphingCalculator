//Примеры уравнений для построения графкиов
/*
 *
*/


using System;
using System.Data;
using System.Drawing;
using System.Globalization;
using System.Linq.Expressions;
using System.Windows.Forms;

namespace GraphingCalculator
{
    public partial class Form1 : Form
    {
        double offsetX = 0;
        double offsetY = 0;
        double scale = 10;
        private bool isDragging = false;
        private Point dragStartPoint;

        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            var panelButtons = new Panel 
            {
                Left = 10,
                Top = 560,
                Width = 950,
                Height = 100,
                AutoScroll = true
            };
            this.Controls.Add(panelButtons);

            AddCalculatorButtons(panelButtons, txtFormula);

            this.MouseWheel += Form1_MouseWheel;

            var pictureBox = this.Controls["pictureBox"] as PictureBox;
            if (pictureBox != null) {
                pictureBox.MouseEnter += (btnSender, args) => Cursor = Cursors.Hand;
                pictureBox.MouseLeave += (btnSender, args) => Cursor = Cursors.Default;

                pictureBox.MouseDown += PictureBox_MouseDown;
                pictureBox.MouseMove += PictureBox_MouseMove;
                pictureBox.MouseUp += PictureBox_MouseUp;
            }
        }

        private void PictureBox_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Middle) {
                isDragging = true;
                dragStartPoint = e.Location;
                Cursor = Cursors.SizeAll;
            }
        }

        private void PictureBox_MouseMove(object sender, MouseEventArgs e)
        {
            if (isDragging) {
                offsetX += (e.X - dragStartPoint.X) / scale;
                offsetY -= (e.Y - dragStartPoint.Y) / scale;
                dragStartPoint = e.Location;

                DrawGraph(txtFormula.Text);
            }
        }

        private void PictureBox_MouseUp(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Middle) {
                isDragging = false;
                Cursor = Cursors.Hand;
            }
        }

        private void Form1_MouseWheel(object sender, MouseEventArgs e)
        {
            scale *= e.Delta > 0 ? 1.1 : 0.9;
            DrawGraph(txtFormula.Text);
        }

        private void AddCalculatorButtons(Panel panel, TextBox txtFormula)
        {
            string[] buttons = {
                "1", "2", "3", "4",
                "5", "6", "7", "8",
                "9", "0", "+", "-",
                "*", "/", "x", "y",
                "(", ")", "sin", "cos",
                "tan", "sqrt", "^", "C"
            };

            int buttonWidth = 50;
            int buttonHeight = 50;
            int margin = 6;

            for (int i = 0; i < buttons.Length; i++) {
                var btn = new Button
                {
                    Text = buttons[i],
                    Width = buttonWidth,
                    Height = buttonHeight,
                    Left = (i % 12) * (buttonWidth + margin),
                    Top = (i / 12) * (buttonHeight + margin)
                };

                btn.Click += (btnSender, args) =>
                {
                    var button = btnSender as Button;
                    if (button != null) {
                        if (button.Text == "C")
                            txtFormula.Text = "";
                        else
                            txtFormula.Text += button.Text;
                    }
                };

                panel.Controls.Add(btn);
            }
        }

        private void DrawGraph(string formula)
        {
            try
            {
                var pictureBox = this.Controls["pictureBox"] as PictureBox;
                if (pictureBox == null) return;

                var bitmap = new Bitmap(pictureBox.Width, pictureBox.Height);
                using (var g = Graphics.FromImage(bitmap)) {
                    g.Clear(Color.White);

                    var centerX = pictureBox.Width / 2;
                    var centerY = pictureBox.Height / 2;

                    g.DrawLine(Pens.Black, 0, centerY, pictureBox.Width, centerY);
                    g.DrawLine(Pens.Black, centerX, 0, centerX, pictureBox.Height);

                    g.DrawLine(Pens.Red, centerX + (int)(0 * scale), centerY, centerX + (int)(1 * scale), centerY);

                    g.DrawString("1", new Font("Arial", 10), Brushes.Red, centerX + 10, centerY - 10);

                    for (int xPixel = 0; xPixel < pictureBox.Width; xPixel++) {
                        double x = (xPixel - centerX) / scale - offsetX;
                        double y = EvaluateFormula(formula, x, offsetY);

                        int yPixel = centerY - (int)((y + offsetY) * scale);

                        if (yPixel >= 0 && yPixel < pictureBox.Height)
                            g.FillEllipse(Brushes.Blue, xPixel, yPixel, 2, 2);
                    }
                }

                pictureBox.Image = bitmap;
            }
            catch (Exception ex) {
                MessageBox.Show($"Error: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private double EvaluateFormula(string formula, double x, double y)
        {
            try
            {
                formula = formula.Replace("x", $"({x.ToString(System.Globalization.CultureInfo.InvariantCulture)})")
                                 .Replace("y", $"({y.ToString(System.Globalization.CultureInfo.InvariantCulture)})");

                formula = HandlePowerOperator(formula);

                formula = formula.Replace("sin", "Math.Sin")
                                 .Replace("cos", "Math.Cos")
                                 .Replace("tan", "Math.Tan")
                                 .Replace("sqrt", "Math.Sqrt");

                if (formula.Contains("Math.Sin") && !formula.Contains("("))
                    formula = formula.Replace("Math.Sin", $"Math.Sin({x})");
                if (formula.Contains("Math.Cos") && !formula.Contains("("))
                    formula = formula.Replace("Math.Cos", $"Math.Cos({x})");
                if (formula.Contains("Math.Tan") && !formula.Contains("("))
                    formula = formula.Replace("Math.Tan", $"Math.Tan({x})");

                var dataTable = new DataTable();
                dataTable.Columns.Add("expression", typeof(string), formula);
                var row = dataTable.NewRow();
                dataTable.Rows.Add(row);

                return Convert.ToDouble(row["expression"]);
            }
            catch (Exception ex) {
                throw new Exception($"Ошибка в формуле: {ex.Message}");
            }
        }

        private string HandlePowerOperator(string formula)
        {
            int index;
            while ((index = formula.IndexOf("^")) >= 0)
            {
                int baseStart = index - 1;
                while (baseStart >= 0 && (char.IsDigit(formula[baseStart]) || formula[baseStart] == ')'))
                    baseStart--;

                int exponentEnd = index + 1;
                while (exponentEnd < formula.Length && (char.IsDigit(formula[exponentEnd]) || formula[exponentEnd] == '('))
                    exponentEnd++;

                string basePart = formula.Substring(baseStart + 1, index - baseStart - 1);
                string exponentPart = formula.Substring(index + 1, exponentEnd - index - 1);

                formula = formula.Substring(0, baseStart + 1) +
                          $"Math.Pow({basePart},{exponentPart})" +
                          formula.Substring(exponentEnd);
            }

            return formula;
        }


        private void btnPlot_Click(object sender, EventArgs e)
        {
            DrawGraph(txtFormula.Text);
        }
    }
}