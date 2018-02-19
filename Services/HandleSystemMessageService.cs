/**
 * Copyright 2018, Camilo Varela
 * https://github.com/camilovarela
 * 
 * All rights reserved Date: 19/02/2018
 */
using Microsoft.Bot.Connector;
using SimpleEchoBot.Constants;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace SimpleEchoBot.Services
{
    class HandleSystemMessageService
    {
        private static HandleSystemMessageService instance;

        public HandleSystemMessageService()
        {

        }

        public static HandleSystemMessageService GetInstance()
        {
            if (instance == null)
            {
                instance = new HandleSystemMessageService();
            }
            return instance;
        }

        /// <summary>
        /// Handles the system activity.
        /// </summary>
        /// <param name="activity">The activity.</param>
        /// <returns>Activity</returns>
        public async Task<Activity> HandleSystemMessage(Activity activity)
        {
            switch (activity.Type)
            {
                case ActivityTypes.DeleteUserData:
                    // Implement user deletion here
                    // If we handle user deletion, return a real message
                    break;
                case ActivityTypes.ConversationUpdate:
                    // Greet the user the first time the bot is added to a conversation.
                    if (activity.MembersAdded.Any(m => m.Id == activity.Recipient.Id))
                    {
                        var connector = new ConnectorClient(new Uri(activity.ServiceUrl));

                        var response = activity.CreateReply();
                        response.Text = SpringBottyConstants.SPRING_BOTTY_GREETINGS;

                        await connector.Conversations.ReplyToActivityAsync(response);
                    }

                    break;
                case ActivityTypes.ContactRelationUpdate:
                    // Handle add/remove from contact lists
                    break;
                case ActivityTypes.Typing:
                    // Handle knowing that the user is typing
                    break;
                case ActivityTypes.Ping:
                    break;
            }

            return null;
        }
    }
}
