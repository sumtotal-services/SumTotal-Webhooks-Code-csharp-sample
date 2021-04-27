using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Net.Http;
using System.IO;
using System.Text;
using Microsoft.AspNetCore.Http;
using System.Diagnostics;
using System.Runtime.InteropServices;
using Newtonsoft.Json;
using System.Security.Cryptography;
using System.Diagnostics.Contracts;
using WebhookListener.Entities;
using Microsoft.Extensions.Options;

namespace WebhookListener.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ListenerController : ControllerBase
    {
        #region Private Members
        private readonly ILogger _Logger;
        private SignatureOptions _SignatureOptions = null;
        #endregion
        /// <summary>
        /// Creates and instance of values controller.
        /// </summary>
        /// <param name="logger"></param>
        public ListenerController(ILogger<ListenerController> logger, IOptionsMonitor<SignatureOptions> signatureOptions)
        {
            _Logger = logger;
            _SignatureOptions = signatureOptions.CurrentValue;
            signatureOptions.OnChange((newOpts) => { _SignatureOptions = newOpts; });
        }

        [HttpPost]
        [Route("listenevent")]
        public IActionResult ListenEvent()
        {
            try
            {
                _Logger.LogDebug("ListenEvent Post Method invoked");
                bool signatureValid = false;
                this.LogHeaders(this.Request);
                using (StreamReader reader = new StreamReader(this.Request.Body))
                {
                    string payload = reader.ReadToEndAsync().GetAwaiter().GetResult();
                    _Logger.LogDebug("payload is : " + payload);
                    var signature = this.Request.Headers["X-SUMT-Signature"];
                    string secretKey = _SignatureOptions.SecretKey;
                    // if you want to validate the payload signature, copy the secretKey from webhookEndpoint in UI and update the secretKey in appsettings.json
                    // we are forming payload signature again in below methods and comparing the generated payload signature value with payload signature in request.header
                    if (secretKey != null && secretKey != "")
                    {
                        // regenerating the payload signature and validating with payloadsignature in request.header
                        signatureValid = this.ValidateSignature(signature, payload, secretKey);
                        //signatureValid - true when both payload signatures are matched , will be false when they are not matched
                        if (signatureValid)
                        {
                            _Logger.LogDebug("validated the appsettings_secretkey with the payload signature and result is matched and appsettings_secretkey is : " + secretKey);
                            return Ok("Success and validated the appsettings_secretkey with the payload signature and result is matched and appsettings_secretkey is :" + secretKey);
                        }
                        else
                        {
                            _Logger.LogDebug("validated the appsettings_secretkey with the payload signature and result is NOT matched and appsettings_secretkey is : " + secretKey);
                            return Ok("Success and validated the appsettings_secretkey with the payload signature and result is NOT matched and appsettings_secretkey is : " + secretKey);
                        }
                    }
                    else
                    {
                        _Logger.LogDebug("not validated the secret key as appsettings_secretkey is empty");
                        return Ok("Success and not validated the secret key as appsettings_secretkey is empty");
                    }
                }
            }
            catch(Exception ex)
            {
                _Logger.LogError("Could not hit the listen event api in listenercontroller", ex);
                return BadRequest("Could not hit the listen event api");
            }
        }

        private void LogHeaders(HttpRequest request)
        {
            StringBuilder sBuilder = new StringBuilder();
            foreach (var kvp in request.Headers)
            {
                sBuilder.AppendLine($"{kvp.Key} : {kvp.Value}");
            }
            sBuilder.AppendLine("");
            _Logger.LogDebug(sBuilder.ToString());
        }

        public virtual bool ValidateSignature(string signature, string payLoad, string secretKey)
        {
            Contract.Requires(!string.IsNullOrEmpty(signature));
            Contract.Requires(!string.IsNullOrEmpty(payLoad));
            Contract.Requires(!string.IsNullOrEmpty(secretKey));

            //getting timestamp value from existing payload signature
            string[] signatureParts = signature.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
            string timeStamp = signatureParts[0].Split(new char[] { '=' }, StringSplitOptions.RemoveEmptyEntries)[1];

            //regenerating the payload signature using secretKey in appsettings.json, payload and timestamp
            string signatureFromPayload = GetPayloadSignature(payLoad, secretKey, timeStamp);
            //validating the generated payload signature with payload signature in request.header
            return string.Equals(signatureFromPayload, signature);
        }

        public virtual string GetPayloadSignature(string payLoad, string secretKey, string timestamp)
        {
            string payloadToSign = string.Join(".", timestamp, payLoad);
            var signature = "t={0},v1={1}";
            string signatureVal = "";
            using (var hmacAlgo = new HMACSHA1(Encoding.UTF8.GetBytes(secretKey)))
            {
                var hash = hmacAlgo.ComputeHash(Encoding.UTF8.GetBytes(payloadToSign));
                signatureVal = ToHexString(hash);
            }
            return string.Format(signature, timestamp, signatureVal);
        }

        private static string ToHexString(byte[] bytes)
        {
            var builder = new StringBuilder(bytes.Length * 2);
            foreach (byte b in bytes)
            {
                builder.AppendFormat("{0:x2}", b);
            }
            return builder.ToString();
        }
    }
}
