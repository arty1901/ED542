using System;
using System.Collections.Generic;
using System.Net;
using System.Windows.Forms;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Schema;
using ED542;
using static System.IO.File;

namespace XMLnamespace
{
    class XML
    {

        private Helper _helper = new Helper();

        // Экземпляр класса, который осуществляет логирование действий
        private static readonly NLog.Logger Logger = NLog.LogManager.GetCurrentClassLogger();

        // Свойство, для отслеживание состояния приложения. Либо состояние создания или редактирования
        private string mode = "create";

        // Свойства, которые соответствуют реквизатам ЭС ED542
        private int _edNo = 0;
        private string _edDate = "";
        private long _edAuthor = 0;
        private decimal _edCode = 0;
        private string _edTypeNo = "";
        private int _armNo = 0;
        private int _edNoRef = 0;
        private string _edDateRef = "";
        private long _edAuthorRef = 0;

        public string Mode
        {
            get => this.mode;
            set => this.mode = value;
        }
        public int EdNo
        {
            get => this._edNo;
            set => this._edNo = value;
        }
        public string EdDate
        {
            get => this._edDate;
            set => this._edDate = value;
        }
        public long EdAuthor
        {
            get => this._edAuthor;
            set => this._edAuthor = value;
        }
        public decimal EdCode
        {
            get => this._edCode;
            set => this._edCode = value;
        }
        public string EdTypeNo
        {
            get => this._edTypeNo;
            set => this._edTypeNo = value;
        }
        public int ArmNo
        {
            get => this._armNo;
            set => this._armNo = value;
        }
        public int EdNoRef
        {
            get => this._edNoRef;
            set => this._edNoRef = value;
        }
        public long EdAuthorRef
        {
            get => _edAuthorRef;
            set => this._edAuthorRef = value;
        }

        public XML() { }

        // ResetAttr Сбрасывает раннее сохраненные значения
        public void ResetAttr()
        {
            this._edNo = 0;
            this._edDate = "";
            this._edAuthor = 0;
            this._edCode = 0;
            this._edTypeNo = "";
            this._armNo = 0;
            this._edNoRef = 0;
            this._edDateRef = "";
            this._edAuthorRef = 0;
        }

        // CreateXml формирует и возвращает XML сообщение
        private XElement CreateXml()
        {
            // Пространство имен
            XNamespace n = "urn:cbr-ru:ed:v2.0";

            XElement ed542 = new XElement(n + "ED542",
                new XAttribute("EDNo", this._edNo),
                new XAttribute("EDDate", this._edDate),
                new XAttribute("EDAuthor", this._edAuthor),
                new XAttribute("RepeatReceptInqCode", this._edCode),
                new XAttribute("ARMNo", this._armNo));

            // В зависимости от Типа запроса, либо будет добавлен элемент EDRefID
            if (this._edCode == 1)
            {
                ed542.Add(new XElement(n + "EDRefID",
                    new XAttribute("EDNo", this._edNoRef),
                    new XAttribute("EDDate", this._edDate),
                    new XAttribute("EDAuthor", this._edAuthorRef)
                    )
               );
            }

            // Либо будет добавлен аттрибут к корневому элементу
            if (this._edCode == 3)
            {
                ed542.Add(new XAttribute("EDTypeNo", this._edTypeNo));
            }

            return ed542;
        }

        public void CreateXmlDoc(ListBox list)
        {
            // XDocument cоздает XMl документ
            XDocument doc = new XDocument(CreateXml());

            // Который проходит валидацию методом ValidateXml
            // Если все хорошо, то сохраняем файл
            if (!ValidateXml(doc))
            {
                SaveXml(doc, list);
            }
        }

        // Скачиванием файл
        public void DownloadXml(string url, ListBox list)
        {
            WebClient wc = new WebClient();
            string externalXml;

            try
            {
                externalXml = wc.DownloadString(url);
            }
            catch (Exception e)
            {
                MessageBox.Show("Xml loading is failed");
                Logger.Error("Xml loading is failed" + e.Message);
                throw;
            }

            // для дальнейшей валидации, скаченный файл надо редактировать
            // а именно надо добавить пространство имен
            XNamespace n = "urn:cbr-ru:ed:v2.0";
            XDocument doc = XDocument.Parse(externalXml);

            foreach (XElement el in doc.Descendants())
            {
                el.Name = n + el.Name.LocalName;
            }

            if (!ValidateXml(doc))
            {
                SaveXml(doc, list);
            }
        }

        // Сохранение XML файла
        public void SaveXml(XDocument doc, ListBox list)
        {
            // Имя файла формируется из текущей даты и времени.
            DateTime now = DateTime.Now;
            string fileName;

            // В зависимости от режима, будет создан новый файл, либо он будет перезаписан
            if (this.mode == "create")
            {
                fileName = this._edDate + "." + now.Hour + now.Minute + now.Second + ".xml";
                list.Items.Add(fileName);
            }
            else
            {
                fileName = list.SelectedItem.ToString();
            }

            try
            {
                doc.Save(_helper.Path + "/" + fileName);
            }
            catch (Exception e)
            {
                Logger.Error("Can not Save file " + e.Message);
                throw;
            }

            MessageBox.Show("File has been saved");
            Logger.Info("File " + fileName + " has been saved");
        }

        // Загружаем выбранный файл и считываем его свойства
        public void ReadXml(string xmlName)
        {
            Logger.Info("File " + xmlName + " is opened for editing");

            XmlDocument doc = new XmlDocument();
            doc.Load(_helper.Path + "/" + xmlName);
            
            _edNo = Int32.Parse(doc.DocumentElement.GetAttribute("EDNo"));
            _edDate = doc.DocumentElement.GetAttribute("EDDate");
            _edAuthor = Int64.Parse(doc.DocumentElement.GetAttribute("EDAuthor"));
            _edCode = Int32.Parse(doc.DocumentElement.GetAttribute("RepeatReceptInqCode"));
            _armNo = Int32.Parse(doc.DocumentElement.GetAttribute("ARMNo"));

            if (_edCode == 3)
            {
                _edTypeNo = doc.DocumentElement.GetAttribute("EDTypeNo");
            }

            if (_edCode == 1)
            {
                foreach (XmlNode node in doc.DocumentElement)
                {
                    _edNoRef = Int32.Parse(node.Attributes["EDNo"].Value);
                    _edDateRef = node.Attributes["EDDate"].Value;
                    _edAuthorRef = Int64.Parse(node.Attributes["EDAuthor"].Value);
                }
            }
        }

        // Для валидации XML файлов
        private bool ValidateXml(XDocument doc)
        {
            // Создается схема
            XmlSchemaSet schema = new XmlSchemaSet();

            // Добавляем в схему xsd файл, который будет проверять входящий файл
            schema.Add("urn:cbr-ru:ed:v2.0", "../../xsd/XMLSchema1.xsd");

            bool error = false;
            string errorMessage = "";
            doc.Validate(schema, (o, e) =>
            {
                errorMessage = e.Message;
                error = true;
            });

            // валидация проходит успешно, если нет ошибок, и результат записывается в лог файл
            if (!error)
            {
                Logger.Info("Validation Successful");
            }
            else
            {
                // Если ошибки, то пользователю выдается сообщение, что валидация не прошла
                // В лог файл записывается сообщение сообщение ошибки
                MessageBox.Show("Validation failed");
                Logger.Error("Validation failed " + errorMessage);
            }

            return error;
        }

        // Удаление файла
        public void DeleteXml(ListBox list)
        {
            object item = list.SelectedItem;
            list.Items.Remove(item);
            Delete(_helper.Path + "/" + item);

            if (!Exists(_helper.Path + "/" + item))
            {
                Logger.Warn("File " + item + "has been deleted");
                MessageBox.Show("File has been deleted");
            }
        }
    }
}
