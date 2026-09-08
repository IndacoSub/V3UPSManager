using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace V3UPSWizard
{
	public static class CustomMessageBox
	{
		public static DialogResult Show(string text, string caption, string button1Text, string button2Text)
		{
			Form form = new Form();
			form.Text = caption;
			Label label = new Label() { Text = text, Dock = DockStyle.Fill, TextAlign = ContentAlignment.MiddleCenter };
			Button button1 = new Button() { Text = button1Text, DialogResult = DialogResult.Yes, Dock = DockStyle.Left };
			Button button2 = new Button() { Text = button2Text, DialogResult = DialogResult.No, Dock = DockStyle.Right };

			form.Controls.Add(label);
			form.Controls.Add(button1);
			form.Controls.Add(button2);
			form.AcceptButton = button1;
			form.CancelButton = button2;
			form.StartPosition = FormStartPosition.CenterParent;
			form.FormBorderStyle = FormBorderStyle.FixedDialog;
			form.MinimizeBox = false;
			form.MaximizeBox = false;
			form.ClientSize = new Size(300, 100);

			return form.ShowDialog();
		}
	}

	public partial class Form1 : Form
	{
		private int currentPage = 0;
		private Panel[] pages;

		private string gameFromFile;   // read from .game if present
		private string installDir;
		private string archivePath;
		private string tempExtractPath;
		private string patchExtractPath;

		// Page 5 controls
		private ProgressBar progressArchive;
		private Label lblArchiveStatus;
		private TextBox txtLogArchive;

		// Page 7 controls
		private ProgressBar progressPatch;
		private Label lblPatchStatus;
		private TextBox txtLogPatch;

		// Readme
		private TextBox txtReadme;

		// Navigation
		private Panel navPanel;
		private Button btnNext;
		private Button btnFinish;

		public Form1()
		{
			InitializeComponent();
			string targetExe = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "V3UPSManager.exe");

			if (!File.Exists(targetExe))
			{
				MessageBox.Show("Executable not found: " + targetExe + ". The execution of this program cannot proceed.", "V3UPSWizard");
				return;
			}
			BuildPages();
			ShowPage(0);
		}

		private void BuildPages()
		{
			// === Page 1: Agreement ===
			var panel1 = new Panel { Dock = DockStyle.Fill };
			var label1 = new Label { Text = "STEP 1: Please make sure to carefully read the End User Agreement below.", Location = new Point(50, 10), Width = 9999, Font = new Font(SystemFonts.DefaultFont, FontStyle.Underline) };
			var txtAgreement = new TextBox
			{
				Multiline = true,
				BorderStyle = BorderStyle.FixedSingle,
				ScrollBars = ScrollBars.Vertical,
				Left = 3,
				BackColor = Color.White,
				//Dock = DockStyle.Fill,
				Top = 33,
				Width = 795,
				Height = 355,
				ReadOnly = true,
				Text =
@"End User Agreement (Team DAIX, 2025)

By checking 'I Agree' you explicitly grant this software permission to scan any and all of your drives and folders in order to locate possible games/apps installation directories.

No data regarding your installed games and apps will be permanently stored anywhere, or shared, or uploaded in any way.

No personal files will be logged, accessed or modified, either.

You acknowledge and accept that this software, and any software linked to or launched by it, is provided 'as is' without any warranties of any kind. 

The authors and distributors assume no responsibility or liability for any damages, data loss, or issues arising from its use.

If you do not agree to these terms, please exit the installer. You may exit the software at any time.

This software may be occasionally be bundled with other pieces of software, data, or files and folders of any kind.

You can find the full source code for this program at the following URL: https://github.com/IndacoSub/V3UPSManager 

Team DAIX (formerly known as 'IndacoSub') is completely unaffiliated with GitHub, Microsoft, or any of their subsidiaries.

You acknowledge and agree that Team DAIX bears no responsibility or liability for the actions, affiliations, or representations of any third parties who may use or distribute this software, or any kind of file to be used in conjunction with it."
			};
			var chkAgree = new CheckBox { Text = "I Agree", Location = new Point(-120, 0), Anchor = AnchorStyles.Right | AnchorStyles.Bottom, Top = 5 };
			panel1.Controls.Add(label1);
			panel1.Controls.Add(txtAgreement);

			// === Page: Install directory (moved after .DAIX.7z extraction) ===
			var panelInstall = new Panel { Dock = DockStyle.Fill };
			var label2 = new Label { Text = "STEP 2: Please choose the game's installation directory.", Location = new Point(50, 10), Width = 9999, Font = new Font(SystemFonts.DefaultFont, FontStyle.Underline) };
			var txtDir = new TextBox { Location = new Point(50, 200), Width = 550, PlaceholderText = "Select the game's installation directory..." };
			var btnBrowseDir = new Button { Text = "Browse...", Location = new Point(650, 200) };

			// Try to detect a Steam library path
			string detectedSteamPath = null;
			foreach (var drive in DriveInfo.GetDrives().Where(d => d.DriveType == DriveType.Fixed && d.IsReady))
			{
				string path1 = Path.Combine(drive.RootDirectory.FullName, @"Program Files (x86)\Steam\steamapps\common\");
				string path2 = Path.Combine(drive.RootDirectory.FullName, @"SteamLibrary\steamapps\common\");

				if (Directory.Exists(path1))
				{
					detectedSteamPath = path1;
					break;
				}
				if (Directory.Exists(path2))
				{
					detectedSteamPath = path2;
					break;
				}
			}

			btnBrowseDir.Click += (s, e) =>
			{
				using (var fbd = new FolderBrowserDialog())
				{
					// If we found a Steam path, start from there
					if (!string.IsNullOrEmpty(detectedSteamPath))
						fbd.SelectedPath = detectedSteamPath;

					if (fbd.ShowDialog() == DialogResult.OK)
						txtDir.Text = fbd.SelectedPath;
				}
			};

			panelInstall.Controls.Add(btnBrowseDir);
			panelInstall.Controls.Add(txtDir);
			panelInstall.Controls.Add(label2);
			var panel2 = panelInstall;

			// === Page 3: Select archive/folder ===
			var panel3 = new Panel { Dock = DockStyle.Fill };
			var label3 = new Label { Text = "STEP 3: Please choose either (1) the extracted patch folder, or (2) a .DAIX.7z file.", Location = new Point(50, 10), Width = 9999, Font = new Font(SystemFonts.DefaultFont, FontStyle.Underline) };
			var txtPath = new TextBox { Location = new Point(50, 200), Width = 550, PlaceholderText = "Select .DAIX.7z file or folder..." };
			var btnBrowse = new Button { Text = "Browse...", Location = new Point(650, 200) };
			btnBrowse.Click += (s, e) =>
			{
				var choice = CustomMessageBox.Show("Choose your patch type", "Select Mode", "DAIX.7Z", "Folder");
				if (choice == DialogResult.No)
				{
					using (var fbd = new FolderBrowserDialog())
					{
						if (fbd.ShowDialog() == DialogResult.OK)
							txtPath.Text = fbd.SelectedPath;
					}
				}
				else if (choice == DialogResult.Yes)
				{
					using (var ofd = new OpenFileDialog())
					{
						ofd.Filter = "DAIX 7-Zip archives|*.DAIX.7z";
						if (ofd.ShowDialog() == DialogResult.OK)
							txtPath.Text = ofd.FileName;
					}
				}
			};
			panel3.Controls.Add(btnBrowse);
			panel3.Controls.Add(txtPath);
			panel3.Controls.Add(label3);

			// === Page 4: Archive extraction ===
			var panel4 = new Panel { Dock = DockStyle.Fill };
			lblArchiveStatus = new Label { Text = "STEP 4: Please wait until the extraction process finishes.", Location = new Point(50, 10), Width = 9999, Font = new Font(SystemFonts.DefaultFont, FontStyle.Underline) };
			progressArchive = new ProgressBar { Location = new Point(50, 50), Width = 700, Minimum = 0, Maximum = 100 };
			txtLogArchive = new TextBox { Location = new Point(50, 75), Width = 700, Height = 320, Multiline = true, ScrollBars = ScrollBars.Vertical };
			panel4.Controls.Add(txtLogArchive);
			panel4.Controls.Add(progressArchive);
			panel4.Controls.Add(lblArchiveStatus);

			// === Page 5: Readme ===
			var panel5 = new Panel { Dock = DockStyle.Fill };
			var label5 = new Label { Text = "STEP 4: Please carefully read the README file. Only proceed if everything looks correct.", Location = new Point(50, 10), Width = 9999, Font = new Font(SystemFonts.DefaultFont, FontStyle.Underline) };
			txtReadme = new TextBox
			{
				Multiline = true,
				BorderStyle = BorderStyle.FixedSingle,
				ScrollBars = ScrollBars.Vertical,
				Left = 3,
				BackColor = Color.White,
				//Dock = DockStyle.Fill,
				Top = 33,
				Width = 795,
				Height = 355,
				ReadOnly = true,
			};
			panel5.Controls.Add(label5);
			panel5.Controls.Add(txtReadme);

			// === Page 6: Patch extraction ===
			var panel6 = new Panel { Dock = DockStyle.Fill };
			lblPatchStatus = new Label { Text = "Extracting PATCH.7z...", Dock = DockStyle.Top };
			progressPatch = new ProgressBar { Dock = DockStyle.Top, Minimum = 0, Maximum = 100 };
			txtLogPatch = new TextBox { Dock = DockStyle.Fill, Multiline = true, ScrollBars = ScrollBars.Vertical };
			panel6.Controls.Add(txtLogPatch);
			panel6.Controls.Add(progressPatch);
			panel6.Controls.Add(lblPatchStatus);

			// === Page 7: Finish ===
			var panel7 = new Panel { Dock = DockStyle.Fill };
			var lblFinish = new Label
			{
				Text = "Installation complete!",
				Dock = DockStyle.Fill,
				TextAlign = System.Drawing.ContentAlignment.MiddleCenter
			};
			panel7.Controls.Add(lblFinish);

			// Add panels
			this.Controls.Add(panel1);
			this.Controls.Add(panel2);
			this.Controls.Add(panel3);
			this.Controls.Add(panel4);
			this.Controls.Add(panel5);
			this.Controls.Add(panel6);
			this.Controls.Add(panel7);

			pages = new[] { panel1, panel2, panel3, panel4, panel5, panel6, panel7 };

			// Navigation

			var line = new GroupBox
			{
				Height = 2,
				Dock = DockStyle.Bottom,
				Text = string.Empty,
			};

			navPanel = new Panel { Dock = DockStyle.Bottom, Height = 35 };
			btnNext = new Button { Text = "Next >", Anchor = AnchorStyles.Right | AnchorStyles.Bottom, Top = 5 };
			btnFinish = new Button { Text = "Finish", Anchor = AnchorStyles.Right | AnchorStyles.Bottom, Visible = false, Top = 5 };
			navPanel.Controls.Add(chkAgree);
			navPanel.Controls.Add(btnNext);
			navPanel.Controls.Add(btnFinish);
			this.Controls.Add(line);
			this.Controls.Add(navPanel);

			navPanel.Resize += (s, e) =>
			{
				btnNext.Left = navPanel.Width - btnNext.Width - 20;
				btnFinish.Left = navPanel.Width - btnFinish.Width - 20;
			};

			// Button events
			btnNext.Click += async (s, e) =>
			{
				if (!ValidatePage(currentPage, chkAgree, txtDir, txtPath)) return;

				if (currentPage < pages.Length - 1)
				{
					ShowPage(currentPage + 1);

					if(currentPage == 1)
					{
						chkAgree.Visible = false;
						chkAgree.Enabled = false;
					}

					if (currentPage == 3)
					{ // after archive selection
						btnFinish.Enabled = false;
						btnNext.Enabled = false;
						await RunArchiveExtractionAsync();
						btnNext.Enabled = true;
						btnFinish.Enabled = true;
					}

					else if (currentPage == 4) // after archive extraction
						LoadReadme();

					else if (currentPage == 5)
					{ // after readme
						btnFinish.Enabled = false;
						btnNext.Enabled = false;
						await RunPatchExtractionAsync();
						btnFinish.Enabled = true;
						btnNext.Enabled = true;
					}
				}

				if (currentPage == pages.Length - 2)
				{
					btnNext.Visible = false;
					btnFinish.Visible = true;
				}
			};

			btnFinish.Click += (s, e) =>
			{
				string targetExe = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "V3UPSManager.exe");

				if (!File.Exists(targetExe))
				{
					MessageBox.Show("Executable not found: " + targetExe + ". The execution of this program cannot proceed.", "V3UPSWizard");
					return;
				}

				// Build arguments dynamically
				string args = $"--install-dir \"{installDir}\" --patch-folder \"{patchExtractPath ?? archivePath}\"";
				if (!string.IsNullOrEmpty(gameFromFile))
					args = $"--game \"{gameFromFile}\" " + args;

				try
				{
					Process.Start(new ProcessStartInfo
					{
						FileName = targetExe,
						Arguments = args,
						UseShellExecute = false,
						CreateNoWindow = false
					});
				}
				catch (Exception ex)
				{
					MessageBox.Show("Failed to launch program: " + ex.Message);
				}

				this.Close();
			};
		}

		private void ShowPage(int index)
		{
			for (int i = 0; i < pages.Length; i++)
				pages[i].Visible = (i == index);
			currentPage = index;
		}

		private bool ValidatePage(int index, CheckBox chkAgree, TextBox txtDir, TextBox txtPath)
		{
			switch (index)
			{
				case 0:
					if (!chkAgree.Checked) { MessageBox.Show("You must agree."); return false; }
					break;
				case 1:
					if (string.IsNullOrWhiteSpace(txtDir.Text) || !Directory.Exists(txtDir.Text))
					{
						MessageBox.Show("Select a valid directory.");
						return false;
					}
					installDir = txtDir.Text;
					break;
				case 2:
					if (string.IsNullOrWhiteSpace(txtPath.Text))
					{
						MessageBox.Show("Please select a folder or .DAIX.7z file.");
						return false;
					}
					archivePath = txtPath.Text;
					if (Directory.Exists(archivePath))
					{
						// Folder selected: skip extraction, go straight to Finish
						patchExtractPath = archivePath;
						btnNext.Visible = false;
						btnFinish.Visible = true;
					}
					break;
			}
			return true;
		}

		private async Task RunArchiveExtractionAsync()
		{
			if (File.Exists(archivePath) && archivePath.EndsWith(".DAIX.7z", StringComparison.OrdinalIgnoreCase))
			{
				tempExtractPath = Path.Combine(Path.GetTempPath(), "DAIX_" + Guid.NewGuid().ToString("N"));
				Directory.CreateDirectory(tempExtractPath);

				lblArchiveStatus.Text = "Extracting archive...";
				progressArchive.Value = 0;

				await Task.Run(() =>
				{
					var psi = new ProcessStartInfo
					{
						FileName = "7za.exe",
						Arguments = $"x \"{archivePath}\" -o\"{tempExtractPath}\" -y",
						UseShellExecute = false,
						RedirectStandardOutput = true,
						RedirectStandardError = true,
						CreateNoWindow = true
					};

					using (var proc = Process.Start(psi))
					{
						proc.OutputDataReceived += (s, e) =>
						{
							if (e.Data != null)
								txtLogArchive.Invoke((Action)(() => txtLogArchive.AppendText(e.Data + Environment.NewLine)));
						};
						proc.BeginOutputReadLine();
						proc.ErrorDataReceived += (s, e) =>
						{
							if (e.Data != null)
								txtLogArchive.Invoke((Action)(() => txtLogArchive.AppendText("[ERR] " + e.Data + Environment.NewLine)));
						};
						proc.BeginErrorReadLine();
						proc.WaitForExit();
					}
				});

				progressArchive.Value = 100;
				lblArchiveStatus.Text = "Archive extraction complete.";

				// Try to read .game file
				string gameFile = Directory.GetFiles(tempExtractPath, "*.game", SearchOption.AllDirectories).FirstOrDefault();
				if (gameFile != null)
				{
					try
					{
						gameFromFile = File.ReadAllLines(gameFile).FirstOrDefault()?.Trim();
					}
					catch { gameFromFile = null; }
				}
			} else
			{
				if(Directory.Exists(archivePath))
				{
					lblArchiveStatus.Text = "Preparations are complete.";
					progressArchive.Value = 100;
					txtLogArchive.Visible = false;
					return;
				}
			}
		}

		private void LoadReadme()
		{
			if (string.IsNullOrEmpty(tempExtractPath)) return;

			string readmePath = Directory.GetFiles(tempExtractPath, "LEGGIMI.txt", SearchOption.AllDirectories).FirstOrDefault()
							 ?? Directory.GetFiles(tempExtractPath, "README.txt", SearchOption.AllDirectories).FirstOrDefault();

			if (readmePath != null)
				txtReadme.Text = File.ReadAllText(readmePath);
			else
				txtReadme.Text = "No README or LEGGIMI file found.";
		}

		private async Task RunPatchExtractionAsync()
		{
			if (string.IsNullOrEmpty(tempExtractPath)) return;

			string patchFile = Directory.GetFiles(tempExtractPath, "PATCH.7z", SearchOption.AllDirectories).FirstOrDefault();
			if (patchFile == null)
			{
				MessageBox.Show("PATCH.7z not found in archive.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
				return;
			}

			patchExtractPath = Path.Combine(Path.GetTempPath(), "PATCH_" + Guid.NewGuid().ToString("N"));
			Directory.CreateDirectory(patchExtractPath);

			lblPatchStatus.Text = "Extracting PATCH.7z...";
			progressPatch.Value = 0;

			await Task.Run(() =>
			{
				var psi = new ProcessStartInfo
				{
					FileName = "7za.exe",
					Arguments = $"x \"{patchFile}\" -o\"{patchExtractPath}\" -y",
					UseShellExecute = false,
					RedirectStandardOutput = true,
					RedirectStandardError = true,
					CreateNoWindow = true
				};

				using (var proc = Process.Start(psi))
				{
					proc.OutputDataReceived += (s, e) =>
					{
						if (e.Data != null)
							txtLogPatch.Invoke((Action)(() => txtLogPatch.AppendText(e.Data + Environment.NewLine)));
					};
					proc.BeginOutputReadLine();
					proc.ErrorDataReceived += (s, e) =>
					{
						if (e.Data != null)
							txtLogPatch.Invoke((Action)(() => txtLogPatch.AppendText("[ERR] " + e.Data + Environment.NewLine)));
					};
					proc.BeginErrorReadLine();
					proc.WaitForExit();
				}
			});

			progressPatch.Value = 100;
			lblPatchStatus.Text = "PATCH.7z extraction complete. Preparations are complete.";
		}
	}
}
