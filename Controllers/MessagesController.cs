/**
 * Copyright 2018, Camilo Varela
 * https://github.com/camilovarela
 * 
 * All rights reserved Date: 19/02/2018
 */
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using Microsoft.Bot.Connector;
using SimpleEchoBot.Constants;
using SimpleEchoBot.Services;

/**
 * This class exposes a rest endpoint in order to communicate with the chatbot.
 * The chatbot analyzes the input message and based on the type of that message,
 * send a specific response to the client.
 * 
 * SpringBotty uses CognitionServices from Azure in order to analyze the intent of
 * the message (LUIS) and a voice recognition (Speech service).
 **/
namespace Microsoft.Bot.Sample.SimpleEchoBot
{
    [BotAuthentication]
    public class MessagesController : ApiController
    {

        /// <summary>
        /// POST: api/Messages
        /// Receive a message from a user and reply to it
        /// </summary>
        public async Task<HttpResponseMessage> Post([FromBody]Activity activity)
        {
            if (activity.Type == ActivityTypes.Message)
            {
                var connector = new ConnectorClient(new Uri(activity.ServiceUrl));
                string message;

                try
                {
                    var audioAttachment = activity.Attachments?.FirstOrDefault(a => a.ContentType.Equals("audio/wav") || a.ContentType.Equals("application/octet-stream"));
                    if (audioAttachment != null)
                    {
                        var stream = await GetAudioStream(connector, audioAttachment);
                        var text = await MicrosoftCognitiveSpeechService.GetInstance().GetTextFromAudioAsync(stream);
                        message = ProcessText(text);
                    }
                    else
                    {
                        message = SpringBottyConstants.NOT_AUDIBLE_FILE;
                    }
                }
                catch (Exception e)
                {
                    message = SpringBottyConstants.AUDIBLE_FILE_ERROR;
                    if (e is HttpException)
                    {
                        var httpCode = (e as HttpException).GetHttpCode();
                        if (httpCode == 401 || httpCode == 403)
                        {
                            message += $" [{e.Message} - tip: Revisa el API KEY de tu proyecto]";
                        }
                        else if (httpCode == 408)
                        {
                            message += $" [{e.Message} - tip: Intentemos con un audio más corto]";
                        }
                    }
                    Trace.TraceError(e.ToString());
                }

                Activity reply = activity.CreateReply(message);
                await connector.Conversations.ReplyToActivityAsync(reply);
            }
            else
            {
                await HandleSystemMessageService.GetInstance().HandleSystemMessage(activity);
            }

            var response = this.Request.CreateResponse(HttpStatusCode.OK);
            return response;
        }

        private static string ProcessText(string text)
        {
            string message = "Voy a procesar el siguiente texto: " + text + ".";

            if (!string.IsNullOrEmpty(text))
            {
                
            }

            return message;
        }

        private static async Task<Stream> GetAudioStream(ConnectorClient connector, Attachment audioAttachment)
        {
            using (var httpClient = new HttpClient())
            {
                // The Skype attachment URLs are secured by JwtToken,
                // you should set the JwtToken of your bot as the authorization header for the GET request your bot initiates to fetch the image.
                // https://github.com/Microsoft/BotBuilder/issues/662
                var uri = new Uri(audioAttachment.ContentUrl);
                if (uri.Host.EndsWith("skype.com") && uri.Scheme == "https")
                {
                    httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", await GetTokenAsync(connector));
                    httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/octet-stream"));
                }

                return await httpClient.GetStreamAsync(uri);
            }
        }

        /// <summary>
        /// Gets the JwT token of the bot. 
        /// </summary>
        /// <param name="connector"></param>
        /// <returns>JwT token of the bot</returns>
        private static async Task<string> GetTokenAsync(ConnectorClient connector)
        {
            var credentials = connector.Credentials as MicrosoftAppCredentials;
            if (credentials != null)
            {
                return await credentials.GetTokenAsync();
            }

            return null;
        }
    }
}