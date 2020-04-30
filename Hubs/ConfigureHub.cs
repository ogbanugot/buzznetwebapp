using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.SignalR;
using System.Threading.Tasks;
using AI;
using System.IO;
using Newtonsoft.Json;
using AI.Core;

namespace BuzznetApp.Hubs
{
    public class ConfigureHub : Hub
    { 
        AI.ML.CNN.Model model = null;
        //string log = "";
        int epochs = 40;
        double learningRate = 0.5, momentum = 0.6;
        string dataFileName = "";
        string labelFileName = "";
        List<AI.Core.fData> dataList = null;
        AI.Core.fDataSet dataSet = null;
        string path = "";


        public async Task ConfigureModel(string configstring, string algorithm, string dataset, string filename)
        {
            await Clients.All.SendAsync("ReceiveMessage", "Configuring model...");
            model = new AI.ML.CNN.Model();
            model.Configure(configstring);

            await Clients.All.SendAsync("ReceiveMessage", "Initializing dataset...");
            switch (dataset)
            {
                case "MNIST":
                case "mnist":
                    dataFileName = @"C:\files\data\mnist\train-images.idx3-ubyte";
                    labelFileName = @"C:\files\data\mnist\train-labels.idx1-ubyte";
                    dataList = (List<AI.Core.fData>)AI.Core.UByteLoader.ReadGrayImage(dataFileName, 1, 0.0, 1.0, labelFileName, 0.0, 1.0);
                    dataSet = new AI.Core.fDataSet();
                    dataSet.fData = dataList;
                    await Clients.All.SendAsync("SetCookie", dataset, "dataset");
                    break;
                case "CIFAR":
                case "cifar":
                    dataFileName = @"C:\files\data\cifar\cifar-10-batches-bin\data_batch_1.bin";
                    dataList = (List<AI.Core.fData>)AI.Core.UByteLoader.ReadColorImage(dataFileName, 1, 0.0, 1.0, 0.0, 1.0);
                    dataSet = new AI.Core.fDataSet();
                    dataSet.fData = dataList;
                    await Clients.All.SendAsync("SetCookie", dataset, "dataset");
                    break;
                default:
                    dataSet = new AI.Core.fDataSet();
                    dataSet.Deserializer(dataset);
                    await Clients.All.SendAsync("SetCookie", dataset, "dataset");
                    break;
            }           
            await Clients.All.SendAsync("ReceiveMessage", "Initializing training...");
            //parse trainer string
            AI.ML.CNN.Trainers.DeltaRule deltaRule = new AI.ML.CNN.Trainers.DeltaRule();
            deltaRule.Configure<AI.ML.CNN.Lossfunc.CategoricalCrossEntropy>(model, epochs, dataSet, learningRate, momentum);
            
            await Clients.All.SendAsync("ReceiveMessage", deltaRule.NextVerbose());
            filename = @"C:\files\model\" + filename;
            AI.Core.ModelSerializer modelSerializer = new AI.Core.ModelSerializer();
            path = modelSerializer.Serialize(configstring, model, filename);           
            await Clients.All.SendAsync("SetCookie", path, "model");
            await Clients.All.SendAsync("ReceiveMessage", "done");
        }
    }
}
