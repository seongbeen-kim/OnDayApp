using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace OnDayGui
{
    public class FieldSpec
    {
        public string Key;
        public string Label;
        public string DefaultValue;
        public bool IsPassword;
        public bool IsMultiline;
        public string[] Options;

        public FieldSpec(string key, string label)
        {
            Key = key;
            Label = label;
            DefaultValue = "";
        }
    }

    public class EntryDialog : Form
    {
        private readonly Dictionary<string, Control> controls = new Dictionary<string, Control>();
        private readonly Dictionary<string, string> values = new Dictionary<string, string>();

        public IDictionary<string, string> Values
        {
            get { return values; }
        }

        public EntryDialog(string title, FieldSpec[] fields)
        {
            Text = title;
            StartPosition = FormStartPosition.CenterParent;
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MinimizeBox = false;
            MaximizeBox = false;
            BackColor = Color.White;
            Font = new Font("Segoe UI", 10F);
            Width = 470;
            Height = Math.Max(230, 110 + fields.Length * 54);

            TableLayoutPanel table = new TableLayoutPanel();
            table.Dock = DockStyle.Fill;
            table.ColumnCount = 2;
            table.RowCount = fields.Length + 1;
            table.Padding = new Padding(18);
            table.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 130));
            table.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            Controls.Add(table);

            for (int i = 0; i < fields.Length; i++)
            {
                FieldSpec field = fields[i];
                Label label = new Label();
                label.Text = field.Label;
                label.Dock = DockStyle.Fill;
                label.TextAlign = ContentAlignment.MiddleLeft;
                label.ForeColor = Color.FromArgb(48, 64, 89);
                table.Controls.Add(label, 0, i);

                Control input;
                if (field.Options != null && field.Options.Length > 0)
                {
                    ComboBox combo = new ComboBox();
                    combo.DropDownStyle = ComboBoxStyle.DropDownList;
                    combo.Items.AddRange(field.Options);
                    combo.SelectedIndex = 0;
                    if (!String.IsNullOrEmpty(field.DefaultValue))
                    {
                        int index = Array.IndexOf(field.Options, field.DefaultValue);
                        if (index >= 0)
                        {
                            combo.SelectedIndex = index;
                        }
                    }
                    input = combo;
                }
                else
                {
                    TextBox textBox = new TextBox();
                    textBox.Text = field.DefaultValue ?? "";
                    textBox.UseSystemPasswordChar = field.IsPassword;
                    textBox.Multiline = field.IsMultiline;
                    textBox.Height = field.IsMultiline ? 70 : 28;
                    input = textBox;
                }

                input.Dock = DockStyle.Fill;
                table.RowStyles.Add(new RowStyle(SizeType.Absolute, field.IsMultiline ? 82 : 46));
                table.Controls.Add(input, 1, i);
                controls[field.Key] = input;
            }

            FlowLayoutPanel buttons = new FlowLayoutPanel();
            buttons.FlowDirection = FlowDirection.RightToLeft;
            buttons.Dock = DockStyle.Fill;

            Button ok = MakeDialogButton("저장", Color.FromArgb(38, 103, 255), Color.White);
            ok.DialogResult = DialogResult.OK;
            ok.Click += delegate { CaptureValues(); };

            Button cancel = MakeDialogButton("취소", Color.FromArgb(232, 236, 244), Color.FromArgb(30, 42, 62));
            cancel.DialogResult = DialogResult.Cancel;

            buttons.Controls.Add(ok);
            buttons.Controls.Add(cancel);
            table.RowStyles.Add(new RowStyle(SizeType.Absolute, 48));
            table.Controls.Add(buttons, 0, fields.Length);
            table.SetColumnSpan(buttons, 2);

            AcceptButton = ok;
            CancelButton = cancel;
        }

        public static bool TryShow(IWin32Window owner, string title, FieldSpec[] fields, out Dictionary<string, string> result)
        {
            using (EntryDialog dialog = new EntryDialog(title, fields))
            {
                bool ok = dialog.ShowDialog(owner) == DialogResult.OK;
                result = new Dictionary<string, string>(dialog.Values);
                return ok;
            }
        }

        private void CaptureValues()
        {
            values.Clear();
            foreach (KeyValuePair<string, Control> item in controls)
            {
                ComboBox combo = item.Value as ComboBox;
                if (combo != null)
                {
                    values[item.Key] = combo.SelectedItem == null ? "" : combo.SelectedItem.ToString();
                }
                else
                {
                    values[item.Key] = item.Value.Text.Trim();
                }
            }
        }

        private static Button MakeDialogButton(string text, Color back, Color fore)
        {
            Button button = new Button();
            button.Text = text;
            button.Width = 92;
            button.Height = 34;
            button.FlatStyle = FlatStyle.Flat;
            button.FlatAppearance.BorderSize = 0;
            button.BackColor = back;
            button.ForeColor = fore;
            button.Margin = new Padding(6);
            return button;
        }
    }
}
