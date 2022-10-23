using RestSharp;

namespace aspnet_sendgrid_mailing
{
    public class SendMail : BackgroundService
    {
        private readonly IHostApplicationLifetime _hostApplicationLifetime;
        private readonly ILogger<SendMail> _logger;
        private int _mailing_interval;
        private bool _save_sent_mail;
        private string _mail_template_file;
        private string _sendgrid_apikey;
        private string _from_name;
        private string _from_email_address;
        private string _subject;
        private List<string> _results = new List<string>();
        private string template_file = Path.Combine(Directory.GetCurrentDirectory() , "data\\mail_template.html");
        private string path_mail_address = Path.Combine(Directory.GetCurrentDirectory(), "data\\mail_adres_list.txt");
        private string sent_mail_address_list = Path.Combine(Directory.GetCurrentDirectory(), "data\\sent_mail_address_list.txt");

        public SendMail(ILogger<SendMail> logger, IHostApplicationLifetime hostApplicationLifetime)
        {
            _hostApplicationLifetime = hostApplicationLifetime;
            _logger = logger;
            _mailing_interval = int.Parse(new ConfigurationBuilder().AddJsonFile("appsettings.json").Build().GetSection("mailing_interval").Value);
            _save_sent_mail = bool.Parse(new ConfigurationBuilder().AddJsonFile("appsettings.json").Build().GetSection("save_sent_mail").Value);
            _mail_template_file = new ConfigurationBuilder().AddJsonFile("appsettings.json").Build().GetSection("mail_template_file").Value;
            _sendgrid_apikey = new ConfigurationBuilder().AddJsonFile("appsettings.json").Build().GetSection("sendgrid_apikey").Value;
            _from_name = new ConfigurationBuilder().AddJsonFile("appsettings.json").Build().GetSection("from_name").Value;
            _from_email_address = new ConfigurationBuilder().AddJsonFile("appsettings.json").Build().GetSection("from_email_address").Value;
            _subject = new ConfigurationBuilder().AddJsonFile("appsettings.json").Build().GetSection("subject").Value;


            //check files...
            if (!File.Exists(sent_mail_address_list))
            {
                File.Create(sent_mail_address_list);
            }

            if (!File.Exists(template_file))
            {
                Console.WriteLine("Template File doesn't exist!");
                Console.WriteLine("Please create mail_template.html file in data folder.");
                Console.WriteLine("Program is closing.");
                Environment.Exit(0);
            }
            else
            {
                template_file = File.ReadAllText(template_file);
            }

            if (File.ReadLines(path_mail_address).Where(l => !string.IsNullOrWhiteSpace(l)).Count()==0)
            {
                Console.WriteLine("Mail address list empty!");
                Console.WriteLine("Please make sure to add e-mail address to the mail_adres_list.txt in data folder.");
                Console.WriteLine("Program is closing.");
                Environment.Exit(0);
            }
            //end check files...


            Console.WriteLine("Press 'y' if you would like to send message!");

            ConsoleKeyInfo cki = Console.ReadKey();

            if (cki.Key.ToString().ToLower() != "y")
            {
                Console.WriteLine("Program is closing.");
                Environment.Exit(0);
            }
            Console.WriteLine("");
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            try
            {
                while (!stoppingToken.IsCancellationRequested)
                {
                    //_logger.LogInformation("Worker running at: {time}", DateTimeOffset.Now);

                    if (File.ReadLines(path_mail_address).Where(l => !string.IsNullOrWhiteSpace(l)).Count() > 0) {

                        string mail = File.ReadLines(path_mail_address).Where(l => !string.IsNullOrWhiteSpace(l)).First();
                        if (!string.IsNullOrEmpty(mail.Trim()))
                        {
                            var client = new RestClient("https://api.sendgrid.com/v3/mail/send");

                            var request = new RestRequest();
                            request.Timeout = -1;
                            request.AddHeader("Authorization", "Bearer " + _sendgrid_apikey);
                            request.AddHeader("Content-Type", "application/json");
                            var body = @"{""personalizations"":" + "\n" +
                                        @"[{""to"": [" + "\n" +
                                        @"{ ""email"":""" + mail + "\" }" + "\n" +
                                        @"]" + "\n" +
                                        @"}]," + "\n" +
                                        @"""from"":" + "\n" +
                                        @"{""email"": """ + _from_email_address + "\"," + "\n" +
                                        @"""name"": """ + _from_name + "\"}," + "\n" +
                                            @"""subject"": """ + _subject + "\"," +
                                            @"""content"": [{""type"": ""text/html"",""value"": """ + template_file + "\"}" + "\n" +
                                            @"]}";

                            request.AddParameter("application/json", body, ParameterType.RequestBody);
                            var response = client.Post(request);
                            //_results.Add(response.StatusCode.ToString());

                            Console.WriteLine("Sent " + mail + ". Status code: " + response.StatusCode.ToString());

                            List<string> lines_sent = File.ReadAllLines(sent_mail_address_list).ToList();
                            lines_sent.Insert((lines_sent.Count - 1>=0)?lines_sent.Count - 1:0, mail);
                            File.WriteAllLines(sent_mail_address_list, lines_sent);

                            var lines_source = File.ReadAllLines(path_mail_address);
                            File.WriteAllLines(path_mail_address, lines_source.Skip(1).ToArray());
                        }
                    }
                    else
                    {
                        Environment.Exit(0);
                    }

                    await Task.Delay((_mailing_interval > 5) ? _mailing_interval * 1000 : 5000, stoppingToken);

                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }

        }

    }
}
