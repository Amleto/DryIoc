using System;
using System.Linq;
using Castle.DynamicProxy;
using DryIoc.ImTools;

namespace DryIoc.IssuesTests.Interception
{
    // Extension methods for wrapping dependencies as forcible lazy using Castle Dynamic Proxy.
    public static class WrapAsLazy
    {
        /// <summary>
        /// Registers a service that is always resolved as lazy wrapper.
        /// </summary>
        /// <typeparam name="TInterface">The type of the interface.</typeparam>
        /// <typeparam name="TClass">The type of the class.</typeparam>
        /// <param name="registrator">The registrator.</param>
        /// <param name="serviceKey">Optional service key.</param>
        public static IRegistrator RegisterAsLazy<TInterface, TClass>(this IRegistrator registrator, object serviceKey = null)
            where TInterface : class
            where TClass : TInterface
        {
            // perform normal registration
            registrator.Register<TInterface, TClass>(serviceKey: serviceKey);

            // register the interface for lazy interception
            return registrator.ResolveAsLazy<TInterface>(serviceKey);
        }

        /// <summary>
        /// Ensures that a service always resolves as lazy proxy.
        /// </summary>
        /// <typeparam name="TInterface">The type of the interface.</typeparam>
        /// <param name="registrator">The registrator.</param>
        /// <param name="serviceKey">Optional service key.</param>
        public static IRegistrator ResolveAsLazy<TInterface>(this IRegistrator registrator, object serviceKey = null)
            where TInterface : class
        {
            return registrator.ResolveAsLazy(typeof(TInterface), serviceKey);
        }

        /// <summary>
        /// Ensures that a service always resolves as lazy proxy (uses DefaultProxyBuilder).
        /// </summary>
        /// <param name="registrator">The registrator.</param>
        /// <param name="interfaceType">The type of the interface.</param>
        /// <param name="serviceKey">Optional service key.</param>
        public static IRegistrator ResolveAsLazyViaProxyBuilder(this IRegistrator registrator, Type interfaceType, object serviceKey = null)
        {
            // registration of lazy interceptor
            registrator.Register(typeof(LazyInterceptor<>), setup: Setup.Wrapper, ifAlreadyRegistered: IfAlreadyRegistered.Keep);

            // lazy proxy wrapper
            var proxyBuilder = new DefaultProxyBuilder();
            var proxyType = proxyBuilder.CreateInterfaceProxyTypeWithTargetInterface(interfaceType,
                ArrayTools.Empty<Type>(), ProxyGenerationOptions.Default);

            // decorator for the generated proxy class
            var decoratorSetup = GetDecoratorSetup(serviceKey);

            // make typeof(LazyInterceptor<interfaceType>[])
            var lazyInterceptorArrayType = typeof(LazyInterceptor<>).MakeGenericType(interfaceType).MakeArrayType();

            // register the proxy class as decorator
            registrator.Register(interfaceType, proxyType,
                Reuse.Transient,
                setup: decoratorSetup,
                made: Made.Of(type => type.PublicConstructors().SingleOrDefault(ctor => ctor.GetParameters().Length != 0),
                    Parameters.Of
                        .Type(typeof(IInterceptor[]), lazyInterceptorArrayType)
                        .Type(interfaceType, r => null)));

            return registrator;
        }

        private static Setup GetDecoratorSetup(object serviceKey = null)
        {
            // Specify an order to apply decorator as a last one
            return serviceKey == null
                ? Setup.Decorator
                : Setup.DecoratorWith(r => serviceKey.Equals(r.ServiceKey));
        }

        /// <summary>
        /// Ensures that a service always resolves as lazy proxy (uses ProxyGenerator, a bit easier to understand).
        /// </summary>
        /// <param name="registrator">The registrator.</param>
        /// <param name="interfaceType">The type of the interface.</param>
        /// <param name="serviceKey">Optional service key.</param>
        public static IRegistrator ResolveAsLazy(this IRegistrator registrator, Type interfaceType, object serviceKey = null)
        {
            var method = typeof(WrapAsLazy).SingleMethod(nameof(CreateLazyProxy), includeNonPublic: true)
                .MakeGenericMethod(interfaceType);
            var decoratorSetup = GetDecoratorSetup(serviceKey);
            registrator.Register(interfaceType, Reuse.Transient, Made.Of(method), decoratorSetup);
            return registrator;
        }

        private static ProxyGenerator ProxyGenerator { get; } = new ProxyGenerator();

        private static T CreateLazyProxy<T>(Lazy<T> lazyValue) where T : class
        {
            var interceptor = new LazyInterceptor<T>(lazyValue);
            return ProxyGenerator.CreateInterfaceProxyWithTargetInterface<T>(null, interceptor);
        }

        private class LazyInterceptor<T> : IInterceptor
            where T : class
        {
            public LazyInterceptor(Lazy<T> lazyTarget)
            {
                LazyTarget = lazyTarget;
            }

            private Lazy<T> LazyTarget { get; }

            public void Intercept(IInvocation invocation)
            {
                var target = invocation.InvocationTarget as T;
                if (target == null)
                {
                    // create the lazy value on the first invocation
                    // ReSharper disable once SuspiciousTypeConversion.Global
                    var changeProxyTarget = (IChangeProxyTarget)invocation;
                    changeProxyTarget.ChangeInvocationTarget(LazyTarget.Value);
#pragma warning disable 618
                    changeProxyTarget.ChangeProxyTarget(LazyTarget.Value);
#pragma warning restore 618
                }

                invocation.Proceed();
            }
        }
    }
}
