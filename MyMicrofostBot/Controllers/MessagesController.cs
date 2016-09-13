using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Description;
using Microsoft.Bot.Connector;
using Newtonsoft.Json;
using System.Text.RegularExpressions;
using System.Text;

namespace MyMicrofostBot
{
    [BotAuthentication]
    public class MessagesController : ApiController
    {
        private static Random randomizer = new Random();
        
        /// <summary>
        /// POST: api/Messages
        /// Receive a message from a user and reply to it
        /// </summary>
        public async Task<HttpResponseMessage> Post([FromBody]Activity activity)
        {
            if (activity.Type == ActivityTypes.Message)
            {
                ConnectorClient connector = new ConnectorClient(new Uri(activity.ServiceUrl));

                // XXX 心情的にはクラス化したい
                var diceReg = new Regex("([1-9][0-9]{0,2})d([1-9][0-9]{0,8})", RegexOptions.IgnoreCase);
                var diceTest = diceReg.Match(activity.Text);
                // XXX どう考えても正しいメッセージの振り分けではない
                // XXX 複数ヒットを処理する
                if (diceTest.Success)
                {
                    var n = Convert.ToInt32(diceTest.Groups[1].Value);
                    var d = Convert.ToInt32(diceTest.Groups[2].Value);
                    var rollResults = Enumerable.Range(0, n).Select(x => randomizer.Next(d) + 1).ToList();
                    var sb = new StringBuilder();
                    sb.Append($"{n}d{d} = {rollResults.Sum()} [{string.Join(", ", rollResults)}]");

                    Activity reply = activity.CreateReply(sb.ToString());
                    await connector.Conversations.ReplyToActivityAsync(reply);
                }
                else
                {
                    // calculate something for us to return
                    int length = (activity.Text ?? string.Empty).Length;

                    // return our reply to the user
                    Activity reply = activity.CreateReply($"You sent {activity.Text} which was {length} characters");
                    await connector.Conversations.ReplyToActivityAsync(reply);
                }
            }
            else
            {
                HandleSystemMessage(activity);
            }
            var response = Request.CreateResponse(HttpStatusCode.OK);
            return response;
        }

        private Activity HandleSystemMessage(Activity message)
        {
            if (message.Type == ActivityTypes.DeleteUserData)
            {
                // Implement user deletion here
                // If we handle user deletion, return a real message
            }
            else if (message.Type == ActivityTypes.ConversationUpdate)
            {
                // Handle conversation state changes, like members being added and removed
                // Use Activity.MembersAdded and Activity.MembersRemoved and Activity.Action for info
                // Not available in all channels
            }
            else if (message.Type == ActivityTypes.ContactRelationUpdate)
            {
                // Handle add/remove from contact lists
                // Activity.From + Activity.Action represent what happened
            }
            else if (message.Type == ActivityTypes.Typing)
            {
                // Handle knowing tha the user is typing
            }
            else if (message.Type == ActivityTypes.Ping)
            {
            }

            return null;
        }
    }
}