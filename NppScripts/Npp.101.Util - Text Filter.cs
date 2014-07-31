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
            LoadFile();
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

        private void Form1_Deactivate(object sender, EventArgs e) {
            if (!this.Visible) {
                Close();
            }
        }

        public void setFocus() {
            this.textBox1.Focus();
        }


        private void LoadFile() {
            string strLine, path;
            int i = 0;
            lines = new StringCollection();
            path = Npp.GetCurrentDocument();
            if (!File.Exists(path)) return;
            FileStream aFile = new FileStream(path, FileMode.Open);
            StreamReader sr = new StreamReader(aFile);
            strLine = sr.ReadLine();
            this.listView1.BeginUpdate();
            while(strLine != null) {
                i++;
                this.lines.Add(strLine);
                ListViewItem lvi = new ListViewItem();
                lvi.UseItemStyleForSubItems  = false;
                lvi.ForeColor = Color.Crimson;
                lvi.Text = "" + i;
                lvi.SubItems.Add(strLine);
                this.listView1.Items.Add(lvi);
                strLine = sr.ReadLine();
            }
            this.listView1.EndUpdate();
            sr.Close();
            aFile.Close();
        }

        private void textBox1_TextChanged(object sender, EventArgs e) {
            string text = this.textBox1.Text.ToUpper().Trim();
            this.listView1.BeginUpdate();
            this.listView1.Items.Clear();
            for (int i = 0; i < this.lines.Count; i++) {
                if (this.lines[i].ToUpper().IndexOf(text) == -1) continue;
                ListViewItem lvi = new ListViewItem();
                lvi.Text = "" + (i + 1);
                lvi.SubItems.Add(this.lines[i]);
                this.listView1.Items.Add(lvi);
            }

            this.listView1.EndUpdate();
            if (listView1.Items.Count == 1) {
                this.listView1.Focus();
                this.listView1.Items[0].Selected = true;
                this.listView1.Items[0].Focused = true;
            }
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
                this.gotoFile();
            }
            else if (e.KeyChar == 27) {//Esc
                Close();
            }
        }

        private void listView1_DoubleClick(object sender, EventArgs e) {
            this.gotoFile();
        }

        private void gotoFile() {
            int linenumber, moveline;
            if(this.listView1.SelectedItems.Count > 0) {
                linenumber = int.Parse(listView1.SelectedItems[0].Text) - 1;
                Editor.gotoLine(linenumber);
            }
        }

        #region Windows Form Designer generated code

        private void InitializeComponent() {
            this.textBox1 = new System.Windows.Forms.TextBox();
            this.listView1 = new System.Windows.Forms.ListView();
            this.columnHeader1 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader2 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.SuspendLayout();
            //
            // textBox1
            //
            this.textBox1.Dock = System.Windows.Forms.DockStyle.Top;
            this.textBox1.Location = new System.Drawing.Point(0, 0);
            this.textBox1.Name = "textBox1";
            this.textBox1.Size = new System.Drawing.Size(624, 20);
            this.textBox1.TabIndex = 0;
            this.textBox1.TextChanged += new System.EventHandler(this.textBox1_TextChanged);
            this.textBox1.KeyDown += new System.Windows.Forms.KeyEventHandler(this.textBox1_KeyDown);
            //
            // listView1
            //
            this.listView1.Activation = System.Windows.Forms.ItemActivation.OneClick;
            this.listView1.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeader1,
            this.columnHeader2});
            this.listView1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.listView1.GridLines = true;
            this.listView1.FullRowSelect = true;
            this.listView1.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.Nonclickable;
            this.listView1.HideSelection = false;
            this.listView1.MultiSelect = false;
            this.listView1.Location = new System.Drawing.Point(0, 20);
            this.listView1.Name = "listView1";
            this.listView1.Size = new System.Drawing.Size(624, 259);
            this.listView1.TabIndex = 1;
            this.listView1.UseCompatibleStateImageBehavior = false;
            this.listView1.View = System.Windows.Forms.View.Details;
            this.listView1.DoubleClick += new System.EventHandler(this.listView1_DoubleClick);
            this.listView1.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.listView1_KeyPress);

            //
            // columnHeader1
            //
            this.columnHeader1.Text = "Line";
            this.columnHeader1.Width = 60;
            //
            // columnHeader2
            //
            this.columnHeader2.Text = "Text";
            this.columnHeader2.Width = 500;

            //
            // Form
            //
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(624, 279);
            this.Top = SystemInformation.WorkingArea.Height - 340;
            this.Left = SystemInformation.WorkingArea.Width - 640;
            this.StartPosition = FormStartPosition.Manual;
            this.Font = new System.Drawing.Font("Courier New", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.Deactivate += new System.EventHandler(this.Form1_Deactivate);
            this.Controls.Add(this.listView1);
            this.Controls.Add(this.textBox1);
            this.TopMost = true;
            this.Name = "FilterForm";
            this.Text = "Text Filter";
            this.ShowInTaskbar = false;
            this.ResumeLayout(false);
            this.PerformLayout();
        }

        #endregion

        private System.Windows.Forms.TextBox textBox1;
        private System.Windows.Forms.ListView listView1;
        private System.Windows.Forms.ColumnHeader columnHeader1;
        private System.Windows.Forms.ColumnHeader columnHeader2;
    }

    public override void Run() {
        FilterForm form =  new FilterForm { BackColor = Color.Aqua };
        form.Show();
    }
}