using System;
using System.Data.SqlClient;
using Dapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;

namespace NetCoreInMemoryOrRedis.Controllers
{
    public class CotacaoMemoyController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }

        [HttpGet]
        public ContentResult Get(
    [FromServices]IConfiguration config,
    [FromServices]IMemoryCache cache)
        {
            string valorJSON = cache.GetOrCreate<string>(
                "Cotacoes", context =>
                {
                    context.SetAbsoluteExpiration(TimeSpan.FromSeconds(30));
                    context.SetPriority(CacheItemPriority.High);

                    using (SqlConnection conexao = new SqlConnection(
                        config.GetConnectionString("BaseCotacoes")))
                    {
                        return conexao.QueryFirst<string>(
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
                });

            return Content(valorJSON, "application/json");
        }
    }
}