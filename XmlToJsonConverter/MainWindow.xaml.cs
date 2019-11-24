using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Collections;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.ComponentModel;
using System.Xml;
using System.Threading;
using InterfaceLibrary1;

namespace XmlToJsonConverter
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private int ConvertedFiles;
        private int Files = 0;
        private double ProgressFactor;
        private List<XmlProcessorData> FilesView;

        private Thread ConvertThread;

        private bool SaveOriginalPath = true;

        // Interface
        private MyButton AddFiles, AddDirectory, ClearListBut;
        public MainWindow()
        {
            InitializeComponent();
            ConverterStylesData.SetStyles();
            ConverterStylesData.RemoveImg = (BitmapImage)TryFindResource("Remove");
            ConverterStylesData.ShowImg = (BitmapImage)TryFindResource("Open");
            ConverterStylesData.RedEye = (BitmapImage)TryFindResource("DisOpen");
            ConverterStylesData.IconProject = (BitmapImage)TryFindResource("XjsIcon");
            SetInterface();

            PathF.TextChanged += PathF_TextChanged;
            Title = "Xml to json converter";
            Icon = ConverterStylesData.IconProject;
            ResizeMode = System.Windows.ResizeMode.NoResize;
            FilesView = new List<XmlProcessorData>();
        }
        private void SetInterface()
        {            
            AddFiles = new MyButton(5, 55, 289, 57, "Добавить файлы...", ConverterStylesData.AddFilesStyle);
            AddFiles.Index = 0;
            AddDirectory = new MyButton(300, 55, 289, 57, "Добавить папку...", ConverterStylesData.AddDirStyle);
            AddDirectory.Index = 1;
            ClearListBut = new MyButton(new Thickness(4, 370, 4, 0), "Очистить список", 26);
            ClearListBut.SetStyle(ConverterStylesData.ClearListStyle);

            ClearListBut.MouseUp += ClearListClick;
            AddFiles.MouseUp += AddXmlObjects;
            AddDirectory.MouseUp += AddXmlObjects;

            MainScr.Children.Add(AddFiles);
            MainScr.Children.Add(AddDirectory);
            MainScr.Children.Add(ClearListBut);
        }

        private void ClearListClick(object sender, MouseButtonEventArgs e)
        {
            ClearList();
        }
        private void ClearList()
        {
            FilesViewer.Children.Clear();
            FilesView.Clear();
            Files = 0;
            ConvertedFiles = 0;
            Progressb.Value = Progressb.Minimum;
            LogGr.Children.Clear();
            LogView.Text = "Список чист.";
            ConvertButton.IsEnabled = false;
        }
        private void ThreadConv()
        {
            string s = "Конвертация выполняется... Обработано " + (ConvertedFiles+1) + "/" + Files;
            Action logw = new Action(() => LogView.Text = s);

            FilesView[ConvertedFiles].BeginConvertation();
            FilesView[ConvertedFiles].ClearConverter();
            ConvertedFiles++;

            Action action = new Action(() => Progressb.Value += ProgressFactor);
            if (Dispatcher.Thread != ConvertThread)
            {
                Dispatcher.Invoke(action);
                Dispatcher.Invoke(logw);
            }
            else { 
                action();
                logw();
            }
            if (ConvertedFiles != Files) ThreadConv();

        }
        #region EventsFunctions
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            ProgressFactor = 100 / (double)Files;
            (sender as Button).IsEnabled = false;
            ConvertThread = new Thread(ThreadConv);
            ConvertThread.Start();
        }
        private void AddXmlObjects(object sender, RoutedEventArgs e)
        {
            int index = (sender as MyButton).Index;
            if (index == 0)
            {
                System.Windows.Forms.OpenFileDialog opf = new System.Windows.Forms.OpenFileDialog();
                opf.Filter = "xml-files (*.xml)| *.xml";
                opf.Multiselect = true;

                if (opf.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    AddXmlFiles(opf.FileNames);
                }
            }
            if (index == 1)
            {
                System.Windows.Forms.FolderBrowserDialog fbd = new System.Windows.Forms.FolderBrowserDialog();
                if (fbd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    AddXmlFiles(System.IO.Directory.GetFiles(fbd.SelectedPath));
                }
            }

        }
        private void AddXmlFiles(string[] pathes)
        {
            CheckForReady();
            int ind;
            for(int i = 0; i<pathes.Length; i++)
            {
                // Check for xml extension
                ind = pathes[i].LastIndexOf('.');
                if (pathes[i].Substring(ind+1) == "xml")
                {
                    FilesView.Add(new XmlProcessorData(pathes[i], Files, FilesViewer));
                    FilesView.Last().Remove.MouseUp += RemoveFromList;

                    Files++;
                }
            }
            LogView.Text = "Файлов загружено: " + Files;
            if (Files > 0) ConvertButton.IsEnabled = true;
        }
        private void CheckDocs()
        {
            FilesViewer.Children.Clear();
            Files = 0;
            for (int i = 0; i < FilesView.Count; i++ )
            {
                FilesView[i].AddToView(FilesViewer);
                FilesView[i].SetIndex(i);
                Files++;
            }
            LogView.Text = "Файлов загружено: " + Files;
            ConvertButton.IsEnabled = (Files > 0);
        }
        private void CheckForReady()
        {
            foreach(XmlProcessorData xpd in FilesView)
                if(xpd.ReadyFile)
                {
                    ClearList();
                    return;
                }
        }
        private void RemoveFromList(object sender, MouseButtonEventArgs e)
        {
            int index = (sender as ImageButton).Index;
            FilesView[index].RemoveFromView(FilesViewer);
            FilesView.RemoveAt(index);
            CheckDocs();
        }
        private void PathF_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (System.IO.Directory.Exists(PathF.Text))
                SaveOriginalPath = false;
            else SaveOriginalPath = true;
        }
        #endregion

        private void FilesViewer_Drop(object sender, DragEventArgs e)
        {
            string[] DroppedData = (string[])e.Data.GetData(DataFormats.FileDrop, false);
            AddXmlFiles(DroppedData);
        }
        private void ShowBrokens()
        {
            int c = 0;
            for(int i = 0; i<FilesView.Count; i++)
            {
                if (FilesView[i].BrokenFile)
                {
                    c++;
                }
                FilesView[i].ProccessEnd();
            }
            TextBlock ResBl = new TextBlock();
            ResBl.Margin = new Thickness(3, 2, 3, 0);
            ResBl.Foreground = Brushes.White;
            ResBl.VerticalAlignment = System.Windows.VerticalAlignment.Top;
            ResBl.Text += "Конвертация завершена. Файлов обработано: " + (Files - c).ToString();
            ResBl.Text += "\r\nПоврежденных файлов: " + (c).ToString();
            LogGr.Children.Add(ResBl);
        }
        private void MoveFiles()
        {
            for(int i = 0; i<FilesView.Count; i++)
            {
                if(!FilesView[i].BrokenFile)
                {
                    FilesView[i].MoveResult(PathF.Text);
                }
            }
            LogView.Text = "Файлы сохранены по пути " + PathF.Text;
        }

        private void SetSavePath(object sender, RoutedEventArgs e)
        {
            System.Windows.Forms.FolderBrowserDialog fbd = new System.Windows.Forms.FolderBrowserDialog();

            if (fbd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                PathF.Text = fbd.SelectedPath;
            }
        }

        private void Progressb_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (Files == ConvertedFiles)
            {
                ConvertThread.Abort();
                ConvertButton.IsEnabled = false;
                ShowBrokens();
                if (SaveOriginalPath == false)
                    MoveFiles();
                else LogView.Text = "Файлы сохранены по исходным путям.";
            }
        }
    }

    public static class ConverterStylesData
    {
        public static BitmapImage RemoveImg, ShowImg;
        public static BitmapImage RedEye;
        public static BitmapImage IconProject;
        public static ButtonStyle AddFilesStyle;
        public static ButtonStyle AddDirStyle;
        public static ButtonStyle ClearListStyle;

        public static void SetStyles()
        {
            
            CustomStyles.SetStandart();
            AddFilesStyle = new ButtonStyle();
            AddFilesStyle.FontSize = 24;
            AddFilesStyle.Border_Thickness = 1;

            AddFilesStyle.Normal = new SolidColorBrush(Color.FromRgb(198, 198, 198));
            AddFilesStyle.Hover = new SolidColorBrush(Color.FromRgb(150, 150, 150));
            AddFilesStyle.Active = new SolidColorBrush(Color.FromRgb(25, 42, 39));

            AddFilesStyle.Text_Normal = Brushes.Black;
            AddFilesStyle.Text_Hover = Brushes.Black;
            AddFilesStyle.Text_Active = new SolidColorBrush(Color.FromRgb(185, 209, 168));

            AddFilesStyle.Border_Normal = new SolidColorBrush(Color.FromRgb(70, 70, 70));
            AddFilesStyle.Border_Hover = new SolidColorBrush(Color.FromRgb(70, 70, 70));
            AddFilesStyle.Border_Active = new SolidColorBrush(Color.FromRgb(90, 90, 90));

            AddDirStyle = new ButtonStyle();
            AddDirStyle.FontSize = 24;
            AddDirStyle.Border_Thickness = 1;

            AddDirStyle.Normal = new SolidColorBrush(Color.FromRgb(221, 221, 221));
            AddDirStyle.Hover = new SolidColorBrush(Color.FromRgb(130, 161, 108));
            AddDirStyle.Active = new SolidColorBrush(Color.FromRgb(41, 31, 55));

            AddDirStyle.Text_Normal = Brushes.Black;
            AddDirStyle.Text_Hover = Brushes.Black;
            AddDirStyle.Text_Active = new SolidColorBrush(Color.FromRgb(178, 174, 165));

            AddDirStyle.Border_Normal = new SolidColorBrush(Color.FromRgb(70, 70, 70));
            AddDirStyle.Border_Hover = new SolidColorBrush(Color.FromRgb(70, 70, 70));
            AddDirStyle.Border_Active = new SolidColorBrush(Color.FromRgb(90, 90, 90));

            ClearListStyle = new ButtonStyle();
            ClearListStyle.FontSize = 14;
            ClearListStyle.Border_Thickness = 1;

            ClearListStyle.Hover = new SolidColorBrush(Color.FromRgb(77, 57, 57));
            ClearListStyle.Normal = new SolidColorBrush(Color.FromRgb(147, 2, 2));
            ClearListStyle.Active = new SolidColorBrush(Color.FromRgb(30, 0, 35));

            ClearListStyle.Text_Hover = new SolidColorBrush(Color.FromRgb(217, 201, 201));
            ClearListStyle.Text_Normal = new SolidColorBrush(Color.FromRgb(217, 201, 201));
            ClearListStyle.Text_Active = new SolidColorBrush(Color.FromRgb(217, 201, 201));

            ClearListStyle.Border_Hover = new SolidColorBrush(Color.FromRgb(189, 189, 189));
            ClearListStyle.Border_Normal = Brushes.Black;
            ClearListStyle.Border_Active = Brushes.White;
        }
    }
}
