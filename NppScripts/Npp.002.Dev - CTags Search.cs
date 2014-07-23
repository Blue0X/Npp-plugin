//npp_shortcut Ctrl+J
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

    class TagsForm : Form {

        public TagsForm() {
            InitializeComponent();
            LoadTagsFile();
        }

        private string projectRoot = "F:\\www\\local\\ThinkQ\\";

        private System.ComponentModel.IContainer components = null;
        private System.Collections.Specialized.StringCollection tagLines = null;

        protected override void Dispose(bool disposing) {
            Hide();
        }

        public void setFocus() {
            Point p;

            if (!this.textBox1.Text.Equals("")) {
                //取光标处单词
                Npp.GetWordAtCursor(out p);
                if (p.X != p.Y) {
                    string text = GetTextBetween(p.X, p.Y);
                    Match match = Regex.Match(text, "([0-9A-Za-z_]+)$");
                    if (match.Success) {
                        this.textBox1.Text = match.Groups[1].Value;
                    }
                }

                this.textBox1.SelectAll();
            }
            this.textBox1.Focus();
        }

        private string GetTextBetween(int start, int end = -1) {
            IntPtr sci = Plugin.GetCurrentScintilla();

            if (end == -1)
                end = (int)Win32.SendMessage(sci, SciMsg.SCI_GETLENGTH, 0, 0);

            using (var tr = new Sci_TextRange(start, end, end - start + 1)) //+1 for null termination
            {
                Win32.SendMessage(sci, SciMsg.SCI_GETTEXTRANGE, 0, tr.NativePointer);
                return tr.lpstrText;
            }
        }

        private void LoadTagsFile() {
            string strLine;
            string file = projectRoot + "ctags.tmp";
            tagLines = new StringCollection();
            if (!File.Exists(file)) return;
            FileStream aFile = new FileStream(file, FileMode.Open);
            StreamReader sr = new StreamReader(aFile);
            strLine = sr.ReadLine();
            this.listView1.BeginUpdate();
            while(strLine != null) {
                if (!strLine.StartsWith("!")) {
                    this.tagLines.Add(strLine);
                    string[] arr = strLine.Split('\t');
                    ListViewItem lvi = new ListViewItem();
                    lvi.Text = arr[0];
                    lvi.SubItems.Add(arr[3]);
                    lvi.SubItems.Add(arr[2].Replace(";\"", ""));
                    lvi.SubItems.Add(arr[1]);
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
            string text = this.textBox1.Text.ToUpper();
            text = text.Trim();
            if (text.Equals("@")) return;

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
                lvi.Text = arr[0];
                lvi.SubItems.Add(arr[3]);
                lvi.SubItems.Add(arr[2].Replace(";\"", ""));
                lvi.SubItems.Add(arr[1]);
                this.listView1.Items.Add(lvi);
            }

            this.listView1.EndUpdate();
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
            string file, linenumber;
            if(this.listView1.SelectedItems.Count > 0) {
                file = this.listView1.SelectedItems[0].SubItems[3].Text;
                linenumber = this.listView1.SelectedItems[0].SubItems[2].Text;
                Win32.SendMessage(Npp.NppHandle, NppMsg.NPPM_DOOPEN, 0, projectRoot + file);
                Win32.SendMessage(Npp.CurrentScintilla, SciMsg.SCI_GOTOLINE, int.Parse(linenumber), 0);
            }
        }

        private void insertSelectedTag(bool replace = false) {
            if(this.listView1.SelectedItems.Count > 0) {
                string tag = this.listView1.SelectedItems[0].Text;
                if (replace) {
                    Point p;
                    Npp.GetWordAtCursor(out p);
                    if (p.X != p.Y) {
                        Win32.SendMessage(Npp.CurrentScintilla, SciMsg.SCI_SETSELECTION, p.X, p.Y);
                    }
                }
                Win32.SendMessage(Npp.CurrentScintilla, SciMsg.SCI_REPLACESEL, 0, tag);
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
            this.TopMost = true;
            this.Controls.Add(this.listView1);
            this.Controls.Add(this.textBox1);
            this.Name = "TagsForm";
            this.Text = "CTags Search - " + projectRoot;
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