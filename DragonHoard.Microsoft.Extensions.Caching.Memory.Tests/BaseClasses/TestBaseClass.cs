﻿using DragonHoard.Core;
using Mecha.Core;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Threading.Tasks;
using Xunit;

namespace TestHelpers
{
    /// <summary>
    /// Test base class
    /// </summary>
    /// <typeparam name="TTestObject">The type of the test object.</typeparam>
    public abstract class TestBaseClass<TTestObject> : TestBaseClass
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="TestBaseClass{TTestObject}"/> class.
        /// </summary>
        protected TestBaseClass()
        {
            ObjectType = typeof(TTestObject);
        }

        /// <summary>
        /// Gets the type of the object.
        /// </summary>
        /// <value>The type of the object.</value>
        protected override Type ObjectType { get; }

        /// <summary>
        /// Gets or sets the test object.
        /// </summary>
        /// <value>The test object.</value>
        protected TTestObject TestObject { get; set; }

        /// <summary>
        /// Attempts to break the object.
        /// </summary>
        /// <returns>The async task.</returns>
        [Fact]
        public Task BreakObject() => Mech.BreakAsync(TestObject, new Options { MaxDuration = 100 });
    }

    /// <summary>
    /// Test base class
    /// </summary>
    [Collection("DragonHoardCollection")]
    public abstract class TestBaseClass
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="TestBaseClass{TTestObject}"/> class.
        /// </summary>
        protected TestBaseClass()
        {
            _ = Mech.Default;
        }

        /// <summary>
        /// Gets the cache.
        /// </summary>
        /// <value>The cache.</value>
        protected static Cache Cache => GetServiceProvider().GetService<Cache>();

        /// <summary>
        /// Gets the type of the object.
        /// </summary>
        /// <value>The type of the object.</value>
        protected abstract Type ObjectType { get; }

        /// <summary>
        /// The service provider lock
        /// </summary>
        private static readonly object ServiceProviderLock = new();

        /// <summary>
        /// The service provider
        /// </summary>
        private static IServiceProvider ServiceProvider;

        /// <summary>
        /// Attempts to break the object.
        /// </summary>
        /// <returns>The async task.</returns>
        [Fact]
        public Task BreakType() => Mech.BreakAsync(ObjectType, new Options { MaxDuration = 100 });

        /// <summary>
        /// Gets the service provider.
        /// </summary>
        /// <returns></returns>
        protected static IServiceProvider GetServiceProvider()
        {
            if (ServiceProvider is not null)
                return ServiceProvider;
            lock (ServiceProviderLock)
            {
                if (ServiceProvider is not null)
                    return ServiceProvider;
                ServiceProvider = new ServiceCollection().AddOptions().AddCanisterModules()?.BuildServiceProvider();
            }
            return ServiceProvider;
        }
    }
}