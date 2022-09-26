using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
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

namespace HddFreezingTest
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            GetRandomDir(_currentDir);
        }
        private string _rootDir = "H:\\"; //My USB-attached RAID
        private string _currentDir = "H:\\";
        private async void GetRandomDir(string dir)
        {
            bool exists = false;

            //Check first if dir exists and use opportunity to wake disk
            //didn't expect that this will sometimes work even with disk sleeping
            await Task.Run(() =>
            {             
                exists = DirectoryExists(dir);
                Debug.WriteLine($"{DateTime.Now} {dir} exists on thread {System.Threading.Thread.CurrentThread.ManagedThreadId}");
            });

            string newdir = string.Empty;
            //... a lot of other things happening
            //this is now in some other user control decoupled from this one
            //Get files on non-main thread so UI remains responsive 
            await Task.Run(() =>
            {
                try
                {
                    var dirInfo = new DirectoryInfo(dir);
                    var dirs = dirInfo.GetDirectories(); //this also works on sleeping disk (sometimes) and won't wake it up (sometimes)
                    Debug.WriteLine($"{DateTime.Now} Got {dirs.Length} dirs on thread {System.Threading.Thread.CurrentThread.ManagedThreadId}");
                    if (dirs.Length != 0)
                    {
                        //going randomly as sometimes we can browse even sleeping disk if files were enumerated recently
                        var rnd = new Random();
                        newdir = dirs[rnd.Next(0, dirs.Length - 1)].FullName;
                        var dirExists = DirectoryExists(newdir);
                        Debug.WriteLine($"{DateTime.Now} Second .Exists for {newdir} on thread {System.Threading.Thread.CurrentThread.ManagedThreadId}");
                    }
                    else
                    {
                        Debug.Write("No more dirs, going back to root");
                        newdir = _rootDir;
                    }
                   
                } catch (Exception ex)
                {
                    Debug.WriteLine(ex.Message);
                    newdir = dir;
                }
            });
            Debug.Write(newdir);

            _currentDir = newdir;
        }

        public static bool DirectoryExists(string path)
        {
            //same freezing issue with this one, but sooner or later I need more attributes, and the other one is faster anyway
            //return Directory.Exists(path); - 

            uint attributes = GetFileAttributes(path);
            if (attributes != 0xFFFFFFFF)
                return ((FileAttributes)attributes).HasFlag(FileAttributes.Directory);
            else
                return false;
        }

        [DllImport("kernel32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        static extern uint GetFileAttributes(string lpFileName);
    }
}
