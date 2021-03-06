﻿//npp_shortcut Alt+J
//css_inc EditorHelper.cs
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.IO;
using System.Text;
using System.Windows.Forms;
using NppScripts;
using NppScripts.EditorHelper;

public class Script : NppScript {

    class FilterForm : Form {

        public FilterForm() {
            InitializeComponent();
            LoadTags();
        }

        private System.ComponentModel.IContainer components = null;
        private System.Collections.Specialized.StringCollection lines = null;

        protected override void Dispose(bool disposing) {
            if (disposing) {
                if (components != null) components.Dispose();
                if (lines != null) lines.Clear();
            }

            base.Dispose(disposing);
        }

        private void Form1_Deactivate(object sender, EventArgs e)
        {
            if (!this.Visible) {
                Close();
            }
        }

        public void setFocus() {
            this.textBox1.Focus();
        }


        private void LoadTags() {
            string path = Npp.GetCurrentDocument();
            int j = 0;
            lines = new StringCollection();
            var procStart = new System.Diagnostics.ProcessStartInfo("ctags", "--fields=-aiklmnSzt+fsK --php-kinds=cidf --excmd=number -f - \"" + path + "\"");
            procStart.CreateNoWindow = true;
            procStart.RedirectStandardOutput = true;
            procStart.UseShellExecute = false;

            var proc = new System.Diagnostics.Process();
            proc.StartInfo = procStart;
            proc.Start();
            var output = proc.StandardOutput.ReadToEnd();
            string outputStr = Convert.ToString(output);
            string[] strs = outputStr.Split(new string[] { Environment.NewLine }, StringSplitOptions.None);
            this.listView1.BeginUpdate();

            for (int i = 0; i < strs.Length; i++) {
                 string str = strs[i].Trim();
                 if (!string.IsNullOrWhiteSpace(str)) {
                    lines.Add(str);
                    string[] arr = strs[i].Split('\t');
                    ListViewItem lvi = new ListViewItem();
                    lvi.UseItemStyleForSubItems  = false;
                    lvi.Text = arr[0];
                    lvi.SubItems.Add(arr[3]);
                    lvi.SubItems.Add(arr[2].Replace(";\"", ""));
                    lvi.SubItems[1].ForeColor = getColor(arr[3]);
                    if (j++ % 2 == 1) {
                        lvi.BackColor = Color.BlanchedAlmond;
                        lvi.SubItems[1].BackColor = Color.BlanchedAlmond;
                        lvi.SubItems[2].BackColor = Color.BlanchedAlmond;
                    }
                    this.listView1.Items.Add(lvi);
                 }
            }

            this.listView1.EndUpdate();
        }

        private void textBox1_TextChanged(object sender, EventArgs e) {
            string text = this.textBox1.Text.ToUpper().Trim();
            int j = 0;
            this.listView1.BeginUpdate();
            this.listView1.Items.Clear();
            for (int i = 0; i < lines.Count; i++) {
                string[] arr = lines[i].Split('\t');
                if (arr[0].ToUpper().IndexOf(text) == -1) continue;
                ListViewItem lvi = new ListViewItem();
                lvi.UseItemStyleForSubItems  = false;
                lvi.Text = arr[0];
                lvi.SubItems.Add(arr[3]);
                lvi.SubItems.Add(arr[2].Replace(";\"", ""));
                lvi.SubItems[1].ForeColor = getColor(arr[3]);
                if (j++ % 2 == 1) {
                    lvi.BackColor = Color.BlanchedAlmond;
                    lvi.SubItems[1].BackColor = Color.BlanchedAlmond;
                    lvi.SubItems[2].BackColor = Color.BlanchedAlmond;
                }
                this.listView1.Items.Add(lvi);
            }

            this.listView1.EndUpdate();
            if (listView1.Items.Count == 1) {
                this.listView1.Focus();
                this.listView1.Items[0].Selected = true;
                this.listView1.Items[0].Focused = true;
            }
        }

        private Color getColor(string type) {
            if (type.Equals("method") || type.Equals("function")) {
                return Color.Maroon;
            }
            else if (type.Equals("class")) {
                return Color.Crimson;
            }
            return Color.LightSeaGreen;
        }

        private void textBox1_KeyDown(object sender, KeyEventArgs e) {
            if (e.KeyValue == 9 || e.KeyValue == 40) {//Tab || ArrowDown
                this.listView1.Focus();
                this.listView1.Items[0].Selected = true;
                this.listView1.Items[0].Focused = true;
            }
            else if (e.KeyValue == 27) {
                Close();
            }
        }

        private void listView1_KeyPress(object sender, KeyPressEventArgs e) {
            if (e.KeyChar == 9) { //Tab
                this.textBox1.Focus();
            }
            else if (e.KeyChar == 13) {//Enter
                this.gotoLine();
                Close();
            }
            else if (e.KeyChar == 27) {//Esc
                Close();
            }
        }

        private void listView1_DoubleClick(object sender, EventArgs e) {
            this.gotoLine();
        }

        private void gotoLine() {
            int linenumber;
            if(this.listView1.SelectedItems.Count > 0) {
                linenumber = int.Parse(listView1.SelectedItems[0].SubItems[2].Text) - 1;
                Editor.gotoLine(linenumber);
            }
        }

        #region Windows Form Designer generated code

        private void InitializeComponent() {
            this.textBox1 = new System.Windows.Forms.TextBox();
            this.listView1 = new System.Windows.Forms.ListView();
            this.columnHeader1 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader2 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader3 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.SuspendLayout();
            //
            // textBox1
            //
            this.textBox1.Dock = System.Windows.Forms.DockStyle.Top;
            this.textBox1.Location = new System.Drawing.Point(0, 0);
            this.textBox1.Name = "textBox1";
            this.textBox1.Size = new System.Drawing.Size(400, 20);
            this.textBox1.TabIndex = 0;
            this.textBox1.TextChanged += new System.EventHandler(this.textBox1_TextChanged);
            this.textBox1.KeyDown += new System.Windows.Forms.KeyEventHandler(this.textBox1_KeyDown);
            //
            // listView1
            //
            this.listView1.Activation = System.Windows.Forms.ItemActivation.OneClick;
            this.listView1.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeader1,
            this.columnHeader2,
            this.columnHeader3});
            this.listView1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.listView1.GridLines = true;
            this.listView1.FullRowSelect = true;
            this.listView1.MultiSelect = false;
            this.listView1.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.Nonclickable;
            this.listView1.HideSelection = false;
            this.listView1.Location = new System.Drawing.Point(0, 20);
            this.listView1.Name = "listView1";
            this.listView1.Size = new System.Drawing.Size(400, 259);
            this.listView1.TabIndex = 1;
            this.listView1.UseCompatibleStateImageBehavior = false;
            this.listView1.View = System.Windows.Forms.View.Details;
            this.listView1.DoubleClick += new System.EventHandler(this.listView1_DoubleClick);
            this.listView1.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.listView1_KeyPress);

            //
            // columnHeader1
            //
            this.columnHeader1.Text = "Name";
            this.columnHeader1.Width = 235;
            //
            // columnHeader2
            //
            this.columnHeader2.Text = "Type";
            this.columnHeader2.Width = 80;
            //
            // columnHeader3
            //
            this.columnHeader3.Text = "Line";
            this.columnHeader3.Width = 60;

            //
            // Form
            //
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(400, 279);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.Font = new System.Drawing.Font("Courier New", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.Deactivate += new System.EventHandler(this.Form1_Deactivate);
            this.Controls.Add(this.listView1);
            this.Controls.Add(this.textBox1);
            this.TopMost = true;
            this.Name = "FilterForm";
            this.Text = "Quick Outline";
            this.ShowInTaskbar = false;
            this.ResumeLayout(false);
            this.PerformLayout();
        }

        #endregion

        private System.Windows.Forms.TextBox textBox1;
        private System.Windows.Forms.ListView listView1;
        private System.Windows.Forms.ColumnHeader columnHeader1;
        private System.Windows.Forms.ColumnHeader columnHeader2;
        private System.Windows.Forms.ColumnHeader columnHeader3;
    }

    public override void Run() {
        FilterForm form =  new FilterForm { BackColor = Color.Aqua };
        form.Show();
    }
}