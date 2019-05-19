using System;
using System.Data;
using System.Numerics;
using System.Threading;
using System.Threading.Tasks;
using Nethereum.ABI.FunctionEncoding.Attributes;
using Nethereum.Contracts;
using Nethereum.Util;
using Nethereum.Web3;
using Nethereum.Web3.Accounts;


namespace SendERC1155
{

    public static class Globals
    {
        public static string TransactionHash;
    }


    class MainClass
    {


        public static void Main()
        {
			//check the balance of a wallet address
            BalanceAsync().Wait();
           
			//enjin cryptoitem ID
            var CID = "0x78...";
			//amount of tokens to send
			var ethtoken_amt = 1;
			//address to send the tokens to
			var ethereum_addr = "0xB...";
			
            TransferAsync(CID, ethereum_addr, ethtoken_amt).Wait();
			System.Environment.Exit(1);

        }


        public static async Task BalanceAsync()
        {
			
			//hardcoded example to check the balance of a particular w
            //Replace with any address you want to check the balance of
            var senderAddress = "0xB...";
			//kovan contractaddress
            var contractAddress = "0x8819a653b6c257b1e3356d902ecb1961d6471dd8";
			//replace with your infura.io acct
            var url = "https://kovan.infura.io/v3/...";
			//Enjin token ID, full 
            var CID = "0x7...";

 
            //no private key we are not signing anything (read only mode)
            var web3 = new Web3(url);

            var balanceOfFunctionMessage = new BalanceOfFunction()
            {
                CryptoID = CID,
                Owner = senderAddress,
            };

            var balanceHandler = web3.Eth.GetContractQueryHandler<BalanceOfFunction>();
            var balance = await balanceHandler.QueryAsync<BigInteger>(contractAddress, balanceOfFunctionMessage);
            Console.WriteLine("Balance of token:  " + balance);

        }

        [Function("balanceOf", "uint256")]
        public class BalanceOfFunction : FunctionMessage
        {

            [Parameter("uint256", "_id", 1)]
            public string CryptoID { get; set; }

            [Parameter("address", "_owner", 2)]
            public string Owner { get; set; }

        }

    

        public static async Task<string> TransferAsync(string CryptoItemID, string ethereum_addr, int ethtoken_amt)
        {

            //Replace with your own variable values            
			//wallet where the item is originating from
            var senderAddress = "...";
			//private key to sign the transaction
            var privatekey = "...";
			//add your own infura.io account info here
            var url = "https://kovan.infura.io/v3/...";
			//kovan contract addr
            var ContractAddress = "0x8819a653b6c257b1e3356d902ecb1961d6471dd8";
   
            var web3 = new Web3(new Account(privatekey), url);

            var transactionMessage = new TransferFunction()
            {
                FromAddress = senderAddress,
                To = ethereum_addr,
                CID = CryptoItemID,
                TokenAmount = ethtoken_amt,
                //Set our own price
                GasPrice = Web3.Convert.ToWei(8, UnitConversion.EthUnit.Gwei)

            };

            var transferHandler = web3.Eth.GetContractTransactionHandler<TransferFunction>();


            try
            {
                Globals.TransactionHash = await transferHandler.SendRequestAsync(ContractAddress, transactionMessage);
            }
            catch (Exception e)
            {
                Console.WriteLine("An error occurred: '{0}'", e);
            }

            var receipt = await web3.Eth.Transactions.GetTransactionReceipt.SendRequestAsync(Globals.TransactionHash);

            while (receipt == null)
            {
                Console.WriteLine("Waiting for transaction "+ Globals.TransactionHash + " to be added to the blockchain");
                System.Threading.Thread.Sleep(30000);

                receipt = await web3.Eth.Transactions.GetTransactionReceipt.SendRequestAsync(Globals.TransactionHash);
            }

            Console.WriteLine("Added to block: "+receipt.BlockNumber.Value.ToString());


            return Globals.TransactionHash;
        }


        [Function("transfer", "bool")]
        public class TransferFunction : FunctionMessage
        {
            [Parameter("address", "_to", 1)]
            public string To { get; set; }

            [Parameter("uint256", "_id", 2)]
            public string CID { get; set; }

            [Parameter("uint256", "_value", 3)]
            public BigInteger TokenAmount { get; set; }
        }




    }

}
