using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Collections;
using System.IO;
using JiebaNet;
using JiebaNet.Segmenter.PosSeg;

namespace NovelGen
{
    public partial class Form1 : Form
    {

        private ArrayList text = new ArrayList();
        private ArrayList inds = new ArrayList();
        private ArrayList vec = new ArrayList();
        private string[] lines;
        private int[] tK;
        private PosSegmenter posSeg;
        private Dictionary<string,int> keyWords = new Dictionary<string,int>();
        public Form1()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            var rng = new Random();
            int st= rng.Next(0, lines.Length-1);
            text = new ArrayList();
            inds = new ArrayList();
            inds.Add(st);
            text.Add(modify(lines[st]));

            label2.Text = lines[st];

            refresh();
    }

        private void listBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            label1.Text = (String)listBox1.SelectedItem;
        }

        private string modify(string s)
        {

            foreach(var p in listBox2.Items)
            {
                string a = ((KeyValuePair<string, string>)p).Key;
                string b = ((KeyValuePair<string, string>)p).Value;
                s=s.Replace(a, b);
            }
            return s;
        }

        private void listBox1_DoubleClick(object sender, EventArgs e)
        {

            int chs = listBox1.SelectedIndex;
            text.Add(modify(lines[tK[chs] + 1]));
            inds.Add(tK[chs] + 1);




            refresh();
        }

        private void 另存为ToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            string localFilePath = ""; 
            SaveFileDialog sfd = new SaveFileDialog(); 
            sfd.Filter = "文本文件|*.txt"; 
            sfd.FilterIndex = 1; 
            sfd.RestoreDirectory = true; 
            if (sfd.ShowDialog() == DialogResult.OK)
            {
                localFilePath = sfd.FileName.ToString();  
                File.AppendAllLines(localFilePath, text.Cast<string>());
            }

        }

        private void 打开ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            OpenFileDialog fileDialog = new OpenFileDialog();
            fileDialog.Multiselect = true;
            fileDialog.Title = "请选择文件";
            fileDialog.Filter = "文本文件|*.txt*"; 
            if (fileDialog.ShowDialog() == DialogResult.OK)
            {
                string file = fileDialog.FileName;     
                label3.Text = file;
            }
        }

        public class KeyCompare : IComparer
        {
            public int Compare(object manA, object manB)
            {
                var man1 = (KeyValuePair<int, string>)manA;
                var man2 = (KeyValuePair<int, string>)manB;
                return man1.Key.CompareTo(man2.Key);
            }
        }
        public class KeyCompare2 : IComparer
        {
            public int Compare(object manA, object manB)
            {
                var man1 = (KeyValuePair<double, int>)manA;
                var man2 = (KeyValuePair<double, int>)manB;
                return man1.Key.CompareTo(man2.Key);
            }
        }
        private void button2_Click(object sender, EventArgs e)
        {

         

            lines = File.ReadAllLines(label3.Text);
            
           

            var dic = new Dictionary<string, int>();
            int all = lines.Length;
            int cnt = 0;
            posSeg = new PosSegmenter();
            foreach (var l in lines){
                cnt++;
                progressBar1.Value = 50*cnt / all;
                var tokens = posSeg.Cut(l);
                foreach(var p in tokens)
                {
                    if (p.Flag != "n")
                        continue;
                    if (dic.ContainsKey(p.Word))
                    {
                        dic[p.Word] += 1;
                    }
                    else
                    {
                        dic[p.Word] = 1;
                    }
                }
            }

            var arr = new ArrayList();
            foreach(var p in dic)
            {
                arr.Add(new KeyValuePair<int,string>(p.Value, p.Key));
            }

            arr.Sort(new KeyCompare());
            arr.Reverse();
            keyWords = new Dictionary<string, int>();
            for(int i = 0; i < 100; i++)
            {
                var p = (KeyValuePair<int, string>)arr[i];
                keyWords[p.Value] = 1;
                 
            }

            vec = new ArrayList();
            var last = new Dictionary<string, int>();
            foreach(var p in keyWords)
            {
                last[p.Key] = -1000;
            }
            for(int i = 0; i < lines.Length; i++)
            {
                progressBar1.Value = 50 + 50 * (i+1) / all;
                var tokens = posSeg.Cut(lines[i]);
                foreach(var p in tokens)
                {
                    if (keyWords.ContainsKey(p.Word))
                    {
                        last[p.Word] = i;
                    }
                }
                double s = 0;
                double[] v = new double[keyWords.Count];
                int j = 0;
                foreach(var p in keyWords)
                {
                    double t = 1.0 / (i - last[p.Key] + 1);
                    s += t * t;
                    v[j] = t;
                    j++;
                }
                s = Math.Sqrt(s);
                for (int k = 0; k < v.Length; k++)
                    v[k] /= s;
                vec.Add(v);
            }
            button1_Click(null, null);
        }
        private int[] topK(int ind,int k = 8)
        {
            int[] res = new int[k+1];
            int gap = 15;
            var arr = new ArrayList(); 
                
            for (int i = 0; i < lines.Length; i++)
            {
                if (ind - gap < i && i < ind + gap)
                    continue;

                double d=0;
                double[] v1 = (double[])vec[i];
                double[] v2 = (double[])vec[ind];
                for (int j = 0; j < v1.Length; j++)
                    d += v1[j] * v2[j]; 
                arr.Add(new KeyValuePair<double, int>(d, i));
            }
            arr.Sort(new KeyCompare2());
            arr.Reverse();
            res[0] = ind;
            for (int i = 0; i < k; i++)
                res[i+1] = ((KeyValuePair<double, int>)arr[i]).Value;
            return res;
        }

        private void refresh()
        {
            int len = 1;
            label2.Text = "";
            int s = 0;
            for(int i = 1; i <= 5; i++)
            {
                if (text.Count - i < 0)
                    continue;
                s += ((string)text[text.Count - i]).Length;
                if (s < 400)
                {
                    len++;
                }
            }
            for (int i = len; i >= 1; i--)
            {
                if (text.Count - i < 0)
                    continue;
                label2.Text += text[text.Count - i];
                label2.Text += "\n";
            }

            int cur = (int)inds[inds.Count - 1];
            tK = topK(cur);
            listBox1.Items.Clear();
            for(int i = 0; i < tK.Length; i++)
            {
                listBox1.Items.Add(lines[tK[i]+1]);
            }

            listBox1.SetSelected(0, true);
        }

        private void button3_Click(object sender, EventArgs e)
        {
            text.RemoveAt(text.Count - 1);
            inds.RemoveAt(inds.Count - 1);
            refresh();
        }

        private void button4_Click(object sender, EventArgs e)
        {
            string a = fromA.Text;
            string b = toB.Text;

            listBox2.Items.Add(new KeyValuePair<string, string>(a, b));
                
         }

        private void button5_Click(object sender, EventArgs e)
        {
            if(listBox2.SelectedItem!=null)
                listBox2.Items.Remove(listBox2.SelectedItem);
        }

        private void listBox1_KeyPress(object sender, KeyPressEventArgs e)
        {
            e.Handled = true;
            if (e.KeyChar == (char)13)
            {
                listBox1_DoubleClick(null, null);
                return;
            }
            if (e.KeyChar==(char)Keys.Back)
            {


                button3_Click(null, null);
                return;
            }
        }

        private void button6_Click(object sender, EventArgs e)
        {
            
            int st = int.Parse(textBox1.Text);
            if (st < 0 || st >= lines.Length)
                return;
            text = new ArrayList();
            inds = new ArrayList();
            inds.Add(st);
            text.Add(modify(lines[st]));

            label2.Text = lines[st];

            refresh();
        }

        private void button7_Click(object sender, EventArgs e)
        {
            string all = "";
            foreach(var s in text)
            {
                all += s;
                all += "\n";
            }
            Clipboard.SetText(all);
        }
    }
}
