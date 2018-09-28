using EOSNewYork.EOSCore;
using EOSNewYork.EOSCore.ActionArgs;
using EOSNewYork.EOSCore.Response;
using EOSNewYork.EOSCore.Response.API;
using EOSNewYork.EOSCore.Utilities;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using NLog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

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

        [HttpGet("GetInfo")]
        public ActionResult<Info> GetInfo()
        {
            var info = chainAPI.GetInfo();
            logger.Info("{0} is currently the head block producer", info.head_block_producer);
            return info;
        }

        [HttpGet("GetAccountBalance")]
        public ActionResult<CurrencyBalance> GetAccountBalance(string account)
        {
            var currencyBalance = chainAPI.GetCurrencyBalance(account, "eosio.token", "EOS");
            logger.Info("The account had {0} balance records. The 1st (and probably the only balance) is {1}", currencyBalance.balances.Count, currencyBalance.balances.First());
            return currencyBalance;
        }

        [HttpGet("GetNewKeyPair")]
        public ActionResult<KeyPair> GetNewKeyPair()
        {
            var keypair = KeyManager.GenerateKeyPair();
            logger.Info("New keypair generated. Private key: {0}, Public key: {1}", keypair.PrivateKey, keypair.PublicKey);
            //logger.Info(keypair.PrivateKey.Length);
            //logger.Info(keypair.PublicKey.Length);
            return keypair;
        }

        [HttpPost, Route("Transfer")]
        public ActionResult<PushTransaction> Transfer(TranferBean bean)
        {
            string fromAccountName = bean.FromAccount;
            string toAccountName = bean.ToAccount;
            string value = bean.Quantity;
            string privateKey = bean.PrivateKey;
            string memo = bean.Memo;

            string _accountName = fromAccountName, _accountNameTo = toAccountName, _permissionName = "active", _code = "eosio.token", _action = "transfer", _memo = memo;
            //prepare arguments to be passed to action
            TransferArgs _args = new TransferArgs() { from = _accountName, to = _accountNameTo, quantity = value, memo = _memo };
            //BuyRamArgs _args = new BuyRamArgs(){ payer = _accountName, receiver = _accountName, quant = "0.001 EOS" };

            //prepare action object
            EOSNewYork.EOSCore.Params.Action action = new ActionUtility(host).GetActionObject(_action, _accountName, _permissionName, _code, _args);

            List<string> privateKeysInWIF = new List<string> { privateKey };

            //push transaction
            var transactionResult = chainAPI.PushTransaction(new[] { action }, privateKeysInWIF);
            logger.Info(transactionResult.transaction_id);
            return transactionResult;
        }


        [HttpPost, Route("NewAccount")]
        public ActionResult<PushTransaction> NewAccount(TranferBean bean)
        {
            string fromAccountName = bean.FromAccount;
            string toAccountName = bean.ToAccount;
            string value = bean.Quantity;
            string privateKey = bean.PrivateKey;
            string memo = bean.Memo;

            string _accountName = fromAccountName, _accountNameTo = toAccountName, _permissionName = "active", _code = "eosio.token", _action = "transfer", _memo = memo;
            //prepare arguments to be passed to action
            TransferArgs _args = new TransferArgs() { from = _accountName, to = _accountNameTo, quantity = value, memo = _memo };
            //BuyRamArgs _args = new BuyRamArgs(){ payer = _accountName, receiver = _accountName, quant = "0.001 EOS" };

            //prepare action object
            EOSNewYork.EOSCore.Params.Action action = new ActionUtility(host).GetActionObject(_action, _accountName, _permissionName, _code, _args);

            List<string> privateKeysInWIF = new List<string> { privateKey };

            //push transaction
            var transactionResult = chainAPI.PushTransaction(new[] { action }, privateKeysInWIF);
            logger.Info(transactionResult.transaction_id);
            return transactionResult;
        }


        [HttpGet("GetActions")]
        public ActionResult<bool> GetActions(string account)
        {
            string accountName = account;
            var actions = historyAPI.GetActions(-1, -10, accountName);
            logger.Info("For account {0} recieved actions {1}", accountName, JsonConvert.SerializeObject(actions));

            // List all actions
            foreach (var action in actions.actions)
            {
                string singleLineData = Regex.Replace(action.action_trace.act.data.ToString(), @"\t|\n|\r", "");
                logger.Info(string.Format("# {0}\t{1}\t{2} => {3}\t{4}", action.account_action_seq, action.block_time_datetime, action.action_trace.act.account + "::" + action.action_trace.act.name, action.action_trace.receipt.receiver, singleLineData));
            }

            logger.Info("----------");

            //List specific actions
            var res = actions.actions.Where(pr => pr.action_trace.act.name == "transfer");
            foreach (var action in res)
            {
                string singleLineData = Regex.Replace(action.action_trace.act.data.ToString(), @"\t|\n|\r", "");

                if (action.action_trace.act.data.from == "eosio.vpay" || action.action_trace.act.data.from == "eosio.bpay")
                    logger.Info(string.Format("# {0}\t{1}\t{2} => {3}\t{4}", action.account_action_seq, action.block_time_datetime, action.action_trace.act.account + "::" + action.action_trace.act.name, action.action_trace.receipt.receiver, singleLineData));
            }
            return true;
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

    public class TranferBean
    {
        public string FromAccount { get; set; }
        public string ToAccount { get; set; }
        public string Quantity { get; set; }
        public string PrivateKey { get; set; }
        public string Memo { get; set; }
    }

}
