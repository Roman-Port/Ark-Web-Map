﻿using ArkWebMapGatewayClient.Messages.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace ArkWebMapGatewayClient.Messages
{
    public class MessageMapDrawingInput : GatewayMessageBase
    {
        public List<ArkTribeMapDrawingPoint> points { get; set; }
        public int mapId { get; set; }
    }
}