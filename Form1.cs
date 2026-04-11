using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using OxyPlot;
using OxyPlot.Axes;
using OxyPlot.Series;
using OxyPlot.WindowsForms;
using org.mariuszgromada.math.mxparser;

namespace Mathway
{
    public partial class Form1 : Form
    {
        private const double DEFAULT_X_MIN = -10.0;
        private const double DEFAULT_X_MAX = 10.0;
        private const double DEFAULT_X_STEP = 0.1;
        private const int DEFAULT_A_VALUE = 1;
        private const int DEFAULT_B_VALUE = 0;
        private const string DEFAULT_FUNCTION = "y=sin(x)";
        private const int TEXT_INPUT_DEBOUNCE_MS = 450;
        private const int PARAMETER_DEBOUNCE_MS = 150;
        private const int MAX_RENDER_POINTS = 20000;

        private static readonly string[] PRESET_FUNCTIONS =
        {
            "y=sin(x)",
            "y=cos(x)",
            "y=a*x+b",
            "y=x^2-4*x+2",
            "y=sin(a*x)+b",
            "y=sqrt(abs(x))",
            "y=exp(-x^2/5)",
            "y=1/x",
            "y=abs(x)"
        };

        private readonly Timer _inputDebounceTimer;
        private readonly ToolTip _toolTip;

        private bool _suppressAutoUpdate;

        private ComboBox _presetComboBox;
        private Label _presetLabel;
        private CheckBox _autoUpdateCheckBox;
        private Button _fitButton;
        private Button _exportButton;
        private StatusStrip _statusStrip;
        private ToolStripStatusLabel _statusTextLabel;
        private ToolStripStatusLabel _statusStatsLabel;

        public Form1()
        {
            InitializeComponent();

            _inputDebounceTimer = new Timer();
            _inputDebounceTimer.Interval = TEXT_INPUT_DEBOUNCE_MS;
            _inputDebounceTimer.Tick += InputDebounceTimer_Tick;

            _toolTip = new ToolTip();

            ConfigureMathParser();
            ConfigureStaticControls();
            CreateAdditionalControls();
            WireEvents();
            ApplyDefaults();
            RelayoutControls();
            TryUpdatePlot(false);
        }

        private void ConfigureMathParser()
        {
            mXparser.setRadiansMode();
            mXparser.enableImpliedMultiplicationMode();
            mXparser.enableAttemptToFixExpStrMode();
        }

        private void ConfigureStaticControls()
        {
            Text = "Графический калькулятор";
            AcceptButton = plotButton;

            controlsPanel.AutoScroll = true;

            sliderA.Minimum = -20;
            sliderA.Maximum = 20;
            sliderA.SmallChange = 1;
            sliderA.LargeChange = 1;
            sliderA.TickFrequency = 2;

            sliderB.Minimum = -20;
            sliderB.Maximum = 20;
            sliderB.SmallChange = 1;
            sliderB.LargeChange = 1;
            sliderB.TickFrequency = 2;

            xMinNumeric.Minimum = -10000;
            xMinNumeric.Maximum = 10000;
            xMinNumeric.DecimalPlaces = 2;
            xMinNumeric.Increment = 0.5m;

            xMaxNumeric.Minimum = -10000;
            xMaxNumeric.Maximum = 10000;
            xMaxNumeric.DecimalPlaces = 2;
            xMaxNumeric.Increment = 0.5m;

            xStepNumeric.Minimum = 0.001m;
            xStepNumeric.Maximum = 100;
            xStepNumeric.DecimalPlaces = 3;
            xStepNumeric.Increment = 0.01m;

            plotButton.Text = "Построить";
            resetButton.Text = "Сбросить";

            plotView.BackColor = Color.White;
            plotView.Model = CreateEmptyPlotModel("График функции");

            _toolTip.SetToolTip(inputTextBox, "Примеры: y=sin(x), y=2x+3, y=sqrt(abs(x)), y=exp(-x^2)");
            _toolTip.SetToolTip(sliderA, "Параметр a можно использовать внутри формулы.");
            _toolTip.SetToolTip(sliderB, "Параметр b можно использовать внутри формулы.");
            _toolTip.SetToolTip(xStepNumeric, "Чем меньше шаг, тем точнее график и выше нагрузка.");
        }

        private void CreateAdditionalControls()
        {
            _presetLabel = new Label
            {
                AutoSize = true,
                Name = "presetLabel",
                Text = "Пресеты:"
            };

            _presetComboBox = new ComboBox
            {
                DropDownStyle = ComboBoxStyle.DropDownList,
                Name = "presetComboBox"
            };
            _presetComboBox.Items.AddRange(PRESET_FUNCTIONS);

            _autoUpdateCheckBox = new CheckBox
            {
                AutoSize = true,
                Name = "autoUpdateCheckBox",
                Text = "Автообновление"
            };

            _fitButton = new Button
            {
                Name = "fitButton",
                Text = "Подогнать вид",
                UseVisualStyleBackColor = true
            };

            _exportButton = new Button
            {
                Name = "exportButton",
                Text = "Экспорт PNG",
                UseVisualStyleBackColor = true
            };

            _statusTextLabel = new ToolStripStatusLabel
            {
                Spring = true,
                TextAlign = ContentAlignment.MiddleLeft
            };

            _statusStatsLabel = new ToolStripStatusLabel
            {
                TextAlign = ContentAlignment.MiddleRight
            };

            _statusStrip = new StatusStrip
            {
                Name = "statusStrip",
                SizingGrip = false
            };
            _statusStrip.Items.Add(_statusTextLabel);
            _statusStrip.Items.Add(_statusStatsLabel);

            controlsPanel.Controls.Add(_presetLabel);
            controlsPanel.Controls.Add(_presetComboBox);
            controlsPanel.Controls.Add(_autoUpdateCheckBox);
            controlsPanel.Controls.Add(_fitButton);
            controlsPanel.Controls.Add(_exportButton);

            Controls.Add(_statusStrip);
        }

        private void WireEvents()
        {
            sliderA.ValueChanged += Parameter_Changed;
            sliderB.ValueChanged += Parameter_Changed;
            xMinNumeric.ValueChanged += Parameter_Changed;
            xMaxNumeric.ValueChanged += Parameter_Changed;
            xStepNumeric.ValueChanged += Parameter_Changed;
            inputTextBox.TextChanged += InputTextBox_TextChanged_Delayed;

            plotButton.Click += PlotButton_Click;
            resetButton.Click += ResetButton_Click;
            _fitButton.Click += FitButton_Click;
            _exportButton.Click += ExportButton_Click;
            _autoUpdateCheckBox.CheckedChanged += AutoUpdateCheckBox_CheckedChanged;
            _presetComboBox.SelectedIndexChanged += PresetComboBox_SelectedIndexChanged;
            controlsPanel.SizeChanged += ControlsPanel_SizeChanged;
        }

        private void ApplyDefaults()
        {
            RunWithoutAutoUpdate(delegate
            {
                inputTextBox.Text = DEFAULT_FUNCTION;
                sliderA.Value = DEFAULT_A_VALUE;
                sliderB.Value = DEFAULT_B_VALUE;
                xMinNumeric.Value = (decimal)DEFAULT_X_MIN;
                xMaxNumeric.Value = (decimal)DEFAULT_X_MAX;
                xStepNumeric.Value = (decimal)DEFAULT_X_STEP;
                _autoUpdateCheckBox.Checked = true;
                _presetComboBox.SelectedItem = DEFAULT_FUNCTION;
                UpdateSliderLabels();
            });
        }

        private void ControlsPanel_SizeChanged(object sender, EventArgs e)
        {
            RelayoutControls();
        }

        private void RelayoutControls()
        {
            int left = 15;
            int width = Math.Max(200, controlsPanel.ClientSize.Width - (left * 2));
            int y = 10;

            PositionLabel(labelInput, left, y);
            y += labelInput.Height + 4;
            PositionControl(inputTextBox, left, y, width, inputTextBox.Height);
            y += inputTextBox.Height + 12;

            PositionLabel(labelXmin, left, y);
            y += labelXmin.Height + 4;
            PositionControl(xMinNumeric, left, y, width, xMinNumeric.Height);
            y += xMinNumeric.Height + 10;

            PositionLabel(labelXmax, left, y);
            y += labelXmax.Height + 4;
            PositionControl(xMaxNumeric, left, y, width, xMaxNumeric.Height);
            y += xMaxNumeric.Height + 10;

            PositionLabel(labelXstep, left, y);
            y += labelXstep.Height + 4;
            PositionControl(xStepNumeric, left, y, width, xStepNumeric.Height);
            y += xStepNumeric.Height + 12;

            PositionLabel(_presetLabel, left, y);
            y += _presetLabel.Height + 4;
            PositionControl(_presetComboBox, left, y, width, _presetComboBox.Height);
            y += _presetComboBox.Height + 12;

            PositionLabel(labelSliderA, left, y);
            y += labelSliderA.Height + 2;
            PositionControl(sliderA, left, y, width, sliderA.Height);
            y += sliderA.Height - 5;
            PositionLabel(cofALabel, left, y);
            y += cofALabel.Height + 10;

            PositionLabel(labelSliderB, left, y);
            y += labelSliderB.Height + 2;
            PositionControl(sliderB, left, y, width, sliderB.Height);
            y += sliderB.Height - 5;
            PositionLabel(cofBLabel, left, y);
            y += cofBLabel.Height + 12;

            PositionControl(_autoUpdateCheckBox, left, y, width, _autoUpdateCheckBox.Height);
            y += _autoUpdateCheckBox.Height + 10;

            PositionControl(plotButton, left, y, width, plotButton.Height);
            y += plotButton.Height + 8;

            int halfWidth = (width - 8) / 2;
            PositionControl(_fitButton, left, y, halfWidth, _fitButton.Height);
            PositionControl(_exportButton, left + halfWidth + 8, y, width - halfWidth - 8, _exportButton.Height);
            y += Math.Max(_fitButton.Height, _exportButton.Height) + 8;

            PositionControl(resetButton, left, y, width, resetButton.Height);
            controlsPanel.AutoScrollMinSize = new Size(0, resetButton.Bottom + 20);
        }

        private static void PositionLabel(Control control, int left, int top)
        {
            control.Location = new Point(left, top);
        }

        private static void PositionControl(Control control, int left, int top, int width, int height)
        {
            control.Location = new Point(left, top);
            control.Size = new Size(width, height);
        }

        private void PlotButton_Click(object sender, EventArgs e)
        {
            TryUpdatePlot(true);
        }

        private void FitButton_Click(object sender, EventArgs e)
        {
            TryUpdatePlot(false);
        }

        private void ResetButton_Click(object sender, EventArgs e)
        {
            ApplyDefaults();
            TryUpdatePlot(false);
        }

        private void ExportButton_Click(object sender, EventArgs e)
        {
            if (plotView.Model == null || plotView.Model.Series.Count == 0)
            {
                MessageBox.Show(this, "Сначала постройте график.", "Экспорт", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            using (SaveFileDialog saveDialog = new SaveFileDialog())
            {
                saveDialog.Filter = "PNG (*.png)|*.png";
                saveDialog.Title = "Сохранить график";
                saveDialog.FileName = BuildExportFileName(inputTextBox.Text);

                if (saveDialog.ShowDialog(this) != DialogResult.OK)
                {
                    return;
                }

                try
                {
                    int width = Math.Max(plotView.Width, 1400);
                    int height = Math.Max(plotView.Height, 900);
                    PngExporter.Export(plotView.Model, saveDialog.FileName, width, height, 96);
                    SetStatus("График сохранен в PNG.", StatusTone.Success, Path.GetFileName(saveDialog.FileName));
                }
                catch (Exception ex)
                {
                    MessageBox.Show(this, "Не удалось сохранить изображение: " + ex.Message, "Экспорт", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void AutoUpdateCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            if (_suppressAutoUpdate)
            {
                return;
            }

            if (_autoUpdateCheckBox.Checked)
            {
                ScheduleAutoUpdate(PARAMETER_DEBOUNCE_MS);
            }
            else
            {
                _inputDebounceTimer.Stop();
                SetStatus("Автообновление выключено. Используйте кнопку \"Построить\".", StatusTone.Info, string.Empty);
            }
        }

        private void PresetComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (_suppressAutoUpdate || _presetComboBox.SelectedItem == null)
            {
                return;
            }

            RunWithoutAutoUpdate(delegate
            {
                inputTextBox.Text = _presetComboBox.SelectedItem.ToString();
            });

            TryUpdatePlot(false);
        }

        private void Parameter_Changed(object sender, EventArgs e)
        {
            UpdateSliderLabels();
            ScheduleAutoUpdate(PARAMETER_DEBOUNCE_MS);
        }

        private void UpdateSliderLabels()
        {
            cofALabel.Text = string.Format(CultureInfo.CurrentCulture, "a: {0}", sliderA.Value);
            cofBLabel.Text = string.Format(CultureInfo.CurrentCulture, "b: {0}", sliderB.Value);
        }

        private void InputTextBox_TextChanged_Delayed(object sender, EventArgs e)
        {
            ScheduleAutoUpdate(TEXT_INPUT_DEBOUNCE_MS);
        }

        private void InputDebounceTimer_Tick(object sender, EventArgs e)
        {
            _inputDebounceTimer.Stop();
            TryUpdatePlot(false);
        }

        private void ScheduleAutoUpdate(int intervalMs)
        {
            if (_suppressAutoUpdate)
            {
                return;
            }

            if (!_autoUpdateCheckBox.Checked)
            {
                _inputDebounceTimer.Stop();
                SetStatus("Изменения готовы. Нажмите \"Построить\" для обновления графика.", StatusTone.Info, string.Empty);
                return;
            }

            _inputDebounceTimer.Interval = intervalMs;
            _inputDebounceTimer.Stop();
            _inputDebounceTimer.Start();
        }

        private bool TryUpdatePlot(bool showMessageBoxOnError)
        {
            _inputDebounceTimer.Stop();

            PlotAttemptResult result = BuildPlotAttemptResult();
            if (!result.Success)
            {
                if (result.ShouldClearPlot)
                {
                    plotView.Model = CreateEmptyPlotModel("График функции");
                    plotView.InvalidatePlot(true);
                }

                SetStatus(result.Message, StatusTone.Error, result.StatsText);

                if (showMessageBoxOnError)
                {
                    MessageBox.Show(this, result.Message, "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }

                return false;
            }

            plotView.Model = CreatePlotModel(result);
            plotView.InvalidatePlot(true);
            SetStatus(BuildSuccessMessage(result), result.WarningMessage == null ? StatusTone.Success : StatusTone.Warning, result.StatsText);
            return true;
        }

        private PlotAttemptResult BuildPlotAttemptResult()
        {
            PlotAttemptResult result = new PlotAttemptResult();
            string userInput = inputTextBox.Text.Trim();

            if (string.IsNullOrWhiteSpace(userInput))
            {
                result.Message = "Введите функцию для построения графика.";
                result.ShouldClearPlot = true;
                return result;
            }

            double xMin = (double)xMinNumeric.Value;
            double xMax = (double)xMaxNumeric.Value;
            double requestedStep = (double)xStepNumeric.Value;

            if (xMin >= xMax)
            {
                result.Message = "Xmin должен быть меньше Xmax.";
                return result;
            }

            if (requestedStep <= 0)
            {
                result.Message = "Шаг X должен быть положительным.";
                return result;
            }

            string expressionText = NormalizeExpression(userInput);
            if (string.IsNullOrWhiteSpace(expressionText))
            {
                result.Message = "Выражение функции не может быть пустым.";
                return result;
            }

            double effectiveStep = requestedStep;
            int sampleCount = CalculateSampleCount(xMin, xMax, effectiveStep);
            string warningMessage = null;

            if (sampleCount > MAX_RENDER_POINTS)
            {
                effectiveStep = (xMax - xMin) / (MAX_RENDER_POINTS - 1);
                sampleCount = MAX_RENDER_POINTS;
                warningMessage = string.Format(
                    CultureInfo.CurrentCulture,
                    "Шаг автоматически увеличен до {0}, чтобы не перегружать построение.",
                    FormatNumber(effectiveStep));
            }

            Expression expression = CreateExpression(expressionText, sliderA.Value, sliderB.Value, xMin);
            if (!expression.checkSyntax())
            {
                result.Message = BuildParserErrorMessage(expression);
                return result;
            }

            List<DataPoint> points = new List<DataPoint>(sampleCount + 16);
            double yMin = double.PositiveInfinity;
            double yMax = double.NegativeInfinity;
            int validPointCount = 0;
            bool previousPointValid = false;
            bool hasDiscontinuities = false;

            for (int i = 0; i < sampleCount; i++)
            {
                double currentX = i == sampleCount - 1 ? xMax : xMin + (i * effectiveStep);
                expression.setArgumentValue("x", currentX);

                double y = expression.calculate();
                if (double.IsNaN(y) || double.IsInfinity(y))
                {
                    if (previousPointValid)
                    {
                        points.Add(DataPoint.Undefined);
                        previousPointValid = false;
                        hasDiscontinuities = true;
                    }

                    continue;
                }

                points.Add(new DataPoint(currentX, y));
                validPointCount++;
                previousPointValid = true;
                yMin = Math.Min(yMin, y);
                yMax = Math.Max(yMax, y);
            }

            if (validPointCount == 0)
            {
                result.Message = "Не удалось построить график. Проверьте выражение или диапазон значений X.";
                result.StatsText = string.Format(
                    CultureInfo.CurrentCulture,
                    "X: [{0}; {1}] | Шаг: {2}",
                    FormatNumber(xMin),
                    FormatNumber(xMax),
                    FormatNumber(effectiveStep));
                return result;
            }

            result.Success = true;
            result.DisplayExpression = expressionText;
            result.XMin = xMin;
            result.XMax = xMax;
            result.YMin = yMin;
            result.YMax = yMax;
            result.Step = effectiveStep;
            result.Points = points;
            result.WarningMessage = warningMessage;
            result.HasDiscontinuities = hasDiscontinuities;
            result.StatsText = string.Format(
                CultureInfo.CurrentCulture,
                "Точек: {0} | Шаг: {1} | Y: [{2}; {3}]",
                validPointCount,
                FormatNumber(effectiveStep),
                FormatNumber(yMin),
                FormatNumber(yMax));

            return result;
        }

        private static Expression CreateExpression(string expressionText, double aValue, double bValue, double initialX)
        {
            Argument xArgument = new Argument("x", initialX);
            Constant aConstant = new Constant("a", aValue);
            Constant bConstant = new Constant("b", bValue);
            Expression expression = new Expression(expressionText, xArgument, aConstant, bConstant);
            expression.setSilentMode();
            expression.enableImpliedMultiplicationMode();
            return expression;
        }

        private static string NormalizeExpression(string userInput)
        {
            string normalized = Regex.Replace(userInput ?? string.Empty, @"^\s*y\s*=\s*", string.Empty, RegexOptions.IgnoreCase);
            normalized = normalized.Trim().ToLowerInvariant();
            normalized = normalized.Replace('−', '-');
            normalized = Regex.Replace(normalized, @"(?<=\d),(?=\d)", ".");
            return normalized;
        }

        private static int CalculateSampleCount(double xMin, double xMax, double xStep)
        {
            double samples = Math.Ceiling((xMax - xMin) / xStep) + 1;
            return Math.Max(2, (int)samples);
        }

        private static string BuildParserErrorMessage(Expression expression)
        {
            string parserMessage = expression.getErrorMessage() ?? string.Empty;

            if (parserMessage.IndexOf("Invalid token", StringComparison.OrdinalIgnoreCase) >= 0)
            {
                return "Обнаружена неизвестная функция или переменная. Используйте x, a, b и стандартные функции вроде sin(x), cos(x), sqrt(x).";
            }

            if (parserMessage.IndexOf("lexical error", StringComparison.OrdinalIgnoreCase) >= 0)
            {
                return "Формула содержит синтаксическую ошибку. Проверьте скобки, операторы и разделители.";
            }

            return "Формула содержит синтаксическую ошибку. Проверьте выражение.";
        }

        private PlotModel CreatePlotModel(PlotAttemptResult result)
        {
            PlotModel model = new PlotModel
            {
                Title = "График: y = " + result.DisplayExpression,
                Subtitle = string.Format(CultureInfo.CurrentCulture, "a = {0}, b = {1}", sliderA.Value, sliderB.Value),
                PlotAreaBorderColor = OxyColors.SlateGray
            };

            LinearAxis xAxis = CreateAxis(AxisPosition.Bottom, "X");
            xAxis.Minimum = result.XMin;
            xAxis.Maximum = result.XMax;

            LinearAxis yAxis = CreateAxis(AxisPosition.Left, "Y");
            ApplyVerticalRange(yAxis, result.YMin, result.YMax);

            model.Axes.Add(xAxis);
            model.Axes.Add(yAxis);

            LineSeries series = new LineSeries
            {
                Color = OxyColors.DodgerBlue,
                StrokeThickness = 2.2,
                MarkerType = MarkerType.None
            };
            series.Points.AddRange(result.Points);
            model.Series.Add(series);

            return model;
        }

        private static PlotModel CreateEmptyPlotModel(string title)
        {
            PlotModel model = new PlotModel
            {
                Title = title,
                Subtitle = "Введите функцию и нажмите \"Построить\".",
                PlotAreaBorderColor = OxyColors.SlateGray
            };

            LinearAxis xAxis = CreateAxis(AxisPosition.Bottom, "X");
            xAxis.Minimum = DEFAULT_X_MIN;
            xAxis.Maximum = DEFAULT_X_MAX;

            LinearAxis yAxis = CreateAxis(AxisPosition.Left, "Y");
            yAxis.Minimum = -2;
            yAxis.Maximum = 2;

            model.Axes.Add(xAxis);
            model.Axes.Add(yAxis);

            return model;
        }

        private static LinearAxis CreateAxis(AxisPosition position, string title)
        {
            return new LinearAxis
            {
                Position = position,
                Title = title,
                MajorGridlineStyle = LineStyle.Solid,
                MinorGridlineStyle = LineStyle.Dot,
                AxislineStyle = LineStyle.Solid,
                AxislineColor = OxyColors.Gray,
                AxislineThickness = 1
            };
        }

        private static void ApplyVerticalRange(LinearAxis axis, double yMin, double yMax)
        {
            double span = yMax - yMin;
            if (span < 1e-9)
            {
                double delta = Math.Max(1.0, Math.Abs(yMax) * 0.25);
                axis.Minimum = yMin - delta;
                axis.Maximum = yMax + delta;
                return;
            }

            double margin = span * 0.1;
            axis.Minimum = yMin - margin;
            axis.Maximum = yMax + margin;
        }

        private void SetStatus(string message, StatusTone tone, string stats)
        {
            _statusTextLabel.Text = message;
            _statusTextLabel.ForeColor = GetStatusColor(tone);
            _statusStatsLabel.Text = stats ?? string.Empty;
            _statusStatsLabel.ForeColor = Color.DimGray;
        }

        private static Color GetStatusColor(StatusTone tone)
        {
            switch (tone)
            {
                case StatusTone.Success:
                    return Color.ForestGreen;
                case StatusTone.Warning:
                    return Color.DarkOrange;
                case StatusTone.Error:
                    return Color.Firebrick;
                default:
                    return Color.DimGray;
            }
        }

        private static string BuildSuccessMessage(PlotAttemptResult result)
        {
            string message = result.WarningMessage ?? "График построен.";

            if (result.HasDiscontinuities)
            {
                message += result.WarningMessage == null ? " Обнаружены разрывы функции." : " Также обнаружены разрывы функции.";
            }

            return message;
        }

        private static string FormatNumber(double value)
        {
            double absolute = Math.Abs(value);
            if ((absolute > 0 && absolute < 0.001) || absolute >= 100000)
            {
                return value.ToString("0.###E+0", CultureInfo.CurrentCulture);
            }

            return value.ToString("0.###", CultureInfo.CurrentCulture);
        }

        private static string BuildExportFileName(string rawExpression)
        {
            string fileName = NormalizeExpression(rawExpression);
            if (string.IsNullOrWhiteSpace(fileName))
            {
                fileName = "graph";
            }

            fileName = fileName.Replace("*", "x");
            foreach (char invalidChar in Path.GetInvalidFileNameChars())
            {
                fileName = fileName.Replace(invalidChar, '_');
            }

            if (fileName.Length > 40)
            {
                fileName = fileName.Substring(0, 40);
            }

            return fileName + ".png";
        }

        private void RunWithoutAutoUpdate(Action action)
        {
            bool previousState = _suppressAutoUpdate;
            _suppressAutoUpdate = true;

            try
            {
                action();
            }
            finally
            {
                _suppressAutoUpdate = previousState;
            }
        }

        private enum StatusTone
        {
            Info,
            Success,
            Warning,
            Error
        }

        private sealed class PlotAttemptResult
        {
            public bool Success { get; set; }
            public bool ShouldClearPlot { get; set; }
            public string Message { get; set; }
            public string DisplayExpression { get; set; }
            public string WarningMessage { get; set; }
            public string StatsText { get; set; }
            public double XMin { get; set; }
            public double XMax { get; set; }
            public double YMin { get; set; }
            public double YMax { get; set; }
            public double Step { get; set; }
            public bool HasDiscontinuities { get; set; }
            public List<DataPoint> Points { get; set; }
        }
    }
}
