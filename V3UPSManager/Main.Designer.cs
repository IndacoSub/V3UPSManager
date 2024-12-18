namespace V3UPSManager
{
	partial class MainWindow
	{
		/// <summary>
		///  Required designer variable.
		/// </summary>
		private System.ComponentModel.IContainer components = null;

		/// <summary>
		///  Clean up any resources being used.
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
		///  Required method for Designer support - do not modify
		///  the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			InstallationPathPreviewTextbox = new TextBox();
			SelectInstallationFolderButton = new Button();
			SelectPatchFolderButton = new Button();
			PatchPathPreviewTextbox = new TextBox();
			InstallButton = new Button();
			LanguageComboBox = new ComboBox();
			ChooseLanguageLabel = new Label();
			UninstallButton = new Button();
			LogConsole = new RichTextBox();
			ShouldMakeBackupsCheckbox = new CheckBox();
			SuspendLayout();
			// 
			// InstallationPathPreviewTextbox
			// 
			InstallationPathPreviewTextbox.Location = new Point(12, 60);
			InstallationPathPreviewTextbox.Name = "InstallationPathPreviewTextbox";
			InstallationPathPreviewTextbox.ReadOnly = true;
			InstallationPathPreviewTextbox.Size = new Size(521, 23);
			InstallationPathPreviewTextbox.TabIndex = 0;
			// 
			// SelectInstallationFolderButton
			// 
			SelectInstallationFolderButton.Location = new Point(551, 59);
			SelectInstallationFolderButton.Name = "SelectInstallationFolderButton";
			SelectInstallationFolderButton.Size = new Size(224, 23);
			SelectInstallationFolderButton.TabIndex = 1;
			SelectInstallationFolderButton.Text = "Seleziona cartella di installazione";
			SelectInstallationFolderButton.UseVisualStyleBackColor = true;
			SelectInstallationFolderButton.Click += SelectInstallationFolderButton_Click;
			// 
			// SelectPatchFolderButton
			// 
			SelectPatchFolderButton.Location = new Point(551, 119);
			SelectPatchFolderButton.Name = "SelectPatchFolderButton";
			SelectPatchFolderButton.Size = new Size(224, 23);
			SelectPatchFolderButton.TabIndex = 2;
			SelectPatchFolderButton.Text = "Seleziona cartella patch";
			SelectPatchFolderButton.UseVisualStyleBackColor = true;
			SelectPatchFolderButton.Click += SelectPatchFolderButton_Click;
			// 
			// PatchPathPreviewTextbox
			// 
			PatchPathPreviewTextbox.Location = new Point(12, 120);
			PatchPathPreviewTextbox.Name = "PatchPathPreviewTextbox";
			PatchPathPreviewTextbox.ReadOnly = true;
			PatchPathPreviewTextbox.Size = new Size(521, 23);
			PatchPathPreviewTextbox.TabIndex = 3;
			// 
			// InstallButton
			// 
			InstallButton.Location = new Point(12, 179);
			InstallButton.Name = "InstallButton";
			InstallButton.Size = new Size(383, 79);
			InstallButton.TabIndex = 4;
			InstallButton.Text = "Installa";
			InstallButton.UseVisualStyleBackColor = true;
			InstallButton.Click += InstallButton_Click;
			// 
			// LanguageComboBox
			// 
			LanguageComboBox.DropDownStyle = ComboBoxStyle.DropDownList;
			LanguageComboBox.FormattingEnabled = true;
			LanguageComboBox.Location = new Point(234, 11);
			LanguageComboBox.Name = "LanguageComboBox";
			LanguageComboBox.Size = new Size(120, 23);
			LanguageComboBox.TabIndex = 5;
			LanguageComboBox.SelectedIndexChanged += LanguageComboBox_SelectedIndexChanged;
			// 
			// ChooseLanguageLabel
			// 
			ChooseLanguageLabel.AutoSize = true;
			ChooseLanguageLabel.Location = new Point(12, 14);
			ChooseLanguageLabel.Name = "ChooseLanguageLabel";
			ChooseLanguageLabel.Size = new Size(216, 15);
			ChooseLanguageLabel.TabIndex = 6;
			ChooseLanguageLabel.Text = "Choose your language / Scegli la lingua";
			// 
			// UninstallButton
			// 
			UninstallButton.Location = new Point(401, 179);
			UninstallButton.Name = "UninstallButton";
			UninstallButton.Size = new Size(387, 79);
			UninstallButton.TabIndex = 7;
			UninstallButton.Text = "Disinstalla";
			UninstallButton.UseVisualStyleBackColor = true;
			UninstallButton.Click += UninstallButton_Click;
			// 
			// LogConsole
			// 
			LogConsole.Location = new Point(12, 273);
			LogConsole.Name = "LogConsole";
			LogConsole.ReadOnly = true;
			LogConsole.Size = new Size(776, 126);
			LogConsole.TabIndex = 8;
			LogConsole.Text = "";
			// 
			// ShouldMakeBackupsCheckbox
			// 
			ShouldMakeBackupsCheckbox.AutoSize = true;
			ShouldMakeBackupsCheckbox.Checked = true;
			ShouldMakeBackupsCheckbox.CheckState = CheckState.Checked;
			ShouldMakeBackupsCheckbox.Location = new Point(551, 13);
			ShouldMakeBackupsCheckbox.Name = "ShouldMakeBackupsCheckbox";
			ShouldMakeBackupsCheckbox.Size = new Size(133, 19);
			ShouldMakeBackupsCheckbox.TabIndex = 9;
			ShouldMakeBackupsCheckbox.Text = "Backup (default: on)";
			ShouldMakeBackupsCheckbox.UseVisualStyleBackColor = true;
			// 
			// MainWindow
			// 
			AutoScaleDimensions = new SizeF(7F, 15F);
			AutoScaleMode = AutoScaleMode.Font;
			ClientSize = new Size(800, 411);
			Controls.Add(ShouldMakeBackupsCheckbox);
			Controls.Add(LogConsole);
			Controls.Add(UninstallButton);
			Controls.Add(ChooseLanguageLabel);
			Controls.Add(LanguageComboBox);
			Controls.Add(InstallButton);
			Controls.Add(PatchPathPreviewTextbox);
			Controls.Add(SelectPatchFolderButton);
			Controls.Add(SelectInstallationFolderButton);
			Controls.Add(InstallationPathPreviewTextbox);
			MaximumSize = new Size(816, 450);
			MinimumSize = new Size(816, 450);
			Name = "MainWindow";
			Text = "V3 UPS Manager";
			ResumeLayout(false);
			PerformLayout();
		}

		#endregion

		private TextBox InstallationPathPreviewTextbox;
		private Button SelectInstallationFolderButton;
		private Button SelectPatchFolderButton;
		private TextBox PatchPathPreviewTextbox;
		private Button InstallButton;
		private ComboBox LanguageComboBox;
		private Label ChooseLanguageLabel;
		private Button UninstallButton;
		public RichTextBox LogConsole;
		private CheckBox ShouldMakeBackupsCheckbox;
	}
}