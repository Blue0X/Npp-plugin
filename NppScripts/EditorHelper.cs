using System.Text;
using System;
using System.Drawing;
using System.Text.RegularExpressions;

namespace NppScripts.EditorHelper {
    class Editor {
        /**
         * 取光标处单词
         * @param $setSelected 是否选中
         */
        static public string getWordAtCursor(bool setSelected = true) {
            Point p;
            string word = "";
            Npp.GetWordAtCursor(out p);
            if (p.X != p.Y) {
                string text = Npp.GetTextBetween(p.X, p.Y);
                Match match = Regex.Match(text, "([0-9A-Za-z_]+)$");
                if (match.Success) {
                    word = match.Groups[1].Value;
                    if (setSelected) {
                        p.X = p.X + (text.Length - word.Length);
                        Win32.SendMessage(Npp.CurrentScintilla, SciMsg.SCI_SETSELECTION, p.X, p.Y);
                    }
                }
            }
            return word;
        }

        /**
         * 打开文件
         */
        static public void open(string path, int line = 0) {
            Win32.SendMessage(Npp.NppHandle, NppMsg.NPPM_DOOPEN, 0, path);
            if (line > 0) gotoLine(line);
        }

        /**
         * 定位行
         */
        static public void gotoLine(int line) {
            Win32.SendMessage(Npp.CurrentScintilla, SciMsg.SCI_GOTOLINE, line, 0);
            int moveline = (int)Win32.SendMessage(Npp.CurrentScintilla, SciMsg.SCI_GETFIRSTVISIBLELINE, 0, 0);
            if (line - moveline < 20) {
                moveline = -20;
            }
            else if (line - moveline > 20) {
                moveline = 20;
            }
            else {
                moveline = 0;
            }
            if (moveline != 0) {
                Win32.SendMessage(Npp.CurrentScintilla, SciMsg.SCI_LINESCROLL, 0, moveline);
            }
        }

        static public void gotoPos(int pos) {
            Win32.SendMessage(Npp.CurrentScintilla, SciMsg.SCI_GOTOPOS, pos, 0);
        }

        static public int getCurrentPos() {
            return (int)Win32.SendMessage(Npp.CurrentScintilla, SciMsg.SCI_GETCURRENTPOS, 0, 0);
        }

        static public int getCurrentColumn() {
            return (int) Win32.SendMessage(Npp.CurrentScintilla, SciMsg.SCI_GETCOLUMN, 0, 0);
        }

        static public string getCurrentPath() {
            string path;
            Win32.SendMessage(Npp.NppHandle, NppMsg.NPPM_GETCURRENTDIRECTORY, 0, out path);
            return path;
        }

        static public string getCurrentLine() {
            string line;
            Win32.SendMessage(Npp.CurrentScintilla, SciMsg.SCI_GETCURLINE, 0, out line);
            return line;
        }

        static public void setUTF8WithoutBOM() {
            Win32.SendMessage(Npp.NppHandle, NppMsg.NPPM_MENUCOMMAND, 0, 45008); //utf-8
        }

        static public void setUTF8() {
            Win32.SendMessage(Npp.NppHandle, NppMsg.NPPM_MENUCOMMAND, 0, 45005); //utf-8
        }

        static public void replaceSel(string text) {
            Win32.SendMessage(Npp.CurrentScintilla, SciMsg.SCI_REPLACESEL, 0, text);
        }
    }
}