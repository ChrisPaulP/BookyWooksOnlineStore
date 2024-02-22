using RabbitMQ.Client;

namespace BookyWooks.RabbitMqMessageBus;

public class RabbitMqConnectionSetUp : IRabbitMqConnectionSetUp
{
    //TODO: move to appSettings file
    private readonly string _hostname;
    private readonly string _password;
    private readonly string _username;
    private IConnection _connection;
   
    public RabbitMqConnectionSetUp()
    {
        _hostname = "localhost";
        _password = "guest";
        _username = "guest";
    }
    private void CreateConnection()
    {
        try
        {
            var factory = new ConnectionFactory
            {
                HostName = _hostname,
                UserName = _username,
                Password = _password
            };
            _connection = factory.CreateConnection();

        }
        catch (Exception)
        {
            //log exception
        }
    }
    public bool IsConnected
    {
        get
        {
            return ConnectionExists();
        }
    }
    private bool ConnectionExists()
    {
        if (_connection != null)
        {
            return true;
        }
        CreateConnection();
        return _connection != null;
    }
    public IModel CreateModel()
    {
        if (!IsConnected)
        {
            throw new InvalidOperationException("No RabbitMQ connections are available to perform this action");
        }

        return _connection.CreateModel();
    }
}