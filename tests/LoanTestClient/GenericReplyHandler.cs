using System;
using System.Reflection;
using NServiceBus;
using NServiceBus.Logging;

namespace LoanTestClient
{
    public static class GenericReplyHandler 
    {
        public static void HandleMessage<T>(ILog logger, T message, IMessageHandlerContext context)
        {
            if (message == null)
            {
                throw new ArgumentNullException(nameof(message));
            }

            if (context is null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            string originatingEndpoint = string.Empty;
            if (context.MessageHeaders.ContainsKey("NServiceBus.OriginatingEndpoint"))
            {
                originatingEndpoint = context.MessageHeaders["NServiceBus.OriginatingEndpoint"];
            }
            logger.Info($"Received {message.GetType().Name} from '{originatingEndpoint}'");

            foreach (PropertyInfo propertyInfo in message.GetType().GetProperties())
            {
                logger.Info($"\t{propertyInfo.Name}:\t\t{propertyInfo.GetValue(message)}");
            }
        }
    }
}
