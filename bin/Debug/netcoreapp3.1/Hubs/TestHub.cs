using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;
using AI;
using System.IO;

namespace BuzznetApp.Hubs
{
    public class TestHub : Hub
    {
        string dataFileName = "";
        string labelFileName = "";
        int[] pixel = null;
        int label = 0;
        int predict = 0;
        Random random = new Random();
        int[] result = new int[2];
        AI.ML.CNN.Model model;
        double[] probs;
        double[] softmaxOutput;
        AI.Core.fDataSet dataSet = new AI.Core.fDataSet();
        AI.Core.ModelSerializer modelSerializer = new AI.Core.ModelSerializer();
        AI.ML.CNN.Trainers.ADAM adam = new AI.ML.CNN.Trainers.ADAM();
        int index = 0;
        double maxValue = 0;
        public async Task Test(string dataset, string filename)
        {

            switch (dataset)
            {
                case "MNIST":
                case "mnist":
                    dataFileName = @"C:\files\data\mnist\train-images.idx3-ubyte";
                    labelFileName = @"C:\files\data\mnist\train-labels.idx1-ubyte";
                    model = modelSerializer.DeserializeCNN(filename);                    
                    List<AI.Core.fData> dataList = (List<AI.Core.fData>)AI.Core.UByteLoader.ReadGrayImage(dataFileName, 1, 0.0, 1.0, labelFileName, 0.0, 1.0);
                    index = random.Next(0, dataList.Count);
                    dataSet.fData = dataList;
                    pixel = dataList[index].Pixel;
                    label = dataList[index].DecodeLabel;
                    //model.predict
                    probs = model.Predict(dataSet.fData[index]);
                    softmaxOutput = adam.Softmax(probs);
                    maxValue = softmaxOutput.Max();
                    predict = softmaxOutput.ToList().IndexOf(maxValue);
                    //int lindex = random.Next(0, 5);
                    //predict = lindex == 2 ? random.Next(0, 9) : label;
                    result[0] = label;
                    result[1] = predict;
                    await Clients.All.SendAsync("ReceiveMessage", pixel);
                    await Clients.All.SendAsync("ReceiveLabel", result);
                    break;
                default:
                    dataSet = new AI.Core.fDataSet();
                    dataSet.Deserializer(dataset);
                    model = modelSerializer.DeserializeCNN(filename);
                    index = random.Next(0, dataSet.fData.Count);
                    pixel = dataSet.fData[index].Pixel;
                    label = dataSet.fData[index].DecodeLabel;
                    //model.predict
                    probs = model.Predict(dataSet.fData[index]);
                    softmaxOutput = adam.Softmax(probs);
                    maxValue = softmaxOutput.Max();
                    predict = softmaxOutput.ToList().IndexOf(maxValue);
                    //int lindex = random.Next(0, 5);
                    //predict = lindex == 2 ? random.Next(0, 9) : label;
                    result[0] = label;
                    result[1] = predict;
                    await Clients.All.SendAsync("ReceiveMessage", pixel);
                    await Clients.All.SendAsync("ReceiveLabel", result);
                    break;
            }
            
        }

    }
}
