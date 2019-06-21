using System;
using System.Net.Sockets;
using System.Net;
using System.Text;

namespace TPRedesCliente
{
    class Program
    {
        private static readonly Socket ClientSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

		static void Main()
        {
            Console.Title = "Cliente";
            ConnectToServer();
            RequestLoop();
            Exit();
        }

        private static void ConnectToServer()
        {
            int attempts = 0;

            while (!ClientSocket.Connected)
            {
                attempts++;
				if(attempts > 1)
				{
					Console.WriteLine("Datos incorrectos");
				}
                Console.WriteLine("Intentando conexion, intento numero: " + attempts);
				DataConnect();
            }

            Console.Clear();
            Console.WriteLine("Conectado");
        }

		private static void DataConnect()
		{
			Console.WriteLine("Ingrese la IP del servidor: ");
			string requestIp = Console.ReadLine();

			Console.WriteLine("Ingrese el PUERTO del servidor: ");
			int requestPort = Int32.Parse(Console.ReadLine());

			try
			{
				ClientSocket.Connect(requestIp, requestPort);
			}
			catch(SocketException)
			{
				Console.Clear();
			}
		}

        private static void RequestLoop()
        {
            Console.WriteLine(@"<Escribe ""x"" para desconectarte>");

            while (true)
            {
                SendRequest();
                ReceiveResponse();
            }
        }

        private static void Exit()
        {
            SendString("x");
            ClientSocket.Shutdown(SocketShutdown.Both);
            ClientSocket.Close();
            Environment.Exit(0);
        }

        private static void SendRequest()
        {
            Console.Write("Escriba su mensaje: ");
            string request = Console.ReadLine();
            SendString(request);

            if (request.ToLower() == "x")
            {
                Exit();
            }
        }

        private static void SendString(string text)
        {
            byte[] buffer = Encoding.ASCII.GetBytes(text);
            ClientSocket.Send(buffer, 0, buffer.Length, SocketFlags.None);
        }

        private static void ReceiveResponse()
        {
			var buffer = new byte[2048];
			int received = ClientSocket.Receive(buffer, SocketFlags.None);
            if (received == 0) return;
            var data = new byte[received];
            Array.Copy(buffer, data, received);
            string text = Encoding.ASCII.GetString(data);
            Console.WriteLine("Mensaje del servidor: " + text);
        }
    }
}
