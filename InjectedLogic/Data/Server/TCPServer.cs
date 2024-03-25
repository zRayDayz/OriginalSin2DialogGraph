using System.Buffers.Binary;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace InjectedLogic;

public class TCPServer
{
    private ArrayPoolKeeper<byte> arrayPoolKeeper;
    private TcpListener tcpListener;
    private CancellationToken cancellationToken;
    private IDataSender dataSender;
    private IIOProvider ioProvider;
    private SyncConfig syncConfig;

    private string ip;
    private int port;
    private int readTimeout;
    private int writeTimeout;
    private int serverWaitTimeBeforeNextSend;

    // UTF-8 BOM signature
    private readonly byte[] headerSignatureBytes = { 0x0, 0xEF, 0xBB, 0xBF };
    private const int bytesLengthOfDataSize = sizeof(int);
    private const int connectionResetByPeerErrorCode = 10054;

    public TCPServer(ConfigManager configManager, IDataSender dataSender, IIOProvider ioProvider, ArrayPoolKeeper<byte> arrayPoolKeeper)
    {
        this.dataSender = dataSender;
        syncConfig = configManager.SyncConfig;
        var config = configManager.Config;
        ip = config.Ip;
        port = config.Port;
        readTimeout = config.ReadTimeout;
        writeTimeout = config.WriteTimeout;
        serverWaitTimeBeforeNextSend = config.ServerWaitTimeBeforeNextSend;
        this.arrayPoolKeeper = arrayPoolKeeper;
        this.ioProvider = ioProvider;
    }
    
    public void StartServer(CancellationToken token)
    {
        cancellationToken = token;
        tcpListener = new TcpListener(IPAddress.Parse(ip), port);
        tcpListener.Start();

        while (true)
        {
            ioProvider.WriteLine("================ SERVER IS STARTING ================" + Environment.NewLine);
            try
            {
                StartDataTransferProcess();
            }
            catch (OperationCanceledException canceledException)
            {
                ioProvider.WriteLine("'CancellationToken' was canceled, the server is closing" + Environment.NewLine);
                break;
            }
            catch (Exception e)
            {
                var errorText = Environment.NewLine +
                                "================ SERVER ERROR ================" +
                                Environment.NewLine +
                                e +
                                Environment.NewLine +
                                "=======================================" +
                                Environment.NewLine;
                ioProvider.WriteErrorLine(errorText);

                bool exitServer = false;
                if (syncConfig.IsServerAutoRestartEnabled == false)
                {
                    lock (syncConfig.ConsoleLock)
                    {
                        exitServer = AskConsoleAboutRestartingServer();
                    }
                }
                if (exitServer) break;
            }
        }
        ioProvider.WriteLine("================ SERVER IS CLOSING ================" + Environment.NewLine);
        
        tcpListener.Stop();
    }

    private bool AskConsoleAboutRestartingServer()
    {
        ioProvider.WriteLine();
        ioProvider.WriteLine("A server error has occurred");
        ioProvider.WriteLine("Restart server?");
        ioProvider.WriteLine("1) yes - default");
        ioProvider.WriteLine("2) no");
        var enteredCommand = ioProvider.ReadLine();
                 
        bool exitServer = false;
        switch (enteredCommand)
        {
            case "yes":
                break;
            case "no":
                exitServer = true;
                break;
            default:
                break;
        }
        return exitServer;
    }

    private void StartDataTransferProcess()
    {
        TcpClient tempClient = null;
        
        ioProvider.WriteLine($"{Environment.NewLine}{DateTime.Now}\tSERVER: Waiting for a Сlient to connect");
        
        // The server will be started in a new thread without any synchronization context. So it is completely safe to use "GetAwaiter().GetResult()"
        tempClient = tcpListener.AcceptTcpClientAsync(cancellationToken).GetAwaiter().GetResult();

        using TcpClient client = tempClient;
        
        ioProvider.WriteLine($"{Environment.NewLine}{DateTime.Now}\tSERVER: Connection accepted from: {client.Client.RemoteEndPoint}");
        
        using NetworkStream stream = client.GetStream();
        stream.ReadTimeout = readTimeout;
        stream.WriteTimeout = writeTimeout;
        
        Span<byte> dataLengthBytes = stackalloc byte[bytesLengthOfDataSize];
        byte[] ACKBytes = Encoding.UTF8.GetBytes("<|ACK|>");
        
        while (true)
        {
            dataSender.OnNewServerCycle();
            
            cancellationToken.ThrowIfCancellationRequested();

            if (client.Connected == false)
            {
                throw new SocketException(connectionResetByPeerErrorCode);
            }

            if (dataSender.IsReadyToSendData())
            {
                byte[] writingBuffer = null;
                try
                {
                    #region Get data and its length
                    ReadOnlySpan<byte> data = dataSender.GetDataToWrite();
                    var dataLength = data.Length;
                    BinaryPrimitives.WriteInt32BigEndian(dataLengthBytes, dataLength);
                    #endregion

                    int lengthOfHeaderSignatureBytes = headerSignatureBytes.Length;
                    writingBuffer = arrayPoolKeeper.GetObject(dataLength + lengthOfHeaderSignatureBytes + bytesLengthOfDataSize);
                    Span<byte> writingBufferSpan = writingBuffer.AsSpan().Slice(0, dataLength + lengthOfHeaderSignatureBytes + bytesLengthOfDataSize);
                    
                    #region Prepare bytes of header + payload (data) to be sent
                    headerSignatureBytes.CopyTo(writingBufferSpan);
                    
                    Span<byte> writingBufferSpanLengthPart = writingBufferSpan.Slice(lengthOfHeaderSignatureBytes);
                    dataLengthBytes.CopyTo(writingBufferSpanLengthPart);
                    
                    Span<byte> writingBufferSpanDataPart = writingBufferSpanLengthPart.Slice(bytesLengthOfDataSize);
                    data.CopyTo(writingBufferSpanDataPart);
                    #endregion

                    if (syncConfig.IsServerLogEnabled) ioProvider.WriteLineSynchronizedWithoutWaiting($"{DateTime.Now}\tSERVER: Sent: {Encoding.UTF8.GetString(writingBufferSpanDataPart)}");
                    stream.Write(writingBufferSpan);

                    ReadData(stream, ACKBytes.Length, ACKBytes);
                    if (syncConfig.IsServerLogEnabled) ioProvider.WriteLineSynchronizedWithoutWaiting($"{DateTime.Now}\tSERVER: The client confirmed receiving the data");

                    dataSender.OnDataSuccessfullySent();
                }
                catch (Exception e)
                {
                    dataSender.OnDataUnsuccessfullySent();
                    throw new Exception("Exception during data transfer process", e);
                }
                finally
                {
                    if (writingBuffer != null) arrayPoolKeeper.ReturnObject(writingBuffer);
                }
            }

            if (serverWaitTimeBeforeNextSend > 0)
            {
                Thread.Sleep(serverWaitTimeBeforeNextSend);
            }
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

}