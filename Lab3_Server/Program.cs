using System;
using System.Net;
using System.Net.Http;
using System.IO;
using Utility;
using System.Text.Json;


namespace Lab3_Server
{
    class Program
    {
        const string okAnswer = "HttpStatusCode.Ok (200)";
        const string baseUri = "http://127.0.0.1:";
        const string pingMethod = "Ping/";
        const string postInputMethod = "PostInputData/";
        const string getAnswerMethod = "GetAnswer/";
        const string stopMethod = "Stop/";

        const string getInputMethod = "GetInputData/";
        const string writeAnswerMethod = "WriteAnswer/";

        static Input input = new Input();
        static Output output = null;
        static HttpListener listener = new HttpListener();
        static int port = -1;

        static void Main(string[] args)
        {
            
            Console.WriteLine("Please, type necessary port below");
            port = Convert.ToInt32(Console.ReadLine());

            string currentUri = baseUri + port + "/";     

            string pingUri = currentUri + pingMethod;
            string postInputUri = currentUri + postInputMethod;
            string getAnswerUri = currentUri + getAnswerMethod;
            string stopUri = currentUri + stopMethod;
            string getInputUri = currentUri + getInputMethod;
            string writeAnswerUri = currentUri + writeAnswerMethod;

            listener.Prefixes.Add(pingUri);
            listener.Prefixes.Add(postInputUri);
            listener.Prefixes.Add(getAnswerUri);
            listener.Prefixes.Add(stopUri);

            listener.Prefixes.Add(getInputUri);
            listener.Prefixes.Add(writeAnswerUri);

            listener.Start();

            bool isWorking = true;

            while (isWorking)
            {
                Console.WriteLine("Waiting for connetions...");

                HttpListenerContext context = listener.GetContext();
                HttpListenerRequest request = context.Request;
                string requestUri = request.Url.ToString();
                HttpListenerResponse response = context.Response;

               if(requestUri == pingUri)
                {
                    Ping(response);
                }
               else if (requestUri == postInputUri)
                {
                    RecieveInput(request);
                }
               else if (requestUri == getAnswerUri)
                {
                    SendAnswer(response);
                }
               else if(requestUri == stopUri)
                {
                    isWorking = false;
                }
                else if (requestUri == getInputUri)
                {
                    SendInput(response);
                }
                else if (requestUri == writeAnswerUri)
                {
                    RecieveOutput(request);
                }

            }

            
        }

        private static void Ping(HttpListenerResponse response)
        {
            string responseString = okAnswer;
            byte[] buffer = System.Text.Encoding.UTF8.GetBytes(responseString);

            response.ContentLength64 = buffer.Length;

            Stream outputStream = response.OutputStream;
            outputStream.Write(buffer, 0, buffer.Length);
            outputStream.Close();
            Console.WriteLine("Ping");
        }

        private static void RecieveInput(HttpListenerRequest request)
        {
            Stream inputStream = request.InputStream;
            byte[] buffer = new byte[1024];

            for(int i = 0; i < buffer.Length; i++)
            {
                buffer[i] = 0;
            }

            inputStream.Read(buffer, 0, buffer.Length);

            
            try
            {
                string inputString = System.Text.Encoding.UTF8.GetString(buffer);
                input = new Input(inputString);
                Console.WriteLine("Got input from jury:");
                Console.WriteLine(inputString);
            }
            catch (JsonException ex)
            {
                Console.WriteLine("Json Exception occured during recieving input from jury. Probably input was not a Json string.");
            }
        }

        private static void SendInput(HttpListenerResponse response)
        {
            if (input != null)
            {
                string responseString = input.SerializeInput();
                byte[] buffer = System.Text.Encoding.UTF8.GetBytes(responseString);

                response.ContentLength64 = buffer.Length;

                Stream outputStream = response.OutputStream;
                outputStream.Write(buffer, 0, buffer.Length);
                outputStream.Close();
                Console.WriteLine("Input was send to client");
            }
            else
            {
                Console.WriteLine("Input wasn't send to client because input is null");
            }
        }

        private static void RecieveOutput(HttpListenerRequest request)
        {
            Stream inputStream = request.InputStream;
            byte[] buffer = new byte[1024];

            for (int i = 0; i < buffer.Length; i++)
            {
                buffer[i] = 0;
            }

            inputStream.Read(buffer, 0, buffer.Length);

            try 
            {
                string inputString = System.Text.Encoding.UTF8.GetString(buffer);
                Console.WriteLine("Got output from client:");
                Console.WriteLine(inputString);
                output = new Output(inputString);
            }
            catch (JsonException ex)
            {
                Console.WriteLine("Json Exception occured during recieving answer from client. Probably answer was not a Json string.");
            }
        }

        private static void SendAnswer(HttpListenerResponse response)
        {
            if(output != null)
            {              
                string responseString = output.SerializeOutput();
                byte[] buffer = System.Text.Encoding.UTF8.GetBytes(responseString);

                response.ContentLength64 = buffer.Length;

                Stream outputStream = response.OutputStream;
                outputStream.Write(buffer, 0, buffer.Length);
                outputStream.Close();
                Console.WriteLine("Answer was send to jury");
            }
            else
            {
                Console.WriteLine("Answer wasn't send to jury because output is null");
            }
        }
    }
}
