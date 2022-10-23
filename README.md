# Asp.Net Sendgrid Bulk Mailing  :+1:

**First step : Set your information in appsettings.json**

>**Properties**
 - Html Mail Template Support
 - Sent mail log.
 - Get mail address from txt file.
 <br>


>**Create api key in your Sendgrid account** "https://app.sendgrid.com/settings/api_keys."

1. **Fill appsettings.json**
```json
{
  "from_name": "Google",
  "from_email_address": "mail@google.com",
  "subject": "Reminder Mail",
  "mail_template_file": "mail_template.html",
  "sendgrid_apikey": "XXXXXXX",
  "save_sent_mail": true, // true or false...
  "mailing_interval": 5, //min 5 second...
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.Hosting.Lifetime": "Information"
    }
  }
}
```
 
 If you have any question please write me. resitgalip@gmail.com
