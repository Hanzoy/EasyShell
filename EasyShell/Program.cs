namespace EasyShell;

static class Program
{
    [STAThread]
    static void Main()
    {
        ApplicationConfiguration.Initialize();

        using var mutex = new Mutex(true, "EasyShell.SingleInstance", out var createdNew);
        if (!createdNew)
        {
            MessageBox.Show("EasyShell 已经在运行。", "EasyShell", MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
        }

        Application.Run(new TrayApplicationContext());
    }
}
