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
        private Random randomFormula = new Random();
        private string[] randomtxtFormulaWords =
        {
            "x+5",
            "x-3",
            "2*x",
            "x/2",
            "x*x",
            "x*x-4",
            "(x+2)*(x-2)",
            "1/x",
            "x*x+2*x+1",
            "x-x/2",
            "x+3*x-7",
            "5-x*x",
            "3*x-2",
            "(x+1)/(x-1)",
            "(x-3)*(x+3)",
            "x*x*x",
            "2*x-5",
            "x/(x+1)",
            "x*x+y*y-1",
            "x+5",
            "sin(x)",
            "x^2",
            "Math.Tan(x) + Math.Sqrt(1 + Math.Pow(x, 2))",
            "Math.Sqrt(Math.Pow(x, 2) + Math.Pow(y, 2))",
            "y = Math.Sqrt(1 - x^2)",
            "Math.Sin(x) + Math.Pow(x, 2)",
            "1/x",
            "x^2+2*x+1",
            "5-x^2",
            "x^3",
            "2*x-5",
            "x/(x+1)"
        };

        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            this.FormBorderStyle = FormBorderStyle.FixedSingle;
            AddCalculatorButtons(panelButtons, txtFormula);

            txtFormula.Text = randomtxtFormulaWords[randomFormula.Next(randomtxtFormulaWords.Length)];

            this.MouseWheel += Form1_MouseWheel;

            var pictureBox = this.Controls["pictureBox"] as PictureBox;
            if (pictureBox != null)
            {
                pictureBox.MouseEnter += (btnSender, args) => Cursor = Cursors.Hand;
                pictureBox.MouseLeave += (btnSender, args) => Cursor = Cursors.Default;

                pictureBox.MouseDown += PictureBox_MouseDown;
                pictureBox.MouseMove += PictureBox_MouseMove;
                pictureBox.MouseUp += PictureBox_MouseUp;
            }
        }

        private void PictureBox_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Middle)
            {
                isDragging = true;
                dragStartPoint = e.Location;
                Cursor = Cursors.SizeAll;
            }
        }

        private void PictureBox_MouseMove(object sender, MouseEventArgs e)
        {
            if (isDragging)
            {
                offsetX += (e.X - dragStartPoint.X) / scale;
                offsetY -= (e.Y - dragStartPoint.Y) / scale;
                dragStartPoint = e.Location;

                DrawGraph(txtFormula.Text);
            }
        }

        private void PictureBox_MouseUp(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Middle)
            {
                isDragging = false;
                Cursor = Cursors.Hand;
            }
        }

        private void Form1_MouseWheel(object sender, MouseEventArgs e)
        {
            double oldScale = scale;
            scale *= e.Delta > 0 ? 1.1 : 0.9;

            double cursorX = (e.X - this.Controls["pictureBox"].Width / 2) / oldScale - offsetX;
            double cursorY = (this.Controls["pictureBox"].Height / 2 - e.Y) / oldScale - offsetY;

            offsetX = cursorX - (e.X - this.Controls["pictureBox"].Width / 2) / scale;
            offsetY = cursorY - (this.Controls["pictureBox"].Height / 2 - e.Y) / scale;

            DrawGraph(txtFormula.Text);
        }

        private void AddCalculatorButtons(Panel panel, TextBox txtFormula)
        {
            string[] buttons = {
                "1", "2", "3", "4",
                "5", "6", "7", "8",
                "9", "0", "+", "-",
                "*", "/", "x", "y",
                "(", ")", "Math.Sin( )", "Math.Cos( )",
                "Math.Tan( )", "Math.Sqrt( )", "^", "C"
            };

            int buttonWidth = 50;
            int buttonHeight = 50;
            int margin = 6;

            for (int i = 0; i < buttons.Length; i++)
            {
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
                    if (button != null)
                    {
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
                using (var g = Graphics.FromImage(bitmap))
                {
                    g.Clear(Color.White);

                    var centerX = pictureBox.Width / 2;
                    var centerY = pictureBox.Height / 2;

                    DrawGrid(g, centerX, centerY, pictureBox.Width, pictureBox.Height);

                    g.DrawLine(Pens.Black, 0, centerY, pictureBox.Width, centerY); // Ось X
                    g.DrawLine(Pens.Black, centerX, 0, centerX, pictureBox.Height); // Ось Y

                    for (int xPixel = 0; xPixel < pictureBox.Width; xPixel++)
                    {
                        double x = (xPixel - centerX) / scale - offsetX;
                        double y = EvaluateFormula(formula, x, offsetY);

                        int yPixel = centerY - (int)((y + offsetY) * scale);

                        if (yPixel >= 0 && yPixel < pictureBox.Height)
                            g.FillEllipse(Brushes.Blue, xPixel, yPixel, 2, 2);
                    }
                }

                pictureBox.Image = bitmap;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void DrawGrid(Graphics g, int centerX, int centerY, int width, int height)
        {
            Pen gridPen = new Pen(Color.LightGray, 1);
            Pen boldGridPen = new Pen(Color.Gray, 1);

            double gridStep = scale;

            for (double x = -offsetX * scale; x < width; x += gridStep)
            {
                int xPixel = (int)(centerX + x);
                g.DrawLine(gridPen, xPixel, 0, xPixel, height);
            }
            for (double x = -offsetX * scale - gridStep; x > -width; x -= gridStep)
            {
                int xPixel = (int)(centerX + x);
                g.DrawLine(gridPen, xPixel, 0, xPixel, height);
            }

            for (double y = offsetY * scale; y < height; y += gridStep)
            {
                int yPixel = (int)(centerY - y);
                g.DrawLine(gridPen, 0, yPixel, width, yPixel);
            }
            for (double y = offsetY * scale - gridStep; y > -height; y -= gridStep)
            {
                int yPixel = (int)(centerY - y);
                g.DrawLine(gridPen, 0, yPixel, width, yPixel);
            }

            for (double x = -offsetX; x < width / scale; x += 1)
            {
                int xPixel = (int)(centerX + x * scale);
                g.DrawString(x.ToString("0"), new Font("Arial", 8), Brushes.Gray, xPixel + 2, centerY + 2);
            }
            for (double y = -offsetY; y < height / scale; y += 1)
            {
                int yPixel = (int)(centerY - y * scale);
                g.DrawString(y.ToString("0"), new Font("Arial", 8), Brushes.Gray, centerX + 2, yPixel - 10);
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
            catch (Exception ex)
            {
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

        private void SaveGraph_Click(object sender, EventArgs e)
        {
            try
            {
                var pictureBox = this.Controls["pictureBox"] as PictureBox;
                if (pictureBox == null || pictureBox.Image == null)
                {
                    MessageBox.Show("График отсутствует!", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                using (var saveFileDialog = new SaveFileDialog())
                {
                    saveFileDialog.Filter = "PNG Image|*.png|JPEG Image|*.jpg|Bitmap Image|*.bmp";
                    saveFileDialog.Title = "Сохранить график";
                    saveFileDialog.FileName = "Graph.png";

                    if (saveFileDialog.ShowDialog() == DialogResult.OK)
                    {
                        var filePath = saveFileDialog.FileName;
                        var format = System.Drawing.Imaging.ImageFormat.Png;

                        if (filePath.EndsWith(".jpg"))
                            format = System.Drawing.Imaging.ImageFormat.Jpeg;
                        else if (filePath.EndsWith(".bmp"))
                            format = System.Drawing.Imaging.ImageFormat.Bmp;

                        pictureBox.Image.Save(filePath, format);
                        MessageBox.Show("График успешно сохранён!", "Успех", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при сохранении: {ex.Message}", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}
