//npp_shortcut Alt+OemPeriod
//css_inc DBHelper.cs
//css_inc EditorHelper.cs

using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.IO;
using System.Text;
using System.Data;
using System.Windows.Forms;
using System.Text.RegularExpressions;
using NppScripts;
using NppScripts.DBHelper;
using NppScripts.EditorHelper;

public class Script : NppScript {

    class EditForm : Form
    {

        public EditForm(FilterForm form, string lang = "") {
            parent = form;
            InitializeComponent();
            comboBox1.Text = lang;
        }

        public void setDBFile(string dbPath) {
            this.dbPath = dbPath;
        }

        public void getCodeSnippet(int id) {
            this.codeId = id;
            DBHelper db = new DBHelper(dbPath);
            DataTable data = db.Query("SELECT * FROM CodeSnippet WHERE id = " + id);
            textBox1.Text = data.Rows[0]["name"].ToString();
            comboBox1.Text = data.Rows[0]["lang"].ToString();
            textBox2.Text = data.Rows[0]["code"].ToString();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (textBox1.Text.Equals("")) return;
            DBHelper db = new DBHelper(dbPath);
            string sql;
            Dictionary<string, object> param = new Dictionary<string, object>();
            param.Add("@name", textBox1.Text);
            param.Add("@lang", comboBox1.Text);
            param.Add("@code", textBox2.Text);
            if (this.codeId > 0) {
                param.Add("@id", codeId);
                sql = "UPDATE CodeSnippet SET name = @name, lang = @lang, code = @code WHERE id= @id";
            }
            else {
                sql = "INSERT INTO CodeSnippet(name, lang, code) VALUES(@name, @lang, @code)";
            }
            db.Execute(sql, param);
            parent.loadSnippets(comboBox1.Text);
            parent.listViewInit();
            parent.setFocus();
            Close();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            Close();
        }

        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;
        private FilterForm parent = null;
        private int codeId = 0;
        private string dbPath = null;
        /// <summary>
        /// Clean up any resources being used.
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
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.comboBox1 = new System.Windows.Forms.ComboBox();
            this.button1 = new System.Windows.Forms.Button();
            this.button2 = new System.Windows.Forms.Button();
            this.textBox1 = new System.Windows.Forms.TextBox();
            this.textBox2 = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.SuspendLayout();
            //
            // comboBox1
            //
            this.comboBox1.FormattingEnabled = true;
            this.comboBox1.Items.AddRange(new object[] {
            "php",
            "js",
            "html",
            "css",
            "txt"});
            this.comboBox1.Location = new System.Drawing.Point(70, 55);
            this.comboBox1.Name = "comboBox1";
            this.comboBox1.Size = new System.Drawing.Size(121, 21);
            this.comboBox1.TabIndex = 1;
            this.comboBox1.Text = "PHP";
            //
            // button1
            //
            this.button1.Location = new System.Drawing.Point(307, 280);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(75, 32);
            this.button1.TabIndex = 3;
            this.button1.Text = "Save(&S)";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.button1_Click);
            //
            // button2
            //
            this.button2.Location = new System.Drawing.Point(410, 280);
            this.button2.Name = "button2";
            this.button2.Size = new System.Drawing.Size(75, 32);
            this.button2.TabIndex = 4;
            this.button2.Text = "Cancel(&C)";
            this.button2.UseVisualStyleBackColor = true;
            this.button2.Click += new System.EventHandler(this.button2_Click);
            //
            // textBox1
            //
            this.textBox1.Location = new System.Drawing.Point(70, 12);
            this.textBox1.Name = "textBox1";
            this.textBox1.Size = new System.Drawing.Size(416, 20);
            this.textBox1.TabIndex = 0;
            //
            // textBox2
            //
            this.textBox2.Location = new System.Drawing.Point(70, 98);
            this.textBox2.Multiline = true;
            this.textBox2.Name = "textBox2";
            this.textBox2.Size = new System.Drawing.Size(416, 147);
            this.textBox2.TabIndex = 2;
            //
            // label1
            //
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(16, 12);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(35, 13);
            this.label1.TabIndex = 5;
            this.label1.Text = "Name";
            //
            // label2
            //
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(16, 55);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(35, 13);
            this.label2.TabIndex = 6;
            this.label2.Text = "Lang";
            //
            // label3
            //
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(16, 98);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(35, 13);
            this.label3.TabIndex = 7;
            this.label3.Text = "Code";
            //
            // Form1
            //
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(500, 326);
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.CancelButton = this.button2;
            this.Font = new System.Drawing.Font("Courier New", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.Controls.Add(this.label3);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.textBox2);
            this.Controls.Add(this.textBox1);
            this.Controls.Add(this.button2);
            this.Controls.Add(this.button1);
            this.Controls.Add(this.comboBox1);
            this.ShowInTaskbar = false;
            this.StartPosition = FormStartPosition.CenterScreen;
            this.TopMost = true;
            this.Name = "EditForm";
            this.Text = "Edit";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.ComboBox comboBox1;
        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.Button button2;
        private System.Windows.Forms.TextBox textBox1;
        private System.Windows.Forms.TextBox textBox2;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label3;
    }

    class FilterForm : Form {

        public FilterForm() {
            InitializeComponent();
        }

        private System.ComponentModel.IContainer components = null;
        private Dictionary<string, Dictionary<string, string>> codeMap = null;
        private Dictionary<string, Dictionary<string, string>> nameMap = null;
        private string dbPath = null;
        private string langs = "php|js|css|html|txt";
        private string currentLang = null;
        private string showingLang = null;

        protected override void Dispose(bool disposing) {
            Hide();
        }

        private void Form1_Deactivate(object sender, EventArgs e)
        {
            if (!this.Visible) {
                Hide();
            }
        }

        public void setDBFile(string dbPath) {
            this.dbPath = dbPath;
        }

        public void setFocus() {
            this.textBox1.Focus();
            currentLang = getCurrentLang();
            if (!nameMap.ContainsKey(currentLang) && langs.IndexOf(currentLang) > -1) {
                loadSnippets(currentLang);
            }
            if (string.IsNullOrWhiteSpace(showingLang) || !showingLang.Equals(currentLang)) {
                this.textBox1.Text = "";
                listViewInit();
            }
            string word = Editor.getWordAtCursor();
            if (!string.IsNullOrWhiteSpace(word)) {
                textBox1.Text = word;
            }

            this.textBox1.SelectAll();
        }

        public bool tryReplaceCurrent() {
            string word = Editor.getWordAtCursor();
            if (string.IsNullOrWhiteSpace(word)) return false;
            currentLang = getCurrentLang();
            if (!nameMap.ContainsKey(currentLang) && langs.IndexOf(currentLang) > -1) {
                loadSnippets(currentLang);
            }
            string id = "";
            int found = 0;
            word = word.ToUpper();
            Dictionary<string, string> dict = nameMap[currentLang];
            foreach (KeyValuePair<string, string> item in dict) {
                if (item.Value.ToUpper().IndexOf(word) == -1) continue;
                id = item.Key;
                found++;
                if (found > 1) break;
            }
            if (found == 1) {
                insertSnippet(id);
                return true;
            }
            return false;
        }

        private string getCurrentLang() {
            string lang = "txt";
            LangType docType = LangType.L_TEXT;
            Win32.SendMessage(Npp.NppHandle, NppMsg.NPPM_GETCURRENTLANGTYPE, 0, ref docType);
            switch (docType) {
                case LangType.L_PHP:
                   lang = "php";
                   break;
                case LangType.L_JS:
                   lang = "js";
                   break;
                case LangType.L_CSS:
                   lang = "css";
                   break;
                case LangType.L_HTML:
                    lang = "html";
                    break;
            }
            return lang;
        }

        public void loadSnippets(string lang) {
            if (nameMap.ContainsKey(lang)) {
                Dictionary<string, string> old = nameMap[lang];
                old.Clear();
                nameMap.Remove(lang);
                old = codeMap[lang];
                old.Clear();
                codeMap.Remove(lang);
            }

            DBHelper db = new DBHelper(dbPath);
            DataTable data = db.Query("SELECT id,name,code FROM CodeSnippet WHERE lang='" + lang + "'");
            Dictionary<string, string> nameDict = new Dictionary<string, string>();
            Dictionary<string, string> codeDict = new Dictionary<string, string>();
            for (int i = 0; i < data.Rows.Count; i++) {
                codeDict.Add(data.Rows[i]["id"].ToString(), data.Rows[i]["code"].ToString());
                nameDict.Add(data.Rows[i]["id"].ToString(), data.Rows[i]["name"].ToString());
            }
            codeMap.Add(lang, codeDict);
            nameMap.Add(lang, nameDict);
        }

        public void listViewInit() {
            Dictionary<string, string> dict = nameMap[currentLang];
            int i = 0;
            listView1.BeginUpdate();
            listView1.Items.Clear();
            foreach (KeyValuePair<string, string> item in dict) {
                ListViewItem lvi = new ListViewItem();
                lvi.Tag = item.Key;
                lvi.Text = item.Value;
                if (i++ % 2 == 1) {
                    lvi.BackColor = Color.BlanchedAlmond;
                }
                listView1.Items.Add(lvi);
            }
            listView1.EndUpdate();
            showingLang = currentLang;
        }

        private void textBox1_TextChanged(object sender, EventArgs e) {
            string text = this.textBox1.Text.ToUpper().Trim();
            if (text.Equals("!")) return;

            Dictionary<string, string> dict = nameMap[currentLang];
            int i = 0;
            this.listView1.BeginUpdate();
            this.listView1.Items.Clear();
            foreach (KeyValuePair<string, string> item in dict) {
                if (item.Value.ToUpper().IndexOf(text) == -1) continue;
                ListViewItem lvi = new ListViewItem();
                lvi.Tag = item.Key;
                lvi.Text = item.Value;
                if (i++ % 2 == 1) {
                    lvi.BackColor = Color.BlanchedAlmond;
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

        private void textBox1_KeyDown(object sender, KeyEventArgs e) {
            if (e.KeyValue == 9 || e.KeyValue == 40) {//Tab || ArrowDown
                this.listView1.Focus();
                this.listView1.Items[0].Selected = true;
                this.listView1.Items[0].Focused = true;
            }
            else if (e.KeyValue == 27) {
                Hide();
            }
            else if (e.KeyValue == 13) {
                editSnippet(true);
            }
        }

        private void listView1_KeyPress(object sender, KeyPressEventArgs e) {
            if (e.KeyChar == 9) { //Tab
                this.textBox1.Focus();
            }
            else if (e.KeyChar == 13) {//Enter
                this.insertSnippet();
                Hide();
            }
            else if (e.KeyChar == 27) {//Esc
                Hide();
            }
        }

        private void listView1_DoubleClick(object sender, EventArgs e) {
            editSnippet();
        }

        private void insertToolStripMenuItem_Click(object sender, EventArgs e) {
            editSnippet(true);
        }

        private void editToolStripMenuItem_Click(object sender, EventArgs e) {
            editSnippet();
        }

        private void deleteToolStripMenuItem_Click(object sender, EventArgs e) {
            if(listView1.SelectedItems.Count > 0) {
                string id = listView1.SelectedItems[0].Tag.ToString();
                DBHelper db = new DBHelper(dbPath);
                db.Execute("DELETE FROM CodeSnippet WHERE id=" + id);
                listView1.Items.Remove(listView1.SelectedItems[0]);
                codeMap[currentLang].Remove(id);
                nameMap[currentLang].Remove(id);
            }
        }

        private void editSnippet(bool insert = false) {
            if (!insert && listView1.SelectedItems.Count == 0) return;

            EditForm ef = new EditForm(this, currentLang);
            ef.setDBFile(dbPath);
            if (insert) {
                ef.Text = "New";
            }
            else {
                string id = listView1.SelectedItems[0].Tag.ToString();
                ef.getCodeSnippet(int.Parse(id));
            }
            ef.ShowDialog();
        }

        private void insertSnippet(string id = null) {
            if(string.IsNullOrWhiteSpace(id) && listView1.SelectedItems.Count > 0) {
                id = listView1.SelectedItems[0].Tag.ToString();
            }
            if (string.IsNullOrWhiteSpace(id)) return;

            string code = codeMap[currentLang][id];
            string indent = "";
            int pos = Editor.getCurrentPos();
            int column = Editor.getCurrentColumn();
            while (column > 0) {
                indent += " ";
                column--;
            }
            code = code.Replace("\r\n", "\r\n" + indent);
            int offset = code.IndexOf("@");
            if (offset > -1) code = code.Replace("@", "");
            Editor.replaceSel(code);
            if (offset > -1) {
                Editor.gotoPos(pos + offset);
            }
        }

        #region Windows Form Designer generated code

        private void InitializeComponent() {
            this.codeMap = new Dictionary<string, Dictionary<string, string>>();
            this.nameMap = new Dictionary<string, Dictionary<string, string>>();
            this.components = new System.ComponentModel.Container();
            this.textBox1 = new System.Windows.Forms.TextBox();
            this.listView1 = new System.Windows.Forms.ListView();
            this.columnHeader1 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.contextMenuStrip1 = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.insertToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.editToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.deleteToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.contextMenuStrip1.SuspendLayout();
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
            this.columnHeader1});
            this.listView1.ContextMenuStrip = this.contextMenuStrip1;
            this.listView1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.listView1.GridLines = true;
            this.listView1.FullRowSelect = true;
            this.listView1.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.Nonclickable;
            this.listView1.HideSelection = false;
            this.listView1.MultiSelect = false;
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
            this.columnHeader1.Width = 400;

            //
            // contextMenuStrip1
            //
            this.contextMenuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.insertToolStripMenuItem,
            this.editToolStripMenuItem,
            this.deleteToolStripMenuItem});
            this.contextMenuStrip1.Name = "contextMenuStrip1";
            this.contextMenuStrip1.Size = new System.Drawing.Size(108, 48);
            //
            // insertToolStripMenuItem
            //
            this.insertToolStripMenuItem.Name = "insertToolStripMenuItem";
            this.insertToolStripMenuItem.Size = new System.Drawing.Size(152, 22);
            this.insertToolStripMenuItem.Text = "Insert";
            this.insertToolStripMenuItem.Click += new System.EventHandler(this.insertToolStripMenuItem_Click);
            //
            // editToolStripMenuItem
            //
            this.editToolStripMenuItem.Name = "editToolStripMenuItem";
            this.editToolStripMenuItem.Size = new System.Drawing.Size(152, 22);
            this.editToolStripMenuItem.Text = "Edit";
            this.editToolStripMenuItem.Click += new System.EventHandler(this.editToolStripMenuItem_Click);
            //
            // deleteToolStripMenuItem
            //
            this.deleteToolStripMenuItem.Name = "deleteToolStripMenuItem";
            this.deleteToolStripMenuItem.Size = new System.Drawing.Size(152, 22);
            this.deleteToolStripMenuItem.Text = "Delete";
            this.deleteToolStripMenuItem.Click += new System.EventHandler(this.deleteToolStripMenuItem_Click);

            //
            // Form
            //
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(420, 279);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.Controls.Add(this.listView1);
            this.Controls.Add(this.textBox1);
            this.Font = new System.Drawing.Font("Courier New", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.Deactivate += new System.EventHandler(this.Form1_Deactivate);
            this.TopMost = true;
            this.Name = "FilterForm";
            this.Text = "Code Snippets";
            this.ShowInTaskbar = false;
            this.contextMenuStrip1.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();
        }

        #endregion

        private System.Windows.Forms.TextBox textBox1;
        private System.Windows.Forms.ListView listView1;
        private System.Windows.Forms.ColumnHeader columnHeader1;
        private System.Windows.Forms.ContextMenuStrip contextMenuStrip1;
        private System.Windows.Forms.ToolStripMenuItem insertToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem editToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem deleteToolStripMenuItem;
    }

    private FilterForm form = null;

    public override void Run() {
        if (form == null) {
            string dbfile = Path.Combine(Path.GetDirectoryName(this.ScriptFile), "CodeSnippets.mdb");
            form =  new FilterForm { BackColor = Color.Aqua };
            form.setDBFile(dbfile);
        }
        if (form.Visible) {
            form.setFocus();
        }
        else if (!form.tryReplaceCurrent()) {
            form.Show();
            form.setFocus();
        }
    }
}