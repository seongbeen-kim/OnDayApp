using System;
using System.Drawing;
using System.Media;
using System.Windows.Forms;

namespace OnDayGui
{
    public class AlarmForm : Form
    {
        private readonly Timer soundTimer = new Timer();
        private int beepCount;

        public AlarmForm(SleepAlarm alarm)
        {
            Text = "OnDay Alarm";
            StartPosition = FormStartPosition.CenterScreen;
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            Width = 420;
            Height = 250;
            BackColor = Color.White;
            Font = new Font("Segoe UI", 11F);

            Label title = new Label();
            title.Text = "기상 알림";
            title.Font = new Font("Segoe UI", 22F, FontStyle.Bold);
            title.ForeColor = Color.FromArgb(38, 103, 255);
            title.Left = 34;
            title.Top = 28;
            title.Width = 260;
            title.Height = 46;
            Controls.Add(title);

            Label body = new Label();
            body.Text = alarm.Date + " " + alarm.WakeUpTime + "\n알람 시간이 되었습니다.";
            body.ForeColor = Color.FromArgb(30, 42, 62);
            body.Left = 38;
            body.Top = 88;
            body.Width = 330;
            body.Height = 62;
            Controls.Add(body);

            Button off = new Button();
            off.Text = "끄기";
            off.Left = 250;
            off.Top = 160;
            off.Width = 110;
            off.Height = 42;
            off.BackColor = Color.FromArgb(38, 103, 255);
            off.ForeColor = Color.White;
            off.FlatStyle = FlatStyle.Flat;
            off.FlatAppearance.BorderSize = 0;
            off.Click += delegate
            {
                soundTimer.Stop();
                DialogResult = DialogResult.OK;
                Close();
            };
            Controls.Add(off);

            soundTimer.Interval = 500;
            soundTimer.Tick += delegate
            {
                if (beepCount >= 4)
                {
                    soundTimer.Stop();
                    return;
                }
                SystemSounds.Exclamation.Play();
                beepCount++;
            };
        }

        protected override void OnShown(EventArgs e)
        {
            base.OnShown(e);
            beepCount = 0;
            soundTimer.Start();
        }
    }
}
