using MqttBrokerWithDashboard.Extensions;
using MqttBrokerWithDashboard.Models;
using MqttBrokerWithDashboard.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace MqttBrokerWithDashboard.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class StatusController : ControllerBase
    {
        private readonly ILogger<StatusController> logger;
        private readonly MqttBrokerService mqttBrokerService;

        public StatusController(ILogger<StatusController> logger, MqttBrokerService brokerService)
        {
            this.logger = logger;
            this.mqttBrokerService = brokerService;
        }

        // GET: api/<ServerStatusController>
        [HttpGet(nameof(GetConnectedClients))]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IEnumerable<MqttClientModel>))]
        public IEnumerable<MqttClientModel> GetConnectedClients()
        {
            return mqttBrokerService.ConnectedClients;
        }
    }

}
