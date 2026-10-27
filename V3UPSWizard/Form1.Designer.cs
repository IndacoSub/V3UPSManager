namespace V3UPSWizard;

partial class Form1
{
    private System.ComponentModel.IContainer components = new System.ComponentModel.Container();

    protected override void Dispose(bool disposing)
    {
        if (disposing) components.Dispose();
        base.Dispose(disposing);
    }

    private void InitializeComponent()
    {
        SuspendLayout();
        AutoScaleDimensions = new SizeF(96F, 96F);
        AutoScaleMode = AutoScaleMode.Dpi;
        Font = new Font("Segoe UI", 10F);
        ClientSize = new Size(1000, 650);
        MinimumSize = new Size(920, 640);
        StartPosition = FormStartPosition.CenterScreen;
        Name = "Form1";
        Text = "V3UPS Setup";
        ResumeLayout(false);
    }
}
