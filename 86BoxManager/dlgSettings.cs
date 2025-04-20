using System;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Windows.Forms;

using Fluent86.Core.Settings;

namespace Fluent86;

public partial class dlgSettings : Form
{
	private readonly ISettingsProvider _settingsProvider;

	private bool settingsChanged = false; //Keeps track of unsaved changes

	public dlgSettings(ISettingsProvider settingsProvider)
	{
		_settingsProvider = settingsProvider;
		InitializeComponent();
	}

	private void dlgSettings_Load(object sender, EventArgs e)
	{
		LoadSettings();
		Get86BoxVersion();

		lblVersion1.Text = Application.ProductVersion.Substring(0, Application.ProductVersion.Length - 2);

#if DEBUG
		lblVersion1.Text += " (Debug)";
#endif
	}

	private void dlgSettings_FormClosing(object sender, FormClosingEventArgs e)
	{
		//Unsaved changes, ask the user to confirm
		if (settingsChanged == true)
		{
			e.Cancel = true;
			DialogResult result = MessageBox.Show("Would you like to save the changes you've made to the settings?", "Unsaved changes", MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question);
			if (result == DialogResult.Yes)
			{
				SaveSettings();
			}
			if (result != DialogResult.Cancel)
			{
				e.Cancel = false;
			}
		}
	}

	private void btnCancel_Click(object sender, EventArgs e)
	{
		Close();
	}

	private void btnApply_Click(object sender, EventArgs e)
	{
		bool success = SaveSettings();
		if (!success)
		{
			return;
		}
		settingsChanged = CheckForChanges();
		btnApply.Enabled = settingsChanged;
	}

	private void btnOK_Click(object sender, EventArgs e)
	{
		if (settingsChanged)
		{
			SaveSettings();
		}
		Close();
	}

	private void txt_TextChanged(object sender, EventArgs e)
	{
		if (string.IsNullOrWhiteSpace(txtEXEdir.Text) || string.IsNullOrWhiteSpace(txtCFGdir.Text))
		{
			btnApply.Enabled = false;
		}
		else
		{
			settingsChanged = CheckForChanges();
			btnApply.Enabled = settingsChanged;
		}
	}

	//Obtains the 86Box version from 86Box.exe
	private void Get86BoxVersion()
	{
		try
		{
			FileVersionInfo vi = FileVersionInfo.GetVersionInfo(txtEXEdir.Text);
			if (vi.FilePrivatePart >= 3541) //Officially supported builds
			{
				lbl86BoxVer1.Text = vi.FileMajorPart.ToString() + "." + vi.FileMinorPart.ToString() + "." + vi.FileBuildPart.ToString() + "." + vi.FilePrivatePart.ToString() + " - fully compatible";
				lbl86BoxVer1.ForeColor = Color.ForestGreen;
			}
			else if (vi.FilePrivatePart >= 3333 && vi.FilePrivatePart < 3541) //Should mostly work...
			{
				lbl86BoxVer1.Text = vi.FileMajorPart.ToString() + "." + vi.FileMinorPart.ToString() + "." + vi.FileBuildPart.ToString() + "." + vi.FilePrivatePart.ToString() + " - partially compatible";
				lbl86BoxVer1.ForeColor = Color.Orange;
			}
			else //Completely unsupported, since version info can't be obtained anyway
			{
				lbl86BoxVer1.Text = "Unknown - may not be compatible";
				lbl86BoxVer1.ForeColor = Color.Red;
			}
		}
		catch (FileNotFoundException)
		{
			lbl86BoxVer1.Text = "86Box.exe not found";
			lbl86BoxVer1.ForeColor = Color.Gray;
		}
	}

	//TODO: Rewrite
	//Save the settings to the registry
	private bool SaveSettings()
	{
		if (cbxLogging.Checked && string.IsNullOrWhiteSpace(txtLogPath.Text))
		{
			DialogResult result = MessageBox.Show("Using an empty or whitespace string for the log path will prevent 86Box from logging anything. Are you sure you want to use this path?", "Warning", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
			if (result == DialogResult.No)
			{
				return false;
			}
		}
		if (!File.Exists(Path.Combine(txtEXEdir.Text)))
		{
			DialogResult result = MessageBox.Show("86Box.exe could not be found in the directory you specified, so you won't be able to use any virtual machines. Are you sure you want to use this path?", "Warning", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
			if (result == DialogResult.No)
			{
				return false;
			}
		}
		try
		{
			SettingsValues newSettings = new()
			{
				BoxExePath = txtEXEdir.Text,
				VmPath = txtCFGdir.Text,
				MinimizeOnVMStart = cbxMinimize.Checked,
				ShowConsole = cbxShowConsole.Checked,
				MinimizeToTray = cbxMinimizeTray.Checked,
				CloseToTray = cbxCloseTray.Checked,
				LogPath = txtLogPath.Text,
				LoggingEnabled = cbxLogging.Checked,
				ShowGridLines = cbxGrid.Checked,
				SortColumn = _settingsProvider.SettingsValues.SortColumn,
				SortOrder = _settingsProvider.SettingsValues.SortOrder
			};

			_settingsProvider.SaveSettings(newSettings);

			settingsChanged = CheckForChanges();
		}
		catch (Exception ex)
		{
			MessageBox.Show("An error has occurred. Please provide the following information to the developer:\n" + ex.Message + "\n" + ex.StackTrace, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
			return false;
		}
		finally
		{
			Get86BoxVersion(); //Get the new exe version in any case
		}
		return true;
	}

	//TODO: Rewrite
	//Read the settings from the registry
	private void LoadSettings()
	{
		SettingsValues currentSettings = _settingsProvider.SettingsValues;
		PopulateBasedOnSettings(currentSettings);
	}

	private async void btnBrowse1_Click(object sender, EventArgs e)
	{
		OpenFileDialog dialog = new OpenFileDialog
		{
			InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyComputer),
			Title = "Select a folder where 86Box program files and the roms folder are located"
		};
		DialogResult result = dialog.ShowDialog();
		if (result == DialogResult.OK && !string.IsNullOrEmpty(dialog.FileName))
		{
			txtEXEdir.Text = dialog.FileName;
		}
	}

	private async void btnBrowse2_Click(object sender, EventArgs e)
	{
		FolderSelectDialog dialog = new FolderSelectDialog
		{
			InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyComputer),
			Title = "Select a folder where your virtual machines (configs, nvr folders, etc.) will be located"
		};

		if (await dialog.Show(Handle))
		{
			txtCFGdir.Text = dialog.FileName;
		}
	}

	private void btnDefaults_Click(object sender, EventArgs e)
	{
		DialogResult result = MessageBox.Show("All settings will be reset to their default values. Do you wish to continue?", "Settings will be reset", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
		if (result == DialogResult.Yes)
		{
			ResetSettings();
		}
	}

	//Resets the settings to their default values
	private void ResetSettings()
	{
		SettingsValues defaultSettings = new();
		PopulateBasedOnSettings(defaultSettings);

		settingsChanged = CheckForChanges();
	}

	//Checks if all controls match the currently saved settings to determine if any changes were made
	private bool CheckForChanges()
	{
		SettingsValues currentSettings = _settingsProvider.SettingsValues;

		btnApply.Enabled = (
			txtEXEdir.Text != currentSettings.BoxExePath ||
			txtCFGdir.Text != currentSettings.VmPath ||
			txtLogPath.Text != currentSettings.LogPath ||
			cbxMinimize.Checked != currentSettings.MinimizeOnVMStart ||
			cbxShowConsole.Checked != currentSettings.ShowConsole ||
			cbxMinimizeTray.Checked != currentSettings.MinimizeToTray ||
			cbxCloseTray.Checked != currentSettings.CloseToTray ||
			cbxLogging.Checked != currentSettings.LoggingEnabled ||
			cbxGrid.Checked != currentSettings.ShowGridLines);

		return btnApply.Enabled;
	}

	private void cbx_CheckedChanged(object sender, EventArgs e)
	{
		settingsChanged = CheckForChanges();
	}

	private void cbxLogging_CheckedChanged(object sender, EventArgs e)
	{
		settingsChanged = CheckForChanges();
		txt_TextChanged(sender, e); //Needed so the Apply button doesn't get enabled on an empty logpath textbox. Too lazy to write a duplicated empty check...
		txtLogPath.Enabled = cbxLogging.Checked;
		btnBrowse3.Enabled = cbxLogging.Checked;
	}

	private void btnBrowse3_Click(object sender, EventArgs e)
	{
		SaveFileDialog ofd = new SaveFileDialog
		{
			DefaultExt = "log",
			Title = "Select a file where 86Box logs will be saved",
			InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyComputer),
			Filter = "Log files (*.log)|*.log"
		};

		if (ofd.ShowDialog() == DialogResult.OK)
		{
			txtLogPath.Text = ofd.FileName;
		}

		ofd.Dispose();
	}

	private void lnkGithub2_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
	{
		lnkGithub2.LinkVisited = true;
		ProcessStartInfo startInfo = new("https://github.com/86Box/86Box")
		{
			UseShellExecute = true
		};
		Process.Start(startInfo);
	}

	private void lnkGithub_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
	{
		lnkGithub.LinkVisited = true;
		ProcessStartInfo startInfo = new("https://github.com/86Box/86BoxManager")
		{
			UseShellExecute = true
		};
		Process.Start(startInfo);
	}

	private void PopulateBasedOnSettings(SettingsValues currentSettings)
	{
		txtEXEdir.Text = currentSettings.BoxExePath;
		txtCFGdir.Text = currentSettings.VmPath;
		cbxLogging.Checked = currentSettings.LoggingEnabled;
		txtLogPath.Text = currentSettings.LogPath;
		cbxMinimize.Checked = currentSettings.MinimizeOnVMStart;
		cbxShowConsole.Checked = currentSettings.ShowConsole;
		cbxMinimizeTray.Checked = currentSettings.MinimizeToTray;
		cbxCloseTray.Checked = currentSettings.CloseToTray;
		cbxGrid.Checked = currentSettings.ShowGridLines;
		txtLogPath.Enabled = cbxLogging.Checked;
		btnBrowse3.Enabled = cbxLogging.Checked;
	}
}