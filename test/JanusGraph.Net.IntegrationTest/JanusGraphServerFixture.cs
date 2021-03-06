﻿#region License

/*
 * Copyright 2018 JanusGraph.Net Authors
 *
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 *
 *      http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */

#endregion

using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using TestContainers.Container.Abstractions.Hosting;
using TestContainers.Container.Abstractions.Models;
using Xunit;

namespace JanusGraph.Net.IntegrationTest
{
    public class JanusGraphServerFixture : IAsyncLifetime
    {
        private readonly GremlinServerContainer _container;

        public JanusGraphServerFixture()
        {
            var config = new ConfigurationBuilder().AddJsonFile("appsettings.json").Build();
            var dockerImage = config["dockerImage"];
            _container = new ContainerBuilder<GremlinServerContainer>()
                .ConfigureDockerImageName(dockerImage)
                .ConfigureLogging(builder =>
                {
                    builder.AddConsole();
                    builder.SetMinimumLevel(LogLevel.Debug);
                })
                .ConfigureContainer((context, container) =>
                {
                    container.ExposedPorts.Add(GremlinServerContainer.GremlinServerPort);

                    container.BindMounts.Add(new Bind
                    {
                        HostPath = $"{AppContext.BaseDirectory}/load_data.groovy",
                        ContainerPath = "/docker-entrypoint-initdb.d/load_data.groovy",
                        AccessMode = AccessMode.ReadOnly
                    });
                })
                .Build();
            _container.ServerStartedCheckTraversal = "g.V().has('name', 'hercules').hasNext()";
        }

        public string Host => _container.Host;
        public int Port => _container.Port;

        public Task InitializeAsync()
        {
            return _container.StartAsync();
        }

        public Task DisposeAsync()
        {
            return _container.StopAsync();
        }
    }
}