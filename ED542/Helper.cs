using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using System.Windows.Forms;

namespace ED542
{
    // Вспомогательный класс, в котором описаны методы "облегчающие" жизнь
    class Helper
    {
        public string Path = "xml";
        // Шаблоны для регулярных выражений
        private string edNoPattern = @"\d{1,9}";
        private string edAuthorPattern = @"\d{10}";
        private string edArmNoPattern = @"\d{2}";
        private string inputPattern = "[^0-9]";

        public Helper() { }

        // ScanDirectory считывает список файлов, которые записаны в папку xml. Эти имена файлов записываются listBox
        public void ScanDirectory(ListBox listBox)
        {
            string[] files = Directory.GetFiles(Path);

            if (files.Length != 0)
            {
                foreach (string file in files)
                {
                    int temp = file.IndexOf(@"\");
                    listBox.Items.Add(file.Substring(temp + 1));
                }
            }
        }

        // ValidateInputData производит валидацию введеных данных
        public bool ValidateInputData(string edNo, string edAuthor, decimal edCode, string armNo, string edNoRef = "", string edAuthorRef = "")
        {
            Regex edNoRegex = new Regex(edNoPattern);
            Regex edAuthorRegex = new Regex(edAuthorPattern);
            Regex edArmRegex = new Regex(edArmNoPattern);

            if (!edNoRegex.IsMatch(edNo))
            {
                MessageBox.Show("Invalid format for EdNo");
                return false;
            }

            if (!edAuthorRegex.IsMatch(edAuthor))
            {
                MessageBox.Show("Invalid format for EdAuthor");
                return false;
            }

            if (!edArmRegex.IsMatch(armNo))
            {
                MessageBox.Show("Invalid format for ArmNo");
                return false;
            }

            if (edCode == 1)
            {
                if (!edNoRegex.IsMatch(edNoRef))
                {
                    MessageBox.Show("Invalid format of EDNo of EDRefID");
                    return false;
                }

                if (!edAuthorRegex.IsMatch(edAuthorRef))
                {
                    MessageBox.Show("Invalid format of EDAuthor of EDRefID");
                    return false;
                }
            }

            return true;
        }

        // ValidateInputText проверяет вводимые данные для тех полей, где требуются числовые значения
        public void ValidateInputText(TextBox box)
        {
            if (Regex.IsMatch(box.Text, inputPattern))
            {
                MessageBox.Show("Only Numbers");
                box.Text = box.Text.Remove(box.TextLength - 1);
                box.SelectionStart = box.TextLength;
            }
        }

        // ResetControls Сбрасывает значения всех полей
        public void ResetControls(Control form)
        {
            foreach (Control control in form.Controls)
            {
                if (control is TextBox)
                {
                    TextBox textBox = (TextBox)control;
                    textBox.Text = null;
                }

                if (control is NumericUpDown)
                {
                    NumericUpDown numeric = (NumericUpDown)control;
                    numeric.Value = 0;
                }
            }
        }
    }
}
