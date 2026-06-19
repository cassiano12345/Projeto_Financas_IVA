using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Security.Cryptography;
using System.Xml.Linq;
using System.IO;
using System.Security.Cryptography.X509Certificates;
using System.Net;
using System.IO.Compression;
using System.Xml;



namespace WS_IVA
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
            this.MaximizeBox = false;
            this.FormBorderStyle = FormBorderStyle.FixedSingle;
            if (args[1] != "")
            {
                string l_file = path + args[1];
                var sFileReader = File.OpenText(l_file);
                string f_linha = sFileReader.ReadToEnd();
                // Carregar o XML
                XDocument doc = XDocument.Parse(f_linha);

                // Obter Username
                XNamespace wss = "http://schemas.xmlsoap.org/ws/2002/12/secext";
                string nif = doc.Descendants(wss + "Username").FirstOrDefault()?.Value;
                Console.WriteLine("Username: " + nif);

                // Obter Password
                string password = doc.Descendants(wss + "Password").FirstOrDefault()?.Value;
                Console.WriteLine("Password: " + password);
                textBox1.Text = nif;
                textBox2.Text = password;
                textBox3.ReadOnly = true;
                textBox4.ReadOnly = true;
            }
        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }

        private String[] args = Environment.GetCommandLineArgs();

        private string path = AppDomain.CurrentDomain.BaseDirectory.ToString() + "Faturas/";

        public string strDigestAT;
        public string strEncryptPasswordAT { get; private set; }
        public string strEncryptCreatedAT { get; private set; }
        public string strEncryptNonceAT { get; private set; }

        public void EncryptAT(string pPwdAT, string pPathCertf)
        {
            try
            {
                // Load public key from certificate
                var certCP = new System.Security.Cryptography.X509Certificates.X509Certificate2();
                certCP.Import(pPathCertf);

                // Ensure the certificate has a public key
                if (!(certCP.PublicKey.Key is RSACryptoServiceProvider rsa))
                {
                    throw new Exception("The certificate does not contain a valid RSA public key.");
                }

                // Generate symmetric key (Ks)
                var Ks = new byte[16]; // AES 128-bit key
                using (var rng = new RNGCryptoServiceProvider())
                {
                    rng.GetBytes(Ks);
                }

                // Encrypt Password (SenhaPF)
                string encryptedPassword = EncryptAES(pPwdAT, Ks);

                // Generate Created (DataCriacao)
                string createdDate = DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ssZ", System.Globalization.CultureInfo.InvariantCulture);

                // Encrypt Created (DataCriacao)
                string encryptedCreated = EncryptAES(createdDate, Ks);

                // Generate Digest for Password
                string digest = CalculateSHA1Digest(pPwdAT, createdDate, Ks);

                // Encrypt symmetric key (Nonce) with RSA
                string encryptedNonce = EncryptRSA(Ks, rsa);

                // Assign results
                strEncryptPasswordAT = encryptedPassword;
                strEncryptCreatedAT = createdDate;
                strEncryptNonceAT = encryptedNonce;
                strDigestAT = digest;

                Console.WriteLine("strEncryptPasswordAT: " + strEncryptPasswordAT);
                Console.WriteLine("strEncryptCreatedAT: " + strEncryptCreatedAT);
                Console.WriteLine("strEncryptNonceAT: " + strEncryptNonceAT);
                Console.WriteLine("strDigestAT: " + strDigestAT);
            }
            catch (Exception ex)
            {
                strEncryptPasswordAT = "";
                strEncryptCreatedAT = "";
                strEncryptNonceAT = "";
                strDigestAT = "";
                Console.WriteLine("Error: " + ex.Message);
            }
        }

        private string EncryptAES(string input, byte[] key)
        {
            using (var aes = Aes.Create())
            {
                aes.Key = key;
                aes.Mode = CipherMode.ECB;
                aes.Padding = PaddingMode.PKCS7;

                using (var encryptor = aes.CreateEncryptor())
                {
                    byte[] inputBytes = Encoding.UTF8.GetBytes(input);
                    byte[] encrypted = encryptor.TransformFinalBlock(inputBytes, 0, inputBytes.Length);
                    return Convert.ToBase64String(encrypted);
                }
            }
        }

        private string EncryptRSA(byte[] data, RSACryptoServiceProvider rsa)
        {
            byte[] encryptedData = rsa.Encrypt(data, false); // false -> PKCS#1 v1.5 padding
            return Convert.ToBase64String(encryptedData);
        }

        private string CalculateSHA1Digest(string password, string created, byte[] key)
        {
            // Concatena os valores de Ks, Created e SenhaPF
            var concatenated = new List<byte>();
            concatenated.AddRange(key); // Ks
            concatenated.AddRange(Encoding.UTF8.GetBytes(created)); // Created
            concatenated.AddRange(Encoding.UTF8.GetBytes(password)); // SenhaPF

            // Calcula o SHA-1 do valor concatenado
            using (var sha1 = SHA1.Create())
            {
                byte[] sha1Hash = sha1.ComputeHash(concatenated.ToArray());

                // Criptografa o SHA-1 gerado usando AES
                using (var aes = Aes.Create())
                {
                    aes.Key = key;
                    aes.Mode = CipherMode.ECB;
                    aes.Padding = PaddingMode.PKCS7;

                    using (var encryptor = aes.CreateEncryptor(aes.Key, null))
                    using (var ms = new MemoryStream())
                    {
                        using (var cs = new CryptoStream(ms, encryptor, CryptoStreamMode.Write))
                        {
                            cs.Write(sha1Hash, 0, sha1Hash.Length);
                            cs.FlushFinalBlock();
                        }
                        byte[] encryptedDigest = ms.ToArray();

                        // Codifica o resultado em Base64
                        return Convert.ToBase64String(encryptedDigest);
                    }
                }
            }
        }


        public static string GetDeclaration_(string filePath)
        {
            string inputFilePath = filePath; // Caminho do ficheiro de entrada
            string outputFilePath = "data.txt"; // Caminho para o ficheiro data.txt
            string zipFilePath = "data.zip";    // Caminho para o ficheiro comprimido


                // Passo 1: Ler o conteúdo do ficheiro de entrada
                string content = File.ReadAllText(inputFilePath);

                // Passo 2: Escrever o conteúdo em data.txt
                File.WriteAllText(outputFilePath, content);

                // Passo 3: Comprimir o data.txt para data.zip
                using (FileStream zipStream = new FileStream(zipFilePath, FileMode.Create))
                using (ZipArchive zipArchive = new ZipArchive(zipStream, ZipArchiveMode.Create))
                {
                    // Adicionar o arquivo manualmente ao ZIP
                    ZipArchiveEntry zipEntry = zipArchive.CreateEntry("data.txt");
                    using (Stream entryStream = zipEntry.Open())
                    using (FileStream fileStream = new FileStream(outputFilePath, FileMode.Open))
                    {
                        fileStream.CopyTo(entryStream);
                    }
                }

                // Passo 4: Converter o ficheiro ZIP para Base64
                byte[] zipBytes = File.ReadAllBytes(zipFilePath);
                string base64String = Convert.ToBase64String(zipBytes);
                string doubleBase64Encoded = Convert.ToBase64String(Encoding.UTF8.GetBytes(base64String));
                Console.WriteLine("Arquivo ZIP em Base64:");
                Console.WriteLine(doubleBase64Encoded);
                return doubleBase64Encoded;
            
        }


        public void SendFileWithCertificate(string filePath, string Authenticat, string soapAction, string certificatePath, string certificatePassword)
        {
            try
            {
                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
                // 1. Carregar o certificado com senha
                var cert = new X509Certificate2(certificatePath, certificatePassword);

                // 2. Criar o cliente web para enviar a requisição
                var request = (HttpWebRequest)WebRequest.Create(Authenticat);
                request.Method = "POST";
                request.ContentType = "text/xml; charset=utf-8";
                request.Headers.Add("submeterDeclaracaoIVA", soapAction);
                request.ClientCertificates.Add(cert);

                // 3. Criar o envelope SOAP (exemplo básico, precisa adaptar conforme o serviço SOAP)
                string soapEnvelope = filePath;

                // 4. Converter o envelope SOAP em bytes
                byte[] soapBytes = Encoding.UTF8.GetBytes(soapEnvelope);
                request.ContentLength = soapBytes.Length;

                // 5. Enviar os dados
                using (Stream requestStream = request.GetRequestStream())
                {
                    requestStream.Write(soapBytes, 0, soapBytes.Length);
                }

                // 6. Receber a resposta do servidor
                using (WebResponse response = request.GetResponse())
                {
                    using (StreamReader reader = new StreamReader(response.GetResponseStream()))
                    {
                        string responseText = reader.ReadToEnd();
                        Console.WriteLine("Resposta do Servidor:");
                        richTextBox1.Clear();
                        XmlDocument xmlDoc = new XmlDocument();
                        xmlDoc.LoadXml(responseText);

                        XmlNodeList mensagens = xmlDoc.GetElementsByTagName("mensagem");

                        foreach (XmlNode node in mensagens)
                        {
                            richTextBox1.AppendText(node.InnerText + Environment.NewLine + "\n");
                        }
                        Console.WriteLine(responseText);
                        //MessageBox.Show(responseText);
                    }
                }
            }
            catch (WebException ex)
            {
                if (ex.Response != null)
                {
                    using (var responseStream = ex.Response.GetResponseStream())
                    using (var reader = new StreamReader(responseStream))
                    {
                        string responseText = reader.ReadToEnd();
                        string[] delim = { "<faultstring>" };
                        //MessageBox.Show("delim: " + delim[0]);
                        string[] linhaselecao = responseText.Split(delim, StringSplitOptions.None);
                        delim = new string[] { "</faultstring>" };
                        linhaselecao = linhaselecao.GetValue(1).ToString().Split(delim, StringSplitOptions.None);
                        richTextBox1.Clear();
                        richTextBox1.Text = linhaselecao[0];
                        //MessageBox.Show("Erro do Servidor: " + responseText);
                        Console.WriteLine("Erro do Servidor: " + responseText);
                    }
                }
                else
                {
                    MessageBox.Show("Erro ao enviar o ficheiro via SOAP: " + ex.Message);
                    Console.WriteLine("Erro ao enviar o ficheiro via SOAP: " + ex.Message);
                }
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (checkBox1.Checked == true)
            {
                try
                {
                    if(comboBox1.Text == "Autenticação do Contribuinte") { 
                    //Confirmar
                    if (args.Length >= 2)
                    {
                        string l_file = path + args[1];
                        var sFileReader = File.OpenText(l_file);
                        string f_linha = sFileReader.ReadToEnd();
                        // Carregar o XML
                        XDocument doc = XDocument.Parse(f_linha);

                        // Obter Username
                        XNamespace wss = "http://schemas.xmlsoap.org/ws/2002/12/secext";

                        // Obter Password
                        string password = textBox2.Text;
                        Console.WriteLine("Password: " + password);
                        


                        EncryptAT(password, AppDomain.CurrentDomain.BaseDirectory.ToString() + @"AT.cer");

                        // Atualizar Password
                        var passwordElement = doc.Descendants(wss + "Password").FirstOrDefault();
                        if (passwordElement != null)
                        {
                            passwordElement.Value = strEncryptPasswordAT;
                            passwordElement.SetAttributeValue("Digest", strDigestAT);
                        }

                        var UsernameElement = doc.Descendants(wss + "Username").FirstOrDefault();
                        if (UsernameElement != null)
                        {
                            UsernameElement.Value = textBox1.Text;
                        }
                            // Atualizar Nonce
                        var nonceElement = doc.Descendants(wss + "Nonce").FirstOrDefault();
                        if (nonceElement != null)
                        {
                            nonceElement.Value = strEncryptNonceAT;
                        }

                        // Atualizar Created
                        var createdElement = doc.Descendants(wss + "Created").FirstOrDefault();
                        if (createdElement != null)
                        {
                            createdElement.Value = strEncryptCreatedAT;
                        }

                        string declaracao_ = GetDeclaration_(AppDomain.CurrentDomain.BaseDirectory.ToString() + "Faturas/Data.txt");

                        // Atualizar declaracao
                        var declaracao = doc.Descendants("declaracao").FirstOrDefault();
                        if (declaracao != null)
                        {
                            declaracao.Value = declaracao_;
                        }

                        string soap = doc.ToString();
                        SendFileWithCertificate(soap, "https://servicos.portaldasfinancas.gov.pt:706/dpivaws/DeclaracaoPeriodicaIVAWebService", "https://servicos.portaldasfinancas.gov.pt/dpivaws/DeclaracaoPeriodicaIVAWebService/", AppDomain.CurrentDomain.BaseDirectory.ToString() + @"TesteWebservices.pfx", "TESTEwebservice");
                    }
                    }
                    else if(comboBox1.Text == "Autenticação do CC com plenos poderes declarativos para o Contribuinte")
                    {
                        if (args.Length >= 2)
                        {
                            string l_file = path + args[1];
                            var sFileReader = File.OpenText(l_file);
                            string f_linha = sFileReader.ReadToEnd();
                            // Carregar o XML
                            XDocument doc = XDocument.Parse(f_linha);

                            // Obter Username
                            XNamespace wss = "http://schemas.xmlsoap.org/ws/2002/12/secext";

                            EncryptAT(textBox2.Text, AppDomain.CurrentDomain.BaseDirectory.ToString() + @"AT.cer");
                            string password_funcionario = strEncryptPasswordAT;
                            string digest_funcionario = strDigestAT;
                            string nonce_funcionario = strEncryptNonceAT;

                            string declaracao_ = GetDeclaration_(AppDomain.CurrentDomain.BaseDirectory.ToString() + "Faturas/Data.txt");
                            // Atualizar declaracao
                            var declaracao = doc.Descendants("declaracao").FirstOrDefault();
                            if (declaracao != null)
                            {
                                declaracao.Value = declaracao_;
                            }
                            string novoHeader = @"<env:Header xmlns:env='http://schemas.xmlsoap.org/soap/envelope/'>
<wss:Security xmlns:wss='http://schemas.xmlsoap.org/ws/2002/12/secext' xmlns:at='http://at.pt/wsp/auth' xmlns:S='http://schemas.xmlsoap.org/soap/envelope/' S:Actor='http://at.pt/actor/SPA' at:Version='2'>
<wss:UsernameToken>
<wss:Username>@@@nifempresa</wss:Username>
</wss:UsernameToken>
</wss:Security>
<wss:Security xmlns:wss='http://schemas.xmlsoap.org/ws/2002/12/secext' xmlns:at='http://at.pt/wsp/auth' xmlns:S='http://schemas.xmlsoap.org/soap/envelope/' S:Actor='http://at.pt/actor/TOC' at:Version='2'>
<wss:UsernameToken>
<wss:Username>@@@nif_funcionaio</wss:Username>
<wss:Password Digest='@@@digestfuncionario'>@@@password</wss:Password>
<wss:Nonce>@@@noncefuncionario</wss:Nonce>
<wss:Created>@@@data</wss:Created>
</wss:UsernameToken>
</wss:Security>
</env:Header>";
                            novoHeader = novoHeader.Replace("@@@nifempresa", textBox3.Text);
                            novoHeader = novoHeader.Replace("@@@nif_funcionaio", textBox1.Text);
                            novoHeader = novoHeader.Replace("@@@noncefuncionario", nonce_funcionario);
                            novoHeader = novoHeader.Replace("@@@digestfuncionario", digest_funcionario);
                            novoHeader = novoHeader.Replace("@@@password", password_funcionario);
                            novoHeader = novoHeader.Replace("@@@data", strEncryptCreatedAT);

                            var headerAntigo = doc.Root.Element(XName.Get("Header", "http://schemas.xmlsoap.org/soap/envelope/"));
                            headerAntigo?.Remove();

                            // Adiciona o novo header
                            XElement novoHeaderElement = XElement.Parse(novoHeader);
                            doc.Root.AddFirst(novoHeaderElement);
                            string soap = doc.ToString();
                            SendFileWithCertificate(soap, "https://servicos.portaldasfinancas.gov.pt:706/dpivaws/DeclaracaoPeriodicaIVAWebService", "https://servicos.portaldasfinancas.gov.pt/dpivaws/DeclaracaoPeriodicaIVAWebService/", AppDomain.CurrentDomain.BaseDirectory.ToString() + @"TesteWebservices.pfx", "TESTEwebservice");
                        }                        
                    }
                    else if (comboBox1.Text == "Autenticação do CC sem plenos poderes declarativos para o Contribuinte")
                    {
                        if (args.Length >= 2)
                        {
                            string l_file = path + args[1];
                            var sFileReader = File.OpenText(l_file);
                            string f_linha = sFileReader.ReadToEnd();
                            // Carregar o XML
                            XDocument doc = XDocument.Parse(f_linha);

                            // Obter Username
                            XNamespace wss = "http://schemas.xmlsoap.org/ws/2002/12/secext";

                            EncryptAT(textBox2.Text, AppDomain.CurrentDomain.BaseDirectory.ToString() + @"AT.cer");
                            string password_funcionario = strEncryptPasswordAT;
                            string digest_funcionario = strDigestAT;
                            string nonce_funcionario = strEncryptNonceAT;
                            string data = strEncryptNonceAT;

                            EncryptAT(textBox4.Text, AppDomain.CurrentDomain.BaseDirectory.ToString() + @"AT.cer");
                            string password_emp = strEncryptPasswordAT;
                            string digest_emp = strDigestAT;
                            string nonce_emp = strEncryptNonceAT;

                            string declaracao_ = GetDeclaration_(AppDomain.CurrentDomain.BaseDirectory.ToString() + "Faturas/Data.txt");
                            // Atualizar declaracao
                            var declaracao = doc.Descendants("declaracao").FirstOrDefault();
                            if (declaracao != null)
                            {
                                declaracao.Value = declaracao_;
                            }

                            string novoHeader = @"
<env:Header xmlns:env='http://schemas.xmlsoap.org/soap/envelope/'>
    <wss:Security xmlns:wss='http://schemas.xmlsoap.org/ws/2002/12/secext' xmlns:at='http://at.pt/wsp/auth' xmlns:S='http://schemas.xmlsoap.org/soap/envelope/' S:Actor='http://at.pt/actor/SPA' at:Version='2'>
        <wss:UsernameToken>
            <wss:Username>@@@nifempresa</wss:Username>
            <wss:Password Digest='@@@digestemp'>@@@passwordemp</wss:Password>
            <wss:Nonce>@@@nonceemp</wss:Nonce>
            <wss:Created>@@@data</wss:Created>
        </wss:UsernameToken>
    </wss:Security>
    <wss:Security xmlns:wss='http://schemas.xmlsoap.org/ws/2002/12/secext' xmlns:at='http://at.pt/wsp/auth' xmlns:S='http://schemas.xmlsoap.org/soap/envelope/' S:Actor='http://at.pt/actor/TOC' at:Version='2'>
        <wss:UsernameToken>
            <wss:Username>@@@nif_funcionaio</wss:Username>
            <wss:Password Digest='@@@digestfuncionario'>@@@passwordfuncionario</wss:Password>
            <wss:Nonce>@@@noncefuncionario</wss:Nonce>
            <wss:Created>@@@data</wss:Created>
        </wss:UsernameToken>
    </wss:Security>
</env:Header>";
                            // Empresa
                            novoHeader = novoHeader.Replace("@@@nifempresa", textBox3.Text);
                            novoHeader = novoHeader.Replace("@@@nonceemp", nonce_emp);
                            novoHeader = novoHeader.Replace("@@@digestemp", digest_emp);
                            novoHeader = novoHeader.Replace("@@@passwordemp", password_emp);
                            novoHeader = novoHeader.Replace("@@@data", strEncryptCreatedAT);
                            // Empresa

                            // Funcionario
                            novoHeader = novoHeader.Replace("@@@nif_funcionaio", textBox1.Text);
                            novoHeader = novoHeader.Replace("@@@noncefuncionario", nonce_funcionario);
                            novoHeader = novoHeader.Replace("@@@digestfuncionario", digest_funcionario);
                            novoHeader = novoHeader.Replace("@@@passwordfuncionario", password_funcionario);
                            novoHeader = novoHeader.Replace("@@@data", strEncryptCreatedAT);
                            // Funcionario


                            var headerAntigo = doc.Root.Element(XName.Get("Header", "http://schemas.xmlsoap.org/soap/envelope/"));
                            headerAntigo?.Remove();

                            // Adiciona o novo header
                            XElement novoHeaderElement = XElement.Parse(novoHeader);
                            doc.Root.AddFirst(novoHeaderElement);
                            string soap = doc.ToString();
                            SendFileWithCertificate(soap, "https://servicos.portaldasfinancas.gov.pt:706/dpivaws/DeclaracaoPeriodicaIVAWebService", "https://servicos.portaldasfinancas.gov.pt/dpivaws/DeclaracaoPeriodicaIVAWebService/", AppDomain.CurrentDomain.BaseDirectory.ToString() + @"TesteWebservices.pfx", "TESTEwebservice");
                        }
                    }
                    else
                    {
                        MessageBox.Show("Indique o tipo de autenticação!");
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Erro ao enviar o e-mail:" + ex.Message);
                }
            }
            else
            {
                /*
                try
                {
                    //Confirmar
                    if (args.Length >= 2)
                    {
                        string l_file = path + args[1];
                        var sFileReader = File.OpenText(l_file);
                        string f_linha = sFileReader.ReadToEnd();
                        // Carregar o XML
                        XDocument doc = XDocument.Parse(f_linha);

                        // Obter Username
                        XNamespace wss = "http://schemas.xmlsoap.org/ws/2002/12/secext";
                        string username = textBox1.Text;
                        Console.WriteLine("Username: " + username);

                        // Obter Password
                        string password = doc.Descendants(wss + "Password").FirstOrDefault()?.Value;
                        Console.WriteLine("Password: " + password);

                        // Obter InvoiceNo
                        XNamespace ns = "http://factemi.at.min_financas.pt/documents";
                        string invoiceNo = doc.Descendants(ns + "InvoiceNo").FirstOrDefault()?.Value;
                        Console.WriteLine("InvoiceNo: " + invoiceNo);

                        // Obter InvoiceDate
                        string invoiceDate = doc.Descendants(ns + "InvoiceDate").FirstOrDefault()?.Value;
                        Console.WriteLine("InvoiceDate: " + invoiceDate);

                        // Obter TaxPayable
                        string taxPayable = doc.Descendants(ns + "TaxPayable").FirstOrDefault()?.Value;
                        Console.WriteLine("TaxPayable: " + taxPayable);

                        EncryptAT(password, AppDomain.CurrentDomain.BaseDirectory.ToString() + @"AT.cer");

                        // Atualizar NIF
                        var UsernameElement = doc.Descendants(wss + "Username").FirstOrDefault();
                        if (UsernameElement != null)
                        {
                            UsernameElement.Value = username;
                        }

                        // Atualizar Password
                        var passwordElement = doc.Descendants(wss + "Password").FirstOrDefault();
                        if (passwordElement != null)
                        {
                            passwordElement.Value = strEncryptPasswordAT;
                            //passwordElement.SetAttributeValue("Digest", strDigestAT);
                        }

                        // Atualizar Nonce
                        var nonceElement = doc.Descendants(wss + "Nonce").FirstOrDefault();
                        if (nonceElement != null)
                        {
                            nonceElement.Value = strEncryptNonceAT;
                        }

                        // Atualizar Created
                        var createdElement = doc.Descendants(wss + "Created").FirstOrDefault();
                        if (createdElement != null)
                        {
                            createdElement.Value = strEncryptCreatedAT;
                        }

                        string soap = doc.ToString();
                        SendFileWithCertificate(soap, "https://servicos.portaldasfinancas.gov.pt:423/fatcorews/ws/", "http://factemi.at.min_financas.pt/documents", AppDomain.CurrentDomain.BaseDirectory.ToString() + @"506511529.pfx", "novasoft_");
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Erro ao enviar o e-mail: " + ex.Message);
                }
                */
                /*
                string resp = GetATGTCode(soap,
                                          "https://servicos.portaldasfinancas.gov.pt:700/fatcorews/ws/",
                                          "https://servicos.portaldasfinancas.gov.pt:700/fatcorews/ws/",
                                          AppDomain.CurrentDomain.BaseDirectory.ToString() + @"506511529.pfx",
                                          "novasoft_");
                */
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void label3_Click(object sender, EventArgs e)
        {
            checkBox1.Visible = false;
            checkBox1.Checked = false;
            label5.Visible = false;
        }

        private void PB_Logo_Click(object sender, EventArgs e)
        {
            checkBox1.Visible = true;
            label5.Visible = true;
        }


        private void comboBox1_SelectedIndexChanged_1(object sender, EventArgs e)
        {
            if (comboBox1.Text == "Autenticação do CC com plenos poderes declarativos para o Contribuinte")
            {
                textBox3.ReadOnly = false;
                textBox4.ReadOnly = true;

            }
            else if (comboBox1.Text == "Autenticação do CC sem plenos poderes declarativos para o Contribuinte")
            {
                textBox3.ReadOnly = false;
                textBox4.ReadOnly = false;
            }
            if (comboBox1.Text == "Autenticação do Contribuinte")
            {
                textBox3.ReadOnly = true;
                textBox4.ReadOnly = true;
            }
        }

        private void label5_Click(object sender, EventArgs e)
        {
            if (checkBox1.Checked == false)
            {
                checkBox1.Checked = true;
            }
            else
            {
                checkBox1.Checked = false;
            }
        }
    }
}
