using System;
using System.Windows.Forms;
using XMLnamespace;

namespace ED542
{
    public partial class ED542 : Form
    {
        private XML _xml = new XML();
        private Helper Helper = new Helper();
        private static readonly NLog.Logger Logger = NLog.LogManager.GetCurrentClassLogger();

        private string _day;
        private string _month;
        private string _year;

        public ED542()
        {
            // Записываем в лог о том, что приложение запущено
            Logger.Info("App has been started");
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            Helper.ScanDirectory(listBox1);
        }

        private void button1_Click(object sender, EventArgs e)
        {
            // проверяем, все ли данные введены корректно
            bool isValid = Helper.ValidateInputData(
                EDNo.Text,
                EDAuthor.Text,
                RepeatReceptInqCode.Value,
                ARMNo.Text,
                EDNoRef.Text,
                EDAuthorRef.Text
            );

            // если что то не так, то делает return
            if (!isValid)
            {
                return;
            }

            // устанавливаем режим создания
            _xml.Mode = "create";

            SetXmlAttr();

            // Создаем файл
            _xml.CreateXmlDoc(listBox1);

            // Сбрасываем все состояния
            Helper.ResetControls(this);
            _xml.ResetAttr();
        }

        private void numericUpDown1_ValueChanged(object sender, EventArgs e)
        {
            // В зависимости от выбранного типа запроса
            // Соотвествующие поля будут отключены, либо включены
            switch (RepeatReceptInqCode.Value)
            {
                case 1:
                    this.EDNoRef.Enabled = true;
                    this.EDAuthorRef.Enabled = true;
                    this.EDTypeNo.Enabled = false;
                    break;
                case 3:
                    this.EDTypeNo.Enabled = true;
                    this.EDNoRef.Enabled = false;
                    this.EDAuthorRef.Enabled = false;
                    break;
                default:
                    this.EDNoRef.Enabled = false;
                    this.EDAuthorRef.Enabled = false;
                    this.EDTypeNo.Enabled = false;
                    break;
            }
        }

        private void editButton_Click(object sender, EventArgs e)
        {
            // Отключаем ненужные кнопки и включаем нужные
            createButton.Enabled = false;
            openXmlButton.Enabled = false;
            saveButton.Enabled = true;

            FillUpFields();
        }

        private void saveButton_Click(object sender, EventArgs e)
        {
            bool isValid = Helper.ValidateInputData(
                EDNo.Text,
                EDAuthor.Text,
                RepeatReceptInqCode.Value,
                ARMNo.Text,
                EDNoRef.Text,
                EDAuthorRef.Text
            );

            if (!isValid)
            {
                return;
            }

            _xml.Mode = "edit";
            
            createButton.Enabled = true;
            saveButton.Enabled = false;

            SetXmlAttr();

            _xml.CreateXmlDoc(listBox1);

            Helper.ResetControls(this);
            _xml.ResetAttr();
        }

        // записывает полученные данные в свойства класса XML
        private void SetXmlAttr()
        {
            _day = EDDate.Value.Day < 10 ? "0" + EDDate.Value.Day : EDDate.Value.Day.ToString();
            _month = EDDate.Value.Month < 10 ? "0" + EDDate.Value.Month : EDDate.Value.Month.ToString();
            _year = EDDate.Value.Year.ToString();
            _xml.EdDate = _year + "-" + _month + "-" + _day; ;

            _xml.EdNo = Int32.Parse(EDNo.Text);
            _xml.EdAuthor = Int64.Parse(EDAuthor.Text);
            _xml.EdCode = RepeatReceptInqCode.Value;
            _xml.ArmNo = Int32.Parse(ARMNo.Text);
            _xml.EdTypeNo = EDTypeNo.Text;

            _xml.EdNoRef = Int32.Parse(EDNoRef.Text);
            _xml.EdAuthorRef = Int64.Parse(EDAuthorRef.Text);
        }
        
        // записывает данные со свойств класса в поля
        private void FillUpFields()
        {
            _xml.ReadXml(listBox1.SelectedItem.ToString());

            DateTime date = DateTime.Parse(_xml.EdDate);

            EDNo.Text = _xml.EdNo.ToString();
            EDAuthor.Text = _xml.EdAuthor.ToString();
            RepeatReceptInqCode.Value = _xml.EdCode;
            ARMNo.Text = _xml.ArmNo.ToString();
            EDDate.Value = date;

            // For RepeatReceptInqCode = 3
            EDTypeNo.Text = _xml.EdTypeNo;

            // For RepeatReceptInqCode = 1
            EDNoRef.Text = _xml.EdNoRef.ToString();
            EDAuthorRef.Text = _xml.EdAuthorRef.ToString();
        }

        private void deleteButton_Click(object sender, EventArgs e)
        {
            _xml.DeleteXml(listBox1);
            Helper.ResetControls(this);
        }

        
        private void listBox1_Click(object sender, EventArgs e)
        {
            if (listBox1.SelectedItem != null)
            {
                deleteButton.Enabled = true;
                openXmlButton.Enabled = true;
                editButton.Enabled = true;
            }
        }

        private void EDNo_TextChanged(object sender, EventArgs e)
        {
            Helper.ValidateInputText(EDNo);
        }

        private void EDAuthor_TextChanged(object sender, EventArgs e)
        {
            Helper.ValidateInputText(EDAuthor);
        }

        private void ARMNo_TextChanged(object sender, EventArgs e)
        {
            Helper.ValidateInputText(ARMNo);
        }

        private void EDNoRef_TextChanged(object sender, EventArgs e)
        {
            Helper.ValidateInputText(EDNoRef);
        }

        private void EDAuthorRef_TextChanged(object sender, EventArgs e)
        {
            Helper.ValidateInputText(EDAuthorRef);
        }

        // Кнопка сброса всех состояний
        private void button1_Click_1(object sender, EventArgs e)
        {
            Helper.ResetControls(this);
            _xml.ResetAttr();
            _xml.Mode = "create";
        }

        // Кнопка для скачивания
        private void button2_Click(object sender, EventArgs e)
        {
            if (UrlTextBox.Text != "")
            {
                _xml.DownloadXml(UrlTextBox.Text, listBox1);
            }
            else
            {
                MessageBox.Show("Please, type in an URL");
            }
        }

        // кнопка, которая открывает выбранный файл, чтобы посмотреть что внутри
        private void openXmlButton_Click(object sender, EventArgs e)
        {
            FillUpFields();
        }
    }
}