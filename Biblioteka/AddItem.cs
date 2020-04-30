using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;
using System.Linq;

namespace Biblioteka
{
    class AddItem
    {
        public void FileOpen()
        {
            var fileDialog = new OpenFileDialog();
            var result = fileDialog.ShowDialog();
            switch(result)
            {
                case DialogResult.OK:
                    string file = fileDialog.FileName;
                    try
                    {
                        DataRead(file);
                    }
                    catch (Exception)
                    {
                        MessageBox.Show("No data in a file :/");
                        break;
                    }
                    break;
                case DialogResult.Cancel:
                default:
                    break;
            }
        }

        private void DataRead(string file)
        {
            TagLib.File f = TagLib.File.Create(file);                             //exceptions?
            if (f.Tag.Title == "" || f.Tag.Title == null) throw new Exception();  //TEMPORARY
            using var db = new LibraryContext();
            db.Add(
                new Song
                {
                    Title = f.Tag.Title,
                    Author = f.Tag.FirstPerformer,
                    Album = f.Tag.Album,
                    Location = file,
                    Source = "PC"
                });
            db.SaveChanges();
        }

    }
}
