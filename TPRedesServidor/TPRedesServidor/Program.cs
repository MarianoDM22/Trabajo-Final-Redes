using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace TPRedesServidor
{
	class Program
	{
		private static readonly Socket serverSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
		private static readonly List<Socket> clientSockets = new List<Socket>();
		private const int BUFFER_SIZE = 2048;
		private static readonly byte[] buffer = new byte[BUFFER_SIZE];
		private const int PORT = 3000;
		private const string IPLOCAL = "127.0.0.1";
		
		static void Main()
		{
			Console.Title = "Servidor";
			SetupServer();

			while(true)
			{

			}
		}

		private static void SetupServer()
		{
			Console.WriteLine("Iniciando servidor...");
			serverSocket.Bind(new IPEndPoint(IPAddress.Parse(IPLOCAL), PORT));
			serverSocket.Listen(0);
			serverSocket.BeginAccept(AcceptCallback, null);
			Console.WriteLine("Servidor listo");
			Console.WriteLine("Esperando clientes...");
		}

		private static void CloseAllSockets()
		{
			foreach (Socket socket in clientSockets)
			{
				socket.Shutdown(SocketShutdown.Both);
				socket.Close();
			}

			serverSocket.Close();
		}

		private static void AcceptCallback(IAsyncResult AR)
		{
			Socket socket;

			try
			{
				socket = serverSocket.EndAccept(AR);
			}
			catch (ObjectDisposedException)
			{
				return;
			}

			clientSockets.Add(socket);
			socket.BeginReceive(buffer, 0, BUFFER_SIZE, SocketFlags.None, ReceiveCallback, socket);
			Console.WriteLine("Cliente conectado...");
			serverSocket.BeginAccept(AcceptCallback, null);
		}

		private static void ReceiveCallback(IAsyncResult AR)
		{
			Socket current = (Socket)AR.AsyncState;
			int received;

				try
				{
					received = current.EndReceive(AR);
				}
				catch (SocketException)
				{
					Console.WriteLine("El cliente se desconecto de forma forzosa");
					current.Close();
					clientSockets.Remove(current);
					return;
				}

				byte[] recBuf = new byte[received];
				Array.Copy(buffer, recBuf, received);
				string text = Encoding.ASCII.GetString(recBuf);
				Console.WriteLine("Mensaje del cliente: " + text);

			if (text.ToLower() == "get time")
			{
				Console.WriteLine("El mensaje es un requerimiento de hora");
				byte[] dataTime = Encoding.ASCII.GetBytes(DateTime.Now.ToLongTimeString());
				current.Send(dataTime);
				Console.WriteLine("La hora se envio al cliente");
			}
			else if (text.ToLower() == "x")
			{
				current.Shutdown(SocketShutdown.Both);
				current.Close();
				clientSockets.Remove(current);
				Console.WriteLine("Cliente desconectado");
				return;
			}
			else
			{
				Console.Write("Escriba su respuesta: ");
				string response = Console.ReadLine();
				byte[] data = Encoding.ASCII.GetBytes(response);
				current.Send(data);

				if (response.ToLower() == "x")
				{
					CloseAllSockets();
				}
			}

			current.BeginReceive(buffer, 0, buffer.Length, SocketFlags.None, ReceiveCallback, current);
		}
	}
}
