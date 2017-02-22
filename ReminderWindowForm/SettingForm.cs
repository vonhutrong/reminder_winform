using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ReminderWindowForm
{
    public partial class SettingForm : Form
    {
        private readonly int TIME_UNIT = 60000; //1000 * 60, sum of milliseconds in 1 minute

        private Timer timer = new Timer();
        private bool allowVisible;

        public SettingForm()
        {
            InitializeComponent();

            loadUserSettings();

            timer.Tick += new EventHandler(OnTimedEvent);
            notifyIcon.Icon = ReminderWindowForm.Properties.Resources.icon;
            notifyIcon.Visible = true;

            if (!string.IsNullOrEmpty(Properties.Settings.Default.timeValue))
            {
                allowVisible = false;
            }
            else
            {
                allowVisible = true;
            }
        }

        private void loadUserSettings()
        {
            timeValue.Text = Properties.Settings.Default.timeValue;
            message.Text = Properties.Settings.Default.message;
            checkBox1.Checked = Properties.Settings.Default.startedWithWindows;
        }

        #region events

        private void comboBox1_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (!char.IsControl(e.KeyChar) && !char.IsDigit(e.KeyChar) &&
                (e.KeyChar != '.'))
            {
                e.Handled = true;
            }

            // only allow one decimal point
            if ((e.KeyChar == '.') && ((sender as ComboBox).Text.IndexOf('.') > -1))
            {
                e.Handled = true;
            }
        }

        private void btnStart_Click(object sender, EventArgs e)
        {
            saveUserSetting();
            start();
            this.Hide();
        }

        private void OnTimedEvent(object sender, EventArgs e)
        {
            showAlert();
        }

        private void OnClickNotifyIcon(object sender, MouseEventArgs e)
        {
            showSettingForm();
        }

        private void settingToolStripMenuItem_Click(object sender, EventArgs e)
        {
            showSettingForm();
        }

        private void SettingForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            e.Cancel = true;
            this.Hide();
        }

        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            exitProgram();
        }

        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);

            bool cursorNotInBar = Screen.GetWorkingArea(this).Contains(Cursor.Position);

            if (this.WindowState == FormWindowState.Minimized)
            {
                this.Hide();
            }
        }

        protected override void SetVisibleCore(bool value)
        {
            if (!allowVisible)
            {
                value = false;
                if (!this.IsHandleCreated) CreateHandle();
            }
            base.SetVisibleCore(value);
        }

        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            setStartingWithWindowsStatus(checkBox1.Checked);
        }

        #endregion

        #region private functions

        private void showSettingForm()
        {
            allowVisible = true;
            this.Show();
            this.WindowState = FormWindowState.Normal;
            this.Activate();
        }
        
        private void start()
        {
            timer.Interval = int.Parse(timeValue.Text) * TIME_UNIT;
            timer.Start();
        }

        private void showAlert()
        {
            MessageBox.Show(new Form { TopMost = true }, message.Text);
        }

        private void exitProgram()
        {
            this.Dispose();
        }

        private void saveUserSetting()
        {
            Properties.Settings.Default.timeValue = timeValue.Text;
            Properties.Settings.Default.message = message.Text;
            Properties.Settings.Default.Save();
        }

        private void setStartingWithWindowsStatus(bool active)
        {
            RegistryKey startWithWindowsRegistryKey = Registry.CurrentUser.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run", true);
            string registryName = "Reminder";
            string registryValue = "\"" + Application.ExecutablePath.ToString() + "\"";
            if (active)
            {
                startWithWindowsRegistryKey.SetValue(registryName, registryValue);
            }
            else
            {
                startWithWindowsRegistryKey.DeleteValue(registryName);
            }
            Properties.Settings.Default.startedWithWindows = active;
            Properties.Settings.Default.Save();
        }

        #endregion
    }
}
