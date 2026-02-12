using System;
using System.IO;
using System.IO.Pipes;
using System.Text;

public class PipeServer
{
    public static NamedPipeServerStream namedPipeServer;
    public static void CreateServer(string pipeName)
    {
        namedPipeServer = new NamedPipeServerStream(pipeName, PipeDirection.Out, 1, PipeTransmissionMode.Byte);
        
        Console.WriteLine($"Server: Waiting for client connection to pipe '{pipeName}'...");
        namedPipeServer.WaitForConnection();
        Console.WriteLine("Server: Client connected.");

        StreamWriter writer = new StreamWriter(namedPipeServer);
        
        writer.WriteLine("Server has been established");
        //writer.Flush(); // Ensure the message is sent immediately
        
        //Console.WriteLine($"Server: Sent message: '{message}'");
        
    }

    public static void SendMessage(string pipeName, string message)
    {
        using (NamedPipeServerStream namedPipeServer = new NamedPipeServerStream(
            pipeName, PipeDirection.Out, 1, PipeTransmissionMode.Byte))
        {
            Console.WriteLine($"Server: Waiting for client connection to pipe '{pipeName}'...");
            namedPipeServer.WaitForConnection();
            Console.WriteLine("Server: Client connected.");

            using (StreamWriter writer = new StreamWriter(namedPipeServer))
            {
                writer.WriteLine(message);
                writer.Flush(); // Ensure the message is sent immediately
            }
            Console.WriteLine($"Server: Sent message: '{message}'");
        }
    }

    public static void sMain()
    {
        // Example usage:
        CreateServer("MyNamedPipe");
    }
}