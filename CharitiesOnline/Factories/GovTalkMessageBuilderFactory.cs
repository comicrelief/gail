using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using CharitiesOnline.MessageBuilders;

namespace CharitiesOnline.Factories
{
    public class GovTalkMessageBuilderFactory : IGovTalkMessageBuilderFactory
    {
        Dictionary<string, Type> messageBuilders;

        public GovTalkMessageBuilderFactory()
        {
            throw new NotImplementedException();
        }

        public GovTalkMessageBuilderBase CreateInstance(string description)
        {
            Type t = GetTypeToCreate(description);

            if (t == null)
                return new UnknownMessageBuilder();

            return Activator.CreateInstance(t) as GovTalkMessageBuilderBase;
        }

        private Type GetTypeToCreate(string messageBuilderName)
        {
            foreach (var messageBuilder in messageBuilders)
            {
                if (messageBuilder.Key.Contains(messageBuilderName))
                {
                    return messageBuilders[messageBuilder.Key];
                }
            }

            return null;
        }

        private void LoadTypesICanReturn()
        {
            messageBuilders = new Dictionary<string, Type>();

            Type[] typesInThisAssembly = System.Reflection.Assembly.GetExecutingAssembly().GetTypes();

            foreach (Type type in typesInThisAssembly)
            {
                if (type.GetInterface(typeof(GovTalkMessageBuilderBase).ToString()) != null)
                {
                    messageBuilders.Add(type.Name.ToLower(), type);
                }
            }
        }
    }
}
