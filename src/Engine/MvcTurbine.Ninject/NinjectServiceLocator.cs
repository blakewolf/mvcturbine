#region License

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

namespace MvcTurbine.Ninject {
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using ComponentModel;
    using global::Ninject;

    /// <summary>
    /// Default implementation of the <seealso cref="IServiceLocator"/> contract with Ninject IoC.
    /// </summary>
    /// <remarks>
    /// To learn more about Ninject, please visit its website: http://ninject.org
    /// </remarks>
    [Serializable]
    public class NinjectServiceLocator : IServiceLocator {
        private NinjectRegistrar currentModule;

        /// <summary>
        /// Default constructor. Locator is instantiated with a new <seealso cref="StandardKernel"/> instance.
        /// </summary>
        public NinjectServiceLocator()
            : this(new StandardKernel()) {
        }

        /// <summary>
        /// Creates an instance of the locator with the specified <seealso cref="IKernel"/>.
        /// </summary>
        /// <param name="kernel">Pre-defined <see cref="IKernel"/> to use within the container.</param>
        public NinjectServiceLocator(IKernel kernel) {
            if (kernel == null) {
                throw new ArgumentNullException("kernel", "The specified Ninject IKernel cannot be null.");
            }

            Container = kernel;
        }

        /// <summary>
        /// Gets the current <see cref="IKernel"/> associated with this instance.
        /// </summary>
        public IKernel Container { get; private set; }

        /// <summary>
        /// Resolves the service of the specified type.
        /// </summary>
        /// <typeparam name="T">Type of service to resolve.</typeparam>
        /// <returns>An instance of the type, null otherwise.</returns>
        public T Resolve<T>() where T : class {
            try {
                return Container.Get<T>();
            } catch (Exception ex) {
                throw new ServiceResolutionException(typeof(T), ex);
            }
        }

        /// <summary>
        /// Resolves the service of the specified type by the given string key.
        /// </summary>
        /// <typeparam name="T">Type of service to resolve.</typeparam>
        /// <param name="key">Unique key to distinguish the service.</param>
        /// <returns>An instance of the type, null otherwise.</returns>
        public T Resolve<T>(string key) where T : class {
            var value = Container.Get<T>(key);

            if (value == null) {
                throw new ServiceResolutionException(typeof(T));
            }

            return value;
        }

        /// <summary>
        /// Resolves the service of the specified type by the given type key.
        /// </summary>
        /// <typeparam name="T">Type of service to resolve.</typeparam>
        /// <param name="type">Key type of the service.</param>
        /// <returns>An instance of the type, null otherwise.</returns>
        public T Resolve<T>(Type type) where T : class {
            try {
                return Container.Get(type) as T;
            } catch (Exception ex) {
                throw new ServiceResolutionException(type, ex);
            }
        }

        /// <summary>
        /// Resolves the list of services of type <see cref="T"/> that are registered 
        /// within the locator.
        /// </summary>
        /// <typeparam name="T">Type of the service to resolve.</typeparam>
        /// <returns>A list of service of type <see cref="T"/>, null otherwise.</returns>
        public IList<T> ResolveServices<T>() where T : class {
            return Container.GetAll<T>().ToList();
        }

        /// <summary>
        /// Releases (disposes) the service instance from within the locator.
        /// </summary>
        /// <param name="instance">Instance of a service to dipose from the locator.</param>
        [Obsolete("Not used with this implementation of IServiceLocator.")]
        public void Release(object instance) {
        }

        /// <summary>
        /// Resets the locator to its initial state clearing all registrations.
        /// </summary>
        public void Reset() {
            if (Container == null)
                return;

            Container.Dispose();
            Container = null;
            currentModule = null;
        }

        public TService Inject<TService>(TService instance) where TService : class {
            Container.Inject(instance);
            return instance;
        }

        [Obsolete("Not used with this implementation of IServiceLocator.")]
        public void TearDown<TService>(TService instance) where TService : class {
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        /// <filterpriority>2</filterpriority>
        public void Dispose() {
            Reset();
        }
    }
}
