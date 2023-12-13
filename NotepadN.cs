using System;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.IO;
using System.Security.Cryptography;

namespace NotepadN
{
    public partial class MainForm : Form
    {
        string fileName, pass;
        bool isInterfaceHidden, isModified, notifyShowed = false;
        public MainForm()
        {
            InitializeComponent();
            this.Icon = Icon.ExtractAssociatedIcon(Application.ExecutablePath);
            KeyPreview = true;
            TopMost = false;
            NoteTextBox.Font = new Font(NoteTextBox.Font.FontFamily, 10);
            NotifyIcon.Visible = false;
            NotifyIcon.MouseClick += new MouseEventHandler(NotifyIcon_MouseDoubleClick);
            Resize += new EventHandler(Form1_Resize);
            infolabel.Text = "Запуск выполнен успешно";
            NoteTextBox.ContextMenuStrip = RightMenuStrip1;
            копироватьToolStripMenuItem.Click += copyMenuItem_Click;
            вставитьToolStripMenuItem.Click += pasteMenuItem_Click;
            string[] arguments = Environment.GetCommandLineArgs();
            if (arguments.Length > 1)
            {
                try
                {
                    fileName = arguments[1];
                    OpenTextFile(arguments[1]);
                }
                catch { infolabel.Text = "Ошибка открытия файла"; }
            }
        }

        private void PreventClosing()
        {
            try
            {
                if (сворачиватьАНеЗакрыватьToolStripMenuItem.Checked == true && WindowState == FormWindowState.Minimized && notifyShowed == false)
                {
                    ShowInTaskbar = false;
                    NotifyIcon.Visible = true;
                    NotifyIcon NI = new NotifyIcon();
                    NI.BalloonTipText = "Кликните по иконке в панели задач чтобы развернуть";
                    NI.BalloonTipTitle = "NotepadN";
                    NI.BalloonTipIcon = ToolTipIcon.Info;
                    NI.Icon = this.Icon;
                    NI.Visible = true;
                    NI.ShowBalloonTip(3000);
                    NI.Dispose();
                    notifyShowed = true;
                }
            }
            catch
            { }
        }
        private void Form1_KeyDown(object sender, KeyEventArgs e)
        {
            try
            {
                if (e.KeyCode == Keys.S && e.Control)
                {
                    SaveTextFile();
                    infolabel.Text = "Файл сохранён";
                }
                if (e.KeyCode == Keys.S && e.Control && e.Shift)
                {
                    SaveAs();
                    infolabel.Text = "Файл сохранён";
                }
                if (e.KeyCode == Keys.X && e.Control) { SaveModifyed(); }
                if (e.KeyCode == Keys.Enter && e.Control) { AddDateTime(); }
                if (e.KeyCode == Keys.L && e.Control) { HideOrShowInterface(); }
                if (e.KeyCode == Keys.G && e.Control)
                {
                    if (FontSelector.ShowDialog() != DialogResult.Cancel) { NoteTextBox.SelectionFont = FontSelector.Font; }
                }
                if (e.KeyCode == Keys.H && e.Control)
                {
                    if (ColorSelector.ShowDialog() != DialogResult.Cancel) { NoteTextBox.SelectionColor = ColorSelector.Color; }
                }
                if (e.KeyCode == Keys.F && e.Control)
                {
                    if (TopMost == false)
                    {
                        TopMost = true;
                        поверхВсехОконToolStripMenuItem.Checked = true;
                        infolabel.Text = "Отображение поверх всех окон включено";
                    }
                    else
                    {
                        TopMost = false;
                        поверхВсехОконToolStripMenuItem.Checked = false;
                        infolabel.Text = "Отображение поверх всех окон отключено";
                    }
                }
            }
            catch { }
        }
        private void SaveModifyed()
        {
            try
            {
                if (NoteTextBox.Text.Length != 0 && isModified == true)
                {
                    DialogResult result = MessageBox.Show("Сохранить изменения в файле?", "Сохранение", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                    if (result == DialogResult.No)
                    {
                        NotifyIcon.Dispose();
                        Environment.Exit(0);
                    }
                    if (result == DialogResult.Yes)
                    {
                        SaveTextFile();
                        Environment.Exit(0);
                    }
                }
                else
                    Environment.Exit(0);
            }
            catch { }
        }
        private void AddDateTime()
        {
            try
            {
                FileAttributes attributes = File.GetAttributes(fileName);
                if ((attributes & FileAttributes.ReadOnly) == FileAttributes.ReadOnly) { infolabel.Text = "Невозможно добавить дату, так как файл открыт только для чтения"; }
                else
                {
                    infolabel.Text = "Добавлены дата и время";
                    NoteTextBox.SelectedText = "\r\n" + Convert.ToString(DateTime.Now) + "\r\n=========================";
                }
            }
            catch { infolabel.Text = "Невозможно добавить дату, так как файл открыт только для чтения"; }
        }
        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            try
            {
                if (сворачиватьАНеЗакрыватьToolStripMenuItem.Checked == true)
                {
                    e.Cancel = true;
                    ShowInTaskbar = false;
                    NotifyIcon.Visible = true;
                    WindowState = FormWindowState.Minimized;
                }
                else { SaveModifyed(); }
            }
            catch { }
        }
        private void выходToolStripMenuItem_Click(object sender, EventArgs e) { SaveModifyed(); }
        private static string GetHash(string text)
        {
            try
            {
                SHA512 sha512Hash = new SHA512Managed();
                byte[] sourceBytes = Encoding.UTF8.GetBytes(text);
                byte[] hashBytes = sha512Hash.ComputeHash(sourceBytes);
                string rawTextHash = BitConverter.ToString(hashBytes).Replace("-", String.Empty);
                return rawTextHash;
            }
            catch { return null; }
        }
        private void SaveAs()
        {
            try
            {
                if (SaveFile.ShowDialog() == DialogResult.Cancel)
                    return;
                fileName = SaveFile.FileName;
                if (Path.GetExtension(fileName) == ".txt") { SaveTXT(); }
                else if (Path.GetExtension(fileName) == ".NoteN") { SaveNoteN(); }
            }
            catch
            { }
        }
        private void SaveTextFile()
        {
            try
            {
                if (Path.GetExtension(fileName) == ".txt") { SaveTXT(); }
                else if (Path.GetExtension(fileName) == ".NoteN") { SaveNoteN(); }
            }
            catch
            { }
        }
        private void SaveTXT()
        {
            try
            {
                using (StreamWriter sw = new StreamWriter(fileName)) { sw.WriteLine(NoteTextBox.Text, pass); }
                infolabel.Text = "Файл сохранён успешно";
                isModified = false;
            }
            catch { infolabel.Text = "Ошибка сохранения файла. Возможно, файл имеет атрибут \"Только для чтения\""; }
        }
        private void SaveNoteN()
        {
            try
            {
                string FullString = GetHash(NoteTextBox.Rtf) + "/NoteNSplitterNoteNAjFiS/" + this.Height + "/NoteNSplitterNoteNAjFiS/" + this.Width + "/NoteNSplitterNoteNAjFiS/" + BackColor.ToArgb() + "/NoteNSplitterNoteNAjFiS/" + создатьToolStripMenuItem.ForeColor.ToArgb() + "/NoteNSplitterNoteNAjFiS/" + TopMost.ToString() + "/NoteNSplitterNoteNAjFiS/" + NoteTextBox.Rtf;
                using (StreamWriter sw = new StreamWriter(fileName)) { sw.WriteLine(EncryptString(FullString, pass)); }
                infolabel.Text = "Файл сохранён успешно";
                isModified = false;
            }
            catch { infolabel.Text = "Ошибка сохранения файла. Возможно, файл имеет атрибут \"Только для чтения\""; }
        }
        private async void OpenTextFile(string fileName)
        {
            try
            {
                if (Path.GetExtension(fileName) == ".txt")
                {
                    using (StreamReader sr = new StreamReader(fileName)) { NoteTextBox.Text = (await sr.ReadToEndAsync()); }
                    infolabel.Text = "Открыт текстовый файл";
                }
                else
                {
                    string DecFileText = "";
                    try
                    {
                        pass = "9ByGEunqAsE3H2VHjc5nLD3kXb087e";
                        string EncfileText = File.ReadAllText(fileName);
                        DecFileText = DecryptString(EncfileText, pass);
                    }
                    catch
                    {
                        pass = Microsoft.VisualBasic.Interaction.InputBox("Введите пароль для расшифровки файла", "Ввод пароля");
                        string EncfileText = File.ReadAllText(fileName);
                        DecFileText = DecryptString(EncfileText, pass);
                    }
                    finally
                    {
                        try
                        {
                            String[] Data = DecFileText.Split(new string[] { "/NoteNSplitterNoteNAjFiS/" }, StringSplitOptions.RemoveEmptyEntries);
                            if (Data[0] == GetHash(Data[6]))
                                infolabel.Text = "Файл открыт успешно";
                            else { infolabel.Text = "Файл открыт, но контрольные суммы не совпадают"; }
                            infolabel.Enabled = true;
                            контрольнаяСумма1ToolStripMenuItem.Visible = true;
                            контрольнаяСумма2ToolStripMenuItem.Visible = true;
                            шрифтToolStripMenuItem.Visible = true;
                            контрольнаяСумма1ToolStripMenuItem.Text = Data[0];
                            контрольнаяСумма2ToolStripMenuItem.Text = GetHash(Data[6]);
                            SetBackColor(Color.FromArgb(Convert.ToInt32(Data[3])));
                            SetButtonColor(Color.FromArgb(Convert.ToInt32(Data[4])));
                            this.Height = Convert.ToInt32(Data[1]);
                            this.Width = Convert.ToInt32(Data[2]);
                            if (Convert.ToBoolean(Data[5]) == true)
                            {
                                TopMost = true;
                                поверхВсехОконToolStripMenuItem.Checked = true;
                            }
                            if (Convert.ToBoolean(Data[5]) == false)
                            {
                                TopMost = false;
                                поверхВсехОконToolStripMenuItem.Checked = false;
                            }
                            NoteTextBox.Rtf = Data[6];
                        }
                        catch { infolabel.Text = "Ошибка парсинга данных, файл создан в несовместимой версии или повреждён"; }
                    }
                }
                NoteTextBox.Enabled = true;
                сохранитьToolStripMenuItem.Visible = true;
                добавитьДатуИВремяCtrlF5ToolStripMenuItem.Visible = true;
                шрифтИРазмерТекстаToolStripMenuItem.Visible = true;
                цветТекстаToolStripMenuItem1.Visible = true;
                this.Text = "NotepadN (" + Path.GetFileName(fileName) + ")";
                NoteTextBox.SelectionStart = NoteTextBox.Text.Length;
                NoteTextBox.ScrollToCaret();
                NoteTextBox.Refresh();
                файлToolStripMenuItem.Visible = true;
                FileAttributes attributes = File.GetAttributes(fileName);
                if ((attributes & FileAttributes.ReadOnly) == FileAttributes.ReadOnly)
                {
                    файлToolStripMenuItem.Checked = true;
                    NoteTextBox.ReadOnly = true;
                }
            }
            catch { infolabel.Text = "Ошибка открытия файла"; }
        }
        private void CreateTextFile()
        {
            try
            {
                if (SaveFile.ShowDialog() == DialogResult.Cancel)
                    return;
                fileName = SaveFile.FileName;
                StreamWriter file = new StreamWriter(fileName);
                file.Close();
                string fileText = File.ReadAllText(fileName);
                if (Path.GetExtension(fileName) != ".txt")
                {
                    pass = Microsoft.VisualBasic.Interaction.InputBox("Введите пароль, которым будет зашифрован файл. Оставьте пустым для стандартного шифрования.", "Ввод пароля");
                    if (pass.Length == 0) { pass = "9ByGEunqAsE3H2VHjc5nLD3kXb087e"; }
                }
                NoteTextBox.Enabled = true;
                NoteTextBox.Text = fileText;
                сохранитьToolStripMenuItem.Visible = true;
                добавитьДатуИВремяCtrlF5ToolStripMenuItem.Visible = true;
                шрифтИРазмерТекстаToolStripMenuItem.Visible = true;
                цветТекстаToolStripMenuItem1.Visible = true;
                infolabel.Text = "Файл создан успешно";
                this.Text = "NotepadN (" + Path.GetFileName(fileName) + ")";
            }
            catch { infolabel.Text = "Ошибка создания файла"; }
        }
        private void HideOrShowInterface()
        {
            try
            {
                if (isInterfaceHidden == false)
                {
                    MainMenu.Visible = false;
                    NoteTextBox.Location = new Point(5, 5);
                    NoteTextBox.Width = NoteTextBox.Width + 12;
                    NoteTextBox.Height = NoteTextBox.Height + 27;
                    isInterfaceHidden = true;
                    скрытьИнтерфейсToolStripMenuItem.Checked = true;
                    this.FormBorderStyle = FormBorderStyle.None;
                    NoteTextBox.ScrollBars = RichTextBoxScrollBars.None;
                    NoteTextBox.MouseWheel += new MouseEventHandler(MouseWheelVoid);
                }
                else
                {
                    MainMenu.Visible = true;
                    NoteTextBox.Location = new Point(12, 27);
                    NoteTextBox.Width = NoteTextBox.Width - 12;
                    NoteTextBox.Height = NoteTextBox.Height - 27;
                    isInterfaceHidden = false;
                    скрытьИнтерфейсToolStripMenuItem.Checked = false;
                    this.FormBorderStyle = FormBorderStyle.Sizable;
                    NoteTextBox.ScrollBars = RichTextBoxScrollBars.Both;
                    NoteTextBox.MouseWheel -= new MouseEventHandler(MouseWheelVoid);
                }
            }
            catch { }
        }
        void MouseWheelVoid(object sender, MouseEventArgs e)
        {
            try
            {
                if (e.Delta > 0)
                {
                    Point pOld = NoteTextBox.GetPositionFromCharIndex(NoteTextBox.SelectionStart);
                    Point pNew = new Point(pOld.X, pOld.Y - (NoteTextBox.Font.Height) * 3);
                    int charIndex = NoteTextBox.GetCharIndexFromPosition(pNew);
                    NoteTextBox.SelectionStart = charIndex;
                }
                else
                {
                    Point pOld = NoteTextBox.GetPositionFromCharIndex(NoteTextBox.SelectionStart);
                    Point pNew = new Point(pOld.X, pOld.Y + (NoteTextBox.Font.Height) * 3);
                    int charIndex = NoteTextBox.GetCharIndexFromPosition(pNew);
                    NoteTextBox.SelectionStart = charIndex;
                }
            }
            catch { }
        }
        private void создатьToolStripMenuItem_Click(object sender, EventArgs e) { CreateTextFile(); }
        private void загрузитьToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                if (OpenFile.ShowDialog() == DialogResult.Cancel)
                    return;
                OpenTextFile(OpenFile.FileName);
                fileName = OpenFile.FileName;
            }
            catch { }
        }
        public static string EncryptString(string ishText, string password)
        {
            try
            {
                SHA512 sha512Hash = new SHA512Managed();
                byte[] sourceBytes = Encoding.UTF8.GetBytes(password);
                byte[] hashBytes = sha512Hash.ComputeHash(sourceBytes);
                string salt1 = BitConverter.ToString(hashBytes).Replace("-", String.Empty);
                if (string.IsNullOrEmpty(ishText))
                    return "";
                byte[] ishTextB = Encoding.UTF8.GetBytes(ishText);
                byte[] cipherTextBytes = null;
                int iterations = 5192;
                byte[] salt = Encoding.ASCII.GetBytes(salt1);
                AesManaged aes = new AesManaged();
                aes.BlockSize = aes.LegalBlockSizes[0].MaxSize;
                aes.KeySize = aes.LegalKeySizes[0].MaxSize;
                Rfc2898DeriveBytes key = new Rfc2898DeriveBytes(password, salt, iterations);
                aes.Key = key.GetBytes(aes.KeySize / 8);
                aes.IV = key.GetBytes(aes.BlockSize / 8);
                aes.Mode = CipherMode.CBC;
                ICryptoTransform transform = aes.CreateDecryptor(aes.Key, aes.IV);
                using (ICryptoTransform encryptor = aes.CreateEncryptor(aes.Key, aes.IV))
                {
                    using (MemoryStream memStream = new MemoryStream())
                    {
                        using (CryptoStream cryptoStream = new CryptoStream(memStream, encryptor, CryptoStreamMode.Write))
                        {
                            cryptoStream.Write(ishTextB, 0, ishTextB.Length);
                            cryptoStream.FlushFinalBlock();
                            cipherTextBytes = memStream.ToArray();
                            memStream.Close();
                            cryptoStream.Close();
                        }
                    }
                }
                aes.Clear();
                return Convert.ToBase64String(cipherTextBytes);
            }
            catch
            { return null; }
        }
        public static string DecryptString(string ciphText, string password)
        {
            try
            {
                SHA512 sha512Hash = new SHA512Managed();
                byte[] sourceBytes = Encoding.UTF8.GetBytes(password);
                byte[] hashBytes = sha512Hash.ComputeHash(sourceBytes);
                string salt1 = BitConverter.ToString(hashBytes).Replace("-", String.Empty);
                if (string.IsNullOrEmpty(ciphText))
                    return "";
                byte[] ishTextB = Encoding.UTF8.GetBytes(ciphText);
                byte[] cipherTextBytes = null;
                int iterations = 5192;
                byte[] salt = Encoding.ASCII.GetBytes(salt1);
                AesManaged aes = new AesManaged();
                aes.BlockSize = aes.LegalBlockSizes[0].MaxSize;
                aes.KeySize = aes.LegalKeySizes[0].MaxSize;
                Rfc2898DeriveBytes key = new Rfc2898DeriveBytes(password, salt, iterations);
                aes.Key = key.GetBytes(aes.KeySize / 8);
                aes.IV = key.GetBytes(aes.BlockSize / 8);
                aes.Mode = CipherMode.CBC;
                cipherTextBytes = Convert.FromBase64String(ciphText);
                byte[] plainTextBytes = new byte[cipherTextBytes.Length];
                int byteCount = 0;
                using (ICryptoTransform decryptor = aes.CreateDecryptor(aes.Key, aes.IV))
                {
                    using (MemoryStream mSt = new MemoryStream(cipherTextBytes))
                    {
                        using (CryptoStream cryptoStream = new CryptoStream(mSt, decryptor, CryptoStreamMode.Read))
                        {
                            byteCount = cryptoStream.Read(plainTextBytes, 0, plainTextBytes.Length);
                            mSt.Close();
                            cryptoStream.Close();
                        }
                    }
                }
                aes.Clear();
                return Encoding.UTF8.GetString(plainTextBytes, 0, byteCount);
            }
            catch { return null; }
        }
        private void добавитьДатуИВремяCtrlF5ToolStripMenuItem_Click(object sender, EventArgs e) { AddDateTime(); }
        private void обАвтореToolStripMenuItem_Click(object sender, EventArgs e) { MessageBox.Show("NotepadN - программа, позволяющая создавать, открывать и сохранять зашифрованные текстовые файлы.\r\n\r\nПрограмма поддерживает аргументы командной строки, через которые можно передать путь к файлу для его немедленного открытия. Или можно просто перетаскивать файлы с расширением .NoteN на .exe файл программы.\r\n\r\n\nNotepadN v.4.0\r\n(c)Naulex, 2023\r\n073797@gmail.com", "О программе", MessageBoxButtons.OK, MessageBoxIcon.Information); }
        private void файлToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                if (файлToolStripMenuItem.Checked == true)
                {
                    File.SetAttributes(fileName, FileAttributes.ReadOnly);
                    NoteTextBox.ReadOnly = true;
                }
                else
                {
                    File.SetAttributes(fileName, FileAttributes.Normal);
                    NoteTextBox.ReadOnly = false;
                }
            }
            catch { }
        }
        private void поверхВсехОконToolStripMenuItem_Click_1(object sender, EventArgs e)
        {
            try
            {
                if (поверхВсехОконToolStripMenuItem.Checked == true)
                {
                    TopMost = true;
                    infolabel.Text = "Отображение поверх всех окон включено";
                }
                else
                {
                    TopMost = false;
                    infolabel.Text = "Отображение поверх всех окон отключено";
                }
            }
            catch { }
        }
        private void шрифтИРазмерТекстаToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                if (FontSelector.ShowDialog() != DialogResult.Cancel) { NoteTextBox.SelectionFont = FontSelector.Font; }
            }
            catch { }
        }
        private void цветТекстаToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            try
            {
                if (ColorSelector.ShowDialog() != DialogResult.Cancel) { NoteTextBox.SelectionColor = ColorSelector.Color; }
            }
            catch { }
        }
        private void цветФонаToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                if (ColorSelector.ShowDialog() != DialogResult.Cancel) { SetBackColor(ColorSelector.Color); }
            }
            catch { }
        }
        private void SetBackColor(Color c)
        {
            try
            {
                NoteTextBox.BackColor = c;
                BackColor = c;
                MainMenu.BackColor = c;
            }
            catch { }
        }
        private void SetButtonColor(Color c)
        {
            try
            {
                создатьToolStripMenuItem.ForeColor = c;
                загрузитьToolStripMenuItem.ForeColor = c;
                настройкиToolStripMenuItem.ForeColor = c;
                сохранитьToolStripMenuItem.ForeColor = c;
                выходToolStripMenuItem.ForeColor = c;
                infolabel.ForeColor = c;
                шрифтToolStripMenuItem.ForeColor = c;
            }
            catch { }
        }
        private void цветКнопокToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                if (ColorSelector.ShowDialog() != DialogResult.Cancel) { SetButtonColor(ColorSelector.Color); }
            }
            catch { }
        }
        private void сохранитьCtrlSToolStripMenuItem_Click(object sender, EventArgs e) { SaveTextFile(); }
        private void сохранитьКакCtrlShiftSToolStripMenuItem_Click(object sender, EventArgs e) { SaveAs(); }
        private void MainMenu_MouseDown(object sender, MouseEventArgs e)
        {
            try
            {
                Capture = false;
                Message m = Message.Create(Handle, 0xa1, new IntPtr(2), IntPtr.Zero);
                WndProc(ref m);
            }
            catch { }
        }
        private void скрытьИнтерфейсToolStripMenuItem_Click(object sender, EventArgs e) { HideOrShowInterface(); }
        private void NoteTextBox_Click(object sender, EventArgs e)
        {
            try
            { шрифтToolStripMenuItem.Text = NoteTextBox.SelectionFont.Name.ToString() + "/" + NoteTextBox.SelectionFont.Size.ToString(); }
            catch { }
        }

        void copyMenuItem_Click(object sender, EventArgs e)
        {
            try { Clipboard.SetText(NoteTextBox.SelectedText.ToString()); }
            catch { }
        }
        void pasteMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                DataFormats.Format Data = DataFormats.GetFormat(DataFormats.Text);
                NoteTextBox.Paste(Data);
            }
            catch { }
        }
        private void шрифтToolStripMenuItem_Click(object sender, EventArgs e) { try { if (FontSelector.ShowDialog() != DialogResult.Cancel) { NoteTextBox.SelectionFont = FontSelector.Font; } } catch { } }
        private void NoteTextBox_LinkClicked(object sender, LinkClickedEventArgs e) { try { System.Diagnostics.Process.Start(e.LinkText); } catch { } }
        private void NoteTextBox_KeyPress(object sender, KeyPressEventArgs e) { isModified = true; }
        private void Form1_Resize(object sender, EventArgs e) { PreventClosing(); }
        private void NotifyIcon_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            try
            {
                NotifyIcon.Visible = false;
                ShowInTaskbar = true;
                WindowState = FormWindowState.Normal;
            }
            catch { }
        }
    }
}
