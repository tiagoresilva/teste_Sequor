using System;
using System.Diagnostics;
using System.ComponentModel.DataAnnotations;
using Newtonsoft.Json;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Net;
using System.Globalization;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using System.IO;

namespace Teste_Sequor
{    
    public partial class Form1 : Form
    {
        [DllImport("user32.DLL", EntryPoint = "ReleaseCapture")]
        private extern static void ReleaseCapture();
        [DllImport("user32.DLL", EntryPoint = "SendMessage")]
        private extern static void SendMessage(System.IntPtr hWnd, int wMsg, int wParam, int lParam);

        public dynamic ordersJson { get; set; }

        public string ordemSelecionada { get; set; }
        public double tempoDeCicloCalculado { get; set; }
        public float tempoDeCiclo { get; set; }
         
        Stopwatch stopwatch = new Stopwatch();
        

        public string produto { get; set; }
        public string imagem { get; set; }
        public Form1()
        {
            InitializeComponent();
            GetOrders();
            
            BindData();
            this.MaximizedBounds = Screen.FromHandle(this.Handle).WorkingArea;


        }
        public void BindData()
        {
            foreach (var j in ordersJson.orders)
            {
                
                comboBoxOrdens.Items.Add(j.order.ToString());             
            }

        }
        public void GetOrders()
        {
            try
            {
                
                string url = "http://web-lumen-transcricao.lumenplatform.io:8080/ProductionOrdersTest/api/orders/GetOrders";

                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
                WebResponse response = request.GetResponse();                

                using (Stream stream = response.GetResponseStream())
                {
                    
                    StreamReader reader = new StreamReader(stream);
                    object objResponse = reader.ReadToEnd();
                    ordersJson = JsonConvert.DeserializeObject(objResponse.ToString());                    
                    stream.Close();
                    response.Close();
                }

            }
            catch (WebException webEx)
            {
                MessageBox.Show(webEx.ToString());
                
            }
            
        }

        private void label2_Click(object sender, EventArgs e)
        {

        }

        private void Menu_Paint(object sender, PaintEventArgs e)
        {
            Pen pen = new Pen(Color.FromArgb(255, 255, 255, 255));
            e.Graphics.DrawLine(pen, 315, 10, 315, 680);
        }
        private void Menu_MouseDown(object sender, MouseEventArgs e)
        {
            ReleaseCapture();
            SendMessage(this.Handle, 0x112, 0xf012, 0);
        }

        private void fechar_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        private void maximizar_Click(object sender, EventArgs e)
        {
            if (WindowState == FormWindowState.Normal)
            { this.WindowState = FormWindowState.Maximized; }
            else { this.WindowState = FormWindowState.Normal; }
        }

        private void minimizar_Click(object sender, EventArgs e)
        {
            this.WindowState = FormWindowState.Minimized;
        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        private void button1_Click(object sender, EventArgs e)
        {

        }

        private void textBox10_TextChanged(object sender, EventArgs e)
        {

        }
        private void SetProduction()
        {
            try
            {
                string url = "http://web-lumen-transcricao.lumenplatform.io:8080/ProductionOrdersTest/api/orders/SetProduction";
                var httpWebRequest = (HttpWebRequest)WebRequest.Create(url);
                httpWebRequest.ContentType = "application/json";
                httpWebRequest.Method = "POST";
                string[] wordsMaterial = comboBoxMaterial.SelectedItem.ToString().Split(' ');
                string material = wordsMaterial[1].Trim();

                using (var streamWriter = new StreamWriter(httpWebRequest.GetRequestStream()))
                {
                    string json = JsonConvert.SerializeObject(new
                    {
                        email = textBoxEmail.Text,
                        order = ordemSelecionada,
                        productionDate = dateTimeApontamento.Value.ToString("yyyy-MM-dd"),
                        productionTime = DateTime.Now.ToString("HH:mm:ss tt"),
                        quantity = Int32.Parse(textBoxQuantidade.Text),
                        materialCode = material,
                        cycleTime = tempoDeCicloCalculado
                    });
                    //MessageBox.Show(json);
                    streamWriter.Write(json);
                }
                var httpResponse = (HttpWebResponse)httpWebRequest.GetResponse();
                using (var streamReader = new StreamReader(httpResponse.GetResponseStream()))
                {
                    
                    
                    object objResponse = streamReader.ReadToEnd();
                    dynamic response = JsonConvert.DeserializeObject(objResponse.ToString());
                    string status = response.status.ToString();
                    MessageBox.Show(status);
                    if (httpResponse.StatusCode == HttpStatusCode.OK)
                    {
                        textBoxEmail.Clear();
                        textBoxProduto.Clear();
                        textBoxImagem.Clear();
                        textBoxQuantidade.Clear();
                        comboBoxMaterial.Items.Clear();
                        comboBoxMaterial.ResetText();
                        textBoxCycleTime.Clear();

                    }
                        
                    streamReader.Close();
                    httpResponse.Close();
                }

            }
            catch (WebException webEx)
            {
                MessageBox.Show(webEx.ToString());
                

            }
        }

        private void buttonSelecionar_Click_1(object sender, EventArgs e)
        {
            
            stopwatch.Reset();
            stopwatch.Stop();
            comboBoxMaterial.Items.Clear();
            if (comboBoxOrdens.SelectedIndex == -1)
            {
                MessageBox.Show("Selecione uma ordem");
            }
            else
            {
                ordemSelecionada = comboBoxOrdens.SelectedItem.ToString();
                foreach (var j in ordersJson.orders)
                {
                    if (j.order == ordemSelecionada)
                    {
                        produto = "Código: " + j.productCode.ToString() +
                            "   |   Descrição: " + j.productDescription.ToString();
                        imagem = j.image.ToString();
                        tempoDeCiclo = Convert.ToSingle(j.cycleTime);
                        if (textBoxEmail.Text == "")
                        {
                            MessageBox.Show("Insira um email");
                        }
                        else if (!IsValidEmail(textBoxEmail.Text))
                        {
                            MessageBox.Show("email inválido");
                        }
                        else
                        {
                            foreach (var m in j.materials)
                            {

                                comboBoxMaterial.Items.Add("Código: " + m.materialCode.ToString() + "   |   Descrição: " + m.materialDescription.ToString());

                            }
                        }


                    }

                }
                textBoxProduto.Text = produto;
                textBoxImagem.Text = imagem;
                stopwatch.Start();
            }            
        }
        public bool IsValidEmail(string source)
        {
            return new EmailAddressAttribute().IsValid(source);
        }

        private void buttonEnviar_Click(object sender, EventArgs e)
        {
            tempoDeCicloCalculado = Math.Round((float)(stopwatch.Elapsed.TotalMilliseconds / 1000), 1);
            textBoxCycleTime.Text = tempoDeCicloCalculado.ToString() + " segundos";            
            if(textBoxQuantidade.Text == "")
            {
                MessageBox.Show("Insira quantidade");            
            }
            else if (comboBoxMaterial.SelectedIndex == -1)
            {
                MessageBox.Show("Selecione um material");
            }
            else if (tempoDeCiclo > tempoDeCicloCalculado)
            {
                MessageBox.Show("Envio não realizado, tempo de ciclo menor que o referente à ordem selecionada");

            }
            else
            {
                SetProduction();
                tempoDeCiclo = 0;
                tempoDeCicloCalculado = 0;
                stopwatch.Reset();
                stopwatch.Stop();

            }
        }

        private void textBoxProduto_TextChanged(object sender, EventArgs e)
        {

        }

        private void comboBoxOrdens_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        private void pictureBox1_Click(object sender, EventArgs e)
        {

        }

        private void dateTimeApontamento_ValueChanged(object sender, EventArgs e)
        {

        }
    }
}
