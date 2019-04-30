using System;
using System.Collections.Generic;
using System.Linq;
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
using System.IO;

namespace LZW
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        List<int> compressed;
        public MainWindow()
        {
            InitializeComponent();
            
        }

        private void ButtonOpen_Click(object sender, RoutedEventArgs e)
        {
            String path = TextBoxPath.Text;

            try
            {
                using (FileStream fs = File.Open(path, FileMode.Open))
                {
                    byte[] data = new byte[102400];
                    int len = fs.Read(data, 0, data.Length);

                    char[] dataChar = new char[len];

                    for (int i = 0; i < len; i++)
                    {
                        dataChar[i] = Convert.ToChar(data[i]); 
                    }

                    String uncompressed = new String(dataChar);

                    TextOpen.Text = "File Size: " + uncompressed.Length.ToString() + "\n";
                    TextOpen.Text += uncompressed;

                    //output encoded
                    compressed = Compress(uncompressed);

                    TextEncoded.Text += "Count: " + compressed.Count.ToString() + '\n';

                    foreach( int i in compressed ){
                        TextEncoded.Text += i.ToString() + ' ';
                    }

                    ButtonSave.IsEnabled = true;                   
                }
            }
            catch (FileNotFoundException)
            {
                TextBoxPath.Text = "File not found!";
            }
            catch (ArgumentException)
            {
                TextBoxPath.Text = "Path is empty!";
            }
            catch (DirectoryNotFoundException)
            {
                TextBoxPath.Text = "Directory not found!";
            }
        }
        

        private void ButtonDecode_Click(object sender, RoutedEventArgs e)
        {
            String path = TextBoxPath.Text;
            List<int> compressedFile = new List<int>();

            try
            {
                using (var fileStream = File.Open(path, FileMode.Open, FileAccess.Read))
                {
                    var br = new BinaryReader(fileStream, Encoding.Default);

                    while (fileStream.Position < fileStream.Length)
                    {
                        compressedFile.Add(Convert.ToInt16(br.ReadInt16()));

                    }

                    TextOpen.Text = Decompress(compressedFile);
                }
            }
            catch (FileNotFoundException)
            {
                TextBoxPath.Text = "File not found!";
            }
                    
        }
                

        private void ButtonSave_Click(object sender, RoutedEventArgs e)
        {
            String path = TextBoxSave.Text;

            try
            {
                using (BinaryWriter bw = new BinaryWriter( File.Open(path, FileMode.OpenOrCreate, FileAccess.Write, FileShare.None)) )
                {
                    foreach(int i in compressed  ){
                        bw.Write( Convert.ToInt16(i) );
                    }
                }
            }
            catch (SystemException)
            {
                throw new SystemException();
            }
            MessageBox.Show("Success!", "LZW compressor");
        }


        //----------------------------------------------------------------------------
        public static List<int> Compress(string uncompressed)
        {
            // build the dictionary
            Dictionary<string, int> dictionary = new Dictionary<string, int>();
            for (int i = 0; i < 256; i++)
                dictionary.Add(((char)i).ToString(), i);

            string p = string.Empty;
            List<int> compressed = new List<int>();

            foreach (char c in uncompressed)
            {
                string pc = p + c;

                if (dictionary.ContainsKey(pc))
                {
                    p = pc;
                }
                else
                {
                    // write w to output
                    compressed.Add(dictionary[p]);
                    // wc is a new sequence; add it to the dictionary
                    dictionary.Add(pc, dictionary.Count);
                    p = c.ToString();
                }
            }

            // write remaining output if necessary
            if (!string.IsNullOrEmpty(p))
                compressed.Add(dictionary[p]);

            return compressed;
        }

        public static string Decompress(List<int> compressed)
        {
            // build the dictionary
            Dictionary<int, string> dictionary = new Dictionary<int, string>();
            for (int i = 0; i < 256; i++)
                dictionary.Add(i, ((char)i).ToString());

            
            string w = dictionary[compressed[0]];
            compressed.RemoveAt(0);
            StringBuilder decompressed = new StringBuilder(w);

            foreach (int k in compressed)
            {
                string entry = null;
                if (dictionary.ContainsKey(k))
                    entry = dictionary[k];
                else if (k == dictionary.Count)
                    entry = w + w[0];

                decompressed.Append(entry);

                // new sequence; add it to the dictionary
                dictionary.Add(dictionary.Count, w + entry[0]);

                w = entry;
            }

            return decompressed.ToString();
        }
    }

}
