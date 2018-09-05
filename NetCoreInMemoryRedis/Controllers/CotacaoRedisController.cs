using System;
using System.Data.SqlClient;
using Dapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Configuration;

namespace NetCoreInMemoryRedis.Controllers
{
    public class CotacaoRedisController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }

        [HttpGet]
        public ContentResult Get(
           [FromServices]IConfiguration config,
           [FromServices]IDistributedCache cache)
        {
            string valorJSON = cache.GetString("Cotacoes");
            if (valorJSON == null)
            {
                using (SqlConnection conexao = new SqlConnection(
                    config.GetConnectionString("BaseCotacoes")))
                {
                    valorJSON = conexao.QueryFirst<string>(
                        "SELECT Sigla " +
                              ",NomeMoeda " +
                              ",UltimaCotacao " +
                              ",GETDATE() AS DataProcessamento " +
                              ",ValorComercial AS 'Cotacoes.Comercial' " +
                              ",ValorTurismo AS 'Cotacoes.Turismo' " +
                        "FROM dbo.Cotacoes " +
                        "ORDER BY NomeMoeda " +
                        "FOR JSON PATH, ROOT('Moedas')");
                }

                DistributedCacheEntryOptions opcoesCache =
                    new DistributedCacheEntryOptions();
                opcoesCache.SetAbsoluteExpiration(
                    TimeSpan.FromMinutes(2));

                cache.SetString("Cotacoes", valorJSON, opcoesCache);
            }

            return Content(valorJSON, "application/json");
        }
    }
}