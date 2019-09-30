﻿using System;
using System.Collections.Generic;
using System.Text;

namespace ArkWebMapGatewayClient
{
    class GatewayConfigFile
    {
        public string gateway_proto;
        public string gateway_host;

        public int gateway_target_version;
        public int gateway_min_version;

        public GatewayConfigFileEndpoints gateway_endpoints;

        public int reconnect_delay_seconds;
        public int buffer_size;
    }

    class GatewayConfigFileEndpoints
    {
        public string sender;
        public string frontend;
        public string subserver;
    }
}
