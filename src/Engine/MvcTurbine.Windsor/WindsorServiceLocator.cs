﻿#region License

//
// Author: Javier Lozano <javier@lozanotek.com>
// Copyright (c) 2009-2010, lozanotek, inc.
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//   http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
//

#endregion

namespace MvcTurbine.Windsor {
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Castle.MicroKernel.Resolvers.SpecializedResolvers;
    using Castle.Windsor;
    using ComponentModel;

    /// <summary>
    /// Implemenation of <see cref="IServiceLocator"/> using <see cref="IWindsorContainer"/> as the default container.
    /// </summary>
    [Serializable]
    public class WindsorServiceLocator : IServiceLocator {
        private WindsorRegistrar registrationList;

        /// <summary>
        /// Default constructor.
        /// </summary>
        public WindsorServiceLocator() : this(CreateContainer()) {
        }

        /// <summary>
        /// Creates an <see cref="IWindsorContainer"/> instance with the right resolvers associated to it.
        /// </summary>
        /// <returns></returns>
        private static IWindsorContainer CreateContainer() {
            IWindsorContainer container = new WindsorContainer();
            var kernel = container.Kernel;
            kernel.Resolver.AddSubResolver(new ArrayResolver(kernel));
            kernel.Resolver.AddSubResolver(new ListResolver(kernel));

            return container;
        }

        /// <summary>
        /// Create an instance of the type an use the specified <see cref="IWindsorContainer"/>.
        /// </summary>
        public WindsorServiceLocator(IWindsorContainer container) {
            Container = container;
        }

        ///<summary>
        /// Gets or sets the current <see cref="IWindsorContainer"/> for the locator.
        ///</summary>
        public IWindsorContainer Container { get; private set; }

        /// <summary>
        /// See <see cref="IServiceLocator.Resolve{T}()<>"/>.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public virtual T Resolve<T>() where T : class {
            try {
                return Container.Resolve<T>();
            } catch (Exception ex) {
                throw new ServiceResolutionException(typeof(T), ex);
            }
        }

        /// <summary>
        /// See <see cref="IServiceLocator.Resolve{T}(string)<>"/>.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key"></param>
        /// <returns></returns>
        public T Resolve<T>(string key) where T : class {
            try {
                return Container.Resolve<T>(key);
            } catch (Exception ex) {
                throw new ServiceResolutionException(typeof(T), ex);
            }
        }

        /// <summary>
        /// See <see cref="IServiceLocator.Resolve{T}(System.Type)<>"/>.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="type"></param>
        /// <returns></returns>
        public T Resolve<T>(Type type) where T : class {
            try {
                return (T)Container.Resolve(type);
            } catch (Exception ex) {
                throw new ServiceResolutionException(typeof(T), ex);
            }
        }

        /// <summary>
        /// See <see cref="IServiceLocator.ResolveServices{T}<>"/>.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public IList<T> ResolveServices<T>() where T : class {
            var services = Container.Kernel.ResolveAll<T>();
            return new List<T>(services);
        }

        /// <summary>
        /// See <see cref="IServiceLocator.Release"/>.
        /// </summary>
        /// <param name="instance"></param>
        public void Release(object instance) {
            Container.Release(instance);
        }

        /// <summary>
        /// See <see cref="IServiceLocator.Reset"/>.
        /// </summary>
        public void Reset() {
            if (Container == null) return;

            Container.Dispose();
            Container = null;
            registrationList = null;
        }

        public TService Inject<TService>(TService instance) where TService : class {
            if (instance == null) return null;

            Type instanceType = instance.GetType();
            instanceType.GetProperties()
                .Where(property => property.CanWrite && Container.Kernel.HasComponent(property.PropertyType))
                .ForEach(property => property.SetValue(instance, Container.Resolve(property.PropertyType), null));

            return instance;
        }

        public void TearDown<TService>(TService instance) where TService : class {
            if (instance == null) return;

            Type instanceType = instance.GetType();

            instanceType.GetProperties()
                .Where(property => Container.Kernel.HasComponent(property.PropertyType))
                .ForEach(property => Container.Release(property.GetValue(instance, null)));
        }

        /// <summary>
        /// Disposes (resets) the current service locator.
        /// </summary>
        public void Dispose() {
            Reset();
        }
    }
}
