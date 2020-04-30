using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;
using AI;
using System.IO;
using Emgu.CV;
using Emgu.CV.CvEnum;
using Emgu.CV.Structure;

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
        string[] cifarResult = new string[2];
        AI.ML.CNN.Model model;
        double[] probs;
        double[] softmaxOutput;
        AI.Core.fDataSet dataSet = new AI.Core.fDataSet();
        AI.Core.ModelSerializer modelSerializer = new AI.Core.ModelSerializer();
        AI.ML.CNN.Trainers.ADAM adam = new AI.ML.CNN.Trainers.ADAM();
        int index = 0;
        double maxValue = 0;
        List<AI.Core.fData> dataList;
        int size = 0;
        Mat matImg;
        Image<Gray, Byte> Gimg;
        Image<Bgr, Byte> Cimg;
        Image<Gray, Byte> Gimage;
        Image<Bgr, Byte> Cimage;
        int[] input_size;
        int[] resizedPixel;
        string[] classes = new string[] { "airplane", "automobile", "bird", "cat", "deer", "dog", "frog", "horse", "ship", "truck" };
        public async Task Test(string dataset, string filename)
        {
            switch (dataset)
            {
                case "MNIST":
                case "mnist":
                    dataFileName = @"C:\files\data\mnist\train-images.idx3-ubyte";
                    labelFileName = @"C:\files\data\mnist\train-labels.idx1-ubyte";
                    model = modelSerializer.DeserializeCNN(filename);                    
                    dataList = (List<AI.Core.fData>)AI.Core.UByteLoader.ReadGrayImage(dataFileName, 1, 0.0, 1.0, labelFileName, 0.0, 1.0);
                    index = random.Next(0, dataList.Count);
                    dataSet.fData = dataList;
                    pixel = dataList[index].Pixel;
                    label = dataList[index].DecodeLabel;
                    //model.predict
                    probs = model.Predict(dataSet.fData[index]);
                    softmaxOutput = adam.Softmax(probs);
                    maxValue = softmaxOutput.Max();
                    predict = softmaxOutput.ToList().IndexOf(maxValue);
                    size = (int)System.Math.Sqrt(dataSet.fData[0].Pixel.Length);
                    matImg = new Mat(size, size, DepthType.Cv8U, 3);
                    Gimg = matImg.ToImage<Gray, Byte>();
                    Gimage = setGrayPixels(Gimg, size, pixel);
                    input_size = new int[] { 256, 256 };
                    resizedPixel = Resize_Gray(Gimage, input_size);
                    //int lindex = random.Next(0, 5);
                    //predict = lindex == 2 ? random.Next(0, 9) : label;
                    result[0] = label;
                    result[1] = predict;
                    await Clients.All.SendAsync("ReceiveMessage", resizedPixel, 1);
                    await Clients.All.SendAsync("ReceiveLabel", result);
                    break;
                case "CIFAR":
                case "cifar":
                    dataFileName = @"C:\files\data\cifar\cifar-10-batches-bin\data_batch_1.bin";
                    model = modelSerializer.DeserializeCNN(filename);
                    dataList = (List<AI.Core.fData>)AI.Core.UByteLoader.ReadColorImage(dataFileName, 1, 0.0, 1.0, 0.0, 1.0);
                    index = random.Next(0, dataList.Count);
                    dataSet.fData = dataList;
                    pixel = dataSet.fData[index].Pixel;
                    label = dataSet.fData[index].DecodeLabel;
                    //model.predict
                    probs = model.Predict(dataSet.fData[index]);
                    softmaxOutput = adam.Softmax(probs);
                    maxValue = softmaxOutput.Max();
                    predict = softmaxOutput.ToList().IndexOf(maxValue);
                    size = (int)System.Math.Sqrt(dataSet.fData[0].Pixel.Length / 3);
                    matImg = new Mat(size, size, DepthType.Cv8U, 3);
                    Cimg = matImg.ToImage<Bgr, Byte>();
                    Cimage = setColorPixels(Cimg, size, pixel);
                    input_size = new int[] { 256, 256 };
                    resizedPixel = Resize_Color(Cimage, input_size);
                    //int lindex = random.Next(0, 5);
                    //predict = lindex == 2 ? random.Next(0, 9) : label;
                    cifarResult[0] = classes[label];
                    cifarResult[1] = classes[predict];
                    await Clients.All.SendAsync("ReceiveMessage", resizedPixel, 3);
                    await Clients.All.SendAsync("ReceiveLabel", cifarResult);
                    break;
                default:
                    dataSet = new AI.Core.fDataSet();
                    dataSet.Deserializer(dataset);
                    pixel = dataSet.fData[index].Pixel;
                    model = modelSerializer.DeserializeCNN(filename);
                    index = random.Next(0, dataSet.fData.Count);                 
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
                    if (model.Input.Output.Count == 3)
                    {
                        size = (int)System.Math.Sqrt(dataSet.fData[0].Pixel.Length / 3);
                        matImg = new Mat(size, size, DepthType.Cv8U, 3);
                        Cimg = matImg.ToImage<Bgr, Byte>();
                        Cimage = setColorPixels(Cimg, size, pixel);
                        input_size = new int[] { 256, 256 };
                        resizedPixel = Resize_Color(Cimage, input_size);
                        cifarResult[0] = classes[label];
                        cifarResult[1] = classes[predict];
                        await Clients.All.SendAsync("ReceiveMessage", resizedPixel, 3);
                        await Clients.All.SendAsync("ReceiveLabel", cifarResult);
                    }
                    else
                    {
                        size = (int)System.Math.Sqrt(dataSet.fData[0].Pixel.Length);
                        matImg = new Mat(size, size, DepthType.Cv8U, 3);
                        Gimg = matImg.ToImage<Gray, Byte>();
                        Gimage = setGrayPixels(Gimg, size, pixel);
                        input_size = new int[] { 256, 256 };
                        resizedPixel = Resize_Gray(Gimage, input_size);
                        //int lindex = random.Next(0, 5);
                        //predict = lindex == 2 ? random.Next(0, 9) : label;
                        result[0] = label;
                        result[1] = predict;
                        await Clients.All.SendAsync("ReceiveMessage", resizedPixel, 1);
                        await Clients.All.SendAsync("ReceiveLabel", result);
                    }                    
                    break;
            }
            
        }

        public Image<Gray, Byte> setGrayPixels(Image<Gray, Byte> img, int size, int[] pixel)
        {
            byte[,,] pix = img.Data;
            int c = 0;
            for (int i = 0; i < size; i++)
                for (int j = 0; j < size; j++)
                {
                    pix[i, j, 0] = (byte)pixel[c++];
                }
            img.Data = pix;
            return img;
        }
        public int[] Resize_Gray(Image<Gray, Byte> img, int[] input_size)
        {
            System.Drawing.Size size = new System.Drawing.Size();
            size.Width = input_size[0];
            size.Height = input_size[1];
            Image<Gray, Byte> img2 = new Image<Gray, Byte>(size.Width, size.Height);
            CvInvoke.Resize(img, img2, size, 0.0, 0.0, Inter.Linear);

            int[] image = new int[size.Height * size.Width];

            byte[,,] pix = new byte[size.Height, size.Width, 1];

            pix = img2.Data;
            int c = 0;
            for (int i = 0; i < size.Height; i++)
                for (int j = 0; j < size.Width; j++)
                {
                    image[c++] = pix[i, j, 0];
                }          

            return image;
        }

        public Image<Bgr, Byte> setColorPixels(Image<Bgr, Byte> img, int size, int[] pixel)
        {
            byte[,,] pix = img.Data;
            int c = 0;
            for (int i = 0; i < size; i++)
                for (int j = 0; j < size; j++)
                {
                    pix[i, j, 0] = (byte)pixel[c++];
                }
            for (int i = 0; i < size; i++)
                for (int j = 0; j < size; j++)
                {
                    pix[i, j, 1] = (byte)pixel[c++];
                }
            for (int i = 0; i < size; i++)
                for (int j = 0; j < size; j++)
                {
                    pix[i, j, 2] = (byte)pixel[c++];
                }

            img.Data = pix;
            return img;
        }
        public int[] Resize_Color(Image<Bgr, Byte> img, int[] input_size)
        {
            System.Drawing.Size size = new System.Drawing.Size();
            size.Width = input_size[0];
            size.Height = input_size[1];
            Image<Bgr, Byte> img2 = new Image<Bgr, Byte>(size.Width, size.Height);
            CvInvoke.Resize(img, img2, size, 0.0, 0.0, Inter.Linear);

            int[] image = new int[size.Height * size.Width * 3];

            byte[,,] pix = new byte[size.Height, size.Width, 3];

            pix = img2.Data;
            int c = 0;
            for (int i = 0; i < size.Height; i++)
                for (int j = 0; j < size.Width; j++)
                {
                    image[c++] = pix[i, j, 0];
                }
            for (int i = 0; i < size.Height; i++)
                for (int j = 0; j < size.Width; j++)
                {
                    image[c++] = pix[i, j, 1];
                }
            for (int i = 0; i < size.Height; i++)
                for (int j = 0; j < size.Width; j++)
                {
                    image[c++] = pix[i, j, 2];
                }

            return image;
        }

    }
}
