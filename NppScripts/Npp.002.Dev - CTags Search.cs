//npp_shortcut Ctrl+J
//css_inc EditorHelper.cs
//css_inc IniFile.cs
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
using System.Text.RegularExpressions;
using NppScripts;
using NppScripts.EditorHelper;
using NppScripts.IniFile;

/**
 * 生成命令Tag文件：ctags -R --fields=-aiklmnSzt+fsK --languages=php --php-kinds=cidf --excmd=number -f ctags.tmp
 *
 * Tab键 - 输入框与列表间切换
 * Esc - 关闭
 * 输入框：
 *      按向下键 - 切换到列表
 * 列表:
 *      Enter、鼠标双击 - 打开源文件
 *      按空格键 - 插入当前选择
 *      后退键 - 替换当前选择
 */
public class Script : NppScript {

    private TagsForm tagsForm = null;

    class IniForm: Form
    {
        public IniForm(TagsForm tagsForm,string file) {
            InitializeComponent();
            this.tagsForm = tagsForm;
            configFile = file;
            textBox1.Text = File.ReadAllText(file);
        }

        private void button1_Click(object sender, EventArgs e) {
            File.WriteAllText(configFile, textBox1.Text);
            tagsForm.LoadTagsFile();
            Close();
        }

        private void button2_Click(object sender, EventArgs e) {
            Close();
        }
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;
        private string configFile = null;
        private TagsForm tagsForm = null;

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
            this.textBox1 = new System.Windows.Forms.TextBox();
            this.button1 = new System.Windows.Forms.Button();
            this.button2 = new System.Windows.Forms.Button();
            this.SuspendLayout();
            //
            // textBox1
            //
            this.textBox1.Dock = System.Windows.Forms.DockStyle.Top;
            this.textBox1.Location = new System.Drawing.Point(0, 0);
            this.textBox1.Multiline = true;
            this.textBox1.Name = "textBox1";
            this.textBox1.Size = new System.Drawing.Size(528, 245);
            this.textBox1.TabIndex = 0;
            //
            // button1
            //
            this.button1.Location = new System.Drawing.Point(316, 262);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(95, 26);
            this.button1.TabIndex = 1;
            this.button1.Text = "Save(&S)";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.button1_Click);
            //
            // button2
            //
            this.button2.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.button2.Location = new System.Drawing.Point(428, 262);
            this.button2.Name = "button2";
            this.button2.Size = new System.Drawing.Size(88, 26);
            this.button2.TabIndex = 2;
            this.button2.Text = "Cancel(&C)";
            this.button2.UseVisualStyleBackColor = true;
            this.button2.Click += new System.EventHandler(this.button2_Click);
            //
            // IniForm
            //
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.button2;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.Font = new System.Drawing.Font("Courier New", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.ClientSize = new System.Drawing.Size(528, 300);
            this.Controls.Add(this.button1);
            this.Controls.Add(this.button2);
            this.Controls.Add(this.textBox1);
            this.ShowInTaskbar = false;
            this.StartPosition = FormStartPosition.CenterScreen;
            this.TopMost = true;
            this.Name = "IniForm";
            this.Text = "CtagsSearch.ini";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TextBox textBox1;
        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.Button button2;
    }

    class TagsForm : Form {

        public TagsForm() {
            InitializeComponent();
            configFile = Path.Combine(Plugin.ScriptsDir, "CtagsSearch.ini");
            if (!File.Exists(configFile)) {
                string configs = "[Projects]" + Environment.NewLine + "Project1="
                            + Environment.NewLine + "Current=Project1" + Environment.NewLine;
                File.WriteAllText(configFile, configs);
            }
            LoadTagsFile();
        }

        private string configFile = null;
        private string projectRoot = "";
        private StringCollection tagLines = null;
        private System.ComponentModel.IContainer components = null;

        protected override void Dispose(bool disposing) {
            Hide();
        }

        private void Form1_Deactivate(object sender, EventArgs e)
        {
            if (!this.Visible) {
                Hide();
            }
        }

        public void setFocus() {
            this.textBox1.Focus();

            string word = Editor.getWordAtCursor(false);
            if (!string.IsNullOrWhiteSpace(word)) {
                textBox1.Text = word;
            }

            this.textBox1.SelectAll();
        }

        public void LoadTagsFile() {
            string file, strLine;
            int j = 0;

            IniFile ini = new IniFile(configFile);
            string projectName = ini.IniReadValue("Projects", "Current");
            projectRoot = ini.IniReadValue("Projects", projectName);
            file = Path.Combine(projectRoot, "ctags.tmp");

            this.Text = "CTags Search -" + projectRoot;

            if (string.IsNullOrWhiteSpace(projectRoot) || !File.Exists(file)) {
                return;
            }

            if (tagLines != null) tagLines.Clear();
            tagLines = new StringCollection();

            FileStream aFile = new FileStream(file, FileMode.Open);
            StreamReader sr = new StreamReader(aFile);
            strLine = sr.ReadLine();
            this.listView1.BeginUpdate();
            while(strLine != null) {
                if (!strLine.StartsWith("!")) {
                    this.tagLines.Add(strLine);
                    string[] arr = strLine.Split('\t');
                    ListViewItem lvi = new ListViewItem();
                    lvi.UseItemStyleForSubItems  = false;
                    lvi.Text = arr[0];
                    lvi.SubItems.Add(arr[3]);
                    lvi.SubItems.Add(arr[2].Replace(";\"", ""));
                    lvi.SubItems.Add(arr[1]);
                    lvi.SubItems[1].ForeColor = getColor(arr[3]);
                    if (j++ % 2 == 1) {
                        lvi.BackColor = Color.BlanchedAlmond;
                        lvi.SubItems[1].BackColor = Color.BlanchedAlmond;
                        lvi.SubItems[2].BackColor = Color.BlanchedAlmond;
                        lvi.SubItems[3].BackColor = Color.BlanchedAlmond;
                    }
                    this.listView1.Items.Add(lvi);
                }

                strLine = sr.ReadLine();
            }

            this.listView1.EndUpdate();

            sr.Close();
            aFile.Close();
        }

        /**
         * 搜索表达式：
         *     word - 单词
         *     @path - 路径
         *     word@path - 单词+路径
         */
        private void textBox1_TextChanged(object sender, EventArgs e) {
            string filterDefine = null, filterPath = null;
            string text = this.textBox1.Text.ToUpper().Trim();
            int j = 0;
            if (text.Equals("@") || text.StartsWith("!")) return;

            if (text.IndexOf("@") > -1) {
                if (text.StartsWith("@")) {
                    filterPath = text.Replace("@", "");
                }
                else {
                    string[] filters = text.Split('@');
                    filterDefine = filters[0];
                    filterPath = filters[1];
                }
            }
            else if (!text.Equals("")) {
                filterDefine = text;
            }

            this.listView1.BeginUpdate();
            this.listView1.Items.Clear();
            for (int i = 0; i < this.tagLines.Count; i++) {
                string[] arr = this.tagLines[i].Split('\t');
                if (!string.IsNullOrWhiteSpace(filterPath) && arr[1].ToUpper().IndexOf(filterPath) == -1) {
                        continue;
                }
                if (!string.IsNullOrWhiteSpace(filterDefine) && arr[0].ToUpper().IndexOf(filterDefine) == -1) {
                        continue;
                }
                ListViewItem lvi = new ListViewItem();
                lvi.UseItemStyleForSubItems  = false;
                lvi.Text = arr[0];
                lvi.SubItems.Add(arr[3]);
                lvi.SubItems.Add(arr[2].Replace(";\"", ""));
                lvi.SubItems.Add(arr[1]);
                lvi.SubItems[1].ForeColor = getColor(arr[3]);
                if (j++ % 2 == 1) {
                    lvi.BackColor = Color.BlanchedAlmond;
                    lvi.SubItems[1].BackColor = Color.BlanchedAlmond;
                    lvi.SubItems[2].BackColor = Color.BlanchedAlmond;
                    lvi.SubItems[3].BackColor = Color.BlanchedAlmond;
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
                Hide();
            }
            else if (e.KeyValue == 13) {
                if (textBox1.Text.StartsWith("!")) {
                    IniForm form = new IniForm(this, configFile);
                    form.ShowDialog();
                }
            }
        }

        private void listView1_KeyPress(object sender, KeyPressEventArgs e) {
            if (e.KeyChar == 9) { //Tab
                this.textBox1.Focus();
            }
            else if (e.KeyChar == 13) {//Enter
                this.openSourceFile();
                Hide();
            }
            else if (e.KeyChar == 27) {//Esc
                Hide();
            }
            else if (e.KeyChar == 32){//Space
                this.insertSelectedTag();
                Hide();
            }
            else if (e.KeyChar == '\b') { //backspace
                this.insertSelectedTag(true);
                Hide();
            }
        }

        private void listView1_DoubleClick(object sender, EventArgs e) {
            this.openSourceFile();
        }

        private void openSourceFile() {
            int linenumber;
            string file;
            if(this.listView1.SelectedItems.Count > 0) {
                file = this.listView1.SelectedItems[0].SubItems[3].Text;
                linenumber = int.Parse(this.listView1.SelectedItems[0].SubItems[2].Text) - 1;
                Editor.open(Path.Combine(projectRoot, file), linenumber);
            }
        }

        private void insertSelectedTag(bool replace = false) {
            if(this.listView1.SelectedItems.Count > 0) {
                string tag = this.listView1.SelectedItems[0].Text;
                if (replace) {
                    Editor.getWordAtCursor(true);
                }
                Editor.replaceSel(tag);
            }
        }

        #region Windows Form Designer generated code

        private void InitializeComponent() {
            this.textBox1 = new System.Windows.Forms.TextBox();
            this.listView1 = new System.Windows.Forms.ListView();
            this.columnHeader1 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader2 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader3 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader4 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
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
            this.columnHeader2,
            this.columnHeader3,
            this.columnHeader4 });
            this.listView1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.listView1.GridLines = true;
            this.listView1.FullRowSelect = true;
            this.listView1.MultiSelect = false;
            this.listView1.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.Nonclickable;
            this.listView1.HideSelection = false;
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
            this.columnHeader1.Text = "Name";
            this.columnHeader1.Width = 200;
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
            // columnHeader4
            //
            this.columnHeader4.Text = "File";
            this.columnHeader4.Width = 500;
            //
            // TagsForm
            //
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(624, 279);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.Font = new System.Drawing.Font("Courier New", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.Deactivate += new System.EventHandler(this.Form1_Deactivate);
            this.TopMost = true;
            this.Controls.Add(this.listView1);
            this.Controls.Add(this.textBox1);
            this.Name = "TagsForm";
            this.Text = "CTags Search";
            this.ShowInTaskbar = false;
            this.ResumeLayout(false);
            this.PerformLayout();
        }

        #endregion

        private System.Windows.Forms.TextBox textBox1;
        private System.Windows.Forms.ListView listView1;
        private System.Windows.Forms.ColumnHeader columnHeader1;
        private System.Windows.Forms.ColumnHeader columnHeader2;
        private System.Windows.Forms.ColumnHeader columnHeader4;
        private System.Windows.Forms.ColumnHeader columnHeader3;
    }

    class BrowserForm : Form {

        public BrowserForm() {
            InitializeComponent();
            bookmarkFile = Path.Combine(Plugin.ScriptsDir, "bookmark.ini");
            if (!File.Exists(bookmarkFile)) {
                File.Create(bookmarkFile).Dispose();
            }
        }

        private System.ComponentModel.IContainer components = null;
        private StringCollection fileList = null;
        private String currentPath = null;
        private String bookmarkFile = null;

        protected override void Dispose(bool disposing) {
            if (disposing) {
                if (components != null) components.Dispose();
                if (fileList != null) fileList.Clear();
            }

            base.Dispose(disposing);
        }

        public void setFocus() {
            textBox1.Text = "";
            currentPath = Editor.getCurrentPath();
            initFileList();
            this.textBox1.Focus();
            this.textBox1.SelectAll();
        }

        private void initFileList() {
            if (fileList != null) fileList.Clear();
            fileList = new StringCollection();
            fileList.Add("@..");
            string[] dirs = Directory.GetDirectories(currentPath);
            for (int i = 0; i < dirs.Length; i++) {
                fileList.Add("@" + Path.GetFileName(dirs[i]));
            }
            string[] files = Directory.GetFiles(currentPath);
            for (int i = 0; i < files.Length; i++) {
                fileList.Add(Path.GetFileName(files[i]));
            }
            initListView();
        }

        private void initListView() {
            listView1.BeginUpdate();
            listView1.Items.Clear();
            for (int i = 0; i < fileList.Count; i++) {
                ListViewItem lvi = new ListViewItem();
                lvi.Text = fileList[i];
                if (fileList[i].StartsWith("@")) {
                    lvi.Font = new Font(lvi.Font, lvi.Font.Style | FontStyle.Bold);
                }
                if (i % 2 == 1) {
                    lvi.BackColor = Color.BlanchedAlmond;
                }
                listView1.Items.Add(lvi);
            }
            listView1.EndUpdate();
        }

        private void textBox1_TextChanged(object sender, EventArgs e) {
            string text = textBox1.Text.Trim().ToUpper();
            int j = 0;
            if (text.StartsWith("!")) return;
            listView1.BeginUpdate();
            listView1.Items.Clear();
            for (int i = 0; i < fileList.Count; i++) {
                if (fileList[i].ToUpper().IndexOf(text) == -1) continue;
                ListViewItem lvi = new ListViewItem();
                lvi.Text = fileList[i];
                if (fileList[i].StartsWith("@")) {
                   lvi.Font = new Font(lvi.Font, lvi.Font.Style | FontStyle.Bold);
                }
                if (j++ % 2 == 1) {
                    lvi.BackColor = Color.BlanchedAlmond;
                }
                listView1.Items.Add(lvi);
            }
            listView1.EndUpdate();
        }

        private void textBox1_KeyDown(object sender, KeyEventArgs e) {
            if (e.KeyValue == 9 || e.KeyValue == 40) {//Tab || ArrowDown
                this.listView1.Focus();
                this.listView1.Items[0].Selected = true;
                this.listView1.Items[0].Focused = true;
            }
            else if (e.KeyValue == 13) {
                string text = textBox1.Text.Trim();
                string file;
                if (text.StartsWith("!")) {
                    if (text.StartsWith("!!")) { //delete
                        if (text.StartsWith("!!@")) { // dir
                            file = Path.Combine(currentPath, text.Substring(3));
                            if (Directory.Exists(file)) {
                                Directory.Delete(file, true);
                            }
                        }
                        else { // file
                            file = Path.Combine(currentPath, text.Substring(2));
                            if (File.Exists(file)) {
                                File.Delete(file);
                            }
                        }
                    }
                    else if (text.StartsWith("!#")) { //bookmark
                        if (text.Equals("!#")) { //list
                            listBookmark();
                            return;
                        }
                        else if (text.Equals("!#add")) { //add
                            addBookmark(currentPath);
                        }
                        else if (text.StartsWith("!#rm")) { // remove
                            deleteBookmark(int.Parse(text.Substring(5)));
                        }
                    }
                    else if (text.StartsWith("!$")) { //change dir
                        file = text.Substring(2);
                        if (Directory.Exists(file)) {
                            currentPath = file;
                            initFileList();
                            this.textBox1.SelectAll();
                            return;
                        }
                    }
                    else {
                        if (text.StartsWith("!@")) { // create
                            file = Path.Combine(currentPath, text.Substring(2));
                            if (!Directory.Exists(file)) {
                                Directory.CreateDirectory(file);
                            }
                        }
                        else { // file
                            file = Path.Combine(currentPath, text.Substring(1));
                            if (!File.Exists(file)) {
                                File.Create(file).Dispose();
                                Editor.open(file);
                                if (file.EndsWith(".php")) {
                                    Editor.setUTF8WithoutBOM();
                                }
                                else {
                                    Editor.setUTF8();
                                }
                            }
                        }

                    }
                    Hide();
                }
            }
            else if (e.KeyValue == 27) {
                Hide();
            }
        }

        private void listBookmark() {
            FileStream aFile = new FileStream(bookmarkFile, FileMode.Open);
            StreamReader sr = new StreamReader(aFile);
            string strLine = sr.ReadLine();
            fileList.Clear();
            while(strLine != null) {
                fileList.Add("@" + strLine);
                strLine = sr.ReadLine();
            }
            sr.Close();
            aFile.Close();
            initListView();
        }

        private void addBookmark(string bookmark) {
            File.AppendAllText(bookmarkFile, bookmark + Environment.NewLine);
        }

        private void deleteBookmark(int num) {
            StringCollection sc = new StringCollection();
            FileStream aFile = new FileStream(bookmarkFile, FileMode.Open);
            StreamReader sr = new StreamReader(aFile);
            string strLine = sr.ReadLine();
            while(strLine != null) {
                sc.Add(strLine);
                strLine = sr.ReadLine();
            }
            sr.Close();
            aFile.Close();
            if (num - 1 > -1) {
                sc.RemoveAt(num - 1);
                String[] bookmarks = new String[sc.Count];
                sc.CopyTo(bookmarks, 0);
                File.WriteAllLines(bookmarkFile, bookmarks);
            }
        }

        private void listView1_KeyPress(object sender, KeyPressEventArgs e) {
            if (e.KeyChar == 9) { //Tab
                this.textBox1.Focus();
            }
            else if (e.KeyChar == 13) {//Enter
                this.executeCommand();
            }
            else if (e.KeyChar == 27) {//Esc
                Hide();
            }
        }

        private void listView1_DoubleClick(object sender, EventArgs e) {
            this.executeCommand();
        }

        private void executeCommand() {
            if (listView1.SelectedItems.Count > 0) {
                string file = this.listView1.SelectedItems[0].Text;
                if (file.StartsWith("@")) {
                    file = file.Substring(1);
                    if (file.Equals("..")) {
                        currentPath = Path.GetDirectoryName(currentPath);
                    }
                    else {
                        currentPath = Path.Combine(currentPath, file);
                    }
                    initFileList();
                    this.textBox1.Focus();
                    this.textBox1.SelectAll();
                }
                else {
                    Hide();
                    file = Path.Combine(currentPath, file);
                    Editor.open(file);
                }
            }
        }


        private void Form1_Deactivate(object sender, EventArgs e)
        {
            Hide();
        }
        #region Windows Form Designer generated code

        private void InitializeComponent() {
            this.components = new System.ComponentModel.Container();
            this.textBox1 = new System.Windows.Forms.TextBox();
            this.listView1 = new System.Windows.Forms.ListView();
            this.columnHeader1 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
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
            this.listView1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.listView1.GridLines = true;
            this.listView1.FullRowSelect = true;
            this.listView1.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.None;
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
            // Form
            //
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(404, 279);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.Controls.Add(this.listView1);
            this.Controls.Add(this.textBox1);
            this.Font = new System.Drawing.Font("Courier New", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.TopMost = true;
            this.Name = "BrowserForm";
            this.Text = "Mini File Browser";
            this.FormBorderStyle = FormBorderStyle.None;
            this.Deactivate += new System.EventHandler(this.Form1_Deactivate);
            this.ShowInTaskbar = false;
            this.ResumeLayout(false);
            this.PerformLayout();
        }

        #endregion

        private System.Windows.Forms.TextBox textBox1;
        private System.Windows.Forms.ListView listView1;
        private System.Windows.Forms.ColumnHeader columnHeader1;
    }

    public override void Run() {
        if (this.tagsForm == null) {
                this.tagsForm =  new TagsForm { BackColor = Color.Aqua };
        }
        if (this.tagsForm.Visible) {
            this.tagsForm.setFocus();
        }
        else {
            this.tagsForm.Show();
            this.tagsForm.setFocus();
        }
    }
}