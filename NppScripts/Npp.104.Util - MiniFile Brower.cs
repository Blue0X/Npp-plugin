//npp_shortcut Alt+O
//css_inc EditorHelper.cs
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.IO;
using System.Text;
using System.Windows.Forms;
using System.Text.RegularExpressions;
using NppScripts;

using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Windows;
using System.Reflection;
using NppScripts.EditorHelper;

/**
 * 特殊输入：
 *   @ - 搜索目录
  *  ! - 命令
  *     !$directory - 更改目录
  *     !filename - 创建文件
  *     !!filename - 删除文件
  *     !@directory - 创建目录
  *     !!@directory - 删除目录
  *
  *     !#         - 书签
  *     !#add      - 添加当前路径
  *     !#rm  num  - 删除书签, num为从1开始的序号
 */
public class Script : NppScript {

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
                            if (!string.IsNullOrEmpty(text.Substring(1)) && !File.Exists(file)) {
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

    private BrowserForm form = null;

    public override void Run() {
        if (form == null) {
            form =  new BrowserForm { BackColor = Color.Aqua };
        }
        if (form.Visible) {
            form.setFocus();
        }
        else {
            form.Show();
            form.setFocus();
        }
    }
}