//npp_shortcut Ctrl+Alt+P
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

public class Script : NppScript {

    class CommandForm : Form {

        [DllImport("user32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
		public static extern bool GetMenuItemInfo(IntPtr hMenu, int uItem, bool fByPosition, MENUITEMINFO lpmii);

		[DllImport("user32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
		public static extern int GetMenuItemCount(IntPtr hMenu);

        [DllImport("user32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
		public static extern int GetMenuItemID(IntPtr hMenu, int pos);

		[StructLayout(LayoutKind.Sequential)]
		public class MENUITEMINFO {
			public int cbSize;
			public uint fMask;
			public uint fType;
			public uint fState;
			public uint wID;
			public IntPtr hSubMenu;
			public IntPtr hbmpChecked;
			public IntPtr hbmpUnchecked;
			public IntPtr dwItemData;
			public IntPtr dwTypeData;
			public uint cch;
			public IntPtr hbmpItem;

    		public MENUITEMINFO() {
        		cbSize = Marshal.SizeOf(typeof(MENUITEMINFO));
    		}
		}

        public CommandForm() {
            InitializeComponent();
            initCommands();
            listViewInit();
        }

        private System.ComponentModel.IContainer components = null;
        private StringCollection commands = null;

        protected override void Dispose(bool disposing) {
            if (disposing) {
                if (components != null) components.Dispose();
                if (commands != null) commands.Clear();
            }

            base.Dispose(disposing);
        }

        public void setFocus() {
            this.textBox1.Focus();
            this.textBox1.SelectAll();
        }

        private void initCommands() {
            commands = new StringCollection();
            IntPtr hMenu = Win32.SendMessage(Npp.NppHandle, NppMsg.NPPM_GETMENUHANDLE, 1, 0);
            MENUITEMINFO mif = new MENUITEMINFO();
			string caption1, caption2, caption3;
            int cmdId = 0;
            IntPtr hSubMenu, hSubSubMenu;
			for (int i = 0; i < GetMenuItemCount(hMenu); i++) {
				mif.fMask = 0x00000040;
				mif.fType = 0x00000000;
				mif.dwTypeData = IntPtr.Zero;
				bool res = GetMenuItemInfo(hMenu, i, true, mif);
				if (!res) continue;
				mif.cch++;
				mif.dwTypeData = Marshal.AllocHGlobal((IntPtr)(mif.cch*2));
				try {
    				res = GetMenuItemInfo(hMenu, i, true, mif);
    				if (!res) continue;
    				caption1 = Marshal.PtrToStringUni(mif.dwTypeData);
                    if (caption1.Equals("jN") || caption1.Equals("&?") || caption1.Equals("&Language"))
                        continue;
				}
				finally {
    				Marshal.FreeHGlobal(mif.dwTypeData);
				}

                mif.fMask = 0x00000004;
                res = GetMenuItemInfo(hMenu, i, true, mif);
                if (!res) continue;
                hSubMenu = mif.hSubMenu;

                //sub menu
                for (int j = 0; j < GetMenuItemCount(hSubMenu); j++) {
                    mif.fMask = 0x00000040;
					mif.fType = 0x00000000;
					mif.dwTypeData = IntPtr.Zero;
					res = GetMenuItemInfo(hSubMenu, j, true, mif);
					if (!res) continue;
					mif.cch++;
					mif.dwTypeData = Marshal.AllocHGlobal((IntPtr)(mif.cch*2));
					try {
    					res = GetMenuItemInfo(hSubMenu, j, true, mif);
    					if (!res) continue;
    					caption2 = Marshal.PtrToStringUni(mif.dwTypeData);
                        caption2 = caption2.Replace("&", "");
					}
					finally {
    					Marshal.FreeHGlobal(mif.dwTypeData);
					}
                    if (string.IsNullOrWhiteSpace(caption2)) continue;

                    mif.fMask = 0x00000004;
                    res = GetMenuItemInfo(hSubMenu, j, true, mif);
                    if (!res) continue;
                    hSubSubMenu = mif.hSubMenu;
                    if ((int)hSubSubMenu < 1) {
                        cmdId = GetMenuItemID(hSubMenu, j);
                        caption2 = Regex.Replace(caption2, "(Alt|Ctrl|Shift).*$", "");
                        caption2 = Regex.Replace(caption2, "F[0-9]{1,2}.*$", "");
                        commands.Add(caption1.Replace("&", "") + "::" + caption2 + "@Menu@" + cmdId);
                    }
                    else {
                        for (int k = 0; k < GetMenuItemCount(hSubSubMenu); k++) {
                            mif.fMask = 0x00000040;
                            mif.fType = 0x00000000;
                            mif.dwTypeData = IntPtr.Zero;
                            res = GetMenuItemInfo(hSubSubMenu, k, true, mif);
                            if (!res) continue;
                            mif.cch++;
                            mif.dwTypeData = Marshal.AllocHGlobal((IntPtr)(mif.cch*2));
                            try {
                                res = GetMenuItemInfo(hSubSubMenu, k, true, mif);
                                if (!res) continue;
                                caption3 = Marshal.PtrToStringUni(mif.dwTypeData);
                                if (caption3.IndexOf("About") > -1) continue;
                            }
                            finally {
                                Marshal.FreeHGlobal(mif.dwTypeData);
                            }
                            if (string.IsNullOrWhiteSpace(caption3)) continue;

                            mif.fMask = 0x00000004;
                            res = GetMenuItemInfo(hSubSubMenu, k, true, mif);
                            if (!res) continue;
                            cmdId = GetMenuItemID(hSubSubMenu, k);
                            caption3 = caption3.Replace("&", "");
                            caption3 = Regex.Replace(caption3, "(Alt|Ctrl|Shift).*$", "");
                            caption3 = Regex.Replace(caption3, "F[0-9]{1,2}.*$", "");
                            commands.Add(caption1.Replace("&", "") + "::" + caption2 + "::" + caption3 + "@Menu@" + cmdId);
                        }
                    }
                }
            }
        }

        private void listViewInit() {
            listView1.BeginUpdate();
            for (int i = 0; i < commands.Count; i++) {
                string[] arr = commands[i].Split('@');
                ListViewItem lvi = new ListViewItem();
                lvi.Tag = "" + i;
                lvi.Text = arr[0];
                if (i % 2 == 1) {
                    lvi.BackColor = Color.BlanchedAlmond;
                }
                listView1.Items.Add(lvi);
            }
            listView1.EndUpdate();
        }

        private void textBox1_TextChanged(object sender, EventArgs e) {
            string text = textBox1.Text.Trim().ToUpper();
            if (text.Length < 2) return;
            int j = 0;
            listView1.BeginUpdate();
            listView1.Items.Clear();
            for (int i = 0; i < commands.Count; i++) {
                string[] arr = commands[i].Split('@');
                if (arr[0].ToUpper().IndexOf(text) == -1) continue;
                ListViewItem lvi = new ListViewItem();
                lvi.Tag = "" + i;
                lvi.Text = arr[0];
                if (j++ % 2 == 1) {
                    lvi.BackColor = Color.BlanchedAlmond;
                }
                listView1.Items.Add(lvi);
            }
            listView1.EndUpdate();
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
            // else MessageBox.Show("" + e.KeyValue);
        }

        private void listView1_KeyPress(object sender, KeyPressEventArgs e) {
            if (e.KeyChar == 9) { //Tab
                this.textBox1.Focus();
            }
            else if (e.KeyChar == 13) {//Enter
                Hide();
                this.executeCommand();
            }
            else if (e.KeyChar == 27) {//Esc
                Hide();
            }
        }

        private void listView1_DoubleClick(object sender, EventArgs e) {
            Hide();
            this.executeCommand();
        }

        private void executeCommand() {
            if (listView1.SelectedItems.Count > 0) {
                int idx = int.Parse(listView1.SelectedItems[0].Tag.ToString());
                // MessageBox.Show(commands[idx]);
                string[] arr = commands[idx].Split('@');
                switch(arr[1]) {
                    case "Script":
                        NppScript script = Plugin.GetScriptByFileName(arr[2]);
                        if (script != null) script.Run();
                    break;
                    case "Menu":
                        Win32.SendMessage(Npp.NppHandle, NppMsg.NPPM_MENUCOMMAND, 0, int.Parse(arr[2]));
                    break;
                    default:
                    break;
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
            //this.listView1.BackColor = System.Drawing.SystemColors.InactiveCaptionText;
            this.listView1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.listView1.GridLines = true;
            this.listView1.FullRowSelect = true;
            this.listView1.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.None;
            this.listView1.HideSelection = false;
            this.listView1.MultiSelect = false;
            this.listView1.Location = new System.Drawing.Point(0, 20);
            this.listView1.Name = "listView1";
            this.listView1.Size = new System.Drawing.Size(500, 259);
            this.listView1.TabIndex = 1;
            this.listView1.UseCompatibleStateImageBehavior = false;
            this.listView1.View = System.Windows.Forms.View.Details;
            this.listView1.DoubleClick += new System.EventHandler(this.listView1_DoubleClick);
            this.listView1.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.listView1_KeyPress);

            //
            // columnHeader1
            //
            this.columnHeader1.Text = "Name";
            this.columnHeader1.Width = 500;

            //
            // Form
            //
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(504, 279);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.Controls.Add(this.listView1);
            this.Controls.Add(this.textBox1);
            this.Font = new System.Drawing.Font("Courier New", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.TopMost = true;
            this.Name = "CommandForm";
            this.Text = "Commander";
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

    private CommandForm form = null;

    public override void Run() {
        // MessageBox.Show("" + Plugin.FuncItems.Items[this.ScriptId]._cmdID);
        if (form == null) {
            form =  new CommandForm { BackColor = Color.Aqua };
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