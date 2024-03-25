using System.Buffers.Binary;
using System.IO;
using System.Net.Sockets;
using System.Text;

namespace DialogGraph;

public class TCPClient : INetworkClient
{
    private const int delay = 1000;
    private readonly byte[] headerSignatureBytes = { 0x0, 0xEF, 0xBB, 0xBF };
    private const int bytesLengthOfHeaderSignature = sizeof(int);
    private const int bytesLengthOfDataSize = sizeof(int);
    private const int maxDataLength = 16384;
    private const int socketIsNotConnectedErrorCode = 10057;
    
    private string ip;
    private int port;
    private int maximumNumberOfAttemptsToConnect;
    private int readTimeout;
    private int writeTimeout;

    private SyncConfig syncConfig;
    private IProgress<byte[]> progress;
    private ILogger logger;
    private IExceptionHandler exceptionHandler;
    private CancellationToken cancellationToken;

    public TCPClient(ConfigManager configManager)
    {
        syncConfig = configManager.SyncConfig;
        var config = configManager.Config;
        ip = config.Ip;
        port = config.Port;
        maximumNumberOfAttemptsToConnect = config.MaximumNumberOfAttemptsToConnect;
        readTimeout = config.ReadTimeout;
        writeTimeout = config.WriteTimeout;
    }
    
    public async Task StartClientAsync(IProgress<byte[]> progress, ILogger logger, IExceptionHandler exceptionHandler, CancellationToken cancellationToken)
    {
        this.logger = logger;
        this.exceptionHandler = exceptionHandler;
        this.progress = progress;
        this.cancellationToken = cancellationToken;
        
        while (true)
        {
            logger.LogText(Environment.NewLine + "================ CLIENT IS STARTING ================" + Environment.NewLine);
            try
            {
                await ConnectToServerAndStartDataTransferProcess();
            }
            catch (OperationCanceledException canceledException)
            {
                logger.LogText("'CancellationToken' was canceled, the client is closing");
                break;
            }
            catch (Exception e)
            {
                var errorText = Environment.NewLine + "================ CLIENT ERROR ================" + Environment.NewLine +
                                e +
                                Environment.NewLine + "==============================================" + Environment.NewLine;
                exceptionHandler.HandleExceptionNonBlockingWay(errorText, this);
            }
            if (syncConfig.IsNetworkClientAutoRestartEnabled == false) break;
            logger.LogText(Environment.NewLine + "================ CLIENT IS RELOADING ================" + Environment.NewLine);
        }
        
        logger.LogText(Environment.NewLine + "================ CLIENT IS CLOSING ================" + Environment.NewLine);
    }


    private async Task ConnectToServerAndStartDataTransferProcess()
    {
        int attemptsToConnect = 0;
        TcpClient client = null;
        while (maximumNumberOfAttemptsToConnect <= 0 || attemptsToConnect < maximumNumberOfAttemptsToConnect)
        {
            cancellationToken.ThrowIfCancellationRequested();
            try
            {
                client = new TcpClient(ip, port);
                break;
            }
            catch (Exception e)
            {
                attemptsToConnect++;
                if (syncConfig.IsNetworkClientLogEnabled) LogText($"CLIENT: №{attemptsToConnect} attempt to connect failed");
                await Task.Delay(delay);
            }
        }

        if (client == null) throw new Exception("The client cannot establish a connection");
        
        await StartDataTransferProcess(client);
    }

    // "Async" methods do not support Span, therefore it is impossible to use "stackalloc"
    private async Task StartDataTransferProcess(TcpClient argClient)
    {
        using TcpClient tcpClient = argClient;
        using NetworkStream stream = tcpClient.GetStream();

        stream.ReadTimeout = readTimeout;
        stream.WriteTimeout = writeTimeout;
        
        if (syncConfig.IsNetworkClientLogEnabled) LogText($"CLIENT: Connection established: {tcpClient.Client.RemoteEndPoint}");
        
        byte[] receivedHeaderSignatureBytes = new byte[bytesLengthOfHeaderSignature];
        byte[] dataLengthBytes = new byte[bytesLengthOfDataSize];
        byte[] dataBuffer = new byte[1024];
        byte[] ACKBytes = Encoding.UTF8.GetBytes("<|ACK|>");

        while (true)
        {
            cancellationToken.ThrowIfCancellationRequested();
            
            if (tcpClient.Connected == false)
            {
                throw new SocketException(socketIsNotConnectedErrorCode);
            }
            
            if (stream.DataAvailable == false)
            {
                if (syncConfig.IsNetworkClientLogEnabled) LogText($"CLIENT: Waiting for a data on: {tcpClient.Client.RemoteEndPoint}");
                await Task.Delay(delay);
                continue;
            }

            ReadData(stream, receivedHeaderSignatureBytes.Length, receivedHeaderSignatureBytes);
            if (receivedHeaderSignatureBytes.SequenceEqual(headerSignatureBytes) == false)
            {
                throw new NotSupportedException("The client lost the beginning of the package");
            }
            
            ReadData(stream, dataLengthBytes.Length, dataLengthBytes);
            int dataLength = BinaryPrimitives.ReadInt32BigEndian(dataLengthBytes);;
            if (syncConfig.IsNetworkClientLogEnabled) LogText("CLIENT: Received Data length: " + dataLength);
            if (dataLength > maxDataLength) throw new OverflowException("The data length was not expected to be so long, an error is likely");
            
            EnsureArrayCapacity(ref dataBuffer, dataLength);
            ReadData(stream, dataLength, dataBuffer);
            
            if (syncConfig.IsNetworkClientLogEnabled) LogReceivedBytes(dataBuffer, dataLength);
            
            if (syncConfig.IsNetworkClientLogEnabled) LogText("CLIENT: Sending ACK");
            stream.Write(ACKBytes);

            byte[] dataCopyToReceive = new byte[dataLength];
            Array.Copy(dataBuffer, dataCopyToReceive, dataLength);
            progress.Report(dataCopyToReceive);
        }
    }
    
    // .Net 6 does not have ReadExactly() method
    void ReadData(NetworkStream stream, int dataLength, in byte[] arrayForData)
    {
        if (arrayForData == null) throw new ArgumentNullException(nameof(arrayForData));
        if (arrayForData.Length < dataLength) throw new ArgumentOutOfRangeException(nameof(dataLength), "The length of the data to be read cannot exceed the size of the array for this data");

        int readBytes;
        int totalReadBytes = 0;

        do
        {
            readBytes = stream.Read(arrayForData, totalReadBytes, dataLength - totalReadBytes);
            totalReadBytes += readBytes;

            if (readBytes == 0)
            {
                // Same exception as in Stream.ReadAtLeastCore()
                throw new EndOfStreamException("The connection was closed by the server. The latest data did not have time to be fully read");
            }
            
        } while (totalReadBytes < dataLength);
        
    }

    void EnsureArrayCapacity(ref byte[] arr, int targetLength)
    {
        if (arr.Length > targetLength) return;
        int closestNewLength = (int)Math.Ceiling((double)targetLength / 256) * 256;
        Array.Resize(ref arr, closestNewLength);
    }

    void LogReceivedBytes(byte[] dataBuffer, int dataLength)
    {
        var tempStrSpan = new Span<byte>(dataBuffer, 0, dataLength);
        string message = Encoding.UTF8.GetString(tempStrSpan);
        
        LogText($"CLIENT: Received bytes (Truncated, 50 first bytes): {CommonHelpers.JoinSpan(tempStrSpan, ", ", 50)}");
        LogText($"CLIENT: Received msg: {message}");
        LogText(Environment.NewLine, false);
    }
    
    void LogText(string text, bool addDateTimeNow = true)
    {
        if (addDateTimeNow)
        {
            var currentTime = DateTime.Now;
            logger.LogText(currentTime + "\t" + text + Environment.NewLine);
        }
        else logger.LogText(text + Environment.NewLine);

    }
}