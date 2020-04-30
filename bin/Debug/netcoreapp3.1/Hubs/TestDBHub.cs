using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;
using AI;
using System.IO;
using Newtonsoft.Json;
using AI.Core;

namespace BuzznetApp.Hubs
{
    public class TestDBHub : Hub
    {        
        int[] pixel = null;
        int label = 0;
        int predict = 0;
        Random random = new Random();
        int[] result = new int[2];
        AI.ML.CNN.Model model;
        double[] probs;
        double[] softmaxOutput;
        public async Task Test(string dataset, string filename)
        {            
            ModelSerializer modelSerializer = new ModelSerializer();
            model = modelSerializer.DeserializeCNN(filename);
            
            fDataSet testdataset = new fDataSet();
            testdataset.Deserializer(dataset);

            int index = random.Next(0, testdataset.fData.Count);
            AI.Core.fDataSet dataSet = new AI.Core.fDataSet();
            dataSet.fData = testdataset.fData;
            pixel = testdataset.fData[index].Pixel;
            label = testdataset.fData[index].DecodeLabel;

            //model.predict
            probs = model.Predict(dataSet.fData[index]);
            AI.ML.CNN.Trainers.ADAM adam = new AI.ML.CNN.Trainers.ADAM();
            softmaxOutput = adam.Softmax(probs);
            double maxValue = softmaxOutput.Max();
            predict = softmaxOutput.ToList().IndexOf(maxValue);

            //int lindex = random.Next(0, 5);
            //predict = lindex == 2 ? random.Next(0, 9) : label;
            result[0] = label;
            result[1] = predict;
            await Clients.All.SendAsync("ReceiveMessage", pixel);
            await Clients.All.SendAsync("ReceiveLabel", result);
        }
    }
}
