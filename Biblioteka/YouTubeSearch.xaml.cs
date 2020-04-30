using Google.Apis.Services;
using Google.Apis.YouTube.v3;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using YoutubeExplode;
using YoutubeExplode.Videos.Streams;

namespace Biblioteka
{
    /// <summary>
    /// Interaction logic for YouTubeSearch.xaml
    /// </summary>
    public partial class YouTubeSearch : Window
    {
        MainWindow MW;
        string loc = null;
        public YouTubeSearch(MainWindow m)
        {
            InitializeComponent();
            MW = m;
        }

        private async void BtnSearch_Click(object sender, RoutedEventArgs e)
        {
            await Search(txtSearch.Text);
        }

        private async void YtDG_Row_DoubleClick(object sender, MouseButtonEventArgs e)
        {
            
            Video vid = (Video)ytDG.SelectedItem;
            await AddYTItemAsync(vid);
            MessageBox.Show("Added");
        }

        private async Task AddYTItemAsync(Video vid)
        {
            await Download(vid.Url, vid.Title);

            using (var db = new LibraryContext())
            {
                db.Add(
                    new Song
                    {
                        Title = vid.Title,
                        Author = vid.Author,
                        Album = null,
                        Location = loc,
                        Source = "YT"
                    });
                db.SaveChanges();
            }
            MW.Refresh();
        }

        public async Task Search(string txtSearch)
        {
            var items = new YoutubeClient();
            List<Video> list = new List<Video>();
            //var streams = items.Search.GetVideosAsync(txtSearch);
            await foreach (var item in items.Search.GetVideosAsync(txtSearch))
            {
                Video vid = new Video();
                vid.Title = item.Title;
                vid.Author = item.Author;
                vid.Url = item.Url;
                list.Add(vid);
            }
            MessageBox.Show("Hi");
            this.ytDG.DataContext = list;
            
        }

        public async Task Download(string url, string name)
        {
            var youtube = new YoutubeClient();

            var streams = await youtube.Videos.Streams.GetManifestAsync(url);

            var streamInfo = streams.GetAudioOnly().WithHighestBitrate();

            if (streamInfo == null)
            {
                MessageBox.Show("error");
                return;
            }
            else
            {
                //var stream = await youtube.Videos.Streams.GetAsync(streamInfo);
                loc = $"Download/{name}.{streamInfo.Container}";
                await youtube.Videos.Streams.DownloadAsync(streamInfo, loc);
            }
        }
    }
}
