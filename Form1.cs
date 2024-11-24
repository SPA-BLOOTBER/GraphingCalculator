using System;
using System.Drawing;
using System.Windows.Forms;
using System.Data;

namespace GraphingCalculator
{
    public partial class Form1 : Form
    {
        private double offsetX = 0;
        private double offsetY = 0;

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
                Height = 500,
                AutoScroll = true 
            };
            this.Controls.Add(panelButtons);

            AddCalculatorButtons(panelButtons, txtFormula);

            this.MouseWheel += Form1_MouseWheel;

            var pictureBox = this.Controls["pictureBox"] as PictureBox;
            if (pictureBox != null)
            {
                pictureBox.MouseEnter += (btnSender, args) => Cursor = Cursors.Hand;
                pictureBox.MouseLeave += (btnSender, args) => Cursor = Cursors.Default;
            }
        }

        private void Form1_MouseWheel(object sender, MouseEventArgs e)
        {
            if (e.Delta > 0)
            {
                offsetY += 1;
            }
            else
            {
                offsetY -= 1;
            }

            DrawGraph(txtFormula.Text);
        }

        private void AddCalculatorButtons(Panel panel, TextBox txtFormula)
        {
            string[] buttons = {
                "1", "2", "3", "4",
                "5", "6", "7", "8",
                "9", "0", "+", "-",
                "*", "/", "x", ".",
                "(", ")", "sin", "cos",
                "tan", "sqrt", "^", "C"
            };

            int buttonWidth = 60;
            int buttonHeight = 50;
            int margin = 5;

            panel.AutoScroll = true;

            for (int i = 0; i < buttons.Length; i++)
            {
                var btn = new Button
                {
                    Text = buttons[i],
                    Width = buttonWidth,
                    Height = buttonHeight,
                    Left = (i % 10) * (buttonWidth + margin),
                    Top = (i / 10) * (buttonHeight + margin)
                };

                btn.Click += (btnSender, args) =>
                {
                    var button = btnSender as Button;
                    if (button != null)
                    {
                        if (button.Text == "C")
                        {
                            txtFormula.Text = "";
                        }
                        else
                        {
                            txtFormula.Text += button.Text;
                        }
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

                // Bitmap график
                var bitmap = new Bitmap(pictureBox.Width, pictureBox.Height);
                using (var g = Graphics.FromImage(bitmap))
                {
                    g.Clear(Color.White);

                    var centerX = pictureBox.Width / 2;
                    var centerY = pictureBox.Height / 2;

                    g.DrawLine(Pens.Black, 0, centerY, pictureBox.Width, centerY); // Ось X
                    g.DrawLine(Pens.Black, centerX, 0, centerX, pictureBox.Height); // Ось Y

                    g.DrawLine(Pens.Red, centerX + (int)(0 * 10), centerY, centerX + (int)(1 * 10), centerY);

                    g.DrawString("1", new Font("Arial", 10), Brushes.Red, centerX + 10, centerY - 10);

                    double scale = 10.0;

                    Point? prevPoint = null;

                    for (int xPixel = 0; xPixel < pictureBox.Width; xPixel++)
                    {
                        // Применяем смещение по оси X
                        double x = (xPixel - centerX - offsetX) / scale;

                        var y = EvaluateFormula(formula, x);

                        // Применяем смещение по оси Y
                        int yPixel = centerY - (int)((y + offsetY) * scale);

                        if (yPixel >= 0 && yPixel < pictureBox.Height)
                        {
                            var currentPoint = new Point(xPixel, yPixel);

                            if (prevPoint.HasValue)
                            {
                                g.DrawLine(Pens.Blue, prevPoint.Value, currentPoint);
                            }

                            prevPoint = currentPoint;
                        }
                        else
                        {
                            prevPoint = null;
                        }
                    }
                }

                pictureBox.Image = bitmap;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private double EvaluateFormula(string formula, double x)
        {
            try
            {
                formula = formula.Replace("x", x.ToString(System.Globalization.CultureInfo.InvariantCulture));

                var dataTable = new DataTable();
                dataTable.Columns.Add("expression", typeof(string), formula);
                var row = dataTable.NewRow();
                dataTable.Rows.Add(row);

                return Convert.ToDouble(row["expression"]); //Конверт строк. "expression" обдж. row стр. бд. в double.
            }
            catch (Exception ex)
            {
                throw new Exception($"Ошибка в формуле: {ex.Message}");
            }
        }

        private void btnPlot_Click(object sender, EventArgs e)
        {
            DrawGraph(txtFormula.Text);
        }
    }
}
