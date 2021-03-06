﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Cetera;
using Cetera.Archive;
using Cetera.Compression;
using Cetera.Font;
using Cetera.Hardware;
using Cetera.Image;
using Cetera.IO;
using Cetera.Layout;
using Cetera.Text;

namespace CeteraTestApp
{
    public partial class TestAppForm : Form
    {
        void TestFile(string path)
        {
            path = path.ToLower();

            if (Path.GetFileName(path) == "code.bin")
            {
                label1.Text = OnionFS.DoStuff(File.ReadAllBytes(path));
                Debug.WriteLine(label1.Text);
            }

            switch (Path.GetExtension(path))
            {
                case ".bcfnt":
                    var fntz = new BCFNT(File.OpenRead(path));
                    BackgroundImage = fntz.bmps[0];
                    break;
                case ".xi":
                    BackgroundImage = new XI(File.OpenRead(path)).Image;
                    break;
                case ".bclim":
                case ".bflim":
                    BackgroundImage = new BXLIM(File.OpenRead(path)).Image;
                    break;
                case ".jtex":
                    BackgroundImage = new JTEX(File.OpenRead(path), false).Image;
                    break;
                case ".msbt":
                    var msbt = new MSBT(File.OpenRead(path));
                    label1.Text = string.Join("\r\n", msbt.Select(i => $"{i.Label}: {string.Concat(MSBT.ToAtoms(i.Text)).Replace("\n", "\\n")}"));
                    break;
                case ".arc":
                    var arc = new DARC(File.OpenRead(path));
                    label1.Text = string.Join("\r\n", arc.Select(i => $"{i.Path}: {i.Data.Length} bytes"));
                    var ent = arc.FirstOrDefault(i => i.Path.EndsWith("lim"));
                    if (ent != null) BackgroundImage = new BXLIM(new MemoryStream(ent.Data)).Image;
                    break;
            }
        }

        void TestXF(string fontpath, string str)
        {
            var xf = new XF(File.OpenRead(fontpath));
            var test = new Bitmap(800, 40);
            using (var g = Graphics.FromImage(test))
            {
                g.FillRectangle(Brushes.Black, 0, 0, test.Width, test.Height);
                float x = 5;
                foreach (var c in str)
                {
                    x = xf.Draw(c, Color.White, g, x, 5);
                }
            }
            BackgroundImage = test;
        }

        void TestDaigasso()
        {
            var fnt = new BCFNT(GZip.OpenRead(@"C:\fti\dumps\daigassoupdate\ExtractedRomFS\patch\font\Basic.bcfnt.gz"));
            var fntSym = new BCFNT(GZip.OpenRead(@"C:\fti\dumps\daigassoupdate\ExtractedRomFS\patch\font\SisterSymbol.bcfnt.gz"));
            var fntRim = new BCFNT(GZip.OpenRead(@"C:\fti\dumps\daigassoupdate\ExtractedRomFS\patch\font\BasicRim.bcfnt.gz"));
            var bmp = (Bitmap)Image.FromFile(@"C:\fti\other_files\daigasso_box.png");
            fnt.SetColor(Color.Black);
            fntSym.SetColor(Color.Black);            
            using (var g = Graphics.FromImage(bmp))
            {
                g.PixelOffsetMode = System.Drawing.Drawing2D.PixelOffsetMode.HighQuality;
                g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;
                g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.Bicubic;
                var s = "Please select the part to edit or create.\n\n　\uE10D　Lyric-related settings\n　\uE117　Save the song score\n　\uE100　Copy / delete / swap parts\n　\uE101　Song-related settings";
                s = "Slide a note from the note palette and enter it on the staff, slide the area where there is no note, you can select the range.\n　\uE10D　Lyric-related settings\n　\uE117　Save the song score\n　\uE100　Copy / delete / swap parts";
                s = "コードパレットからオリジナルコードを\nスライドして、楽譜上に入力してください。\n　[1]～[4]　　　[オ1]～[オ64]の表示を切り替え\n　[コード設定]　オリジナルコードの設定\n　[基本]　　　　基本コードに切り替え";
                s = "Please choose a chord from the\npalette and enter it into the score.\n　[1] - [4] Toggle display between [1]-[64]\n　[Chord setting] Original chord setting\n　[Basic] Switch to basic chord";
                float txtOffsetX = 32, txtOffsetY = 12;
                float x = 0, y = 0;
                foreach (var c in s)
                {
                    var fntToUse = fnt;
                    if (c >> 12 == 0xE)
                        fntToUse = fntSym;

                    var char_width = fntToUse.GetWidthInfo(c).char_width * 0.6f;
                    if (c == '\n' || x + char_width >= 336)
                    {
                        x = 0;
                        y += fnt.LineFeed * 0.6f;
                        if (c == '\n') continue;
                    }
                    fntToUse.Draw(c, g, x + txtOffsetX, y + txtOffsetY, 0.6f, 0.6f);
                    x += char_width;
                }

                txtOffsetX = 0;
                x = 0;
                y = 133;
                foreach (var c in s)
                {
                    var fntToUse = fntRim;

                    var char_width = fntToUse.GetWidthInfo(c).char_width * 0.87f;
                    if (c == '\n' || x + char_width >= 400)
                    {
                        x = 0;
                        y += fntToUse.LineFeed;
                        if (c == '\n') continue;
                    }
                    fntToUse.Draw(c, g, x + txtOffsetX, y + txtOffsetY, 0.87f, 0.87f);
                    x += char_width;
                }
            }
            BackgroundImage = bmp;
            //BackgroundImage = fntSym.bmp;
        }

        void TestRecompressEtc1()
        {
            var settings = new ImageSettings { Format = Format.ETC1A4 };
            var stp = Stopwatch.StartNew();
            var tmp1 = Common.Save((Bitmap)BackgroundImage, settings);
            Text = stp.Elapsed.ToString();
            stp.Restart();
            BackgroundImage = Common.Load(tmp1, settings);
            //BackgroundImage.Save(@"C:\Users\Adib\Desktop\blergh123.png");
            label1.Text = stp.Elapsed.ToString();
            //label1.Text = Etc1.WorstErrorEver.ToString();
        }

        public void TestLayout(string path)
        {
            var lyt = new BCLYT(File.OpenRead(path));
            var pic1 = lyt.sections.Where(z => z.Magic == "pic1").Select(z => (BCLYT.Pane)z.Object).ToList();
            foreach (var pic in pic1)
            {
                Debug.WriteLine($"<pic1 x=\"{pic.translation.x}\" y=\"{pic.translation.y}\" width=\"{pic.size.x}\" height=\"{pic.size.y}\" imgID=\"{1}\"");
            }
        }

        public void TestListRocketTxt1s()
        {
            foreach (var path in Directory.GetFiles(@"C:\fti\dumps\rocketslime\ExtractedRomFS\data\Game\Layout\", "*.arc", SearchOption.AllDirectories))
            {
                var arc = new DARC(File.OpenRead(path));
                //if (arc.Count(x => Path.GetExtension(x.Path) == ".bclyt") < 1) throw new Exception();
                foreach (var item in arc)
                {
                    if (!item.Path.EndsWith(".bclyt")) continue;
                    //Debug.WriteLine(Path.GetFileName(path) + "\\" + Path.GetFileName(item.Path));
                    var bclyt = new BCLYT(new MemoryStream(item.Data));
                    if (!bclyt.sections.Any(sec => sec.Magic == "txt1")) continue;
                    //if (Path.GetFileNameWithoutExtension(path) != Path.GetFileNameWithoutExtension(item.Path)) Debug.WriteLine(path);
                    Debug.WriteLine(path.Substring(57) + "\\" + Path.GetFileName(item.Path));
                    foreach (var txt in bclyt.sections.Where(s => s.Magic == "txt1"))
                    {
                        var tuple = (Tuple<BCLYT.Pane, BCLYT.TextBox, string>)(txt.Object);
                        var txtBox = tuple.Item2;
                        var txtPane = tuple.Item1;
                        var blah = txtBox.string_length == 0 ? "" : $" charLimit=\"{txtBox.string_length / 2 - 1}\"";
                        if (txtBox.string_length == 0) throw new Exception();
                        Debug.WriteLine('\t' + $"<{txt.Magic} name=\"{txtPane.name}\" width=\"{txtPane.size.x}\"{blah} text=\"{tuple.Item3.Replace("\n", "\\n")}\">");
                    }
                }
            }
        }

        public void TestListDaigassoTxt1s()
        {
            var set = new HashSet<string>();
            foreach (var path in Directory.GetFiles(@"C:\fti\dumps\daigassoupdate\ExtractedRomFS\patch\graphics", "*.arc.gz", SearchOption.AllDirectories))
            {
                var arc = new DARC(GZip.OpenRead(path));
                //if (arc.Count(x => Path.GetExtension(x.Path) == ".bclyt") < 1) throw new Exception();
                foreach (var item in arc)
                {
                    if (!item.Path.EndsWith(".bclyt")) continue;
                    //Debug.WriteLine(Path.GetFileName(path) + "\\" + Path.GetFileName(item.Path));
                    var bclyt = new BCLYT(new MemoryStream(item.Data));
                    if (bclyt.sections.All(sec => sec.Magic != "txt1")) continue;
                    //if (Path.GetFileNameWithoutExtension(path) != Path.GetFileNameWithoutExtension(item.Path)) Debug.WriteLine(path);
                    Debug.WriteLine(path.Substring(57) + "\\" + Path.GetFileName(item.Path));
                    foreach (var txt in bclyt.sections.Where(s => s.Magic == "txt1"))
                    {
                        var tuple = (Tuple<BCLYT.Pane, BCLYT.TextBox, string>)(txt.Object);
                        var txtBox = tuple.Item2;
                        var txtPane = tuple.Item1;
                        //var blah = txtBox.string_length == 0 ? "" : $" charLimit=\"{txtBox.string_length / 2 - 1}\"";
                        if (txtBox.string_length != txtBox.buffer_length) throw new Exception();
                        //Debug.WriteLine('\t' + $"<{txt.Magic} name=\"{txtPane.name}\" size=\"{txtPane.size.x},{txtPane.size.y}\"{blah} text=\"{tuple.Item3.Replace("\n", "\\n")}\">");
                        var sb = new StringBuilder("\t<" + txt.Magic);
                        sb.Append($" name=\"{txtPane.name}\"");
                        sb.Append($" size=\"{txtPane.size}\"");
                        sb.Append($" scale=\"{txtPane.scale}\"");
                        sb.Append($" fontsize=\"{txtBox.font_size}\"");
                        sb.Append($" kerning=\"{txtBox.font_kerning}\"");
                        sb.Append($" postype0=\"{txtPane.base_position_type}\"");
                        sb.Append($" postype1=\"{txtBox.position_type}\"");
                        sb.Append($" alignment=\"{txtBox.text_align}\"");
                        var fnt = (List<string>)bclyt.sections.First(sec => sec.Magic == "fnl1").Object;
                        sb.Append($" font=\"{fnt[txtBox.fontID]}\"");
                        sb.Append($" length=\"{txtBox.string_length}\"");
                        sb.Append($" text=\"{tuple.Item3.Replace("\n", "\\n")}\"");
                        sb.Append(">");
                        Debug.WriteLine(sb.ToString());
                        var x = tuple.Item3.Replace("\n", "\\n");
                        if (x.Contains('|'))
                            set.Add(x);
                    }
                }
            }
            Debug.WriteLine(string.Join("\n", set));
        }

        public void TestDaigassoImageConversion()
        {
            //return;
            foreach (var path in Directory.GetFiles(@"C:\fti\dumps\daigassoupdate\ExtractedRomFS\patch\graphics", "*.arc.gz", SearchOption.AllDirectories))
            {
                var arc = new DARC(GZip.OpenRead(path));

                var ms = GZip.OpenRead(path);
                //using (var bw = new BinaryWriter()
                var newpath = string.Join("_", path.Split('\\').SkipWhile(s => s != "graphics").Skip(1));
                newpath = newpath.Substring(0, newpath.Length - 7);
                foreach (var item in arc)
                {
                    if (Path.GetExtension(item.Path) != ".bclim") continue;
                    //var pngfile = @"C:\fti\dbbp\images\" + $"{newpath}_{Path.GetFileNameWithoutExtension(item.Path)}.png";
                    //if (!File.Exists(pngfile)) continue;

                    //var modified = (Bitmap)Image.FromFile(pngfile);
                    var bclim = new BXLIM(new MemoryStream(item.Data));
                    //bclim.Image = (Bitmap)Image.FromFile(pngfile);
                    //if (bclim.Settings.Format != Format.HL88) continue;
                    //bclim.Image.Save(@"C:\Users\Adib\Desktop\hilo8\" + $"{newpath}_{Path.GetFileNameWithoutExtension(item.Path)}.png");

                }
            }
        }

        public void TestCodeBins()
        {
            foreach (var path in Directory.GetFiles(@"C:\Users\Adib\Desktop\lotsofcode", "*.bin"))
            {
                Debug.WriteLine(path);
                Debug.WriteLine(OnionFS.DoStuff(File.ReadAllBytes(path)));
            }
        }

        public TestAppForm()
        {
            InitializeComponent();
            BackgroundImageLayout = ImageLayout.None;
            AllowDrop = true;
            DragEnter += (s, e) => e.Effect = DragDropEffects.Copy;
            DragDrop += (s, e) => TestFile(((string[])e.Data.GetData(DataFormats.FileDrop)).First());
            Load += DoEverythingElse;
        }

        public void DoEverythingElse(object sender, EventArgs e)
        {
            //var fnt = new BCFNT(File.OpenRead(@"C:\Users\Adib\Desktop\pikachu.bcfnt"));
            //int k = 1;

            TestFile(@"C:\fti\sample_files\topmenu_talk.bflim");
            //TestFile(@"C:\fti\sample_files\flyer.bclim");
            //TestFile(@"C:\fti\sample_files\criware.xi");
            //TestFile(@"C:\fti\dumps\traveler\ExtractedRomFS\ctr\ttp\ar\ar_mikoto.xi");
            //TestFile(@"C:\fti\sample_files\zor_cmbko4.jtex");
            //TestXF(@"C:\fti\sample_files\nrm_main.xf", "Time Travelers （タイムトラベラーズ Taimu Toraberazu） is a video game \"without a genre\" developed by Level-5");
            //TestLayout(@"C:\fti\sample_files\ms_normal.bclyt");
            //TestLayout(@"C:\Users\Adib\Downloads\Game_over.bclyt");
            //TestDaigasso();

            TestCodeBins();

            //TestListDaigassoTxt1s();
            //TestDaigassoImageConversion();




            return;

            ////var lyt = new BCLYT(File.OpenRead(@"C:\Users\Adib\Desktop\ms_normal.bclyt"));
            //var lyt = new BCLYT(File.OpenRead(@"C:\Users\Adib\Desktop\TtrlTxt_U.bclyt"));
            ////return;

            ////var bytes = File.ReadAllBytes(@"C:\Users\Adib\Desktop\Basic.bcfnt").Skip(128).ToArray();
            ////var bytes = File.ReadAllBytes(@"C:\Users\Adib\Desktop\rocket.bcfnt").Skip(128).ToArray();
            ////BackColor = Color.Red;
            ////var bmp = ImageCommon.FromTexture(bytes, 128, 128 * 4, ImageCommon.Format.L4, ImageCommon.Orientation.RightDown);
            ////BackgroundImage = bmp;
            ////bmp.Save(@"C:\Users\Adib\Desktop\rocket.png");
            ////var fnt = new BCFNT(@"C:\Users\Adib\Desktop\MAJOR 3DS CLEANUP\Basic.bcfnt");
            ////var fnt = new BCFNT(File.OpenRead(@"C:\Users\Adib\Desktop\rocket.bcfnt.gz"));
            ////var ms = new MemoryStream();
            ////new GZipStream(File.OpenRead(@"C:\Users\Adib\Desktop\pikachu.bcfnt.gz"), CompressionMode.Decompress).CopyTo(ms);
            ////ms.Position = 0;
            ////var fnt = new BCFNT(ms);
            ////BackgroundImage = fnt.bmp;

            ////return;


            //zzz.Save(@"C:\Users\Adib\Desktop\tmpscreen.png");
        }
    }
}
