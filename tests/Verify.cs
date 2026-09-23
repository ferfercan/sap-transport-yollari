using System;
using System.Drawing;
using System.Windows.Forms;
using TransportYollari;
using PathConverter = TransportYollari.Converter;

class Verify
{
    static int count;
    static void Check(bool ok, string label) { if (!ok) throw new Exception(label); count++; }
    [STAThread]
    static void Main(string[] args)
    {
        string first, second, error;
        foreach (string input in new[] { "KHDK978324", "K978324", "978324", " khdk978324 ", "\"K978324\"", "'978324'" })
        {
            Check(PathConverter.TryConvert(input, PathConverter.DefaultRoot, out first, out second, out error), input);
            Check(first == @"\\KZCS4PPD\sapmnt\trans\cofiles\K978324.KHD", "cofiles " + input);
            Check(second == @"\\KZCS4PPD\sapmnt\trans\data\R978324.KHD", "data " + input);
        }
        Check(PathConverter.TryConvert("000001", @"\\other\share\trans\", out first, out second, out error), "custom root");
        Check(first == @"\\other\share\trans\cofiles\K000001.KHD" && second == @"\\other\share\trans\data\R000001.KHD", "zeroes and trailing separator");
        Check(PathConverter.TryConvert("978324", "D:/trans/", out first, out second, out error) && first == @"D:\trans\cofiles\K978324.KHD", "local root");
        foreach (string input in new[] { "", "97832", "9783245", "ABCK978324", "K97 8324", "978324\n978325", "KHD978324" })
            Check(!PathConverter.TryConvert(input, PathConverter.DefaultRoot, out first, out second, out error) && first == "" && second == "", "invalid input " + input);
        Check(!PathConverter.TryConvert("978324", "  ", out first, out second, out error), "empty root");
        Application.EnableVisualStyles();
        Application.SetCompatibleTextRenderingDefault(false);
        using (MainForm form = new MainForm())
        {
            form.ShowInTaskbar = false;
            form.StartPosition = FormStartPosition.Manual;
            form.Location = new Point(-30000, -30000);
            form.Show();
            Application.DoEvents();
            TextBox input = (TextBox)form.Controls.Find("TransportInput", true)[0];
            TextBox root = (TextBox)form.Controls.Find("ExportRootInput", true)[0];
            TextBox output = (TextBox)form.Controls.Find("ExportCofileOutput", true)[0];
            TextBox data = (TextBox)form.Controls.Find("ExportDataOutput", true)[0];
            Button both = (Button)form.Controls.Find("ExportCopyBoth", true)[0];
            TextBox importRoot = (TextBox)form.Controls.Find("ImportRootInput", true)[0];
            TextBox importCofile = (TextBox)form.Controls.Find("ImportCofileOutput", true)[0];
            TextBox importData = (TextBox)form.Controls.Find("ImportDataOutput", true)[0];
            Button importBoth = (Button)form.Controls.Find("ImportCopyBoth", true)[0];
            Check(importRoot.Text == @"\\KZCKHXAPP\sapmnt\trans" && !importBoth.Enabled, "import initial state");
            Check(form.Controls.Find("TransportInput", true).Length == 1, "single shared input");
            Check(!both.Enabled && root.Text == PathConverter.DefaultRoot, "initial UI state");
            input.Text = "KHDK978324";
            Check(both.Enabled && output.Text == @"\\KZCS4PPD\sapmnt\trans\cofiles\K978324.KHD", "live input");
            Check(importBoth.Enabled && importCofile.Text == @"\\KZCKHXAPP\sapmnt\trans\cofiles\K978324.KHD" && importData.Text == @"\\KZCKHXAPP\sapmnt\trans\data\R978324.KHD", "import uses shared input");
            root.Text = @"D:\trans";
            Check(data.Text == @"D:\trans\data\R978324.KHD", "live root");
            Check(importData.Text == @"\\KZCKHXAPP\sapmnt\trans\data\R978324.KHD", "independent export root");
            importRoot.Text = @"E:\target";
            Check(importData.Text == @"E:\target\data\R978324.KHD" && data.Text == @"D:\trans\data\R978324.KHD", "independent import root");
            input.Text = "K000007";
            Check(importData.Text == @"E:\target\data\R000007.KHD" && data.Text == @"D:\trans\data\R000007.KHD", "shared update preserves zeroes");
            importRoot.Text = "";
            Check(!importBoth.Enabled && importData.Text == "" && both.Enabled, "empty import leaves export valid");
            ((Button)form.Controls.Find("ImportResetRoot", true)[0]).PerformClick();
            Check(importRoot.Text == PathConverter.ImportDefaultRoot && importBoth.Enabled, "import reset");
            ((Button)form.Controls.Find("ExportResetRoot", true)[0]).PerformClick();
            Check(root.Text == PathConverter.DefaultRoot && data.Text == @"\\KZCS4PPD\sapmnt\trans\data\R000007.KHD", "export reset");
            input.Text = "invalid";
            Check(!both.Enabled && output.Text == "" && data.Text == "", "clear stale output");
            Check(!importBoth.Enabled && importCofile.Text == "" && importData.Text == "", "clear stale import output");
            root.Text = PathConverter.DefaultRoot;
            input.Text = "KHDK978324";
            form.PerformLayout();
            using (Bitmap bitmap = new Bitmap(form.Width, form.Height))
            {
                form.DrawToBitmap(bitmap, new Rectangle(0, 0, bitmap.Width, bitmap.Height));
                bitmap.Save(args[0]);
            }
        }
        Console.WriteLine("PASS: " + count + " conversion and UI checks.");
    }
}
