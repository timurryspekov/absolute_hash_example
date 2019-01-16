using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Ports;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace absolute_blockchain
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        string path = "";
        public MainWindow()
        {
            InitializeComponent();

            //com port array for a connection to our quantum generator
            string[] portNames = SerialPort.GetPortNames();
           path = System.IO.Directory.GetCurrentDirectory() + @"\blocks\";
            try
            {
                for (int index = 0; index < portNames.Length; ++index)
                {
                    this.comboBox.Items.Add((object)portNames[index]);

                }
                this.comboBox.SelectedValue = (object)portNames[0];

            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }

            //Add item for the combobox about encryption
            type_enc.Items.Add("No");
            type_enc.Items.Add("Yes");
            type_enc.SelectedIndex = 0;
        }

        //connection to our quantum random number generator
        private void podkl()
        {
            try
            {
                this.sp1.PortName = this.comboBox.Text;
                this.sp1.BaudRate = 9600;
                this.sp1.DataBits = 8;
                this.sp1.DataReceived += new SerialDataReceivedEventHandler(this.sp_DataReceived);
                sp1.Open();

                MessageBox.Show("Ok!");

            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        //max char for random
        int max_char = char.MaxValue;
        SerialPort sp1 = new SerialPort();

        //string with data from quantum random number generator
        string quantum = "";

        //data from qrng received event 
        private void sp_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            try
            {
                this.Dispatcher.BeginInvoke((Delegate)new LineReceivedEvent(this.LineReceived), new object[1]
                {
         this.sp1.ReadLine()
                });
            }
            catch
            {
            }
        }
        private delegate void LineReceivedEvent(string command);
        private void LineReceived(string command)
        {
            //MessageBox.Show(command);
            quantum = command;
            sp1.Write("f");
            int result;
            int.TryParse(command, out result);
          
        }


        //connect button click
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                //connect to the qrng
                podkl();
                sp1.Write("f");

                //turn on qrng
                sp1.Write("o");
            }
            catch
            {

            }
        }

        //button off click
        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            try
            {
                //turn off
                sp1.Write("f");
            }
            catch
            {

            }
        }


        //random char generator for key
        char random_char(char c)
        {
            Random r = new Random(DateTime.Now.Millisecond);
            char first = (char)(0);
            try
            {
                sp1.Write("f");
                sp1.Write("o");
            }
            catch
            {

            }
            for (int i = 0; i < quantum.Length; i++)
            {
                first += quantum[i];
            }
            return (char)((r.Next(0, max_char) + DateTime.Now.Millisecond+first) % max_char);
        }

      
        public string key = "";

        
        //add button click
        private void Button_Click_2(object sender, RoutedEventArgs e)
        {
            try
            {
                string path2 = path + pr.Text + @"\";

                //if encryption yes
                if (type_enc.SelectedIndex == 1)
                {

                    //random key generation

                    key = "";
                    char first = (char)(0);
                    for (int i = 0; i < quantum.Length; i++)
                    {
                        first += quantum[i];
                    }
                    //MessageBox.Show("Quantum char: " + first.ToString());

                    for (int i = 0; i < 16; i++)
                    {
                        if (i != 0)
                        {
                            key += (char)(random_char(key[i - 1]));
                        }
                        else
                        {
                            key += (char)(random_char(first));
                        }

                    }

                    //encrypt the file
                    encrypt();
                }
                else
                {
                    //copy file into blocks folder
                    OpenFileDialog fd = new OpenFileDialog();

                    fd.ShowDialog();

                    if (!Directory.Exists(path2))
                    {
                        DirectoryInfo di = Directory.CreateDirectory(path2);
                    }

                    File.Copy(fd.FileName, path2 + pr.Text + ".abs");
                }

                //find last index of hash blocks
                int max_id = 0;
                string max_file = "";
                foreach (string f in Directory.GetFiles(path + "hash", ".", SearchOption.AllDirectories))
                {

                    int f1 = Convert.ToInt32(System.IO.Path.GetFileNameWithoutExtension(f));
                    if (f1 > max_id)
                    {
                        max_id = f1;
                        max_file = f;
                    }
                }

                //read data from the last hash block
                string old_our = "";
                string old_md5 = "";
                string old_sha256 = "";
                if (max_id != 0)
                {
                    var readFile = new StreamReader(new FileStream(max_file, FileMode.Open, FileAccess.ReadWrite), Encoding.UTF8);
                    old_our = readFile.ReadLine();
                    old_md5 = readFile.ReadLine();
                    old_sha256 = readFile.ReadLine();
                    readFile.Close();
                }
                max_id++;

                //new hash block generation
                string md5 = (Encoding.UTF8.GetString(MD5.Create().ComputeHash(File.ReadAllBytes(path2 + pr.Text + ".abs"))));
                string new_md5 = Encoding.UTF8.GetString(SHA256.Create().ComputeHash(Encoding.UTF8.GetBytes(md5 + old_md5)));
                string sha256 = Encoding.UTF8.GetString(SHA256.Create().ComputeHash(File.ReadAllBytes(path2 + pr.Text + ".abs")));
                string new_sha256 = Encoding.UTF8.GetString(SHA256.Create().ComputeHash(Encoding.UTF8.GetBytes(sha256 + old_sha256)));
                var bytes = File.ReadAllBytes(path2 + pr.Text + ".abs");
                string new_our = our(Encoding.UTF8.GetBytes(our(bytes) + old_our));
                var write = new StreamWriter(new FileStream(path + @"hash\" + max_id.ToString(), FileMode.Create, FileAccess.ReadWrite), Encoding.UTF8);
                write.WriteLine(new_our);
                write.WriteLine(new_md5);
                write.WriteLine(new_sha256);
                write.WriteLine(pr.Text);
                write.WriteLine(author.Text);
                write.Close();
                MessageBox.Show("Ok!");
            }
            catch (Exception r)
            {
                MessageBox.Show(r.Message);
            }
        }

        //our hash algorithm
        string our(byte[] bytes)
        {
            double old_d = 0;

            for (int i = 0; i < bytes.Length - 1; i += 2)
            {
                double d = Math.Pow(2, Convert.ToDouble(bytes[i]) / 100) + Math.Pow(2, Convert.ToDouble(bytes[i + 1]) / 100);
                old_d += d;
            }
            return old_d.ToString();
        }


        //aes encryption
        void encrypt()
        {
            try
            {
                byte[] bytes;
                OpenFileDialog fd = new OpenFileDialog();

                fd.ShowDialog();

                using (FileStream fsSource = new FileStream(fd.FileName,
           FileMode.Open, FileAccess.Read))
                {

                    bytes = new byte[fsSource.Length];
                    int numBytesToRead = (int)fsSource.Length;
                    int numBytesRead = 0;
                    while (numBytesToRead > 0)
                    {

                        int n = fsSource.Read(bytes, numBytesRead, numBytesToRead);


                        if (n == 0)
                            break;

                        numBytesRead += n;
                        numBytesToRead -= n;
                    }
                    numBytesToRead = bytes.Length;



                }
               
                byte[] passwordBytes = Encoding.UTF8.GetBytes(key);

                passwordBytes = SHA256.Create().ComputeHash(passwordBytes);

                byte[] bytesEncrypted = AES_Encrypt(bytes, passwordBytes);
                string path2 = path + pr.Text + @"\";
                if (!Directory.Exists(path2))
                {
                    DirectoryInfo di = Directory.CreateDirectory(path2);
                }
                string fileEncrypted = path2+pr.Text+".abs";

                File.WriteAllBytes(fileEncrypted, bytesEncrypted);

                var write = new StreamWriter(new FileStream(path2+"key.txt", FileMode.Create, FileAccess.ReadWrite), Encoding.UTF8);
                write.Write(key);
                write.Close();
            }
            catch
            {

            }

        }
        public byte[] AES_Encrypt(byte[] bytesToBeEncrypted, byte[] passwordBytes)
        {
            byte[] encryptedBytes = null;


            byte[] saltBytes = new byte[] { 1, 2, 3, 5, 7, 11, 13, 17, 19, 23 };

            using (MemoryStream ms = new MemoryStream())
            {
                using (RijndaelManaged AES = new RijndaelManaged())
                {
                    AES.KeySize = 256;
                    AES.BlockSize = 128;

                    var key = new Rfc2898DeriveBytes(passwordBytes, saltBytes, 1000);
                    AES.Key = key.GetBytes(AES.KeySize / 8);
                    AES.IV = key.GetBytes(AES.BlockSize / 8);

                    AES.Mode = CipherMode.CBC;

                    using (var cs = new CryptoStream(ms, AES.CreateEncryptor(), CryptoStreamMode.Write))
                    {
                        cs.Write(bytesToBeEncrypted, 0, bytesToBeEncrypted.Length);
                        cs.Close();
                    }
                    encryptedBytes = ms.ToArray();
                }
            }

            return encryptedBytes;
        }
    }
   
}
