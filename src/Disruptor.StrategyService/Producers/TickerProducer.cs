﻿using Disruptor.StrategyService.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Disruptor.StrategyService.Producers;

internal class TickerProducer : IValueProducer<PairCandle>
{
    // TODO: Read a file with past data and send it to the disruptor
    // 1438956180,3.0,3.0,3.0,3.0,81.85727776,2
    // ts,open,high,low,close,volume,transactions
    public FileInfo transactionFile { get; set; }
    public TickerProducer(string transactionSourceFilePath)
    {
        transactionFile = new FileInfo(transactionSourceFilePath);
    }

    public void ProduceEvents(Action<PairCandle> createTickerAction)
    {
        List<PairCandle> transactions = File.ReadAllLines(transactionFile.FullName).Select(v => PairCandle.FromCSV(v)).ToList();
        //transactions.ForEach(t => createTickerAction.Invoke(t));
        int counter = 0;
        foreach (var transaction in transactions)
        {
            if (counter > 1000) break;
            createTickerAction(transaction);
            counter++;
        }
    }

    public void ProduceEvents(Action<List<PairCandle>> createTickerAction)
    {
        List<PairCandle> transactions = File.ReadAllLines(transactionFile.FullName).Select(v => PairCandle.FromCSV(v)).ToList();
        int batchSize = 1000;
        //int batchesToSend = (int)Math.Floor(Convert.ToDecimal(transactions.Count() / batchSize));
        int batchesToSend = 10;
        int curSkip = batchSize * batchesToSend;
        for (int i = batchesToSend; i > 0; i--)
        {
            createTickerAction(transactions.SkipLast(curSkip).TakeLast(batchSize).ToList());
            curSkip -= batchSize;
        }
    }
}