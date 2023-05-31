using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace EDI.InboundMsgProxy.Function.Model
{
    public class QueueMsgDto
    {
        [JsonProperty(Required=Required.Always)]
        public string Message { get; set; }

        [JsonProperty(Required = Required.Always)]
        public string TargetUrl { get; set; }
    }
}
