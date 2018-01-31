using System;
using System.Net;
using UnityEngine;
using System.Net.Mail;
using System.Collections;
using System.Net.Security;
using System.Collections.Generic;
using System.Security.Cryptography.X509Certificates;

	public struct Email 
	{
		public string emailTo;
		public string emailFrom;
		public string subject;
		public string body;
	}

public static class ErrorManager
{
	public static Dictionary<string, string> errorMsg = new Dictionary<string, string>() 
	{ 
		{"Could not resolve host: silkke.net", "Connection problem. Please verify if you're connected to internet."}, 
	  	{"Client authentication failed", "Wrong login or password."},
        {"invalid_credentials", "Wrong login or password"},
	};

 	public static void sendMail(Email newMail)
         {
             MailMessage mail = new MailMessage();
 
             mail.From = new MailAddress(newMail.emailFrom);
             mail.To.Add(newMail.emailTo);
             mail.Subject = newMail.subject;
             mail.Body = newMail.body;
 
             SmtpClient smtpServer = new SmtpClient("smtp.gmail.com");
             smtpServer.Port = 587;
             smtpServer.Credentials = new System.Net.NetworkCredential("youraddress@gmail.com", "yourpassword") as ICredentialsByHost;
             smtpServer.EnableSsl = true;
             ServicePointManager.ServerCertificateValidationCallback = 
                 delegate(object s, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors) 
                     { return true; };
             smtpServer.Send(mail);
             Debug.Log("success");
         
         }
}
