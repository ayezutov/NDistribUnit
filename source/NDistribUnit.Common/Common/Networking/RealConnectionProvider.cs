using System;
using System.Configuration;
using System.Diagnostics;
using System.Reflection;
using System.ServiceModel;
using System.ServiceModel.Configuration;
using System.ServiceModel.Description;
using NDistribUnit.Common.Common.Communication;

namespace NDistribUnit.Common.Common.Networking
{
    /// <summary>
    /// 
    /// </summary>
    public class RealConnectionProvider : IConnectionProvider
    {
        /// <summary>
        /// Gets the connection.
        /// </summary>
        /// <typeparam name="TServiceContract">The type of the service contract.</typeparam>
        /// <param name="address"></param>
        /// <returns></returns>
        public TServiceContract GetConnection<TServiceContract>(EndpointAddress address) where TServiceContract : class
        {
            var factory = new ChannelFactory<TServiceContract>(new NetTcpBinding("NDistribUnit.Default"));
            var behaviors = factory.Endpoint.Behaviors;

            Configuration config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
            var endpointBehaviors = ServiceModelSectionGroup.GetSectionGroup(config).Behaviors.EndpointBehaviors;

            MethodInfo createBehaviorMethodInfo = typeof(BehaviorExtensionElement).GetMethod("CreateBehavior",
BindingFlags.Instance | BindingFlags.NonPublic);


            if (endpointBehaviors.ContainsKey(string.Empty))
            {
                var defaultBehavior = endpointBehaviors[string.Empty];
                foreach (var behaviorExtensionElement in defaultBehavior)
                {
                    var behavior = createBehaviorMethodInfo.Invoke(behaviorExtensionElement, null) as IEndpointBehavior;

                    Debug.Assert(behaviorExtensionElement.BehaviorType != null);
                    if (behaviors.Contains(behaviorExtensionElement.BehaviorType))
                    {
                        behaviors.Remove(behaviorExtensionElement.BehaviorType);
                    }
                    behaviors.Add(behavior);
                }
            }
            return factory.CreateChannel(address);
        }
    }
}