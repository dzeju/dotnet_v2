using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Reflection;
using Vlc.DotNet.Wpf;

namespace Biblioteka
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly DirectoryInfo vlcLibDirectory;
        private VlcControl control;
        private string currentlyPlaying = null;
        public MainWindow()
        {
            InitializeComponent();
            Refresh();

            var currentAssembly = Assembly.GetEntryAssembly();
            var currentDirectory = new FileInfo(currentAssembly.Location).DirectoryName;
            // Default installation path of VideoLAN.LibVLC.Windows
            vlcLibDirectory = new DirectoryInfo(System.IO.Path.Combine(currentDirectory, "libvlc", IntPtr.Size == 4 ? "win-x86" : "win-x64"));
        }

        private void BtnAdd_Click(object sender, RoutedEventArgs e)
        {
            AddItem AddI = new AddItem();
            AddI.FileOpen();
            Refresh();
        }

        public void Refresh()
        {
            using var db = new LibraryContext();
            DG.DataContext = db.Songs.ToList<Song>();
        }

        private void BtnRemove_Click(object sender, RoutedEventArgs e)
        {
            //Song model = new Song();
            MessageBoxResult dR = MessageBox.Show("Delete EVERYTHING?", "Confirm", MessageBoxButton.YesNo);
            if (MessageBoxResult.Yes == dR)
            {
                using var db = new LibraryContext();
                db.Songs.RemoveRange(db.Songs);
                db.SaveChanges();
                Refresh();
            }
        }

        private void BtnDelete_Click(object sender, RoutedEventArgs e)
        {
            Song model = (Song)DG.SelectedItem;
            if (model != null)
            {
                MessageBoxResult dR = MessageBox.Show("Delete \"" + model.Title + "\"?", "Confirm", MessageBoxButton.YesNo);
                if (MessageBoxResult.Yes == dR)
                {
                    using var db = new LibraryContext();
                    var entry = db.Entry(model);
                    if (entry.State == Microsoft.EntityFrameworkCore.EntityState.Detached)
                        db.Songs.Attach(model);
                    if (model.Source == "YT")
                        File.Delete(model.Location);
                    db.Songs.Remove(model);
                    db.SaveChanges();
                    Refresh();
                }
            }
        }

        private void BtnClose_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void BtnPlay_Click(object sender, RoutedEventArgs e)
        {
            Song model = (Song)DG.SelectedItem;
            if (this.control != null && control.SourceProvider.MediaPlayer.IsPlaying() && model.Location == currentlyPlaying) 
            { 
                control.SourceProvider.MediaPlayer.Pause();
                this.btnPlayPause.Content = FindResource("Play");
            }
            else if (this.control != null && control.SourceProvider.MediaPlayer.IsPlaying() != true && model.Location == currentlyPlaying)
            {
                control.SourceProvider.MediaPlayer.Play();
                this.btnPlayPause.Content = FindResource("Pause");
            }
            else
            {
                this.control?.Dispose();
                this.control = new VlcControl();
                //this.ControlContainer.Content = this.control;
                this.control.SourceProvider.CreatePlayer(this.vlcLibDirectory);

                // This can also be called before EndInit
                this.control.SourceProvider.MediaPlayer.Log += (_, args) =>
                {
                    string message = $"libVlc : {args.Level} {args.Message} @ {args.Module}";
                    System.Diagnostics.Debug.WriteLine(message);
                };

                this.currentlyPlaying = model.Location;
                if (model.Source == "PC")
                    control.SourceProvider.MediaPlayer.Play(new FileInfo(currentlyPlaying));
                else if (model.Source == "YT")
                    control.SourceProvider.MediaPlayer.Play(new FileInfo(currentlyPlaying));
                this.btnPlayPause.Content= FindResource("Pause");
            }
        
        }

        private void BtnStop_Click(object sender, RoutedEventArgs e)
        {
            this.control?.Dispose();
            this.control = null;
        }

        private void BtnYouTube_Click(object sender, RoutedEventArgs e)
        {
            var yt = new YouTubeSearch(this)
            {
                DataContext = this
            };
            try
            {
                yt.Show();
                Refresh();
            }
            catch { }
        }
    }
}
