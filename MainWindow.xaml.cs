using System.Collections.ObjectModel;
using System;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Microsoft.Win32;
using System.Globalization;
using System.Windows.Data;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Forms;
using System.Windows.Media.Imaging;
using System.Collections.Generic;

namespace WPFLab2 {
    public partial class MainWindow : Window {
        public ObservableCollection<ListImageInfo> listViewImagesInfo = new ObservableCollection<ListImageInfo>();
        ListImageInfo selectedImageInfo = new ListImageInfo();
        public List<IEffect> effectsList;
        int SelectedEffectIndex;
        public MainWindow() {
            InitializeComponent();
            this.LoadDirectories();
            this.tilesListView.DataContext = listViewImagesInfo;
            this.selectImageInfoExpander.DataContext = selectedImageInfo;
            this.InitEffects();
            SelectedEffectIndex = -1;
            this.slideshowMenuItem.DataContext = effectsList;
            this.effectComboBox.DataContext = effectsList;
        }


        //https://www.codeproject.com/Articles/390514/Playing-with-a-MVVM-Tabbed-TreeView-for-a-File-Exp
        public void LoadDirectories() {
            var drives = DriveInfo.GetDrives();
            foreach (var drive in drives) {
                this.driveTreeView.Items.Add(this.GetItem(drive));
            }
        }
        private TreeViewItem GetItem(DriveInfo drive) {
            var item = new TreeViewItem {
                Header = drive.Name,
                DataContext = drive,
                Tag = drive
            };
            this.AddItem(item);
            item.Expanded += new RoutedEventHandler(item_Expanded);
            return item;
        }

        private TreeViewItem GetItem(DirectoryInfo directory) {
            var item = new TreeViewItem {
                Header = directory.Name,
                DataContext = directory,
                Tag = directory
            };
            this.AddItem(item);
            item.Expanded += new RoutedEventHandler(item_Expanded);
            item.MouseLeftButtonUp += treeItem_Selected; // view item click
            return item;
        }

        private TreeViewItem GetItem(FileInfo file) {
            var item = new TreeViewItem {
                Header = file.Name,
                DataContext = file,
                Tag = file
            };
            return item;
        }

        private void AddItem(TreeViewItem item) {
            item.Items.Add(new DummyTreeViewItem());
        }

        private bool HasDummy(TreeViewItem item) {
            return item.HasItems && (item.Items.OfType<TreeViewItem>().ToList().FindAll(tvi => tvi is DummyTreeViewItem).Count > 0);
        }

        private void RemoveDummy(TreeViewItem item) {
            var dummies = item.Items.OfType<TreeViewItem>().ToList().FindAll(tvi => tvi is DummyTreeViewItem);
            foreach (var dummy in dummies) {
                item.Items.Remove(dummy);
            }
        }

        private void ExploreDirectories(TreeViewItem item) {
            var directoryInfo = (DirectoryInfo)null;
            if (item.Tag is DriveInfo) {
                directoryInfo = ((DriveInfo)item.Tag).RootDirectory;
            }
            else if (item.Tag is DirectoryInfo) {
                directoryInfo = (DirectoryInfo)item.Tag;
            }
            else if (item.Tag is FileInfo) {
                directoryInfo = ((FileInfo)item.Tag).Directory;
            }
            if (object.ReferenceEquals(directoryInfo, null)) return;
            foreach (var directory in directoryInfo.GetDirectories()) {
                var isHidden = (directory.Attributes & FileAttributes.Hidden) == FileAttributes.Hidden;
                var isSystem = (directory.Attributes & FileAttributes.System) == FileAttributes.System;
                if (!isHidden && !isSystem) {
                    item.Items.Add(this.GetItem(directory));
                }
            }
        }

        private void ExploreFiles(TreeViewItem item) {
            var directoryInfo = (DirectoryInfo)null;
            if (item.Tag is DriveInfo) {
                directoryInfo = ((DriveInfo)item.Tag).RootDirectory;
            }
            else if (item.Tag is DirectoryInfo) {
                directoryInfo = (DirectoryInfo)item.Tag;
            }
            else if (item.Tag is FileInfo) {
                directoryInfo = ((FileInfo)item.Tag).Directory;
            }
            if (object.ReferenceEquals(directoryInfo, null)) return;
            foreach (var file in directoryInfo.GetFiles()) {
                var isHidden = (file.Attributes & FileAttributes.Hidden) == FileAttributes.Hidden;
                var isSystem = (file.Attributes & FileAttributes.System) == FileAttributes.System;
                if (!isHidden && !isSystem) {
                    item.Items.Add(this.GetItem(file));
                }
            }
        }

        void item_Expanded(object sender, RoutedEventArgs e) {
            var item = (TreeViewItem)sender;
            if (this.HasDummy(item)) {
                this.Cursor = System.Windows.Input.Cursors.Wait;
                this.RemoveDummy(item);
                this.ExploreDirectories(item);
                this.ExploreFiles(item);
                this.Cursor = System.Windows.Input.Cursors.Arrow;
            }
        }

        //populating image fileStr names
        void treeItem_Selected(object sender, RoutedEventArgs e) {
            listViewImagesInfo.Clear();
            var treeViewItem = (TreeViewItem)sender;
            var directory = (DirectoryInfo)treeViewItem.Tag;
            foreach (var file in directory.GetFiles()) {
                if (isImage(file)) {
                    var image = System.Drawing.Image.FromFile(file.FullName);
                    ListImageInfo imageInfo = new ListImageInfo {
                        Selected = false,
                        ImageName = file.Name,
                        Width = image.Width,
                        Height = image.Height,
                        Path = file.FullName,
                        Size = Math.Round((double)file.Length / 1024, 1)
                    };
                this.listViewImagesInfo.Add(imageInfo);
                }
            }
        }
        bool isImage(FileInfo file) {
            string extension = file.Extension.ToLower();
            return extension == ".jpg" || extension == ".jpeg" || extension == ".png" || extension == ".gif";
        }

        private void ExitMenuItem_Click(object sender, RoutedEventArgs e) {
            this.Close();
        }

        //event listeners
        //https://stackoverflow.com/questions/24449988/how-to-get-fileStr-path-from-openfiledialog-and-folderbrowserdialog
        private void OpenFolderMenuItem_Click(object sender, RoutedEventArgs e) {
            listViewImagesInfo.Clear();
            var folderBrowserDialog = new FolderBrowserDialog();
            if (folderBrowserDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK) {
                string selectedFolder = folderBrowserDialog.SelectedPath;
                string[] files = Directory.GetFiles(selectedFolder);
                foreach (string fileStr in files) {
                    FileInfo fileInfo = new FileInfo(fileStr);
                    if (isImage(fileInfo)) {
                        var image = System.Drawing.Image.FromFile(fileStr);
                        ListImageInfo imageInfo = new ListImageInfo {
                            Selected = false,
                            ImageName = fileInfo.Name,
                            Width = image.Width,
                            Path = fileStr,
                            Height = image.Height,
                            Size = Math.Round((double)fileInfo.Length / 1024, 1)
                        };
                        this.listViewImagesInfo.Add(imageInfo);
                    }
                }
            }
        }

        private void ListViewItem_Click(object sender, MouseButtonEventArgs e) {
            System.Windows.Controls.ListViewItem listViewItem = (System.Windows.Controls.ListViewItem)sender;
            if (listViewItem != null) {
                int index = tilesListView.ItemContainerGenerator.IndexFromContainer(listViewItem);
                if (index >= 0) {
                    selectedImageInfo.Update(listViewImagesInfo[index]);
                    selectedImageInfo.Selected = true;
                }
            }
        }

        private void AboutMenuItem_Click(object sender, RoutedEventArgs e) {
            System.Windows.MessageBox.Show("This is a simple slide show application.",
            "About", MessageBoxButton.OK, MessageBoxImage.Information);

        }

        private void InitEffects() {
            effectsList = new List<IEffect> {
                new HorizontalEffect(),
                new OpacityEffect(),
                new VerticalEffect()
            };
        }

        public void StartSlideShow() {
            List<BitmapImage> bitmapList = new List<BitmapImage>();
            foreach(var imageInfo in listViewImagesInfo) {
                bitmapList.Add(imageInfo.Bitmap);
            }
            Opacity = 0.5;
            var slideShowWindow = new SlideshowWindow(bitmapList, effectsList[SelectedEffectIndex]);
            if (slideShowWindow!.ShowDialog()!.Value) {          
            }
            Opacity = 1;
        }

        private void StartSlideshowButton_Click(object sender, RoutedEventArgs e) {
            SelectedEffectIndex = this.effectComboBox.SelectedIndex;
            this.StartSlideShow();
        }

        private void slideshowMenuItem_ClickH(object sender, RoutedEventArgs e) {
            //MenuItem menuItem = (MenuItem)sender;
            //ContextMenu parentMenu = (ContextMenu)menuItem.Parent;
            //SelectedEffectIndex = slideshowMenuItem.Items.IndexOf(menuItem);
            SelectedEffectIndex = 0;
            this.StartSlideShow();
        }
        private void slideshowMenuItem_ClickO(object sender, RoutedEventArgs e) {
            SelectedEffectIndex = 1;
            this.StartSlideShow();
        }
        private void slideshowMenuItem_ClickV(object sender, RoutedEventArgs e) {
            SelectedEffectIndex = 2;
            this.StartSlideShow();
        }

        private void Combobox_Selection(object sender, MouseButtonEventArgs e) {
            SelectedEffectIndex = this.effectComboBox.SelectedIndex;
        }
    }
    public class DummyTreeViewItem : TreeViewItem {
        public DummyTreeViewItem()
            : base() {
            base.Header = "SomeDummy";
            base.Tag = "SomeDummy";
        }
    }

    //https://learn.microsoft.com/en-us/dotnet/desktop/wpf/data/how-to-implement-property-change-notification?view=netframeworkdesktop-4.8
    public class ListImageInfo : INotifyPropertyChanged {
        public event PropertyChangedEventHandler? PropertyChanged;

        private bool selected;
        public bool Selected {
            get { return selected; }
            set {
                selected = value;
                OnPropertyChanged();
            }
        }

        private string? imageName;
        public string? ImageName { 
            get { return imageName; }
            set {
                imageName = value;
                OnPropertyChanged();
            }
        }

        private int width;
        public int Width {
            get { return width; }
            set {
                width = value;
                OnPropertyChanged();
            }
        }

        private int height;
        public int Height {
            get { return height; }
            set {
                height = value;
                OnPropertyChanged();
            }
        }

        private double size;
        public double Size {
            get { return size; }
            set {
                size = value;
                OnPropertyChanged();
            }
        }

        public string? Path { private get; set; }

        public BitmapImage Bitmap {
            get { return GetBitmapImage(); }
            private set { OnPropertyChanged(); }
        }

        private BitmapImage GetBitmapImage() {
            BitmapImage bitmapImage = new BitmapImage();
            bitmapImage.BeginInit();
            bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
            bitmapImage.UriSource = new Uri(Path);
            bitmapImage.DecodePixelWidth = this.width;
            bitmapImage.DecodePixelHeight = this.height;
            bitmapImage.EndInit();

            return bitmapImage;
        }

        public void Update(ListImageInfo info) {
            this.Selected = info.selected;
            this.Height = info.height;
            this.Width = info.width;
            this.Size = info.size;
            this.ImageName = info.imageName;
        }

        public ListImageInfo() { }

        protected void OnPropertyChanged([CallerMemberName] string name = null) {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }

    public class BoolToVisibilityConverter : IValueConverter {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
            if (value is bool && (bool)value == true)
                return Visibility.Visible;
            return Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
            throw new NotImplementedException();
        }
    }

    public class InverseBoolToVisibilityConverter : IValueConverter {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
            if (value is bool && (bool)value == true)
                return Visibility.Collapsed;
            return Visibility.Visible;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
            throw new NotImplementedException();
        }
    }
}
