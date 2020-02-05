using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Confluent.Kafka;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace Auth.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AccountController : ControllerBase
    {
        private readonly AppSettings _appSettings;

        public AccountController(IOptions<AppSettings> options)
        {
            _appSettings = options.Value;
        }

        [HttpGet]
        [Authorize(Roles = "Admin,Tester")]
        public IActionResult Get() => Ok(new[] { 1, 2, 4, 5, 6 });

        [HttpPost("/api/v1/messages")]
        public IActionResult SendMessage([FromServices] IOptions<Shared.KafakOptions> options)
        {
            var config = new ProducerConfig { BootstrapServers = options.Value.BootstrapServers };

            using (var producer = new ProducerBuilder<Null, string>(config).Build())
            {
                try
                {
                    var sendResult = producer
                                        .ProduceAsync("pedido", new Message<Null, string> { Value = $"messagem {Guid.NewGuid().ToString()}" })
                                            .GetAwaiter()
                                                .GetResult();


                }
                catch (ProduceException<Null, string> e)
                {
                    Console.WriteLine($"Delivery failed: {e.Error.Reason}");
                }
            }
            return Ok();
        }

        [HttpPost]
        public IActionResult Auth()
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(_appSettings.Secret);
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new Claim[]
                {
                    new Claim(ClaimTypes.Name, "Admin"),
                    new Claim(ClaimTypes.Role, "Tester"),
                    new Claim(ClaimTypes.Role, "Operator")
                }),
                Expires = DateTime.UtcNow.AddDays(7),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);

            return Ok(new { token = tokenHandler.WriteToken(token) });
        }        

    }
}