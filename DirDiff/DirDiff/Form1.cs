using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace DirDiff
{
    public partial class DirDiffForm : Form
    {
        Dictionary<string, bool> _names1;
        Dictionary<string, bool> _names2;
        public DirDiffForm()
        {
            _names1 = new Dictionary<string, bool>();
            _names2 = new Dictionary<string, bool>();
            InitializeComponent();
            //this.listBox1.Items.AddRange(new List<string> { "1", "2" }.ToArray());
            //this.listBox1.DataSource = new List<string> { "1", "2" };
            listBox1.MouseWheel += ListBox1_MouseWheel;
            listBox2.MouseWheel += ListBox2_MouseWheel;

            InitByClipBoard();
        }



        private void listBox1_DrawItem(object sender, DrawItemEventArgs e)
        {
            if (listBox1.Items.Count <= 0)
                return;
            string k = listBox1.GetItemText(listBox1.Items[e.Index]);
            
            if (_names1.ContainsKey(k) && _names1[k] == false)
            {
                e.Graphics.FillRectangle(new SolidBrush(Color.LightPink), e.Bounds);                
            }
            else
            {
                e.Graphics.DrawRectangle(new Pen(Color.LightGreen, 2), e.Bounds);
            }
            e.Graphics.DrawString(k, this.Font, new SolidBrush(Color.Black), new PointF(e.Bounds.X, e.Bounds.Y));
            
        }

        private void listBox2_DrawItem(object sender, DrawItemEventArgs e)
        {
            if (listBox2.Items.Count <= 0)
                return;
            string k = listBox2.GetItemText(listBox2.Items[e.Index]);

            if (_names2.ContainsKey(k) && _names2[k] == false)
            {
                e.Graphics.FillRectangle(new SolidBrush(Color.LightPink), e.Bounds);
            }
            else
            {
                e.Graphics.DrawRectangle(new Pen(Color.LightGreen, 2), e.Bounds);
            }
            e.Graphics.DrawString(k, this.Font, new SolidBrush(Color.Black), new PointF(e.Bounds.X, e.Bounds.Y));
        }


        private void ListBox1_MouseWheel(object sender, MouseEventArgs e)
        {
            int top1 = listBox1.TopIndex;
            listBox2.TopIndex = top1;// > listBox2.Items.Count
        }
        private void ListBox2_MouseWheel(object sender, MouseEventArgs e)
        {
            int top2 = listBox2.TopIndex;
            listBox1.TopIndex = top2;// > listBox2.Items.Count
        }

        private void listBox1_KeyDown(object sender, KeyEventArgs e)
        {
            if(e.KeyData == (Keys.V|Keys.Control))
            {
                var names = GetCopiedPaths();
                SetList1(names);
            }
            if (e.KeyData == Keys.Delete)
            {
                SetList1(new List<string>());
            }
        }

        private void listBox2_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyData == (Keys.V | Keys.Control))
            {
                var names = GetCopiedPaths();
                SetList2(names);
            }
            if (e.KeyData == Keys.Delete)
            {
                SetList2(new List<string>());
            }
        }


        private void btn1_Click(object sender, EventArgs e)
        {
            using (var dlg = new FolderBrowserDialog())
            {
                var ret = dlg.ShowDialog();
                if (ret == DialogResult.OK)
                {
                    var dir = dlg.SelectedPath;
                    this.path1.Text = dir;
                    var files = Directory.GetFileSystemEntries(dir);
                    var names = files.Select(x => Path.GetFileName(x)).ToList();
                    SetList1(names);
                }
            }
        }

        private void btn2_Click(object sender, EventArgs e)
        {
            using (var dlg = new FolderBrowserDialog())
            {
                var ret = dlg.ShowDialog();
                if (ret == DialogResult.OK)
                {
                    var dir = dlg.SelectedPath;
                    this.path2.Text = dir;
                    var files = Directory.GetFileSystemEntries(dir);
                    var names = files.Select(x => Path.GetFileName(x)).ToList();
                    SetList2(names);
                }
            }
        }


        private void SetList1(List<string> names1)
        {
            names1.Sort();
            _names1 = names1.ToDictionary(k => k, v => true);
            Compare();
            listBox1.DataSource = _names1.Keys.ToList();
            listBox1.Refresh();
        }

        private void SetList2(List<string> names2)
        {
            names2.Sort();
            _names2 = names2.ToDictionary(k => k, v => true);
            Compare();
            listBox2.DataSource = _names2.Keys.ToList();
            listBox2.Refresh();
        }

        private void Compare()
        {
            if (_names1.Any())
            {
                if (_names2.Any())
                {
                    foreach (string k1 in _names1.Keys.ToList())
                    {
                        _names1[k1] = _names2.Keys.Contains(k1);
                    }
                }
                else
                {
                    foreach (string k1 in _names1.Keys.ToList())
                    {
                        _names1[k1] = true;
                    }
                }                
            }
            if(_names2.Any())
            {
                if (_names1.Any())
                {
                    foreach (string k2 in _names2.Keys.ToList())
                    {
                        _names2[k2] = _names1.Keys.Contains(k2);
                    }
                }
                else
                {
                    foreach (string k2 in _names2.Keys.ToList())
                    {
                        _names2[k2] = true;
                    }
                }
            }
            listBox1.Refresh();
            listBox2.Refresh();
        }

        private void InitByClipBoard()
        {
            List<string> dirs = new List<string>();
            List<string> copys = new List<string>();
            var paths = Clipboard.GetFileDropList();
            foreach(string path in paths)
            {
                copys.Add(path);
                if (Directory.Exists(path))
                {
                    dirs.Add(path);
                }
            }
            if(dirs.Any())
            {
                if (dirs.Count <= 2)
                {
                    var files1 = Directory.GetFileSystemEntries(dirs[0]);
                    var names1 = files1.Select(x => Path.GetFileName(x)).ToList();
                    SetList1(names1);
                    if (dirs.Count == 2)
                    {
                        var files2 = Directory.GetFileSystemEntries(dirs[1]);
                        var names2 = files2.Select(x => Path.GetFileName(x)).ToList();
                        SetList2(names2);
                    }
                }
                else if (dirs.Count > 2)
                {
                    var names1 = copys.Select(x => Path.GetFileName(x)).ToList();
                    SetList1(names1);
                }
                
            }
        }

        private List<string> GetCopiedPaths()
        {
            List<string> copys = new List<string>();
            var paths = Clipboard.GetFileDropList();
            foreach (string path in paths)
            {
                copys.Add(path);
            }
            var names = copys.Select(x => Path.GetFileName(x)).ToList();
            return names;
        }
    }
}
