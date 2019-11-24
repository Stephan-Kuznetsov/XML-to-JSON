using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Controls;
using System.Threading.Tasks;
using InterfaceLibrary1;
using System.Windows.Media;
using System.ComponentModel;

namespace XmlToJsonConverter
{
    class XmlProcessorData
    {
        public bool BrokenFile, ReadyFile;
        private ConverterXmlToJson Converter;
        private TextBlock State;
        public TextView FileName, NameL, Progress;
        public ImageButton Remove;
        public ImageButton OpenConverted;
        private string name;
        private string saved;

        public XmlProcessorData(string filepath, int index, Grid g)
        {
            Converter = new ConverterXmlToJson(filepath);
            int begin = filepath.LastIndexOf('\\');
            int end = filepath.LastIndexOf(".xml");
            name = filepath.Substring(begin + 1, end-1 -begin);
            float OffsX = 5;
            NameL = new TextView(OffsX, 5 + 35 * index, 70, 25, "Файл " + (index+1), CustomStyles.TitleStyle);
            OffsX += 75;
            FileName = new TextView(OffsX, 5 + 35 * index, 225, 25, name, CustomStyles.TitleStyle);
            FileName.ToolTip = name;
            FileName.TextWrapping = System.Windows.TextWrapping.NoWrap;
            OffsX += 235;
            Progress = new TextView(OffsX, 5 + 35 * index, 80, 25, "Состояние");
            OffsX += 85;
            State = new TextBlock();
            State.Margin = new System.Windows.Thickness(OffsX, 5 + 35 * index, 0, 0);
            State.Width = 100;
            State.Height = 25;
            State.Text = "Ожидание";
            State.HorizontalAlignment = System.Windows.HorizontalAlignment.Left;
            State.VerticalAlignment = System.Windows.VerticalAlignment.Top;
            State.TextAlignment = System.Windows.TextAlignment.Center;
            State.FontSize = 16;
            State.Background = new SolidColorBrush(Color.FromArgb(255, 37, 38, 41));
            State.Foreground= Brushes.White;
            OffsX += 105;
            Remove = new ImageButton(OffsX, 5 + 35 * index, 30, 25, ConverterStylesData.RemoveImg);
            Remove.Index = index;
            OffsX += 35;
            OpenConverted = new ImageButton(OffsX, 5 + 35 * index, 30, 25, ConverterStylesData.RedEye);
            OpenConverted.ToolTip = "Просмотр доступен только после конвертации";
            OffsX += 35;
            OpenConverted.MouseUp += OpenInBrowser;
            OpenConverted.IsEnabled = false;

            g.Children.Add(NameL);
            g.Children.Add(FileName);
            g.Children.Add(Progress);
            g.Children.Add(State);

            Remove.AddToControl(g);
            OpenConverted.AddToControl(g);
        }
        public void BeginConvertation()
        {
            Converter.Convert();
            BrokenFile = Converter.IsBroken;
            if (!Converter.IsBroken)
            {
                saved = Converter.SaveResultInJson();
            }
        }

        private void OpenInBrowser(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            System.Diagnostics.Process.Start(saved);
        }
        public void ProccessEnd()
        {
            if (!BrokenFile)
            {
                State.Foreground = Brushes.ForestGreen;
                State.Text = "Успех";
                OpenConverted.SetContentImage(ConverterStylesData.ShowImg);
                OpenConverted.ToolTip = "Просмотр конвертированного файла в браузере";
                OpenConverted.IsEnabled = true;
            }
            else
            {
                State.Foreground = Brushes.Red;
                State.Text = "Ошибка";
            }
            Remove.IsEnabled = false;
            ReadyFile = true;
        }
        public void RemoveFromView(Grid g)
        {
            g.Children.Remove(State);
            g.Children.Remove(FileName);
            Remove.RemoveFromScreen(g);
            OpenConverted.RemoveFromScreen(g);
            g.Children.Remove(NameL);
            g.Children.Remove(Progress);
        }
        public void AddToView(Grid g)
        {
            g.Children.Add(State);
            g.Children.Add(FileName);
            Remove.AddToControl(g);
            OpenConverted.AddToControl(g);
            g.Children.Add(NameL);
            g.Children.Add(Progress);
        }
        public void MoveResult(string path)
        {
            path += "\\" + name + "_converted.json";
            System.IO.File.Move(saved, path);
        }
        public void SetIndex(int index)
        {
            Remove.Index = index;
            float OffsX = 5;
            NameL.Margin = new System.Windows.Thickness(OffsX, 5 + 35 * index, 0, 0);
            OffsX += 75;
            FileName.Margin = new System.Windows.Thickness(OffsX, 5 + 35 * index, 0, 0);
            OffsX += 205;
            Progress.Margin = new System.Windows.Thickness(OffsX, 5 + 35 * index, 0, 0);
            OffsX += 105;
            State.Margin = new System.Windows.Thickness(OffsX, 5 + 35 * index, 0, 0);
            OffsX += 105;
            Remove.SetPosition(OffsX, 5 + 35 * index);
            OffsX += 35;
            OpenConverted.SetPosition(OffsX, 5 + 35 * index);
            OffsX += 35;
        }
        public void ClearConverter()
        {
            Converter = null;
            GC.Collect();
        }
    }
}
