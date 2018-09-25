using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using EOSNewYork.EOSCore;
using EOSNewYork.EOSCore.Response.API;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using NLog;

namespace EOSWebApiDemo.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [EnableCors("any")] //设置跨域处理的 代理
    public class EOSIOController : ControllerBase
    {

        static Logger logger = NLog.LogManager.GetCurrentClassLogger();
        //static string host = "http://dev.cryptolions.io:18888";
        static string host = "http://api.eosnewyork.io";
        static TableAPI tableAPI = new TableAPI(host);
        static ChainAPI chainAPI = new ChainAPI(host);
        static HistoryAPI historyAPI = new HistoryAPI(host);
        static string privateKeyWIF = "";

        public EOSIOController() {

        }
        // GET api/values
        [HttpGet("GetInfo")]
        public ActionResult<Info> GetInfo()
        {
            var info = chainAPI.GetInfo();
            logger.Info("{0} is currently the head block producer", info.head_block_producer);
            return info;
        }

        // GET api/values/5
        [HttpGet("{id}")]
        public ActionResult<string> Get(int id)
        {
            return "value";
        }

        // POST api/values
        [HttpPost]
        public void Post([FromBody] string value)
        {
        }

        // PUT api/values/5
        [HttpPut("{id}")]
        public void Put(int id, [FromBody] string value)
        {
        }

        // DELETE api/values/5
        [HttpDelete("{id}")]
        public void Delete(int id)
        {
        }
    }
}
