using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using System.IO;

namespace MagtifunSMS
{
    public partial class FrmMagtifun : Form
    {
        Login login;

        readonly string _loginFile = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + "/login.txt";
        readonly string _mobNumbersFile = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + "/contacts.txt";
        
        string _number, _name, _line, _messageCount;


        public FrmMagtifun()
        {
            InitializeComponent();
            login = new Login();

            PopulateListbox();
            TryAutoLogin();
        }

#region Methods

        /// <summary>
        /// Generate Status message
        /// </summary>
        /// <param name="pageContent">pagecontent from post request</param>
        public void GenerateStatus(string pageContent)
        {
            if (pageContent.Contains("მოგესალმებით"))
            {
                //ვიღებთ მესიჯების რაოდენობას კონკრეტული ადგილიდან - 'თქვენს ანგარიშზეა'-დან
                //სიმბოლოების რაოდენობის გადათვლით. ასევე ვიღებთ სახელსაც.
                _messageCount = pageContent.Substring(pageContent.IndexOf("თქვენს ანგარიშზეა", StringComparison.Ordinal) + 59, 2);
                _name = pageContent.Substring(pageContent.IndexOf("მოგესალმებით", StringComparison.Ordinal) + 55, 85).Split('<', '>')[1];

                Status = "Logged In";
                StatusColor = Color.Green;

                MessageCount = int.Parse(_messageCount);
                GreetName = _name;
            }
            else if (pageContent.Contains("success"))
            {
                Status = "Message Sent";
                StatusColor = Color.Green;
                if (CharCount < 147)
                {
                    MessageCount -= 1; //მესიჯების რაოდენობას ვამცირებთ გაგზავნის მერე
                }
                else if (CharCount < 293)
                {
                    MessageCount -= 2;
                }
                else
                {
                    MessageCount -= 3;
                }
            }
            else
            {
                Status = "Something went wrong. Try again.";
                StatusColor = Color.Red;
            }
        }

        /// <summary>
        /// Log in to account
        /// </summary>
        /// <param name="user">username</param>
        /// <param name="password">password</param>
        public void Login(string user, string password)
        {
            lblStatus.Text = "logging...";
            lblStatus.ForeColor = Color.Purple;

            try
            {
                string postData = "act=" + 1 + "&user=" + user +
                    "&password=" + password + "&remember=on";
                string requestUrl = @"http://www.magtifun.ge/index.php?page=11&lang=ge";


                GenerateStatus(login.PostRequest(postData, requestUrl));

                if (lblStatus.Text == "Logged In")
                {
                    tbNum.Enabled = true;
                    tbMsg.Enabled = true;
                    btnSend.Enabled = true;
                    tbUser.Enabled = false;
                    tbPass.Enabled = false;
                    btnLogin.Enabled = false;
                }
            }
            catch
            {
                // ignored
            }
        }

        /// <summary>
        /// Try Auto Login with credentials from file
        /// </summary>
        public void TryAutoLogin()
        {
            string user, pass;
            if (File.Exists(_loginFile))
            {
                using (StreamReader reader = new StreamReader(_loginFile))
                {
                    user = reader.ReadLine();
                    pass = reader.ReadLine();
                }
                Login(user, pass);
            }
        }

        /// <summary>
        /// Populate Listbox with Contacts from file
        /// </summary>
        public void PopulateListbox()
        {
            List<string> contacts = new List<string>();

            if (File.Exists(_mobNumbersFile))
            {
                using (StreamReader reader = new StreamReader(_mobNumbersFile))
                {
                    string line;
                    while ((line = reader.ReadLine()) != null)
                    {
                        _number = line.Split(',')[0];
                        _name = " - " + line.Split(',')[1].Trim() + "";

                        contacts.Add(_number + _name);
                    }
                }
            }
            listBoxNumbers.Items.AddRange(contacts.ToArray());
        }
#endregion

#region Events
        private void btnLogin_Click(object sender, EventArgs e)
        {
            string user = tbUser.Text;
            string pass = tbPass.Text;
            Login(user, pass);
        }

        private void btnSend_Click(object sender, EventArgs e)
        {
            lblStatus.Text = "Sending...";
            lblStatus.ForeColor = Color.Purple;

            try
            {
                string postData = "recipients=" + tbNum.Text + "&message_body=" + tbMsg.Text;
                string requestUrl = @"http://www.magtifun.ge/scripts/sms_send.php";

                
                if (tbNum.Text != string.Empty && tbMsg.Text != string.Empty)
                {
                    GenerateStatus(login.PostRequest(postData, requestUrl));
                }
                else
                {
                    lblStatus.Text = "Write correct number and a message.";
                    lblStatus.ForeColor = Color.Red;
                }
            }
            catch
            {
                // ignored
            }
        }

        private void tbMsg_TextChanged(object sender, EventArgs e)
        {
            int count = tbMsg.Text.Length;

            lblCharCount.Text = count.ToString();

            if (count > 445)
            {
                btnSend.Enabled = false;
                tbMsg.BackColor = Color.Red;
            }
            else
            {
                btnSend.Enabled = true;
                tbMsg.BackColor = SystemColors.Window;
            }
        }



        private void listBoxNumbers_SelectedIndexChanged(object sender, EventArgs e)
        {
            _line = listBoxNumbers.SelectedItem.ToString();
            _number = _line.Split(' ')[0];

            tbNum.Text = _number;
        }
#endregion

#region Properties

        public string Status
        {
            get { return lblStatus.Text; }
            set { lblStatus.Text = value; }
        }
        public Color StatusColor
        {
            get { return lblStatus.ForeColor; }
            set { lblStatus.ForeColor = value; }
        }

        public int MessageCount
        {
            get { return int.Parse(lblMessageCount.Text); }
            set { lblMessageCount.Text = value.ToString(); }
        }

        public string GreetName
        {
            get { return lblName.Text; }
            set { lblName.Text = value; }
        }

        public int CharCount
        {
            get { return int.Parse(lblCharCount.Text); }
        }
#endregion
    }
}