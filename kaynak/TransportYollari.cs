using System;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using System.Windows.Forms;

namespace TransportYollari
{
    public static class Converter
    {
        public const string DefaultRoot = @"\\KZCS4PPD\sapmnt\trans";
        public const string ImportDefaultRoot = @"\\KZCKHXAPP\sapmnt\trans";
        public static bool TryConvert(string input, string root, out string cofile, out string data, out string error)
        {
            cofile = data = error = "";
            string value = (input ?? "").Trim();
            if (value.Length >= 2 && ((value[0] == '"' && value[value.Length - 1] == '"') ||
                (value[0] == '\'' && value[value.Length - 1] == '\'')))
                value = value.Substring(1, value.Length - 2).Trim();
            Match match = Regex.Match(value, @"\A(?:KHDK|K)?([0-9]{6})\z", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);
            if (!match.Success)
            {
                error = value.Length == 0 ? "Başlamak için soldaki transport numarasını girin." :
                    "6 haneli numara girin: KHDK978324, K978324 veya 978324.";
                return false;
            }
            string folder = (root ?? "").Trim().Trim('"').Trim().Replace('/', '\\').TrimEnd('\\');
            if (folder.Length == 0) { error = "Ana klasör yolunu girin."; return false; }
            string number = match.Groups[1].Value;
            cofile = folder + @"\cofiles\K" + number + ".KHD";
            data = folder + @"\data\R" + number + ".KHD";
            return true;
        }
    }

    public sealed class PathPanel : TableLayoutPanel
    {
        public readonly TextBox Root = new TextBox();
        private readonly TextBox cofile = new TextBox();
        private readonly TextBox data = new TextBox();
        private readonly Button copyCofile = new Button();
        private readonly Button copyData = new Button();
        private readonly Button copyBoth = new Button();
        private readonly Label status = new Label();
        private static readonly Color Ink = Color.FromArgb(28, 43, 63);
        private static readonly Color Muted = Color.FromArgb(93, 109, 128);
        private readonly Color accent;
        public PathPanel(string prefix, string title, string description, string defaultRoot, TextBox sharedInput, bool isExport)
        {
            accent = isExport ? Color.FromArgb(29, 94, 213) : Color.FromArgb(12, 123, 111);
            Dock = DockStyle.Fill;
            BackColor = Color.White;
            Padding = new Padding(20, 16, 20, 16);
            Margin = new Padding(0);
            ColumnCount = 1;
            RowCount = 12;
            ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            int[] heights = { 40, 34, 25, 45, 25, 49, 25, 48, 25, 48, 49 };
            foreach (int height in heights) RowStyles.Add(new RowStyle(SizeType.Absolute, height));
            RowStyles.Add(new RowStyle(SizeType.Percent, 100));
            Label heading = LabelFor(title, accent);
            heading.Font = new Font("Segoe UI", 20F, FontStyle.Bold);
            Controls.Add(heading, 0, 0);
            Controls.Add(LabelFor(description, Muted), 0, 1);
            if (isExport)
            {
                Controls.Add(LabelFor("TRANSPORT NUMARASI · ORTAK", Muted), 0, 2);
                ConfigureText(sharedInput, false);
                Controls.Add(sharedInput, 0, 3);
            }
            else
            {
                Controls.Add(LabelFor("ORTAK TRANSPORT", Muted), 0, 2);
                Label hint = LabelFor("Soldaki transport numarası kullanılır.", Muted);
                Controls.Add(hint, 0, 3);
            }
            Controls.Add(LabelFor("ANA KLASÖR", Muted), 0, 4);
            TableLayoutPanel rootRow = MakeRow();
            Root.Name = prefix + "RootInput";
            Root.AccessibleName = title + " ana klasör";
            ConfigureText(Root, false);
            Root.Text = defaultRoot;
            rootRow.Controls.Add(Root, 0, 0);
            Button reset = new Button();
            ConfigureButton(reset, defaultRoot.Length == 0 ? "Temizle" : "Varsayılan", false);
            reset.Name = prefix + "ResetRoot";
            reset.Click += delegate { Root.Text = defaultRoot; };
            rootRow.Controls.Add(reset, 1, 0);
            Controls.Add(rootRow, 0, 5);
            Controls.Add(LabelFor("COFILES", Muted), 0, 6);
            Controls.Add(OutputRow(cofile, copyCofile, prefix + "CofileOutput", title + " cofiles yolu"), 0, 7);
            Controls.Add(LabelFor("DATA", Muted), 0, 8);
            Controls.Add(OutputRow(data, copyData, prefix + "DataOutput", title + " data yolu"), 0, 9);
            ConfigureButton(copyBoth, "İkisini kopyala", true);
            copyBoth.Dock = DockStyle.None;
            copyBoth.Width = 160;
            copyBoth.Name = prefix + "CopyBoth";
            Controls.Add(copyBoth, 0, 10);
            status.Name = prefix + "Status";
            status.Dock = DockStyle.Fill;
            status.ForeColor = Muted;
            Controls.Add(status, 0, 11);
            copyCofile.Click += delegate { Copy(cofile.Text); };
            copyData.Click += delegate { Copy(data.Text); };
            copyBoth.Click += delegate { Copy(cofile.Text + Environment.NewLine + data.Text); };
            Root.TextChanged += delegate { UpdateOutputs(sharedInput.Text); };
            sharedInput.TextChanged += delegate { UpdateOutputs(sharedInput.Text); };
            UpdateOutputs(sharedInput.Text);
        }
        private static Label LabelFor(string text, Color color)
        {
            return new Label { Text = text, ForeColor = color, Dock = DockStyle.Fill, TextAlign = ContentAlignment.MiddleLeft };
        }
        private static void ConfigureText(TextBox field, bool readOnly)
        {
            field.Dock = DockStyle.Top;
            field.Font = new Font("Segoe UI", 10F);
            field.ReadOnly = readOnly;
            field.BackColor = Color.White;
            field.ForeColor = Ink;
            field.BorderStyle = BorderStyle.FixedSingle;
            field.Margin = new Padding(3, 5, 8, 3);
        }
        private static TableLayoutPanel MakeRow()
        {
            TableLayoutPanel row = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 2, RowCount = 1, Margin = Padding.Empty };
            row.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            row.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 100));
            return row;
        }
        private TableLayoutPanel OutputRow(TextBox field, Button button, string name, string accessibleName)
        {
            TableLayoutPanel row = MakeRow();
            ConfigureText(field, true);
            field.Name = name;
            field.AccessibleName = accessibleName;
            ConfigureButton(button, "Kopyala", false);
            button.Name = "Copy" + name;
            button.AccessibleName = accessibleName + " kopyala";
            row.Controls.Add(field, 0, 0);
            row.Controls.Add(button, 1, 0);
            return row;
        }
        private void ConfigureButton(Button button, string text, bool primary)
        {
            button.Text = text;
            button.Dock = DockStyle.Top;
            button.Height = 32;
            button.FlatStyle = FlatStyle.Flat;
            button.FlatAppearance.BorderColor = primary ? accent : Color.FromArgb(207, 216, 229);
            button.BackColor = primary ? accent : Color.White;
            button.ForeColor = primary ? Color.White : Ink;
            button.Margin = new Padding(3, 3, 3, 3);
            button.Cursor = Cursors.Hand;
            button.UseVisualStyleBackColor = false;
        }
        private void UpdateOutputs(string input)
        {
            string first, second, error;
            bool valid = Converter.TryConvert(input, Root.Text, out first, out second, out error);
            cofile.Text = first;
            data.Text = second;
            copyCofile.Enabled = copyData.Enabled = copyBoth.Enabled = valid;
            status.ForeColor = Muted;
            status.Text = valid ? "Yollar hazır. KHD sistem uzantısı kullanılır." : error;
        }
        private void Copy(string value)
        {
            if (String.IsNullOrEmpty(value)) return;
            try { Clipboard.SetText(value); status.ForeColor = accent; status.Text = "Panoya kopyalandı."; }
            catch (ExternalException) { status.Text = "Pano kullanılamıyor. Tekrar deneyin veya Ctrl+C kullanın."; }
        }
    }

    public sealed class MainForm : Form
    {
        public MainForm()
        {
            Text = "SAP Transport Yolları · Export / Import";
            Font = new Font("Segoe UI", 10F);
            BackColor = Color.FromArgb(232, 238, 245);
            ClientSize = new Size(1260, 580);
            MinimumSize = new Size(1000, 620);
            StartPosition = FormStartPosition.CenterScreen;
            AutoScaleMode = AutoScaleMode.Dpi;
            Padding = new Padding(16);
            TableLayoutPanel halves = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 3, RowCount = 1, Margin = Padding.Empty };
            halves.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
            halves.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 2));
            halves.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
            halves.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
            TextBox transport = new TextBox { Name = "TransportInput", AccessibleName = "Ortak transport numarası" };
            halves.Controls.Add(new PathPanel("Export", "Export", "Dışarı aktarılacak transport dosyalarının yolları", Converter.DefaultRoot, transport, true), 0, 0);
            halves.Controls.Add(new PathPanel("Import", "Import", "İçeri aktarılacak transport dosyalarının yolları", Converter.ImportDefaultRoot, transport, false), 2, 0);
            Controls.Add(halves);
            Shown += delegate { transport.Focus(); };
        }
    }
    internal static class Program
    {
        [STAThread]
        private static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new MainForm());
        }
    }
}
