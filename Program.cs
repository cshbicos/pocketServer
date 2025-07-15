using System.CommandLine;
using System.CommandLine.Parsing;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace de.mrman.pocketserver;

class Program
{

    static Task<int> Main(string[] args)
    {

        RootCommand rootCommand = new("A simple pocket knive of a server");

        Argument<int> portNumberArgument = new("Port")
        {
            Description = "The port to listen on",
            HelpName = "Port"

        };
        portNumberArgument.Validators.Add(result =>
        {
            if (result.GetValue(portNumberArgument) is <= 0 or > 99999)
                result.AddError("Port must be greater then 0");

        });
        rootCommand.Arguments.Add(portNumberArgument);

        rootCommand.SetAction((parseResult, cancellationToken) =>
        {
            int portNumber = parseResult.GetValue(portNumberArgument);
            return DoRootCommand(portNumber, cancellationToken);
        });


        return rootCommand.Parse(args).InvokeAsync();
    }

    public static async Task<int> DoRootCommand(int portNumber, CancellationToken cancellationToken)
    {
        var ipEndPoint = new IPEndPoint(IPAddress.Any, portNumber);
        TcpListener listener = new(ipEndPoint);

        try
        {
            listener.Start();
            Console.WriteLine("[Server] Running on port " + portNumber);


            using var tcpClient = await listener.AcceptTcpClientAsync(cancellationToken);
            try
            {
                Console.WriteLine("[Server] Client has connected");
                using var networkStream = tcpClient.GetStream();

                byte[] buffer = new byte[1024];
                while (true)
                {
                    if (networkStream.Read(buffer, 0, 1) == 0)
                        break;
                    string line = Encoding.UTF8.GetString(buffer);
                    Console.Write(line);
                }
            }
            catch (Exception)
            {
                Console.WriteLine("[Server] client connection lost");
            }
        }
        finally
        {
            listener.Stop();
        }

        return 1;
    }

}