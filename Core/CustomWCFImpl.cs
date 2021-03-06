﻿//Extension for WCF allowing dependent services.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using AutoDL.Services;
using AutoDL.Data;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.ServiceModel.Dispatcher;
using System.ServiceModel.Description;

namespace AutoDL.WCF
{
    /// <summary>
    /// Service host that accepts dependent service class.
    /// </summary>
    /// <typeparam name="TDep">Dependent object type.</typeparam>
    internal class DependentServiceHost<TDep> : ServiceHost
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="serviceType">Service class type.</param>
        /// <param name="dependency">Dependency object.</param>
        public DependentServiceHost(Type serviceType, TDep dependency, params Uri[] uris)
            : base(serviceType, uris)
        {
            if (dependency == null)
            {
                throw new ArgumentNullException("dependency");
            }
            this.Description.Behaviors.Add(new DependencyInstanceProvider<TDep>(serviceType, dependency));
        }
    }

    /// <summary>
    /// Instance provider for <see cref="DependentServiceHost{TDep}"/>.
    /// </summary>
    /// <typeparam name="TDep">Dependent object type.</typeparam>
    internal class DependencyInstanceProvider<TDep> : IInstanceProvider, IServiceBehavior
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="serviceType">Service class type.</param>
        /// <param name="dependency">Dependency object.</param>
        public DependencyInstanceProvider(Type serviceType, TDep dependency)
        {
            _dependency = dependency;
            _serviceType = serviceType;         
        }

        //Members
        private readonly TDep _dependency;
        private readonly Type _serviceType;

        //IInstanceProvider implementation
        public object GetInstance(InstanceContext instanceContext, Message message)
        {
            return Activator.CreateInstance(_serviceType, _dependency);
        }
        public object GetInstance(InstanceContext instanceContext)
        {
            return GetInstance(instanceContext, null);
        }
        public void ReleaseInstance(InstanceContext instanceContext, object instance) 
        {
            IDisposable disposable = instance as IDisposable;
            if (disposable != null)
            {
                disposable.Dispose();
            }
        }

        //IServiceBehavior implementation
        public void AddBindingParameters(ServiceDescription serviceDescription, ServiceHostBase serviceHostBase, 
            System.Collections.ObjectModel.Collection<ServiceEndpoint> endpoints, BindingParameterCollection bindingParameters) { }
        public void ApplyDispatchBehavior(ServiceDescription serviceDescription, ServiceHostBase serviceHostBase)
        {
            foreach (ChannelDispatcher cd in serviceHostBase.ChannelDispatchers)
            {
                foreach(EndpointDispatcher ed in cd.Endpoints)
                {
                    if (!ed.IsSystemEndpoint)
                    {
                        ed.DispatchRuntime.InstanceProvider = this;
                    }
                }
            }
        }
        public void Validate(ServiceDescription serviceDescription, ServiceHostBase serviceHostBase) { }
    }
}
